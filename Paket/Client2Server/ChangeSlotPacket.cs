
public class ChangeSlotPacket : Packet<ChangeSlotData>
{
    public class ChangeSlotSerializer : Serializer
    {
        public bool Serialize(ChangeSlotData data)
        {
            bool ret = true;
            ret &= Serialize(data.Index);
            return ret;
        }

        public bool Deserialize(ref ChangeSlotData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            byte index = 0;

            ret &= Deserialize(ref index);

            element = new ChangeSlotData(index);

            return ret;
        }
    }

    public ChangeSlotPacket(ChangeSlotData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public ChangeSlotPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        m_data = new ChangeSlotData();
        ChangeSlotSerializer serializer = new ChangeSlotSerializer();
        serializer.SetDeserializedData(data);
        serializer.Deserialize(ref m_data);
    }

    public override byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        ChangeSlotSerializer serializer = new ChangeSlotSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }
}

public class ChangeSlotData
{
    byte index;

    public byte Index { get { return index; } }

    public ChangeSlotData()
    {
        index = 0;
    }

    public ChangeSlotData(byte newIndex)
    {
        index = newIndex;
    }
}