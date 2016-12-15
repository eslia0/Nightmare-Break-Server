public class MonsterSpawnListPacket : Packet<DungeonData>
{
    public class MonsterSpawnListSerializer : Serializer
    {
        public bool Serialize(DungeonData data)
        {
            bool ret = true;

            ret &= Serialize(data.StageNum);

            for (int i = 0; i < data.StageNum; i++)
            {
                for (int j = 0; j < data.StageData[i].MonsterSpawnData.Count; j++)
                {
                    ret &= Serialize(data.StageData[i].MonsterSpawnData[j].MonsterId);
                    ret &= Serialize(data.StageData[i].MonsterSpawnData[j].MonsterNum);
                }
            }

            return ret;
        }

        public bool Deserialize(ref DungeonData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            byte stageNum = 0;
            byte monsterNum = 0;
            byte monsterId = 0;

            ret &= Deserialize(ref stageNum);

            for (int i = 0; i < stageNum; i++)
            {
                ret &= Deserialize(ref monsterId);
                ret &= Deserialize(ref monsterNum);

                element.StageData[i] = new Stage(i);
                element.StageData[i].AddMonster(monsterId, monsterNum);
            }

            return ret;
        }
    }

    public MonsterSpawnListPacket(DungeonData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public MonsterSpawnListPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        m_data = new DungeonData();
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