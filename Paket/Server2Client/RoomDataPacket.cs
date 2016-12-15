using System.Text;

public class RoomDataPacket : Packet<RoomData>
{
    public class RoomDataSerializer : Serializer
    {
        public bool Serialize(RoomData data)
        {
            bool ret = true;

            for (int i = 0; i < RoomManager.maxPlayerNum; i++)
            {
                ret &= Serialize((byte)Encoding.Unicode.GetBytes(data.RoomUserData[i].UserName).Length);
                ret &= Serialize(data.RoomUserData[i].UserName);
                ret &= Serialize((byte)data.RoomUserData[i].UserGender);
                ret &= Serialize((byte)data.RoomUserData[i].UserClass);
                ret &= Serialize((byte)data.RoomUserData[i].UserLevel);
            }

            return ret;
        }

        public bool Deserialize(ref RoomData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            byte[] userNameLength = new byte[RoomManager.maxPlayerNum];
            string[] userName = new string[RoomManager.maxPlayerNum];
            byte[] userGender = new byte[RoomManager.maxPlayerNum];
            byte[] userClass = new byte[RoomManager.maxPlayerNum];
            byte[] userLevel = new byte[RoomManager.maxPlayerNum];

            for (int i = 0; i < RoomManager.maxPlayerNum; i++)
            {
                ret &= Deserialize(ref userNameLength[i]);
                ret &= Deserialize(out userName[i], userNameLength[i]);
                ret &= Deserialize(ref userGender[i]);
                ret &= Deserialize(ref userClass[i]);
                ret &= Deserialize(ref userLevel[i]);
            }

            element = new RoomData(userName, userGender, userClass, userLevel);

            return ret;
        }
    }

    public RoomDataPacket(RoomData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public RoomDataPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        m_data = new RoomData();
        RoomDataSerializer serializer = new RoomDataSerializer();
        serializer.SetDeserializedData(data);
        serializer.Deserialize(ref m_data);
    }

    public override byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        RoomDataSerializer serializer = new RoomDataSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }
}

public class RoomData
{
    RoomUserData[] roomUserData;

    public RoomUserData[] RoomUserData { get { return roomUserData; } }

    public RoomData()
    {
        roomUserData = new RoomUserData[RoomManager.maxPlayerNum];

        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            roomUserData[i] = new RoomUserData();
        }
    }

    public RoomData(string[] newUserName, byte[] newUserGender, byte[] newUserClass, byte[] newUserLevel)
    {
        roomUserData = new RoomUserData[RoomManager.maxPlayerNum];

        for (int i = 0; i < RoomManager.maxPlayerNum; i++)
        {
            roomUserData[i] = new RoomUserData(newUserName[i], newUserGender[i], newUserClass[i], newUserLevel[i]);
        }
    }

    public RoomData(Room room)
    {
        roomUserData = room.RoomUserData; 
    }
}