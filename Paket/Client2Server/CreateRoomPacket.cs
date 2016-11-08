
public class CreateRoomPacket : IPacket<CreateRoomData>
{
    public class CreateRoomSerializer : Serializer
    {
        public bool Serialize(CreateRoomData data)
        {
            bool ret = true;
            ret &= Serialize(data.dungeonId);
            ret &= Serialize(data.dungeonLevel);
            ret &= Serialize(data.roomName);

            return ret;
        }

        public bool Deserialize(ref CreateRoomData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            byte dungeonId = 0;
            byte dungeonLevel = 0;
            string total;

            ret &= Deserialize(ref dungeonId);
            ret &= Deserialize(ref dungeonLevel);
            ret &= Deserialize(out total, (int)GetDataSize());

            element.dungeonId = dungeonId;
            element.dungeonLevel = dungeonLevel;
            element.roomName = total;
            
            return ret;
        }
    }

    CreateRoomData m_data;

    public CreateRoomPacket(CreateRoomData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public CreateRoomPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        m_data = new CreateRoomData();
        CreateRoomSerializer serializer = new CreateRoomSerializer();
        serializer.SetDeserializedData(data);
        serializer.Deserialize(ref m_data);
    }

    public byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        CreateRoomSerializer serializer = new CreateRoomSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }

    public CreateRoomData GetData() // 데이터 얻기(수신용)
    {
        return m_data;
    }

    public int GetPacketId()
    {
        return (int)ClientPacketId.Create;
    }
}

public class CreateRoomData
{
    public byte dungeonId;
    public byte dungeonLevel;
    public string roomName;

    public CreateRoomData()
    {
        roomName = "";
        dungeonId = 0;
        dungeonLevel = 0;
    }

    public CreateRoomData(string newRoomName, int newId, int newLevel)
    {
        roomName = "";
        dungeonId = (byte)newId;
        dungeonLevel = (byte)newLevel;
    }
}