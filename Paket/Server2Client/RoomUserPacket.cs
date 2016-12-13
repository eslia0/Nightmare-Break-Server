using System.Text;

public class RoomUserPacket : Packet<RoomUserData>
{
    public class RoomUserSerializer : Serializer
    {
        public bool Serialize(RoomUserData data)
        {
            bool ret = true;

            for (int i = 0; i < RoomManager.maxPlayerNum; i++)
            {
                ret &= Serialize((byte)Encoding.Unicode.GetBytes(data.UserName[i]).Length);
                ret &= Serialize(data.UserName[i]);
                ret &= Serialize(data.UserClass[i]);
                ret &= Serialize(data.UserLevel[i]);
            }

            return ret;
        }

        public bool Deserialize(ref RoomUserData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            byte[] userNameLength = new byte[RoomManager.maxPlayerNum];
            string[] userName = new string[RoomManager.maxPlayerNum];
            byte[] userClass = new byte[RoomManager.maxPlayerNum];
            byte[] userLevel = new byte[RoomManager.maxPlayerNum];

            for (int i = 0; i < RoomManager.maxPlayerNum; i++)
            {
                ret &= Deserialize(ref userNameLength[i]);
                ret &= Deserialize(out userName[i], userNameLength[i]);
                ret &= Deserialize(ref userClass[i]);
                ret &= Deserialize(ref userLevel[i]);
            }

            element = new RoomUserData(userName, userClass, userLevel);

            return ret;
        }
    }

    public RoomUserPacket(RoomUserData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public RoomUserPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        m_data = new RoomUserData();
        RoomUserSerializer serializer = new RoomUserSerializer();
        serializer.SetDeserializedData(data);
        serializer.Deserialize(ref m_data);
    }

    public override byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        RoomUserSerializer serializer = new RoomUserSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }
}

public class RoomUserData
{
    string[] userName;
    byte[] userClass;
    byte[] userLevel;

    public string[] UserName { get { return userName; } }
    public byte[] UserClass { get { return userClass; } }
    public byte[] UserLevel { get { return userLevel; } }

    public RoomUserData()
    {
        userName = new string[RoomManager.maxPlayerNum];
        userClass = new byte[RoomManager.maxPlayerNum];
        userLevel = new byte[RoomManager.maxPlayerNum];
    }

    public RoomUserData(string[] newUserName, byte[] newUserClass, byte[] newUserLevel)
    {
        userName = newUserName;
        userClass = newUserClass;
        userLevel = newUserLevel;
    }

    public RoomUserData(Room room)
    {
        for(int i =0; i< RoomManager.maxPlayerNum; i++)
        {
            userName[i] = room.UserName[i];
            userClass[i] = (byte)room.UserClass[i];
            userLevel[i] = (byte)room.UserLevel[i];
        }        
    }
}