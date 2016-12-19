using System;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

public class DataSender
{
    Queue<DataPacket> msgs;
    DataPacket packet;
    Socket client;
    byte[] msg;

    object sendLock;

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
                lock (sendLock)
                {
                    packet = msgs.Dequeue();
                }

                try
                {
                    client = packet.client;
                    msg = packet.msg;

                    client.Send(msg);
                }
                catch(Exception e)
                {
                    Console.WriteLine("DataSender::DataSend 에러 " + e.Message);
                }
            }
        }
    }
}