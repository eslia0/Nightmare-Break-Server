using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

public class DataHandler
{
    public enum Result
    {
        CreateSuccess = 1,
        CreateFail,
        DeleteSuccess,
        DeleteFailByWrongId,
        DeleteFailByWrongPw,
        LoginSuccess,
        LoginFailByWrongId,
        LoginFailByWrongPw,
        LoginFailByAlreadyLogin,
        LogoutSuccess,
        LogoutFail,
    };

    public enum Source
    {
        ServerSource = 0,
        ClientSource,
    }

    public const int matchUserNum = 2;

    public const byte tcpSource = 0;
    public const byte udpSource = 1;

    public Queue<TcpPacket> receiveMsgs;
    public Queue<TcpPacket> sendMsgs;

    public Dictionary<Socket, string> LoginUser;

    AccountDatabase database;

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
        LoginUser = new Dictionary<Socket, string>();

        SetNotifier();

        Thread handleThread = new Thread(new ThreadStart(DataHandle));
        handleThread.Start();
    }

    public void DataHandle()
    {
        while (true)
        {
            if (receiveMsgs.Count != 0)
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

                //출처 분리
                byte source = tcpPacket.msg[0];

                //타입 분리
                //byte[] Id = ResizeByteArray(1, UnityServer.packetId, ref msg);
                byte Id = tcpPacket.msg[1];

                HeaderData headerData = new HeaderData();

                //Dictionary에 등록된 델리게이트 메소드에서 PacketId를 반환받는다.
                if (m_notifier.TryGetValue(Id, out recvNotifier))
                {
                    ServerPacketId packetId = recvNotifier(msg);
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
                    Console.WriteLine("패킷 출처 : " + source);
                    Console.WriteLine("패킷 ID : " + Id);
                    headerData.Id = (byte)ServerPacketId.None;
                }
            }
        }
    }

    public void SetNotifier()
    {
        m_notifier.Add((int)ClientPacketId.GameClose, GameClose);
    }

    public ServerPacketId CreateAccount(byte[] data)
    {
        Console.WriteLine(tcpPacket.client.RemoteEndPoint.ToString() + " 가입요청");

        AccountPacket accountPacket = new AccountPacket(data);
        AccountPacketData accountData = accountPacket.GetData();

        Console.WriteLine("아이디 : " + accountData.Id + "패스워드 : " + accountData.password);

        try
        {
            if (database.AddAccountData(accountData.Id, accountData.password))
            {
                msg[0] = (byte)Result.CreateSuccess;
                Console.WriteLine("가입 성공");
            }
            else
            {
                msg[0] = (byte)Result.CreateFail;
                Console.WriteLine("가입 실패");
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::AddPlayerData 에러");
            Console.WriteLine("가입 실패");
            msg[0] = (byte)Result.CreateFail;
        }

        Array.Resize(ref msg, 1);
        msg = CreateResultPacket(msg, ServerPacketId.CreateResult);

        return ServerPacketId.CreateResult;
    }

    public ServerPacketId DeleteAccount(byte[] data)
    {
        Console.WriteLine(tcpPacket.client.RemoteEndPoint.ToString() + " 탈퇴요청");

        AccountPacket accountPacket = new AccountPacket(data);
        AccountPacketData accountData = accountPacket.GetData();

        Console.WriteLine("아이디 : " + accountData.Id + "패스워드 : " + accountData.Id);

        try
        {
            if (database.DeleteAccountData(accountData.Id, accountData.password) == Result.DeleteSuccess)
            {
                msg[0] = (byte)Result.DeleteSuccess;
                Console.WriteLine("탈퇴 성공");
            }
            else if(database.DeleteAccountData(accountData.Id, accountData.password) == Result.DeleteFailByWrongId)
            {
                msg[0] = (byte)Result.DeleteFailByWrongId;
                Console.WriteLine("탈퇴 실패 - 아이디");
            }
            else if (database.DeleteAccountData(accountData.Id, accountData.password) == Result.DeleteFailByWrongPw)
            {
                msg[0] = (byte)Result.DeleteFailByWrongPw;
                Console.WriteLine("탈퇴 실패 - 비밀번호");
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::RemovePlayerData 에러");
            Console.WriteLine("탈퇴 실패");
            msg[0] = (byte)Result.DeleteFailByWrongId;
        }

        Array.Resize(ref msg, 1);
        msg = CreateResultPacket(msg, ServerPacketId.DeleteResult);

        return ServerPacketId.DeleteResult;
    }

    public ServerPacketId Login(byte[] data)
    {
        Console.WriteLine(tcpPacket.client.RemoteEndPoint.ToString() + " 로그인");

        AccountPacket accountPacket = new AccountPacket(data);
        AccountPacketData accountData = accountPacket.GetData();

        Console.WriteLine("아이디 : " + accountData.Id + "비밀번호 : " + accountData.password);

        try
        {
            if (database.AccountData.Contains(accountData.Id))
            {
                if (((AccountData)database.AccountData[accountData.Id]).PW == accountData.password)
                {
                    if (!LoginUser.ContainsValue(accountData.Id))
                    {
                        msg[0] = (byte)Result.LoginSuccess;
                        Console.WriteLine("로그인 성공");
                        LoginUser.Add(tcpPacket.client, accountData.Id);
                    }
                    else
                    {
                        Console.WriteLine("현재 접속중인 아이디입니다.");

                        if (CompareIP(GetSocket(accountData.Id).RemoteEndPoint.ToString(), tcpPacket.client.RemoteEndPoint.ToString()))
                        {
                            LoginUser.Remove(GetSocket(accountData.Id));
                            Console.WriteLine("현재 접속중 해제");
                        }                        
                        msg[0] = (byte)Result.LoginFailByAlreadyLogin;
                    }
                }
                else
                {
                    Console.WriteLine("패스워드가 맞지 않습니다.");
                    msg[0] = (byte)Result.LoginFailByWrongPw;
                }
            }
            else
            {
                Console.WriteLine("존재하지 않는 아이디입니다.");
                msg[0] = (byte)Result.LoginFailByWrongId;
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::Login.ContainsValue 에러");
            msg[0] = (byte)Result.LoginFailByWrongId;
        }

        Array.Resize(ref msg, 1);

        msg = CreateResultPacket(msg, ServerPacketId.LoginResult);

        return ServerPacketId.LoginResult;
    }

    public ServerPacketId ReLogin(byte[] data)
    {
        Console.WriteLine(tcpPacket.client.RemoteEndPoint.ToString() + " 재로그인요청");

        AccountPacket accountPacket = new AccountPacket(data);
        AccountPacketData accountData = accountPacket.GetData();

        Console.WriteLine("아이디 : " + accountData.Id);

        try
        {
            if (database.AccountData.Contains(accountData.Id))
            {
                if (!LoginUser.ContainsValue(accountData.Id))
                {
                    msg[0] = (byte)Result.LoginSuccess;
                    Console.WriteLine("로그인 성공");
                    LoginUser.Add(tcpPacket.client, accountData.Id);
                }
                else
                {
                    Console.WriteLine("현재 접속중인 아이디입니다.");

                    if (CompareIP(GetSocket(accountData.Id).RemoteEndPoint.ToString(), tcpPacket.client.RemoteEndPoint.ToString()))
                    {
                        LoginUser.Remove(GetSocket(accountData.Id));
                        Console.WriteLine("현재 접속중 해제");
                    }
                    msg[0] = (byte)Result.LoginFailByAlreadyLogin;
                }
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::ReLogin.ContainsValue 에러");
            msg[0] = (byte)Result.LoginFailByWrongId;
        }

        return ServerPacketId.LoginResult;
    }

    public ServerPacketId Logout(byte[] data)
    {
        Console.WriteLine(tcpPacket.client.RemoteEndPoint.ToString() + " 로그아웃요청");

        string id = LoginUser[tcpPacket.client];

        msg = new byte[1];

        try
        {
            if (LoginUser.ContainsValue(id))
            {
                LoginUser.Remove(tcpPacket.client);
                Console.WriteLine(id + "로그아웃");
                msg[0] = (byte)Result.LogoutSuccess;
            }
            else
            {
                Console.WriteLine("로그인되어있지 않은 아이디입니다. : " + id);
                msg[0] = (byte)Result.LogoutFail;
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::Logout.ContainsValue 에러");
            msg[0] = (byte)Result.LogoutFail;
        }

        Array.Resize(ref msg, 1);

        msg = CreateResultPacket(msg, ServerPacketId.LoginResult);

        return ServerPacketId.None;
    }

    public ServerPacketId GameClose(byte[] data)
    {
        Console.WriteLine("게임종료");

        try
        {
            Console.WriteLine(tcpPacket.client.RemoteEndPoint.ToString() + "가 접속을 종료했습니다.");

            if (LoginUser.ContainsKey(tcpPacket.client))
            {
                string Id = LoginUser[tcpPacket.client];
                database.FileSave(Id + ".data", database.GetAccountData(Id));
                database.UserData.Remove(Id);

                LoginUser.Remove(tcpPacket.client);
            }
        }
        catch
        {
            Console.WriteLine("DataHandler::LoginUser.Close 에러");
        }

        return ServerPacketId.None;
    }

    public void MatchUser()
    {
        Console.WriteLine("유저 매칭");
        string[] ip = new string[matchUserNum];

        for (int i = 0; i < matchUserNum; i++)
        {
            ip[i] = DataReceiver.clients[i].RemoteEndPoint.ToString();
        }

        MatchData matchData = new MatchData(ip);
        MatchDataPacket matchDataPacket = new MatchDataPacket(matchData);
        msg = CreatePacket(matchDataPacket, ServerPacketId.Match);

        for (int i = 0; i < matchUserNum; i++)
        {
            if (DataReceiver.clients[i] != null)
            {
                TcpPacket packet = new TcpPacket(msg, DataReceiver.clients[i]);
                Console.WriteLine(packet.msg.Length);
                sendMsgs.Enqueue(packet);
            }

            DataReceiver.clients[i] = null;
        }

        DataReceiver.userNum = 0;
    }

    public void CheckUser()
    {
        for (int i = 0; i < 2; i++)
        {
            if (DataReceiver.clients[i] == null)
            {
                return;
            }
        }

        MatchUser();
    }

    byte[] CreateHeader<T>(IPacket<T> data, ServerPacketId Id)
    {
        byte[] msg = data.GetPacketData();

        HeaderData headerData = new HeaderData();
        HeaderSerializer headerSerializer = new HeaderSerializer();

        headerData.Id = (byte)Id;
        headerData.source = (byte)Source.ServerSource;
        headerData.length = (short)msg.Length;

        headerSerializer.Serialize(headerData);
        byte[] header = headerSerializer.GetSerializedData();

        return header;
    }

    byte[] CreatePacket<T>(IPacket<T> data, ServerPacketId Id)
    {
        byte[] msg = data.GetPacketData();
        byte[] header = CreateHeader(data, Id);
        byte[] packet = CombineByte(header, msg);

        return packet;
    }

    byte[] CreateResultPacket(byte[] msg, ServerPacketId Id)
    {
        HeaderData headerData = new HeaderData();
        HeaderSerializer HeaderSerializer = new HeaderSerializer();

        headerData.Id = (byte)Id;
        headerData.source = (byte)Source.ServerSource;
        headerData.length = (short)msg.Length;
        HeaderSerializer.Serialize(headerData);
        msg = CombineByte(HeaderSerializer.GetSerializedData(), msg);
        return msg;
    }

    bool CompareIP(string ip1, string ip2)
    {
        if(ip1.Substring(0, ip1.IndexOf(":")) == ip2.Substring(0, ip2.IndexOf(":")))
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
        foreach (KeyValuePair<Socket, string> client in LoginUser)
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

    public static byte[] CombineByte (byte[] array1, byte[] array2)
	{
		byte[] array3 = new byte[array1.Length + array2.Length];
		Array.Copy (array1, 0, array3, 0, array1.Length);
		Array.Copy (array2, 0, array3, array1.Length, array2.Length);
		return array3;
	}

	public static byte[] CombineByte (byte[] array1, byte[] array2, byte[] array3)
	{
		byte[] array4 = CombineByte (CombineByte (array1, array2), array3);;
		return array4;
	}
}

[Serializable]
public class TcpClient
{
	public Socket client;
	public string Id;

	public TcpClient (Socket newClient)
	{
		client = newClient;
		Id = "";
	}
}

public class HeaderData
{
    // 헤더 == [2바이트 - 패킷길이][1바이트 - 출처][1바이트 - ID]
    public short length; // 패킷의 길이
    public byte source; // 패킷의 출처
    public byte Id; // 패킷 ID
}