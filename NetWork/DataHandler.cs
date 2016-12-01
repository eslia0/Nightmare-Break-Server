using System;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

public enum Result
{
    Success = 0,
    Fail,
}

public class DataHandler
{
    public enum Source
    {
        ServerSource = 0,
        ClientSource,
    }
    
    public const byte tcpSource = 0;
    public const byte udpSource = 1;

    public Queue<DataPacket> receiveMsgs;
    public Queue<DataPacket> sendMsgs;

    public Dictionary<Socket, string> loginUser;
    public Dictionary<string, UserState> userState;

    AccountDatabase database;
    RoomManager roomManager;

    object receiveLock;
    object sendLock;

    RecvNotifier recvNotifier;
    public delegate void RecvNotifier(DataPacket packet);
    private Dictionary<int, RecvNotifier> m_notifier = new Dictionary<int, RecvNotifier>();

    public DataHandler(Queue<DataPacket> receiveQueue, Queue<DataPacket> sendQueue, object newReceiveLock, object newSendLock)
    {
        receiveMsgs = receiveQueue;
        sendMsgs = sendQueue;
        receiveLock = newReceiveLock;
        sendLock = newSendLock;
        loginUser = new Dictionary<Socket, string>();
        userState = new Dictionary<string, UserState>();

        SetNotifier();

        database = AccountDatabase.Instance;
        database.InitailizeDatabase();
        roomManager = new RoomManager();

        Thread handleThread = new Thread(new ThreadStart(DataHandle));
        handleThread.Start();
    }

    public void DataHandle()
    {
        while (true)
        {
            if (receiveMsgs.Count > 0)
            {
                DataPacket packet = new DataPacket();

                //패킷을 Dequeue 한다 패킷 : 메시지 타입 + 메시지 내용, 소켓
                lock (receiveLock)
                {
                    try
                    {
                        packet = receiveMsgs.Dequeue();
                    }
                    catch
                    {
                        Console.WriteLine("DataHandler::DataHandle.Dequeue 에러");
                    }
                }

                HeaderData headerData = new HeaderData();
                HeaderSerializer headerSerializer = new HeaderSerializer();
                headerSerializer.SetDeserializedData(packet.msg);
                headerSerializer.Deserialize(ref headerData);

                ResizeByteArray(0, UnityServer.packetSource + UnityServer.packetId, ref packet.msg);

                //Dictionary에 등록된 델리게이트 메소드에서 PacketId를 반환받는다.
                if (m_notifier.TryGetValue(headerData.id, out recvNotifier))
                {
                    recvNotifier(packet);
                }
                else
                {
                    Console.WriteLine("DataHandler::DataHandle.TryGetValue 에러 ");
                    Console.WriteLine("패킷 출처 : " + headerData.source);
                    Console.WriteLine("패킷 ID : " + headerData.id);
                }
            }
        }
    }

    public void SetNotifier()
    {
        m_notifier.Add((int)ClientPacketId.CreateAccount, CreateAccount);
        m_notifier.Add((int)ClientPacketId.DeleteAccount, DeleteAccount);
        m_notifier.Add((int)ClientPacketId.Login, Login);
        m_notifier.Add((int)ClientPacketId.Logout, Logout);
        m_notifier.Add((int)ClientPacketId.GameClose, GameClose);
        m_notifier.Add((int)ClientPacketId.CreateCharacter, CreateCharacter);
        m_notifier.Add((int)ClientPacketId.DeleteCharacter, DeleteCharacter);
        m_notifier.Add((int)ClientPacketId.SelectCharacter, SelectCharacter);
        m_notifier.Add((int)ClientPacketId.CreateRoom, CreateRoom);
        m_notifier.Add((int)ClientPacketId.EnterRoom, EnterRoom);
        m_notifier.Add((int)ClientPacketId.ExitRoom, ExitRoom);
        m_notifier.Add((int)ClientPacketId.RoomUserData, RoomUserData);
        m_notifier.Add((int)ClientPacketId.StartGame, StartGame);
        m_notifier.Add((int)ClientPacketId.RequestUDPConnection, RequestUDPConnection);
        m_notifier.Add((int)ClientPacketId.UDPConnectComplete, UDPConnectComplete);
    }

    public void CreateAccount(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "가입요청");

        AccountPacket accountPacket = new AccountPacket(packet.msg);
        AccountData accountData = accountPacket.GetData();

        Console.WriteLine("아이디 : " + accountData.Id + "패스워드 : " + accountData.Password);

        Result result = Result.Fail;

        try
        {
            if (database.AddAccountData(accountData.Id, accountData.Password))
            {
                result = Result.Success;
                Console.WriteLine("가입 성공");
            }
            else
            {
                result = Result.Fail;
                Console.WriteLine("가입 실패");
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::CreateAccount.AddPlayerData 에러");
            result = Result.Fail;
        }

        ResultData resultData = new ResultData((byte)result);
        ResultPacket resultDataPacket = new ResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.CreateAccountResult);

        byte[] msg = CreatePacket(resultDataPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    public void DeleteAccount(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + " 탈퇴요청");

        AccountPacket accountPacket = new AccountPacket(packet.msg);
        AccountData accountData = accountPacket.GetData();

        Console.WriteLine("아이디 : " + accountData.Id + "패스워드 : " + accountData.Id);

        Result result = Result.Fail;

        try
        {
            if (database.DeleteAccountData(accountData.Id, accountData.Password) == Result.Success)
            {
                result = Result.Success;
                Console.WriteLine("탈퇴 성공");
            }
            else if (database.DeleteAccountData(accountData.Id, accountData.Password) == Result.Fail)
            {
                result = Result.Fail;
                Console.WriteLine("탈퇴 실패");
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::DeleteAccount.RemovePlayerData 에러");
            result = Result.Fail;
        }

        ResultData resultData = new ResultData((byte)result);
        ResultPacket resultDataPacket = new ResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.DeleteAccountResult);

        byte[] msg = CreatePacket(resultDataPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    public void Login(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + " 로그인");

        AccountPacket accountPacket = new AccountPacket(packet.msg);
        AccountData accountData = accountPacket.GetData();

        Console.WriteLine("아이디 : " + accountData.Id + "비밀번호 : " + accountData.Password);

        Result result = Result.Fail;

        try
        {
            if (database.AccountData.Contains(accountData.Id))
            {
                if (((AccountData)database.AccountData[accountData.Id]).Password == accountData.Password)
                {
                    if (!loginUser.ContainsValue(accountData.Id))
                    {
                        result = Result.Success;
                        Console.WriteLine("로그인 성공");
                        loginUser.Add(packet.client, accountData.Id);
                        userState.Add(accountData.Id, new UserState(accountData.Id, -1));

                        database.AddUserData(accountData.Id);
                    }
                    else
                    {
                        Console.WriteLine("현재 접속중인 아이디입니다.");

                        if (CompareIP(GetSocket(accountData.Id).RemoteEndPoint.ToString(), packet.client.RemoteEndPoint.ToString()))
                        {
                            loginUser.Remove(GetSocket(accountData.Id));
                            Console.WriteLine("현재 접속중 해제");
                        }
                        result = Result.Fail;
                    }
                }
                else
                {
                    Console.WriteLine("패스워드가 맞지 않습니다.");
                    result = Result.Fail;
                }
            }
            else
            {
                Console.WriteLine("존재하지 않는 아이디입니다.");
                result = Result.Fail;
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::Login.ContainsValue 에러");
            result = Result.Fail;
        }

        ResultData resultData = new ResultData((byte)result);
        ResultPacket resultDataPacket = new ResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.LoginResult);

        byte[] msg = CreatePacket(resultDataPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    /*
    public ServerPacketId ReLogin(DataPacket packet)
    {
        Console.WriteLine(tcpPacket.client.RemoteEndPoint.ToString() + " 재로그인요청");

        AccountPacket accountPacket = new AccountPacket(data);
        AccountData accountData = accountPacket.GetData();

        Console.WriteLine("아이디 : " + accountData.Id);

        try
        {
            if (database.AccountData.Contains(accountData.Id))
            {
                if (!loginUser.ContainsValue(accountData.Id))
                {
                    msg[0] = (byte)Result.Success;
                    Console.WriteLine("로그인 성공");
                    loginUser.Add(tcpPacket.client, accountData.Id);
                }
                else
                {
                    Console.WriteLine("현재 접속중인 아이디입니다.");

                    if (CompareIP(GetSocket(accountData.Id).RemoteEndPoint.ToString(), tcpPacket.client.RemoteEndPoint.ToString()))
                    {
                        loginUser.Remove(GetSocket(accountData.Id));
                        Console.WriteLine("현재 접속중 해제");
                    }
                    msg[0] = (byte)Result.Fail;
                }
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::ReLogin.ContainsValue 에러");
            msg[0] = (byte)Result.Fail;
        }

        return ServerPacketId.LoginResult;
    }
    */

    public void Logout(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + " 로그아웃요청");

        string id = loginUser[packet.client];

        Result result = Result.Fail;

        try
        {
            if (loginUser.ContainsKey(packet.client))
            {
                if (userState.ContainsKey(id))
                {
                    if (userState[id].state >= 0)
                    {
                        roomManager.ExitRoom(userState[id].state, packet.client);
                    }

                    userState.Remove(id);
                }

                loginUser.Remove(packet.client);
                Console.WriteLine(id + "로그아웃");
                result = Result.Success;
            }
            else
            {
                Console.WriteLine("로그인되어있지 않은 아이디입니다. : " + id);
                result = Result.Fail;
            }

            if (userState.ContainsKey(id))
            {
                userState.Remove(id);
            }
            else
            {
                Console.WriteLine("로그인되어있지 않은 캐릭터입니다. : " + id);
                result = Result.Fail;
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::Logout.ContainsKey 에러");
            result = Result.Fail;
        }

        ResultData resultData = new ResultData((byte)result);
        ResultPacket resultDataPacket = new ResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.LogoutResult);

        byte[] msg = CreatePacket(resultDataPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //게임 종료
    public void GameClose(DataPacket packet)
    {
        try
        {
            packet.client.Close();

            string id = loginUser[packet.client];

            if (userState.ContainsKey(id))
            {
                if(userState[id].state >= 0)
                {
                    roomManager.ExitRoom(userState[id].state, packet.client);
                }

                userState.Remove(id);
            }

            if (loginUser.ContainsKey(packet.client))
            {
                database.FileSave(id + ".data", database.GetUserData(id));
                database.UserData.Remove(id);

                loginUser.Remove(packet.client);
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::LoginUser.Close 에러");
        }
    }

    //캐릭터 생성
    public void CreateCharacter(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "캐릭터 생성");

        CreateCharacterPacket createCharacterPacket = new CreateCharacterPacket(packet.msg);
        CreateCharacterData createCharacterData = createCharacterPacket.GetData();

        string id = loginUser[packet.client];
        UserData userData = database.GetUserData(id);

        Result result = Result.Fail;

        try
        {
            userData.CreateHero(createCharacterData);
            database.FileSave(id + ".data", userData);

            result = Result.Success;
        }
        catch
        {
            Console.WriteLine("DataHandler::Createcharacter.CreateHero 에러");
            result = Result.Fail;
        }

        ResultData resultData = new ResultData((byte)result);
        ResultPacket resultDataPacket = new ResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.CreateCharacterResult);

        byte[] msg = CreatePacket(resultDataPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //캐릭터 삭제
    public void DeleteCharacter(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "캐릭터 삭제");

        DeleteCharacterPacket deleteCharacterPacket = new DeleteCharacterPacket(packet.msg);
        DeleteCharacterData deleteCharacterData = deleteCharacterPacket.GetData();

        string id = "";

        try
        {
            id = loginUser[packet.client];
        }
        catch
        {
            Console.WriteLine("Datahandler::SelectCharacter.loginUser 에러");
        }

        UserData userData = database.GetUserData(id);

        Result result = Result.Fail;

        try
        {
            result = Result.Success;
            userData.DeleteHero(deleteCharacterData.Index);
        }
        catch
        {
            Console.WriteLine("DataHandler::Createcharacter.CreateHero 에러");
            result = Result.Fail;
        }

        database.FileSave(id + ".data", userData);

        ResultData resultData = new ResultData((byte)result);
        ResultPacket resultDataPacket = new ResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.DeleteChracterResult);

        byte[] msg = CreatePacket(resultDataPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }
    
    //캐릭터 선택 후 게임 시작
    public void SelectCharacter(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "캐릭터 선택");

        SelectCharacterPacket selectCharacterPacket = new SelectCharacterPacket(packet.msg);
        SelectCharacterData selectCharacterData = selectCharacterPacket.GetData();

        string id = "";

        try
        {
            id = loginUser[packet.client];
        }
        catch
        {
            Console.WriteLine("Datahandler::SelectCharacter.loginUser 에러");
        } 

        Result result = Result.Fail;

        try
        {
            userState[id] = new UserState(id, selectCharacterData.Index);
            result = Result.Success;
        }
        catch
        {
            Console.WriteLine("DataHandler::Createcharacter.CreateHero 에러");
            result = Result.Fail;
        }

        ResultData resultData = new ResultData((byte)result);
        ResultPacket resultDataPacket = new ResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.SelectCharacterResult);

        byte[] msg = CreatePacket(resultDataPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //캐릭터 정보 요청
    public void RequestCharacterStatus(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "캐릭터 정보 요청");

        string id = loginUser[packet.client];
        int character = userState[id].characterId;

        HeroData heroData = database.GetHeroData(id, character);
        CharacterStatusData characterStatusData = heroData.GetCharacterStatusData();
        CharacterStatusPacket characterStatusPacket = new CharacterStatusPacket(characterStatusData);
        characterStatusPacket.SetPacketId((int)ServerPacketId.CharacterStatus);

        byte[] msg = CreatePacket(characterStatusPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //방 목록 요청
    public void RequestRoomList(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "방 목록 요청");

        RoomListPacket roomListPacket = roomManager.GetRoomList();
        roomListPacket.SetPacketId((int)ServerPacketId.RoomList);

        byte[] msg = CreatePacket(roomListPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //스킬 투자
    public void SkillUp(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "스킬 투자");
        SkillUpPacket skillUpPacket = new SkillUpPacket(packet.msg);
        SkillUpData skillUpData = skillUpPacket.GetData();

        string id = loginUser[packet.client];
        int characterId = userState[id].characterId;

        HeroData heroData = database.GetHeroData(id, characterId);

        heroData.SkillUp(skillUpData.SkillIndex);
    }

    //장비 강화
    public void EquipUpgrade(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "장비 강화");
        EquipUpgradePacket equipUpgradePacket = new EquipUpgradePacket(packet.msg);
        EquipUpgradeData equipUpgradeData = equipUpgradePacket.GetData();

        string id = loginUser[packet.client];
        int characterId = userState[id].characterId;

        HeroData heroData = database.GetHeroData(id, characterId);

        heroData.EquipUpgrade(equipUpgradeData.EquipIndex);
    }

    //방 생성
    public void CreateRoom(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "방 생성");
        CreateRoomPacket createRoomPacket = new CreateRoomPacket(packet.msg);
        CreateRoomData createRoomData = createRoomPacket.GetData();

        string id = loginUser[packet.client];
        int characterId = userState[id].characterId;

        Console.WriteLine("Id : " + id);
        Console.WriteLine("characterId : " + characterId);

        int result = roomManager.CreateRoom(packet.client, database.GetHeroData(id, characterId), createRoomData);

        Console.WriteLine("방 생성 번호 : " + result);

        RoomResultData resultData = new RoomResultData(result + 1);
        RoomResultPacket resultDataPacket = new RoomResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.CreateRoomResult);

        byte[] msg = CreatePacket(resultDataPacket);
        packet = new DataPacket(msg, packet.client);
        
        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //방 입장
    public void EnterRoom(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "방 입장");
        EnterRoomPacket enterRoomPacket = new EnterRoomPacket(packet.msg);
        EnterRoomData enterRoomData = enterRoomPacket.GetData();

        string id = loginUser[packet.client];
        int characterId = userState[id].characterId;

        int result = -1;

        try
        {
            Console.WriteLine(enterRoomData.RoomNum + "번 방에 " + packet.client.RemoteEndPoint.ToString() + "유저 입장");

            result = roomManager.Room[enterRoomData.RoomNum].AddPlayer(packet.client, database.GetHeroData(id, characterId));

            if (result != -1)
            {
                userState[id].state = enterRoomData.RoomNum;
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::EnterRoom.AddPlayer 에러");
            result = -1;
        }

        RoomResultData resultData = new RoomResultData(result + 1);
        RoomResultPacket resultDataPacket = new RoomResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.EnterRoomResult);

        byte[] msg = CreatePacket(resultDataPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //방 유저 정보 요청
    public void RoomUserData(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "방 유저 정보 요청");

        string id = loginUser[packet.client];

        RoomUserData roomUserData = new RoomUserData(roomManager.Room[userState[id].state]);
        RoomUserPacket roomUserPacket = new RoomUserPacket(roomUserData);
        roomUserPacket.SetPacketId((int)ServerPacketId.RoomUserData);

        byte[] msg = CreatePacket(roomUserPacket);
        packet = new DataPacket(msg, packet.client);
        
        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //방 퇴장
    public void ExitRoom(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "방 퇴장");
        ExitRoomPacket exitRoomPacket = new ExitRoomPacket(packet.msg);
        ExitRoomData exitRoomData = exitRoomPacket.GetData();

        string id = loginUser[packet.client];
        int characterId = userState[id].characterId;

        Result result = Result.Fail;

        try
        {
            if (roomManager.Room[exitRoomData.RoomNum].DeletePlayer(packet.client))
            {
                userState[id].state = 0;
                result = Result.Success;
            }
            else
            {
                result = Result.Fail;
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::ExitRoom.DeletePlayer 에러");
            result = Result.Fail;
        }

        ResultData resultData = new ResultData((byte)result);
        ResultPacket resultDataPacket = new ResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.ExitRoomResult);

        byte[] msg = CreatePacket(resultDataPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //게임 시작
    public void StartGame(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "게임 시작");

        string id = loginUser[packet.client];
        int roomNum = userState[id].state;

        byte result = (byte) Result.Success;

        ResultData resultData = new ResultData(result);
        ResultPacket resultDataPacket = new ResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.StartGame);

        byte[] msg = CreatePacket(resultDataPacket);
        
        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            if (roomManager.Room[roomNum].Socket[i] != null)
            {
                Console.WriteLine(i + "번 유저 : " + roomManager.Room[roomNum].Socket[i].RemoteEndPoint.ToString());
                packet = new DataPacket(msg, roomManager.Room[roomNum].Socket[i]);

                lock (sendLock)
                {
                    sendMsgs.Enqueue(packet);
                }
            }
        }
    }

    //UDP 연결
    public void RequestUDPConnection(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "UDP 연결 요청");

        string id = loginUser[packet.client];
        int roomNum = userState[id].state;

        string[] ip = new string[roomManager.Room[roomNum].PlayerNum];

        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            if (roomManager.Room[roomNum].Socket[i] != null)
            {
                ip[i] = roomManager.Room[roomNum].Socket[i].RemoteEndPoint.ToString();
            }
        }

        UDPConnectionData udpConnctionData = new UDPConnectionData(ip);
        UDPConnectionPacket udpConnctionDataPacket = new UDPConnectionPacket(udpConnctionData);
        udpConnctionDataPacket.SetPacketId((int)ServerPacketId.UDPConnection);

        byte[] msg = CreatePacket(udpConnctionDataPacket);

        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //UDP 연결 완료
    public void UDPConnectComplete(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "UDP 연결 완료");

        string id = loginUser[packet.client];
        int roomNum = userState[id].state;
        int playerNum = roomManager.Room[roomNum].FindPlayerWithSocket(packet.client);
        roomManager.Room[roomNum].Ready[playerNum] = true;

        for (int i = 0; i < roomManager.Room[roomNum].PlayerNum; i++)
        {
            if (!roomManager.Room[roomNum].Ready[i])
            {
                return;
            }
        }

        Console.WriteLine(roomManager.Room[roomNum] + "번 방 게임 시작");

        ResultData resultData = new ResultData();
        ResultPacket resultPacket = new ResultPacket(resultData);
        resultPacket.SetPacketId((int)ServerPacketId.StartDungeon);

        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            if(roomManager.Room[roomNum].Socket[i] != null)
            {
                packet = new DataPacket(CreatePacket(resultPacket), roomManager.Room[roomNum].Socket[i]);

                Console.WriteLine("보내는 ip : " + roomManager.Room[roomNum].Socket[i]);

                lock (sendLock)
                {
                    sendMsgs.Enqueue(packet);
                }
            }            
        }        
    }

    //패킷의 헤더 생성
    byte[] CreateHeader<T>(Packet<T> data)
    {
        byte[] msg = data.GetPacketData();

        HeaderData headerData = new HeaderData();
        HeaderSerializer headerSerializer = new HeaderSerializer();

        headerData.length = (short)msg.Length;
        headerData.source = (byte)Source.ServerSource;
        headerData.id = (byte)data.GetPacketId();

        headerSerializer.Serialize(headerData);
        byte[] header = headerSerializer.GetSerializedData();

        return header;
    }

    //패킷 생성
    byte[] CreatePacket<T>(Packet<T> data)
    {
        byte[] msg = data.GetPacketData();
        byte[] header = CreateHeader(data);
        byte[] packet = CombineByte(header, msg);

        return packet;
    }

    bool CompareIP(string ip1, string ip2)
    {
        if (ip1.Substring(0, ip1.IndexOf(":")) == ip2.Substring(0, ip2.IndexOf(":")))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Socket GetSocket(string Id)
    {
        foreach (KeyValuePair<Socket, string> client in loginUser)
        {
            if (client.Value == Id)
            {
                return client.Key;
            }
        }

        return null;
    }

    //index 부터 length만큼을 잘라 반환하고 매개변수 배열을 남은 만큼 잘라서 반환한다
    public static byte[] ResizeByteArray(int index, int length, ref byte[] array)
    {
        //desArray = 자르고 싶은 배열
        //sourArray = 자르고 남은 배열 => 원래 배열
        //ref 연산자로 원래 배열을 변경하게 된다.

        byte[] desArray = new byte[length];
        byte[] sourArray = new byte[array.Length - length];

        Array.Copy(array, index, desArray, 0, length);
        Array.Copy(array, length, sourArray, 0, array.Length - length);
        array = sourArray;

        return desArray;
    }

    public static byte[] CombineByte(byte[] array1, byte[] array2)
    {
        byte[] array3 = new byte[array1.Length + array2.Length];
        Array.Copy(array1, 0, array3, 0, array1.Length);
        Array.Copy(array2, 0, array3, array1.Length, array2.Length);
        return array3;
    }

    public static byte[] CombineByte(byte[] array1, byte[] array2, byte[] array3)
    {
        byte[] array4 = CombineByte(CombineByte(array1, array2), array3);
        return array4;
    }
}

public class UserState
{
	public string id;
    public int characterId;
    public int state;

	public UserState(string newId, int newCharacterId)
	{
		id = newId;
        characterId = newCharacterId;
        state = -1;
	}
}

public class HeaderData
{
    // 헤더 == [2바이트 - 패킷길이][1바이트 - 출처][1바이트 - ID]
    public short length; // 패킷의 길이
    public byte source; // 패킷의 출처
    public byte id; // 패킷 ID
}