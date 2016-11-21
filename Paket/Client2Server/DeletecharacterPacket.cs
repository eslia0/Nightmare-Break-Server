
public class DeleteCharacterPacket : Packet<DeleteCharacterData>
{
    public class DeleteCharacterSerializer : Serializer
    {
        public bool Serialize(DeleteCharacterData data)
        {
            bool ret = true;
            ret &= Serialize(data.Index);
            return ret;
        }

        public bool Deserialize(ref DeleteCharacterData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            byte index = 0;

            ret &= Deserialize(ref index);

            element = new DeleteCharacterData(index);

            return ret;
        }
    }

    public DeleteCharacterPacket(DeleteCharacterData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public DeleteCharacterPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        m_data = new DeleteCharacterData();
        DeleteCharacterSerializer serializer = new DeleteCharacterSerializer();
        serializer.SetDeserializedData(data);
        serializer.Deserialize(ref m_data);
    }

    public override byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        DeleteCharacterSerializer serializer = new DeleteCharacterSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }
}

public class DeleteCharacterData
{
    byte index;

    public byte Index { get { return index; } }

    public DeleteCharacterData()
    {
        index = 0;
    }

    public DeleteCharacterData(byte newIndex)
    {
        index = newIndex;
    }
}