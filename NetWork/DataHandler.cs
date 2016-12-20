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
    private static DataHandler instance;

    public static DataHandler Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new DataHandler();
            }

            return instance;
        }
    }

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
    DungeonDatabase dungeonDatabase;
    MonsterDatabase monsterDatabase;
    CharacterDatabase characterDatabase;
    RoomManager roomManager;

    object receiveLock;
    object sendLock;

    public RoomManager RoomManager {get { return roomManager; } }

    RecvNotifier recvNotifier;
    public delegate void RecvNotifier(DataPacket packet);
    private Dictionary<int, RecvNotifier> m_notifier = new Dictionary<int, RecvNotifier>();

    public DataHandler()
    {
        receiveMsgs = new Queue<DataPacket>();
        sendMsgs = new Queue<DataPacket>();
        receiveLock = new object();
        sendLock = new object();
        loginUser = new Dictionary<Socket, string>();
        userState = new Dictionary<string, UserState>();

        SetNotifier();

        database = AccountDatabase.Instance;
        database.InitailizeDatabase();
        dungeonDatabase = DungeonDatabase.Instance;
        dungeonDatabase.InitializeDungeonDatabase();
        monsterDatabase = MonsterDatabase.Instance;
        monsterDatabase.InitializeMonsterDatabase();

        roomManager = new RoomManager();

        Thread handleThread = new Thread(new ThreadStart(DataHandle));
        handleThread.Start();
    }

    public DataHandler(Queue<DataPacket> receiveQueue, Queue<DataPacket> sendQueue, object newReceiveLock, object newSendLock)
    {
        instance = this;

        receiveMsgs = receiveQueue;
        sendMsgs = sendQueue;
        receiveLock = newReceiveLock;
        sendLock = newSendLock;
        loginUser = new Dictionary<Socket, string>();
        userState = new Dictionary<string, UserState>();

        SetNotifier();

        database = AccountDatabase.Instance;
        database.InitailizeDatabase();
        monsterDatabase = MonsterDatabase.Instance;
        monsterDatabase.InitializeMonsterDatabase();
        dungeonDatabase = DungeonDatabase.Instance;
        dungeonDatabase.InitializeDungeonDatabase();
        monsterDatabase = MonsterDatabase.Instance;
        monsterDatabase.InitializeMonsterDatabase();
        characterDatabase = CharacterDatabase.Instance;
        characterDatabase.InitializeCharacterDatabase();
        roomManager = new RoomManager();

        Thread handleThread = new Thread(new ThreadStart(DataHandle));
        handleThread.Start();
        //Thread logoutCheckThread = new Thread(new ThreadStart(CheckLogoutUser));
        //logoutCheckThread.Start();
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
        m_notifier.Add((int)ClientPacketId.ServerConnectionAnswer, ServerConnectionAnswer);
        m_notifier.Add((int)ClientPacketId.CreateAccount, CreateAccount);
        m_notifier.Add((int)ClientPacketId.DeleteAccount, DeleteAccount);
        m_notifier.Add((int)ClientPacketId.Login, Login);
        m_notifier.Add((int)ClientPacketId.Logout, Logout);
        m_notifier.Add((int)ClientPacketId.GameClose, GameClose);
        m_notifier.Add((int)ClientPacketId.RequestCharacterList, RequestCharacterList);
        m_notifier.Add((int)ClientPacketId.CreateCharacter, CreateCharacter);
        m_notifier.Add((int)ClientPacketId.DeleteCharacter, DeleteCharacter);
        m_notifier.Add((int)ClientPacketId.RequestCharacterStatus, RequestCharacterStatus);
        m_notifier.Add((int)ClientPacketId.RequestRoomList, RequestRoomList);
        m_notifier.Add((int)ClientPacketId.ReturnToSelect, ReturnToSelect);
        m_notifier.Add((int)ClientPacketId.CreateRoom, CreateRoom);
        m_notifier.Add((int)ClientPacketId.EnterRoom, EnterRoom);
        m_notifier.Add((int)ClientPacketId.ExitRoom, ExitRoom);
        m_notifier.Add((int)ClientPacketId.RequestRoomUserData, RequestRoomUserData);
        m_notifier.Add((int)ClientPacketId.SwapPlayer, SwapPlayer);
        m_notifier.Add((int)ClientPacketId.StartGame, StartGame);
        m_notifier.Add((int)ClientPacketId.RequestMonsterSpawnList, RequestMonsterSpawnList);
        m_notifier.Add((int)ClientPacketId.RequestMonsterStatusData, RequestMonsterStatusData);
        m_notifier.Add((int)ClientPacketId.RequestUdpConnection, RequestUDPConnection);
        m_notifier.Add((int)ClientPacketId.LoadingComplete, LoadingComplete);
    }

    //연결 체크
    public void ServerConnectionCheck(DataPacket packet)
    {
        ResultData resultData = new ResultData();
        ResultPacket resultPacket = new ResultPacket(resultData);
        resultPacket.SetPacketId((int)ServerPacketId.ServerConnectionCheck);

        byte[] msg = CreatePacket(resultPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //연결 체크 답신
    public void ServerConnectionAnswer(DataPacket packet)
    {
        ConnectionCheck newConnectionCheck = ConnectionChecker.FindClientWithSocket(packet.client);

        if (newConnectionCheck != null)
        {
            newConnectionCheck.IsConnected = true;
        }
    }

    public void CheckLogoutUser()
    {
        while (true)
        {
            if (loginUser.Keys.Count > 0)
            {
                List<Socket> clients = new List<Socket>(loginUser.Keys);

                foreach (Socket client in clients)
                {

                }
            }
        }
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
            if (database.AddAccountData(accountData))
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
            else
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
                    List<string> id = new List<string>(loginUser.Values);

                    
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

                            try
                            {
                                if (userState.ContainsKey(accountData.Id))
                                {
                                    if (userState[accountData.Id].state >= 0)
                                    {
                                        roomManager.ExitRoom(userState[accountData.Id].state, packet.client);
                                    }

                                    userState.Remove(accountData.Id);
                                }
                            }
                            catch
                            {
                                Console.WriteLine("DataHandler::GameClose.ContainsKey - roomManager 에러");
                                Console.WriteLine("방에 입장하지 않았습니다.");
                            }

                            database.DeleteUserData(accountData.Id);

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
        catch(Exception e)
        {
            Console.WriteLine("DataHandler::Login.ContainsValue 에러" + e.Message);
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
        
        Result result = Result.Fail;

        if (RemoveUserData(packet.client))
        {
            result = Result.Success;
        }
        else
        {
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
            Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "접속 종료");
        }
        catch
        {
            Console.WriteLine("DataHandler::GameClose.Socket 에러");
            return;
        }

        if (RemoveUserData(packet.client))
        {
            Console.WriteLine("접속종료 처리 성공");
        }
        else
        {
            Console.WriteLine("접속종료 처리 에러");
        }

        packet.client.Close();
    }

    //접속 종료 처리
    public bool RemoveUserData(Socket client)
    {
        string id = "";

        try
        {
            //로그인 했을 때
            if (loginUser.ContainsKey(client))
            {
                id = loginUser[client];

                //유저 상태 추가
                if (userState.ContainsKey(id))
                {
                    //캐릭터를 선택했을 때
                    if (userState[id].characterId >= 0)
                    {
                        //방에 입장했을 때
                        if (userState[id].state >= 0)
                        {
                            int roomNum = userState[id].state;

                            //게임에 입장했을 때
                            if (roomManager.Room[roomNum].State == (int)RoomState.inGame)
                            {

                            }
                            roomManager.ExitRoom(roomNum, client);

                            //방에서 퇴장 시킴
                            userState[id].state = -1;
                        }

                        //캐릭터 선택 안 한 상태
                        userState[id].characterId = -1;
                    }

                    // 유저 상태 제거
                    userState.Remove(id);
                }

                //유저 데이터 저장 후 리스트에서 삭제
                database.FileSave(id + ".data", database.GetUserData(id));
                database.UserData.Remove(id);

                //로그아웃 처리
                loginUser.Remove(client);
            }

            ConnectionChecker.RemoveClient(client);
            
            return true;
        }
        catch(Exception e)
        {
            Console.WriteLine("DataHandler::RemoveUserData.에러 " + e.Message);
            return false;
        }
    }

    //캐릭터 리스트 요청
    public void RequestCharacterList(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "캐릭터 리스트 요청");

        string id = loginUser[packet.client];
        UserData userData = database.GetUserData(id);

        CharacterList characterList = new CharacterList(userData.HeroData);
        CharacterListPacket characterListPacket = new CharacterListPacket(characterList);
        characterListPacket.SetPacketId((int)ServerPacketId.CharacterList);

        byte[] msg = CreatePacket(characterListPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //캐릭터 생성
    public void CreateCharacter(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "캐릭터 생성");

        CreateCharacterPacket createCharacterPacket = new CreateCharacterPacket(packet.msg);
        CreateCharacterData createCharacterData = createCharacterPacket.GetData();

        Console.WriteLine("이름 : " + createCharacterData.HName);
        Console.WriteLine("직업 : " + createCharacterData.HClass);
        Console.WriteLine("성별 : " + createCharacterData.Gender);

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
            Console.WriteLine("DataHandler::CreateCharacter.CreateHero 에러");
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
            Console.WriteLine("Datahandler::DeleteCharacter.loginUser 에러");
        }

        UserData userData = database.GetUserData(id);
        Result result = Result.Fail;

        try
        {
            userData.DeleteHero(deleteCharacterData.Index);
            result = Result.Success;
        }
        catch
        {
            Console.WriteLine("DataHandler::DeleteCharacter.DeleteHero에러");
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

    //캐릭터 선택
    public void SelectCharacter(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "캐릭터 선택");

        CharacterIndexPacket characterIndexPacket = new CharacterIndexPacket(packet.msg);
        CharacterIndexData characterIndexData = characterIndexPacket.GetData();

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

        try
        {
            userState[id].characterId = characterIndexData.Index;
        }
        catch
        {
            Console.WriteLine("DataHandler::SelectCharacter.userState 에러");
        }
    }

    //캐릭터 정보 요청
    public void RequestCharacterStatus(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "캐릭터 정보 요청");

        CharacterIndexPacket characterIndexPacket = new CharacterIndexPacket(packet.msg);
        CharacterIndexData characterIndexData = characterIndexPacket.GetData();
        
        string id = loginUser[packet.client];
        int character = characterIndexData.Index;

        userState[id].characterId = character;

        HeroData heroData = database.GetHeroData(id, character);
        CharacterStatusData characterStatusData = new CharacterStatusData(heroData);
        CharacterStatusPacket characterStatusPacket = new CharacterStatusPacket(characterStatusData);
        characterStatusPacket.SetPacketId((int)ServerPacketId.CharacterStatus);

        Console.WriteLine(characterStatusData.HClass + ", " + characterStatusData.Gender);

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
        try
        {
            Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "방 목록 요청");
        }
        catch
        {
            Console.WriteLine("DataHandler::RequestRoomList.Socket 에러");
            return;
        }

        RoomListPacket roomListPacket = roomManager.GetRoomList();
        roomListPacket.SetPacketId((int)ServerPacketId.RoomList);

        byte[] msg = CreatePacket(roomListPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //대기방 -> 캐릭터 선택
    public void ReturnToSelect(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "캐릭터 선택창으로 돌아가기");

        string id = loginUser[packet.client];

        Result result = Result.Fail;

        if (userState.ContainsKey(id))
        {
            userState[id].characterId = -1;

            result = Result.Success;
        }
        else
        {
            Console.WriteLine("DataHandler::ReturnToSelect.ContainsKey 에러");

            result = Result.Fail;
        }

        ResultData resultData = new ResultData((byte)result);
        ResultPacket resultPacket = new ResultPacket(resultData);
        resultPacket.SetPacketId((int)ServerPacketId.ReturnToSelectResult);
        
        byte[] msg = CreatePacket(resultPacket);
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

        string id = "";

        try
        {
            id = loginUser[packet.client];
        }
        catch
        {
            Console.WriteLine("현재 로그인 되어있지 않은 아이디 입니다.");
            return;
        }

        int characterId = userState[id].characterId;

        if (characterId == -1)
        {
            Console.WriteLine("캐릭터가 선택되지 않았습니다.");
            return;
        }

        Console.WriteLine("Id : " + id);
        Console.WriteLine("characterId : " + characterId);
        Console.WriteLine("방 제목 : " + createRoomData.RoomName);

        int result = roomManager.CreateRoom(packet.client, database.GetHeroData(id, characterId), createRoomData);

        Console.WriteLine("방 생성 번호 : " + result);

        RoomNumberData resultData = new RoomNumberData(result);
        RoomNumberPacket resultPacket = new RoomNumberPacket(resultData);
        resultPacket.SetPacketId((int)ServerPacketId.CreateRoomNumber);

        byte[] msg = CreatePacket(resultPacket);
        packet = new DataPacket(msg, packet.client);
        
        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }

        List<string> clients = new List<string>(loginUser.Values);

        foreach (string client in clients)
        {
            if (userState.ContainsKey(client))
            {
                if (userState[client].state == -1)
                {
                    packet = new DataPacket(new byte[0], FindSocketWithId(client));

                    RequestRoomList(packet);
                }
            }
        }
    }

    //방 입장
    public void EnterRoom(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "방 입장");
        EnterRoomPacket enterRoomPacket = new EnterRoomPacket(packet.msg);
        EnterRoomData enterRoomData = enterRoomPacket.GetData();

        string id = "";

        try
        {
            id = loginUser[packet.client];
        }
        catch
        {
            Console.WriteLine("현재 로그인 되어있지 않은 아이디 입니다.");
            return;
        }
        
        int characterId = userState[id].characterId;

        if (characterId == -1)
        {
            Console.WriteLine("캐릭터가 선택되지 않았습니다.");
            return;
        }

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

        if(result == -1)
        {
            return;
        }

        RoomNumberData resultData = new RoomNumberData(result);
        RoomNumberPacket resultPacket = new RoomNumberPacket(resultData);
        resultPacket.SetPacketId((int)ServerPacketId.EnterRoomNumber);

        byte[] msg = CreatePacket(resultPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }

        for (int playerIndex = 0; playerIndex < RoomManager.maxPlayerNum; playerIndex++)
        {
            if (roomManager.Room[enterRoomData.RoomNum].Socket[playerIndex] != null && playerIndex != result)
            {
                packet = new DataPacket(new byte[0], roomManager.Room[enterRoomData.RoomNum].Socket[playerIndex]);
                RequestRoomUserData(packet);
            }
        }
    }

    //방 유저 정보 요청
    public void RequestRoomUserData(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "방 유저 정보 요청");

        string id = loginUser[packet.client];
        
        RoomData roomData = new RoomData(roomManager.Room[userState[id].state], userState[id].state);
        RoomDataPacket roomDataPacket = new RoomDataPacket(roomData);
        roomDataPacket.SetPacketId((int)ServerPacketId.RoomData);

        Console.WriteLine(roomData.DungeonName);

        byte[] msg = CreatePacket(roomDataPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //자리 옮기기
    public void SwapPlayer(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "방 자리 옮기기 요청");

        ChangeSlotPacket changeSlotPacket = new ChangeSlotPacket(packet.msg);
        ChangeSlotData changeSlotData = changeSlotPacket.GetData();

        string id = loginUser[packet.client];
        int roomIndex = userState[id].state;
        int myIndex = roomManager.Room[roomIndex].FindPlayerWithSocket(packet.client);

        roomManager.Room[roomIndex].SwapPlayer(myIndex, changeSlotData.Index);
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
        int roomNum = -1;
        bool empty = false;

        Console.WriteLine(userState[id].state);

        try
        {
            empty = roomManager.ExitRoom(userState[id].state, packet.client);
            roomNum = userState[id].state;
            userState[id].state = -1;
            result = Result.Success;
        }
        catch
        {
            Console.WriteLine("DataHandler::ExitRoom.DeletePlayer 에러");
            result = Result.Fail;
            return;
        }

        ResultData resultData = new ResultData((byte)result);
        ResultPacket resultDataPacket = new ResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.ExitRoomNumber);

        byte[] msg = CreatePacket(resultDataPacket);
        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }

        Console.WriteLine(roomNum);

        if (roomNum != -1 && !empty)
        {
            for (int playerIndex = 0; playerIndex < RoomManager.maxPlayerNum; playerIndex++)
            {
                if (roomManager.Room[exitRoomData.RoomNum].Socket[playerIndex] != null)
                {
                    packet = new DataPacket(new byte[0], roomManager.Room[exitRoomData.RoomNum].Socket[playerIndex]);
                    RequestRoomUserData(packet);
                }
            }
        }
    }

    //게임 시작
    public void StartGame(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "게임 시작");

        string id = loginUser[packet.client];
        int roomNum = userState[id].state;

        if (roomNum < 0)
        {
            Console.WriteLine("방에 입장해있지 않습니다.");
            return;
        }

        roomManager.Room[roomNum].GameStart();

        byte result = (byte) Result.Success;

        ResultData resultData = new ResultData(result);
        ResultPacket resultPacket = new ResultPacket(resultData);

        bool setHost = false;

        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            resultPacket.SetPacketId((int)ServerPacketId.StartGame);
            byte[] msg = CreatePacket(resultPacket);

            if (roomManager.Room[roomNum].Socket[i] != null)
            {
                Console.WriteLine(i + "번 유저 : " + roomManager.Room[roomNum].Socket[i].RemoteEndPoint.ToString());
                packet = new DataPacket(msg, roomManager.Room[roomNum].Socket[i]);

                lock (sendLock)
                {
                    sendMsgs.Enqueue(packet);
                }

                if (!setHost)
                {
                    setHost = true;
                    resultPacket.SetPacketId((int)ServerPacketId.SetHost);
                    msg = CreatePacket(resultPacket);

                    packet = new DataPacket(msg, roomManager.Room[roomNum].Socket[i]);

                    lock (sendLock)
                    {
                        sendMsgs.Enqueue(packet);
                    }
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
        int ipIndex = 0;

        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            if (roomManager.Room[roomNum].Socket[i] != null)
            {
                ip[ipIndex] = roomManager.Room[roomNum].Socket[i].RemoteEndPoint.ToString();
                ipIndex++;
            }
        }

        UDPConnectionData udpConnctionData = new UDPConnectionData(ip);
        UDPConnectionPacket udpConnctionDataPacket = new UDPConnectionPacket(udpConnctionData);
        udpConnctionDataPacket.SetPacketId((int)ServerPacketId.UdpConnection);

        byte[] msg = CreatePacket(udpConnctionDataPacket);

        packet = new DataPacket(msg, packet.client);

        Console.WriteLine("UDP연결 정보 송신");

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //몬스터 소환 리스트 요청
    public void RequestMonsterSpawnList(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "몬스터 소환 리스트 요청");

        RequestDungeonDataPacket requestDungeonDataPacket = new RequestDungeonDataPacket(packet.msg);
        RequestDungeonData requestDungeonData = requestDungeonDataPacket.GetData();
        
        DungeonLevelData dungeonLevelData = dungeonDatabase.GetDungeonBaseData(requestDungeonData.DungeonId).GetLevelData(requestDungeonData.DungeonLevel);
        
        MonsterSpawnListPacket monsterSpawnListPacket = new MonsterSpawnListPacket(dungeonLevelData);
        monsterSpawnListPacket.SetPacketId((int)ServerPacketId.MonsterSpawnList);
        
        byte[] msg = CreatePacket(monsterSpawnListPacket);

        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //몬스터 스텟 데이터 요청
    public void RequestMonsterStatusData(DataPacket packet)
    {
        Console.WriteLine(packet.client.RemoteEndPoint.ToString() + "몬스터 스텟 데이터 요청");

        RequestDungeonDataPacket requestDungeonDataPacket = new RequestDungeonDataPacket(packet.msg);
        RequestDungeonData requestDungeonData = requestDungeonDataPacket.GetData();

        DungeonLevelData dungeonLevelData = dungeonDatabase.GetDungeonBaseData(requestDungeonData.DungeonId).GetLevelData(requestDungeonData.DungeonLevel);

        int monsterNum = dungeonLevelData.GetMonsterNum();
        MonsterBaseData[] monsterBaseData = new MonsterBaseData[monsterNum];
        int dataIndex = 0;

        for (int stageIndex = 0; stageIndex < dungeonLevelData.Stages.Count; stageIndex++)
        {
            for (int monsterIndex = 0; monsterIndex < dungeonLevelData.Stages[stageIndex].MonsterSpawnData.Count; monsterIndex++)
            {
                int monsterId = dungeonLevelData.Stages[stageIndex].MonsterSpawnData[monsterIndex].MonsterId;
                int monsterLevel = dungeonLevelData.Stages[stageIndex].MonsterSpawnData[monsterIndex].MonsterLevel;

                monsterBaseData[dataIndex] = new MonsterBaseData(monsterDatabase.GetBaseData(monsterId));
                MonsterLevelData monsterLevelData = new MonsterLevelData(monsterDatabase.GetBaseData(monsterId).GetLevelData(monsterLevel));
                monsterBaseData[dataIndex].MonsterLevelData.Clear();
                monsterBaseData[dataIndex].AddLevelData(monsterLevelData);
                dataIndex++;
            }
        }

        MonsterStatusData monsterStatusData = new MonsterStatusData((byte)monsterNum, monsterBaseData);
        MonsterStatusPacket monsterStatusPacket = new MonsterStatusPacket(monsterStatusData);
        monsterStatusPacket.SetPacketId((int)ServerPacketId.MonsterStatusData);
        
        byte[] msg = CreatePacket(monsterStatusPacket);

        packet = new DataPacket(msg, packet.client);

        lock (sendLock)
        {
            sendMsgs.Enqueue(packet);
        }
    }

    //UDP 연결 완료
    public void LoadingComplete(DataPacket packet)
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

                lock (sendLock)
                {
                    sendMsgs.Enqueue(packet);
                }
            }            
        }        
    }

    Socket FindSocketWithId(string id)
    {
        List<Socket> clients = new List<Socket>(loginUser.Keys);

        foreach (Socket user in clients)
        {
            if (loginUser[user] == id)
            {
                return user;
            }
        }

        return null;
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