public class MonsterSpawnListPacket : Packet<MonsterSpawnList>
{
    public class MonsterSpawnListSerializer : Serializer
    {
        public bool Serialize(MonsterSpawnList data)
        {
            bool ret = true;

            ret &= Serialize(data.MonsterKind);

            for (int i = 0; i < data.MonsterKind; i++)
            {
                ret &= Serialize(data.MonsterSpawnData[i].MonsterId);
                ret &= Serialize(data.MonsterSpawnData[i].MonsterNum);
            }

            return ret;
        }

        public bool Deserialize(ref MonsterSpawnList element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            byte monsterKind = 0;
            byte monsterNum = 0;
            byte monsterId = 0;

            ret &= Deserialize(ref monsterKind);
            element = new MonsterSpawnList(monsterKind);

            for (int i = 0; i < monsterKind; i++)
            {
                ret &= Deserialize(ref monsterId);
                ret &= Deserialize(ref monsterNum);

                element.MonsterSpawnData[i] = new MonsterSpawnData(monsterId, monsterNum);
            }

            return ret;
        }
    }

    public MonsterSpawnListPacket(MonsterSpawnList data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public MonsterSpawnListPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        m_data = new MonsterSpawnList();
        MonsterSpawnListSerializer serializer = new MonsterSpawnListSerializer();
        serializer.SetDeserializedData(data);
        serializer.Deserialize(ref m_data);
    }

    public override byte[] GetPacketData() // 바이트형 패킷(송신용)
    {
        MonsterSpawnListSerializer serializer = new MonsterSpawnListSerializer();
        serializer.Serialize(m_data);
        return serializer.GetSerializedData();
    }
}

public class MonsterSpawnList
{
    byte monsterKind;
    MonsterSpawnData[] monsterSpawnData;

    public int MonsterNum
    {
        get
        {
            int sum = 0;

            for (int i =0; i<monsterKind;i++)
            {
                sum += monsterSpawnData[i].MonsterNum;
            }            

            return sum;
        }
    }
    public byte MonsterKind { get { return monsterKind; } }
    public MonsterSpawnData[] MonsterSpawnData { get { return monsterSpawnData; } }

    public MonsterSpawnList()
    {
        monsterKind = 0;
        monsterSpawnData = new MonsterSpawnData[monsterKind];
    }

    public MonsterSpawnList(byte newMonsterKind)
    {
        monsterKind = newMonsterKind;
        monsterSpawnData = new MonsterSpawnData[monsterKind];
    }

    public MonsterSpawnList(byte newMonsterKind, MonsterSpawnData[] newMonsterSpawnData)
    {
        monsterKind = newMonsterKind;
        monsterSpawnData = newMonsterSpawnData;
    }
}

public class MonsterSpawnData
{
    byte monsterId;
    byte monsterNum;

    public byte MonsterId { get { return monsterId; } }
    public byte MonsterNum { get { return monsterNum; } }

    public MonsterSpawnData()
    {
        monsterId = 0;
        monsterNum = 0;
    }

    public MonsterSpawnData(byte newMonsterId, byte newMonsterNum)
    {
        monsterId = newMonsterId;
        monsterNum = newMonsterNum;
    }
}