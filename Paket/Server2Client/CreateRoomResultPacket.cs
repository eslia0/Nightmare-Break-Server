
public class CreateRoomResultPacket : Packet<CreateRoomResultData>
{
    public class CreateRoomResultSerializer : Serializer
    {
        public bool Serialize(CreateRoomResultData data)
        {
            bool ret = true;
            ret &= Serialize(data.RoomNum);

            return ret;
        }

        public bool Deserialize(ref CreateRoomResultData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            byte roomNum = 0;

            ret &= Deserialize(ref roomNum);

            element = new CreateRoomResultData(roomNum);

            return ret;
        }
    }
    
    public CreateRoomResultPacket(CreateRoomResultData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public CreateRoomResultPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        m_data = new CreateRoomResultData();
        CreateRoomResultSerializer serializer = new CreateRoomResultSerializer();
        serializer.SetDeserializedData(data);
        serializer.Deserialize(ref m_data);
    }

    public override byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        CreateRoomResultSerializer serializer = new CreateRoomResultSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }
}

public class CreateRoomResultData
{
    byte roomNum;

    public byte RoomNum { get { return roomNum; } }

    public CreateRoomResultData()
    {
        roomNum = 0;
    }

    public CreateRoomResultData(int newRoomNum)
    {
        roomNum = (byte)newRoomNum;
    }
}