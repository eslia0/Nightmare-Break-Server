using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

public class DataReceiver
{
	public Socket listenSock;
	Queue<DataPacket> msgs;

	object receiveLock;

    AsyncCallback asyncAcceptCallback;
    AsyncCallback asyncReceiveLengthCallBack;
	AsyncCallback asyncReceiveDataCallBack;

    //초기화
    //소켓 초기화 및 콜백 메소드 설정, BeginAccept
    public DataReceiver(Queue<DataPacket> newQueue, IPAddress newAddress, int newPort, object newLock)
	{
		listenSock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		listenSock.Bind (new IPEndPoint (newAddress, newPort));
		listenSock.Listen (10);

		msgs = newQueue;
		receiveLock = newLock;

        asyncAcceptCallback = new AsyncCallback (HandleAsyncAccept);
		asyncReceiveLengthCallBack = new AsyncCallback (HandleAsyncReceiveLength);
		asyncReceiveDataCallBack = new AsyncCallback (HandleAsyncReceiveData);

        listenSock.BeginAccept (asyncAcceptCallback, (Object)listenSock);
	}

    //Accept 콜백 메소드
    //Begin ReceiveLength 메소드
    public void HandleAsyncAccept(IAsyncResult asyncResult)
    {
        Socket listenSock = (Socket)asyncResult.AsyncState;
        Socket clientSock;

        try
        {
            clientSock = listenSock.EndAccept(asyncResult);
        }
        catch
        {
            clientSock = null;
            Console.WriteLine("DataReceiver::HandleAsyncAccept.EndAccept 에러");
        }

        if (clientSock != null)
        {
            Console.WriteLine(clientSock.RemoteEndPoint.ToString() + " 접속");

            AsyncData asyncData = new AsyncData(clientSock);
            clientSock.BeginReceive(asyncData.msg, 0, UnityServer.packetLength, SocketFlags.None, asyncReceiveLengthCallBack, (Object)asyncData);
        }

        listenSock.BeginAccept(asyncAcceptCallback, (Object)listenSock);
    }

    //ReceiveLength 콜백 메소드
    //패킷 헤더의 데이타길이의 길이인 2byte만큼 데이타를 받는다
    public void HandleAsyncReceiveLength(IAsyncResult asyncResult)
    {
        AsyncData asyncData = (AsyncData)asyncResult.AsyncState;
        Socket clientSock = asyncData.clientSock;

        try
        {
            asyncData.msgSize = (short)clientSock.EndReceive(asyncResult);
        }
        catch
        {
            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine("DataReceiver::HandleAsyncReceiveLength.EndReceive 에러");
            clientSock.Close();
            return;
        }

        if (asyncData.msgSize <= 0)
        {
            Console.WriteLine(asyncData.clientSock.RemoteEndPoint.ToString() + " 접속 종료");
            clientSock.Close();
            return;
        }

        //데이터를 받았을 때
        if (asyncData.msgSize >= UnityServer.packetLength)
        {
            short msgSize = 0;

            //성공적으로 데이타를 받았을 때 - ReceiveData 로 넘어간다.
            try
            {
                msgSize = BitConverter.ToInt16(asyncData.msg, 0);
                asyncData = new AsyncData(clientSock);
                clientSock.BeginReceive(asyncData.msg, 0, msgSize + UnityServer.packetSource + UnityServer.packetId, SocketFlags.None, asyncReceiveDataCallBack, (Object)asyncData);
            }
            //데이타 받기를 실패하거나 변환에 실패했을 때 - ReceiveLength를 재실행
            catch
            {
                Console.WriteLine("DataReceiver::HandleAsyncReceiveLength.BitConverter 에러");
                asyncData = new AsyncData(clientSock);
                clientSock.BeginReceive(asyncData.msg, 0, UnityServer.packetLength, SocketFlags.None, asyncReceiveLengthCallBack, (Object)asyncData);
            }
        }
        else
        {
            asyncData = new AsyncData(clientSock);
            clientSock.BeginReceive(asyncData.msg, 0, UnityServer.packetLength, SocketFlags.None, asyncReceiveLengthCallBack, (Object)asyncData);
        }
    }

    //ReceiveData 콜백 메소드
    //패킷 헤더의 데이터타입의 길이인 1byte + 데이타길이 만큼 데이타를 받는다.
	public void HandleAsyncReceiveData(IAsyncResult asyncResult)
	{
		AsyncData asyncData = (AsyncData) asyncResult.AsyncState;
		Socket clientSock = asyncData.clientSock;

		try
		{
			asyncData.msgSize = (short) clientSock.EndReceive (asyncResult);
		}
		catch
		{
            Console.WriteLine("DataReceiver::HandleAsyncReceiveData.EndReceive 에러");
            return;
		}

        if (asyncData.msgSize <= 0)
        {
            Console.WriteLine(clientSock.RemoteEndPoint.ToString() + "접속 종료.");
            clientSock.Close();
            return;
        }

        Console.WriteLine("받은 메시지 길이 : " + asyncData.msgSize);

        if (asyncData.msgSize >= UnityServer.packetId + UnityServer.packetSource)
		{
			Array.Resize (ref asyncData.msg, asyncData.msgSize);
            DataPacket packet = new DataPacket (asyncData.msg, clientSock);

            lock (receiveLock)
            {
                //패킷을 큐에 넣는다. 패킷 : 메시지 타입 + 메시지 내용, 소켓
                try
                {
                    msgs.Enqueue (packet);
				}
				catch
				{
					Console.WriteLine ("DataReceiver::Enqueue 에러");
				}
			}
		}

		asyncData = new AsyncData(clientSock);
		clientSock.BeginReceive (asyncData.msg, 0, UnityServer.packetLength, SocketFlags.None, asyncReceiveLengthCallBack, (Object)asyncData);
	}
}


public class AsyncData
{
	public Socket clientSock;
	public byte[] msg;
	public short msgSize;
	public const int msgMaxLength = 1024;

	public AsyncData (Socket newClient)
	{
		msg = new byte[msgMaxLength];
		clientSock = newClient;
	}
}

public class DataPacket
{
	public byte[] msg;
	public Socket client;

    public DataPacket()
    {
        msg = new byte[1024];
        client = null;
    }

	public DataPacket(byte[] newMsg, Socket newclient)
	{
		msg = newMsg;
		client = newclient;
	}
}