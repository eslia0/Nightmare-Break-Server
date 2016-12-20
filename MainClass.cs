using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

public class UnityServer
{
    public const short packetId = 1;
    public const short packetSource = 1;
    public const short packetLength = 2;

    public static void Main(string[] args)
    {
        Queue<DataPacket> receiveData = new Queue<DataPacket>();
        Queue<DataPacket> sendData = new Queue<DataPacket>();

        object receiveLock = new object();
        object sendLock = new object();

        string myIP = "";

        for (int addressIndex = 0; addressIndex < Dns.GetHostAddresses(Dns.GetHostName()).Length; addressIndex++)
        {
            string ip = Dns.GetHostAddresses(Dns.GetHostName())[addressIndex].ToString();

            if (ip.Length >= 7 || ip.Length <= 15)
            {
                myIP = Dns.GetHostAddresses(Dns.GetHostName())[addressIndex].ToString();
            }
        }

        Console.WriteLine(myIP);

        DataReceiver dataReceiver = new DataReceiver(receiveData, IPAddress.Parse(myIP), 8800, receiveLock);
        DataHandler dataHandler = new DataHandler(receiveData, sendData, receiveLock, sendLock);
        DataSender dataSender = new DataSender(sendData, sendLock);
        ConnectionChecker ConnectionChecker = new ConnectionChecker();

        while (true)
        {
            if (Console.KeyAvailable)
            {
                string key = Console.ReadLine();

                if (key == "p" || key == "P")
                {

                }
            }
        }
    }
}