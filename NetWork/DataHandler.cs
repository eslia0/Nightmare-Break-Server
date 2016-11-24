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

    public Queue<TcpPacket> receiveMsgs;
    public Queue<TcpPacket> sendMsgs;

    public Dictionary<Socket, string> loginUser;
    public Dictionary<string, int> loginCharacter;

    AccountDatabase database;
    RoomManager roomManager;

    object receiveLock;
    object sendLock;

    TcpPacket tcpPacket;

    byte[] msg = new byte[1024];

    RecvNotifier recvNotifier;
    public delegate ServerPacketId RecvNotifier(byte[] data);
    private Dictionary<int, RecvNotifier> m_notifier = new Dictionary<int, RecvNotifier>();

    public DataHandler(Queue<TcpPacket> receiveQueue, Queue<TcpPacket> sendQueue, object newReceiveLock, object newSendLock)
    {
        receiveMsgs = receiveQueue;
        sendMsgs = sendQueue;
        receiveLock = newReceiveLock;
        sendLock = newSendLock;
        loginUser = new Dictionary<Socket, string>();
        loginCharacter = new Dictionary<string, int>();

        SetNotifier();

        database = AccountDatabase.Instance;
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
                //패킷을 Dequeue 한다 패킷 : 메시지 타입 + 메시지 내용, 소켓
                lock (receiveLock)
                {
                    try
                    {
                        tcpPacket = receiveMsgs.Dequeue();
                    }
                    catch
                    {
                        Console.WriteLine("DataHandler::DataHandle.Dequeue 에러");
                    }
                }

                HeaderData headerData = new HeaderData();
                HeaderSerializer headerSerializer = new HeaderSerializer();
                headerSerializer.SetDeserializedData(tcpPacket.msg);
                headerSerializer.Deserialize(ref headerData);

                ResizeByteArray(0, UnityServer.packetSource + UnityServer.packetId, ref tcpPacket.msg);

                //Dictionary에 등록된 델리게이트 메소드에서 PacketId를 반환받는다.
                if (m_notifier.TryGetValue(headerData.id, out recvNotifier))
                {
                    ServerPacketId packetId = recvNotifier(tcpPacket.msg);

                    //send 할 id를 반환받음
                    if (packetId != ServerPacketId.None)
                    {
                        tcpPacket = new TcpPacket(msg, tcpPacket.client);
                        sendMsgs.Enqueue(tcpPacket);
                    }
                }
                else
                {
                    Console.WriteLine("DataHandler::DataHandle.TryGetValue 에러 ");
                    Console.WriteLine("패킷 출처 : " + headerData.source);
                    Console.WriteLine("패킷 ID : " + headerData.id);
                    headerData.id = (byte)ServerPacketId.None;
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
    }

    public ServerPacketId CreateAccount(byte[] data)
    {
        Console.WriteLine(tcpPacket.client.RemoteEndPoint.ToString() + " 가입요청");

        AccountPacket accountPacket = new AccountPacket(data);
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
        msg = CreatePacket(resultDataPacket);

        return ServerPacketId.CreateAccountResult;
    }

    public ServerPacketId DeleteAccount(byte[] data)
    {
        Console.WriteLine(tcpPacket.client.RemoteEndPoint.ToString() + " 탈퇴요청");

        AccountPacket accountPacket = new AccountPacket(data);
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
        msg = CreatePacket(resultDataPacket);

        return ServerPacketId.DeleteAccountResult;
    }

    public ServerPacketId Login(byte[] data)
    {
        Console.WriteLine(tcpPacket.client.RemoteEndPoint.ToString() + " 로그인");

        AccountPacket accountPacket = new AccountPacket(data);
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
        msg = CreatePacket(resultDataPacket);

        return ServerPacketId.LoginResult;
    }

    /*
    public ServerPacketId ReLogin(byte[] data)
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

    public ServerPacketId Logout(byte[] data)
    {
        Console.WriteLine(tcpPacket.client.RemoteEndPoint.ToString() + " 로그아웃요청");

        string id = loginUser[tcpPacket.client];

        Result result = Result.Fail;

        try
        {
            if (loginUser.ContainsValue(id))
            {
                loginUser.Remove(tcpPacket.client);
                Console.WriteLine(id + "로그아웃");
                result = Result.Success;
            }
            else
            {
                Console.WriteLine("로그인되어있지 않은 아이디입니다. : " + id);
                result = Result.Fail;
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::Logout.ContainsValue 에러");
            result = Result.Fail;
        }

        ResultData resultData = new ResultData((byte)result);
        ResultPacket resultDataPacket = new ResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.LogoutResult);
        msg = CreatePacket(resultDataPacket);

        return ServerPacketId.None;
    }

    //게임 종료
    public ServerPacketId GameClose(byte[] data)
    {
        try
        {
            Console.WriteLine(tcpPacket.client.RemoteEndPoint.ToString() + "가 접속을 종료했습니다.");
            tcpPacket.client.Close();

            if (loginUser.ContainsKey(tcpPacket.client))
            {
                string id = loginUser[tcpPacket.client];
                database.FileSave(id + ".data", database.GetUserData(id));
                database.UserData.Remove(id);

                loginUser.Remove(tcpPacket.client);
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::LoginUser.Close 에러");
        }

        return ServerPacketId.None;
    }

    //캐릭터 생성
    public ServerPacketId CreateCharacter(byte[] data)
    {
        Console.WriteLine("캐릭터 생성");
        CreateCharacterPacket createCharacterPacket = new CreateCharacterPacket(data);
        CreateCharacterData createCharacterData = createCharacterPacket.GetData();

        string id = loginUser[tcpPacket.client];
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
        msg = CreatePacket(resultDataPacket);

        return ServerPacketId.CreateCharacterResult;
    }

    //캐릭터 삭제
    public ServerPacketId DeleteCharacter(byte[] data)
    {
        Console.WriteLine("캐릭터 삭제");
        DeleteCharacterPacket deleteCharacterPacket = new DeleteCharacterPacket(data);
        DeleteCharacterData deleteCharacterData = deleteCharacterPacket.GetData();

        string id = loginUser[tcpPacket.client];
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
        msg = CreatePacket(resultDataPacket);

        return ServerPacketId.DeleteChracterResult;
    }
    
    //캐릭터 선택 후 게임 시작
    public ServerPacketId SelectCharacter(byte[] data)
    {
        Console.WriteLine("캐릭터 선택");
        SelectCharacterPacket selectCharacterPacket = new SelectCharacterPacket(data);
        SelectCharacterData selectCharacterData = selectCharacterPacket.GetData();

        string id = loginUser[tcpPacket.client];

        Result result = Result.Fail;

        try
        {
            loginCharacter.Add(id, selectCharacterData.Index);
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
        msg = CreatePacket(resultDataPacket);

        return ServerPacketId.SelectCharacterResult;
    }

    //방 목록 요청
    public ServerPacketId RequestRoomList(byte[] data)
    {
        Console.WriteLine("방 목록 요청");

        RoomListPacket roomListPacket = roomManager.GetRoomList();
        msg = CreatePacket(roomListPacket);

        return ServerPacketId.RoomList;
    }

    //방 생성
    public ServerPacketId CreateRoom(byte[] data)
    {
        Console.WriteLine("방 생성");
        CreateRoomPacket createRoomPacket = new CreateRoomPacket(data);
        CreateRoomData createRoomData = createRoomPacket.GetData();

        string id = loginUser[tcpPacket.client];
        int characterId = loginCharacter[id];

        Result result = Result.Fail;

        if (roomManager.CreateRoom(tcpPacket.client, database.GetHeroData(id, characterId), createRoomData) > 0)
        {
            result = Result.Success;
        }
        else
        {
            result = Result.Fail;
        }

        ResultData resultData = new ResultData((byte)result);
        ResultPacket resultDataPacket = new ResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.CreateRoomResult);
        msg = CreatePacket(resultDataPacket);

        return ServerPacketId.CreateRoomResult;
    }

    //방 입장
    public ServerPacketId EnterRoom(byte[] data)
    {
        Console.WriteLine("방 입장");
        EnterRoomPacket enterRoomPacket = new EnterRoomPacket(data);
        EnterRoomData enterRoomData = enterRoomPacket.GetData();

        string id = loginUser[tcpPacket.client];
        int characterId = loginCharacter[id];

        Result result = Result.Fail;

        try
        {
            if(roomManager.Room[enterRoomData.RoomNum].AddPlayer(tcpPacket.client, database.GetHeroData(id, characterId)))
            {
                result = Result.Success;
            }
            else
            {
                result = Result.Fail;
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::EnterRoom.AddPlayer 에러");
            result = Result.Fail;
        }

        ResultData resultData = new ResultData((byte)result);
        ResultPacket resultDataPacket = new ResultPacket(resultData);
        resultDataPacket.SetPacketId((int)ServerPacketId.EnterRoomResult);
        msg = CreatePacket(resultDataPacket);

        return ServerPacketId.EnterRoomResult;
    }

    public ServerPacketId ExitRoom(byte[] data)
    {
        Console.WriteLine("방 퇴장");
        ExitRoomPacket exitRoomPacket = new ExitRoomPacket(data);
        ExitRoomData exitRoomData = exitRoomPacket.GetData();

        string id = loginUser[tcpPacket.client];
        int characterId = loginCharacter[id];

        Result result = Result.Fail;

        try
        {
            if (roomManager.Room[exitRoomData.RoomNum].DeletePlayer(tcpPacket.client))
            {
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
        msg = CreatePacket(resultDataPacket);

        return ServerPacketId.ExitRoomResult;
    }

    //유저 매칭
    public void MatchUser(byte[] data)
    {
        Console.WriteLine("유저 매칭");

        int roomNum = data[0];
        string[] ip = new string[roomManager.Room[roomNum].PlayerNum];

        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            ip[i] = roomManager.Room[roomNum].Socket[i].RemoteEndPoint.ToString();
        }

        MatchData matchData = new MatchData(ip);
        MatchDataPacket matchDataPacket = new MatchDataPacket(matchData);
        msg = CreatePacket(matchDataPacket);

        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            if (roomManager.Room[roomNum].Socket[i] != null)
            {
                TcpPacket packet = new TcpPacket(msg, roomManager.Room[roomNum].Socket[i]);
                Console.WriteLine(packet.msg.Length);
                sendMsgs.Enqueue(packet);
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

[Serializable]
public class TcpClient
{
	public Socket client;
	public string id;

	public TcpClient (Socket newClient)
	{
		client = newClient;
		id = "";
	}
}

public class HeaderData
{
    // 헤더 == [2바이트 - 패킷길이][1바이트 - 출처][1바이트 - ID]
    public short length; // 패킷의 길이
    public byte source; // 패킷의 출처
    public byte id; // 패킷 ID
}