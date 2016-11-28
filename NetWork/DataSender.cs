using System;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

public class DataSender
{
    Queue<DataPacket> msgs;
    DataPacket tcpPacket;
    Socket client;
    byte[] msg;

    Object sendLock;

    public DataSender(Queue<DataPacket> newQueue, object newSendLock)
    {
        msgs = newQueue;
        sendLock = newSendLock;

        Thread sendThread = new Thread(new ThreadStart(DataSend));
        sendThread.Start();
    }

    public void DataSend()
    {
        while (true)
        {            
            if (msgs.Count != 0)
            {
                Console.WriteLine("메시지전송");

                lock (sendLock)
                {
                    tcpPacket = msgs.Dequeue();
                }

                Console.WriteLine("패킷 길이 : " + tcpPacket.msg.Length);
                Console.WriteLine("패킷 출처 : " + tcpPacket.msg[2]);
                Console.WriteLine("패킷 타입 : " + tcpPacket.msg[3]);

                try
                {
                    client = tcpPacket.client;
                    msg = tcpPacket.msg;

                    client.Send(msg);
                }
                catch
                {
                    Console.WriteLine("DataSender::DataSend 에러");
                }
            }
        }
    }
}