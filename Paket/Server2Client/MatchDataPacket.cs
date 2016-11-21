
public class MatchDataPacket : Packet<MatchData>
{
    public class MatchDataSerializer : Serializer
    {
        public bool Serialize(MatchData data)
        {
            bool ret = true;
            ret &= Serialize(data.playerNum);

            for (int i = 0; i < data.playerNum; i++)
            {
                ret &= Serialize(data.ip[i]);
                ret &= Serialize(',');
            }

            return ret;
        }

        public bool Deserialize(ref MatchData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            byte playerNum = 0;
            string total;

            ret &= Deserialize(ref playerNum);
            ret &= Deserialize(out total, (int)GetDataSize());

            string[] str = total.Split(',');

            if (str.Length < playerNum)
            {
                return false;
            }

            element.playerNum = playerNum;

            for (int i = 0; i < playerNum; i++)
            {
                element.ip[i] = str[i];
            }

            return ret;
        }
    }

    public MatchDataPacket(MatchData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public MatchDataPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        m_data = new MatchData();
        MatchDataSerializer serializer = new MatchDataSerializer();
        serializer.SetDeserializedData(data);
        serializer.Deserialize(ref m_data);
    }

    public override byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        MatchDataSerializer serializer = new MatchDataSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }
}

public class MatchData
{
    public byte playerNum;
    public string[] ip;

    public MatchData()
    {
        playerNum = 4;
        ip = new string[playerNum];
    }

    public MatchData(string[] newIp)
    {
        playerNum = (byte)newIp.Length;
        ip = newIp;
    }
}