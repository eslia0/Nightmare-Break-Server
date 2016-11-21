
public class SelectCharacterPacket : Packet<SelectCharacterData>
{
    public class SelectCharacterSerializer : Serializer
    {
        public bool Serialize(SelectCharacterData data)
        {
            bool ret = true;
            ret &= Serialize(data.Index);
            return ret;
        }

        public bool Deserialize(ref SelectCharacterData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            byte index = 0;

            ret &= Deserialize(ref index);

            element = new SelectCharacterData(index);

            return ret;
        }
    }
    
    public SelectCharacterPacket(SelectCharacterData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public SelectCharacterPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        m_data = new SelectCharacterData();
        SelectCharacterSerializer serializer = new SelectCharacterSerializer();
        serializer.SetDeserializedData(data);
        serializer.Deserialize(ref m_data);
    }

    public override byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        SelectCharacterSerializer serializer = new SelectCharacterSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }
}

public class SelectCharacterData
{
    byte index;

    public byte Index { get { return index; } }

    public SelectCharacterData()
    {
        index = 0;
    }

    public SelectCharacterData(byte newIndex)
    {
        index = newIndex;
    }
}