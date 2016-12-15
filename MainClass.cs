using System.Net;
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

        DataReceiver dataReceiver = new DataReceiver(receiveData, IPAddress.Parse("192.168.94.88"), 8800, receiveLock);
        DataHandler dataHandler = new DataHandler(receiveData, sendData, receiveLock, sendLock);
        DataSender dataSender = new DataSender(sendData, sendLock);

        while (true)
        {
            Thread.Sleep(1000);
            if(System.Console.KeyAvailable)
            {
                string key = System.Console.ReadLine();

                if(key == "p" || key == "P")
                {
                    dataHandler.RoomManager.PrintRoomList();
                }
            }
        }
    }
}
