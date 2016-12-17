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

        string dungeonName = DungeonDatabase.Instance.GetDungeonData(createRoomData.DungeonId, createRoomData.DungeonLevel).Name;
        Console.WriteLine(dungeonName);
        room[index] = new Room(createRoomData.RoomName, dungeonName, createRoomData.DungeonId, createRoomData.DungeonLevel);

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

    public bool ExitRoom(int roomNum, Socket player)
    {
        room[roomNum].DeletePlayer(player);

        if(room[roomNum].PlayerNum <= 0)
        {
            Console.WriteLine("모든 플레이어가 나갔습니다.");
            room[roomNum] = new Room();

            return true;
        }

        return false;
    }

    public RoomListPacket GetRoomList()
    {
        RoomListData roomListData = new RoomListData(room);
        RoomListPacket roomListPacket = new RoomListPacket(roomListData);

        return roomListPacket;
    }

    public void PrintRoomList()
    {
        for (int i = 0; i < maxRoomNum; i++)
        {
            Console.WriteLine(room[i].RoomName);
            Console.WriteLine(room[i].DungeonId);
            Console.WriteLine(room[i].DungeonLevel);
        }
    }
}

public enum RoomState
{
    empty = 0,
    wait = 1,
    Full = 2,
    inGame = 3,
}

public class Room
{
    string roomName;
    string dungeonName;
    int dungeonId;
    int dungeonLevel;
    int playerNum;
    int state;
    RoomUserData[] roomUserData;
    Socket[] socket;
    bool[] ready;

    public string RoomName { get { return roomName; } }
    public string DungeonName{ get { return dungeonName; } }
    public int DungeonId { get { return dungeonId; } }
    public int DungeonLevel { get { return dungeonLevel; } }
    public int PlayerNum { get { return playerNum; } }
    public int State { get { return state; } }
    public RoomUserData[] RoomUserData { get { return roomUserData; } }
    public Socket[] Socket { get { return socket; } }
    public bool[] Ready { get { return ready; } }

    public Room()
    {
        roomName = "";
        dungeonName = "";
        playerNum = 0;
        dungeonId = 0;
        dungeonLevel = 0;
        state = (int)RoomState.empty;
        roomUserData = new RoomUserData[RoomManager.maxPlayerNum];
        socket = new Socket[RoomManager.maxPlayerNum];        
        ready = new bool[RoomManager.maxPlayerNum];

        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            roomUserData[i] = new RoomUserData();
        }
    }

    public Room(string newRoomName, string newDungeonName, int newDungeonId, int newDungeonLevel)
    {
        roomName = newRoomName;
        dungeonName = newDungeonName;
        dungeonId = newDungeonId;
        dungeonLevel = newDungeonLevel;
        playerNum = 0;
        state = (int)RoomState.empty;
        roomUserData = new RoomUserData[RoomManager.maxPlayerNum];
        socket = new Socket[RoomManager.maxPlayerNum];
        ready = new bool[RoomManager.maxPlayerNum];

        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            roomUserData[i] = new RoomUserData();
        }
    }

    public Room(string newRoomName, int newDungeonId, int newDungeonLevel, RoomUserData[] newRoomUserData, int newPlayerNum)
    {
        roomName = newRoomName;
        dungeonName = "";
        dungeonId = newDungeonId;
        dungeonLevel = newDungeonLevel;
        playerNum = newPlayerNum;
        roomUserData = newRoomUserData;
        socket = new Socket[RoomManager.maxPlayerNum];
        ready = new bool[RoomManager.maxPlayerNum];
    }

    public Room(string newRoomName, string newDungeonName, int newDungeonId, int newDungeonLevel, RoomUserData[] newRoomUserData, int newPlayerNum)
    {
        roomName = newRoomName;
        dungeonName = newDungeonName;
        dungeonId = newDungeonId;
        dungeonLevel = newDungeonLevel;
        playerNum = newPlayerNum;
        roomUserData = newRoomUserData;
        socket = new Socket[RoomManager.maxPlayerNum];
        ready = new bool[RoomManager.maxPlayerNum];
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

        roomUserData[index] = new RoomUserData(newData.Name, newData.Gender, newData.HClass, newData.Level);
        socket[index] = newPlayer;
        playerNum++;

        if (playerNum < RoomManager.maxPlayerNum)
        {
            state = (int)RoomState.wait;
        }
        else if (playerNum == RoomManager.maxPlayerNum)
        {
            state = (int)RoomState.Full;
        }

        Console.WriteLine(index + "번 슬롯에 유저 입장");
        Console.WriteLine("유저 수 : " + playerNum);

        return index;
    }

    public bool DeletePlayer(Socket player)
    {
        int index = FindPlayerWithSocket(player);

        if(index == -1)
        {
            return false;
        }

        roomUserData[index] = new RoomUserData();
        socket[index] = null;
        playerNum--;

        if(playerNum == 0)
        {
            state = (int)RoomState.empty;
        }

        return true;
    }

    public void SwapPlayer(int origSlot, int DestiSlot)
    {
        RoomUserData tempData = roomUserData[origSlot];
        roomUserData[origSlot] = roomUserData[DestiSlot];
        roomUserData[DestiSlot] = tempData;
    }

    public void GameStart()
    {
        state = (int)RoomState.inGame;
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