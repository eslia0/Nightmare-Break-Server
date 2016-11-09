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

    public int CreateRoom(Socket host, string name, int dungeonId, int dungeonLevel)
    {
        int index = FindEmptyRoom();

        if(index < 0)
        {
            Console.WriteLine("최대 생성 개수를 초과했습니다.");
            return -1;
        }

        room[index] = new Room(host, name, dungeonId, dungeonLevel);

        return index;
    }

    //소켓을 주면, 그에 따른 유저의 정보를 받아와서
    public bool EnterRoom(int roomNum, int classId, string id, int level)
    {
        if(room[roomNum].PlayerNum >= maxPlayerNum)
        {
            Console.WriteLine("입장 가능 인원수를 초과했습니다.");
            return false;
        }

        room[roomNum].AddPlayer(classId, id, level);

        return true;
    }

    public void ExitRoom(int roomNum, int index)
    {
        room[roomNum].DeletePlayer(index);

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
    private Socket host;
    private int playerNum;
    private int[] classId;
    private string[] name;
    private int[] level;

    public int PlayerNum { get { return playerNum; } }

    public Room()
    {
        roomNum = 0;
        roomName = "";
        host = null;
        playerNum = 0;
        dungeonId = 0;
        dungeonLevel = 0;
        classId = new int[RoomManager.maxPlayerNum];
        name = new string[RoomManager.maxPlayerNum];
        level = new int[RoomManager.maxPlayerNum];
    }

    public Room(Socket newHost, string newName, int newDungeonId, int newDungeonLevel)
    {
        roomNum = 0;
        roomName = newName;
        host = newHost;
        playerNum = 0;
        dungeonId = newDungeonId;
        dungeonLevel = newDungeonLevel;
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

    public bool AddPlayer(int newClassId, string newName, int newLevel)
    {
        int index = FindEmptySlot();

        if (index < 0)
        {
            Console.WriteLine("최대 인원수를 초과하였습니다.");
            return false;
        }

        classId[index] = newClassId;
        name[index] = newName;
        level[index] = newLevel;
        playerNum++;

        return true;
    }

    public void DeletePlayer(int index)
    {
        classId[index] = 0;
        name[index] = "";
        level[index] = 0;
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
