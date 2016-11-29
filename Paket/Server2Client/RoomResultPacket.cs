
public class RoomResultPacket : Packet<RoomResultData>
{
    public class RoomResultSerializer : Serializer
    {
        public bool Serialize(RoomResultData data)
        {
            bool ret = true;
            ret &= Serialize(data.RoomNum);

            return ret;
        }

        public bool Deserialize(ref RoomResultData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            byte roomNum = 0;

            ret &= Deserialize(ref roomNum);

            element = new RoomResultData(roomNum);

            return ret;
        }
    }
    
    public RoomResultPacket(RoomResultData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public RoomResultPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        m_data = new RoomResultData();
        RoomResultSerializer serializer = new RoomResultSerializer();
        serializer.SetDeserializedData(data);
        serializer.Deserialize(ref m_data);
    }

    public override byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        RoomResultSerializer serializer = new RoomResultSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }
}

public class RoomResultData
{
    byte roomNum;

    public byte RoomNum { get { return roomNum; } }

    public RoomResultData()
    {
        roomNum = 0;
    }

    public RoomResultData(int newRoomNum)
    {
        roomNum = (byte)newRoomNum;
    }
}