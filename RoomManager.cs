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
            Console.WriteLine("모든 플레이어가 나갔습니다.");
            room[roomNum] = new Room();
        }
    }

    public RoomListPacket GetRoomList()
    {
        RoomListData roomListData = new RoomListData(room);
        RoomListPacket roomListPacket = new RoomListPacket(roomListData);

        return roomListPacket;
    }
}

public class Room
{
    string roomName;
    int dungeonId;
    int dungeonLevel;
    int playerNum;
    RoomUserData[] roomUserData;
    Socket[] socket;
    bool[] ready;

    public int PlayerNum { get { return playerNum; } }
    public string RoomName { get { return roomName; } }
    public int DungeonId { get { return dungeonId; } }
    public int DungeonLevel { get { return dungeonLevel; } }
    public RoomUserData[] RoomUserData { get { return roomUserData; } }
    public Socket[] Socket { get { return socket; } }
    public bool[] Ready { get { return ready; } }

    public Room()
    {
        roomName = "";
        playerNum = 0;
        dungeonId = 0;
        dungeonLevel = 0;
        roomUserData = new RoomUserData[RoomManager.maxPlayerNum];
        socket = new Socket[RoomManager.maxPlayerNum];        
        ready = new bool[RoomManager.maxPlayerNum];

        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            roomUserData[i] = new RoomUserData();
        }
    }

    public Room(string newName, int newDungeonId, int newDungeonLevel)
    {
        roomName = newName;
        playerNum = 0;
        dungeonId = newDungeonId;
        dungeonLevel = newDungeonLevel;
        roomUserData = new RoomUserData[RoomManager.maxPlayerNum];
        socket = new Socket[RoomManager.maxPlayerNum];
        ready = new bool[RoomManager.maxPlayerNum];

        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            roomUserData[i] = new RoomUserData();
        }
    }

    public Room(string newName, byte newDungeonId, byte newDungeonLevel, RoomUserData[] newRoomUserData)
    {
        roomName = newName;
        playerNum = 0;
        dungeonId = newDungeonId;
        dungeonLevel = newDungeonLevel;
        ready = new bool[RoomManager.maxPlayerNum];
        roomUserData = newRoomUserData;
    }

    public int FindEmptySlot()
    {
        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            if(roomUserData[i].UserLevel == 0)
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

    public int AddPlayer(Socket newPlayer, HeroData newData)
    {
        int index = FindEmptySlot();

        if (index < 0)
        {
            Console.WriteLine("최대 인원수를 초과하였습니다.");
            return -1;
        }

        roomUserData[index] = new RoomUserData(newData.Name, newData.Gender, newData.Level);
        .userClass = newData.HClass;
        roomUserData[index].userName = newData.Name;
        userLevel[index] = newData.Level;
        socket[index] = newPlayer;
        playerNum++;

        Console.WriteLine(index + "번 슬롯에 유저 입장");

        return index;
    }

    public bool DeletePlayer(Socket player)
    {
        int index = FindPlayerWithSocket(player);

        if(index == -1)
        {
            return false;
        }

        userClass[index] = 0;
        userName[index] = "";
        userLevel[index] = 0;
        socket[index] = null;
        playerNum--;

        return true;
    }

    public void SwapPlayer(int origSlot, int DestiSlot)
    {
        int tempInt = userClass[origSlot];
        userClass[origSlot] = DestiSlot;
        DestiSlot = tempInt;

        tempInt = userLevel[origSlot];
        userLevel[origSlot] = userLevel[DestiSlot];
        userLevel[DestiSlot] = tempInt;

        string tempString = userName[origSlot];
        userName[origSlot] = userName[DestiSlot];
        userName[DestiSlot] = tempString;
    }
}

public class RoomUserData
{
    string userName;
    int userGender;
    int userClass;
    int userLevel;

    public string UserName { get { return userName; } }
    public int UserGender { get { return userGender; } }
    public int UserClass { get { return userClass; } }
    public int UserLevel { get { return userLevel; } }

    public RoomUserData()
    {
        userName = "";
        userGender = 0;
        userClass = 0;
        userLevel = 0;
    }

    public RoomUserData(string newUserName, int newUserGender, int newUserClass, int newUserLevel)
    {
        userName = newUserName;
        userGender = newUserGender;
        userClass = newUserClass;
        userLevel = newUserLevel;
    }
}