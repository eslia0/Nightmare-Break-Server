using System;
using System.Net.Sockets;

public class RoomManager
{
    public const int maxPlayerNum = 4;
    public const int maxRoomNum = 20;

    Room[] room;

    public Room[] Room { get { return room; } }

    //방 생성 및 번호 부여
    public RoomManager()
    {
        room = new Room[maxRoomNum];

        for (int i = 0; i < maxRoomNum; i++)
        {
            room[i] = new Room();
            room[i].roomNum = i + 1;
        }
    }

    public int FindEmptyRoom()
    {
        for (int i = 0; i < maxRoomNum; i++)
        {
            if (room[i].PlayerNum == 0)
            {
                return i;
            }
        }

        return -1;
    }

    public int CreateRoom(Socket player, HeroData heroData, CreateRoomData createRoomData)
    {
        int index = FindEmptyRoom();

        if(index < 0)
        {
            Console.WriteLine("최대 생성 개수를 초과했습니다.");
            return -1;
        }

        room[index] = new Room(createRoomData.RoomName, createRoomData.DungeonId, createRoomData.DungeonLevel);
        room[index].AddPlayer(player, heroData);

        return index;
    }

    //소켓을 주면, 그에 따른 유저의 정보를 받아와서
    public bool EnterRoom(Socket socket, HeroData heroData, int roomNum)
    {
        if(room[roomNum].PlayerNum >= maxPlayerNum)
        {
            Console.WriteLine("입장 가능 인원수를 초과했습니다.");
            return false;
        }

        room[roomNum].AddPlayer(socket, heroData);

        return true;
    }

    public void ExitRoom(int roomNum, Socket player)
    {
        room[roomNum].DeletePlayer(player);

        if(room[roomNum].PlayerNum <= 0)
        {
            room[roomNum] = new Room();
        }
    }
}

public class Room
{
    public int roomNum;
    private string roomName;
    private int dungeonId;
    private int dungeonLevel;
    private Socket[] socket;
    private int playerNum;
    private int[] classId;
    private string[] name;
    private int[] level;

    public Socket[] Socket { get { return socket; } }
    public int PlayerNum { get { return playerNum; } }

    public Room()
    {
        roomNum = 0;
        roomName = "";
        playerNum = 0;
        dungeonId = 0;
        dungeonLevel = 0;
        socket = new Socket[RoomManager.maxPlayerNum];
        classId = new int[RoomManager.maxPlayerNum];
        name = new string[RoomManager.maxPlayerNum];
        level = new int[RoomManager.maxPlayerNum];
    }

    public Room(string newName, int newDungeonId, int newDungeonLevel)
    {
        roomNum = 0;
        roomName = newName;
        playerNum = 0;
        dungeonId = newDungeonId;
        dungeonLevel = newDungeonLevel;
        socket = new Socket[RoomManager.maxPlayerNum];
        classId = new int[RoomManager.maxPlayerNum];
        name = new string[RoomManager.maxPlayerNum];
        level = new int[RoomManager.maxPlayerNum];
    }

    public int FindEmptySlot()
    {
        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            if(level[i] == 0)
            {
                return i;
            }
        }

        return -1;
    }

    public int FindPlayerWithSocket(Socket player)
    {
        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            if (socket[i] == player)
            {
                return i;
            }
        }

        return -1;
    }

    public bool AddPlayer(Socket newPlayer, HeroData newData)
    {
        int index = FindEmptySlot();

        if (index < 0)
        {
            Console.WriteLine("최대 인원수를 초과하였습니다.");
            return false;
        }

        classId[index] = newData.HClass;
        name[index] = newData.Name;
        level[index] = newData.Level;
        socket[index] = newPlayer;
        playerNum++;

        return true;
    }

    public void DeletePlayer(Socket player)
    {
        int index = FindPlayerWithSocket(player);

        if(index == -1)
        {
            return;
        }

        classId[index] = 0;
        name[index] = "";
        level[index] = 0;
        socket[index] = null;
        playerNum--;
    }

    public void SwapPlayer(int origSlot, int DestiSlot)
    {
        int tempInt = classId[origSlot];
        classId[origSlot] = DestiSlot;
        DestiSlot = tempInt;

        tempInt = level[origSlot];
        level[origSlot] = level[DestiSlot];
        level[DestiSlot] = tempInt;

        string tempString = name[origSlot];
        name[origSlot] = name[DestiSlot];
        name[DestiSlot] = tempString;
    }
}
