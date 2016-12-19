using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

public class ConnectionChecker
{
    public static List<ConnectionCheck> clients;
    static Socket client;

    public ConnectionChecker()
    {
        clients = new List<ConnectionCheck>();
    }

    public static void AddClient(Socket newClient)
    {
        try
        {
            ConnectionCheck newCheck = new ConnectionCheck(newClient);
            clients.Add(newCheck);

            client = newClient;
            Thread connectionCheckThread = new Thread(new ThreadStart(ConnectionChecking));
            connectionCheckThread.Start();
        }
        catch(Exception e)
        {
            Console.WriteLine("체크 에러 " + e.Message);
        }
    }

    public static bool RemoveClient(Socket client)
    {
        ConnectionCheck newCheck = FindClientWithSocket(client);

        if (clients.Remove(newCheck))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static ConnectionCheck FindClientWithSocket(Socket client)
    {
        for (int clientIndex = 0; clientIndex < clients.Count; clientIndex++)
        {
            if (clients[clientIndex].Client == client)
            {
                return clients[clientIndex];
            }
        }

        return null;
    }

    public static void ConnectionChecking()
    {
        Socket check = client;

        Console.WriteLine("체크 시작 : " + check.RemoteEndPoint.ToString());

        while (check != null)
        {
            try
            {
                DataPacket packet;

                if (check != null)
                {
                    packet = new DataPacket(new byte[0], check);
                    DataHandler.Instance.ServerConnectionCheck(packet);
                    FindClientWithSocket(check).IsConnected = false;
                }
                else
                {
                    break;
                }

                Thread.Sleep(3000);

                if (check == null)
                {
                    if (!FindClientWithSocket(check).IsConnected)
                    {
                        packet = new DataPacket(new byte[0], FindClientWithSocket(check).Client);
                        DataHandler.Instance.GameClose(packet);
                        clients.Remove(FindClientWithSocket(check));
                    }
                }
                else
                {
                    break;
                }
            }
            catch
            {
                break;
            }
        }

        Console.WriteLine("체크 종료");
    }
}

public class ConnectionCheck
{
    Socket client;
    bool isConnected;

    public Socket Client { get { return client; } }
    public bool IsConnected { get { return isConnected; } set { isConnected = value; } }

    public ConnectionCheck()
    {
        client = null;
        isConnected = false;
    }

    public ConnectionCheck(Socket newClient)
    {
        client = newClient;
        isConnected = false;
    }
}