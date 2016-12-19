public class MonsterSpawnListPacket : Packet<DungeonLevelData>
{
    public class MonsterSpawnListSerializer : Serializer
    {
        public bool Serialize(DungeonLevelData data)
        {
            bool ret = true;
            
            //총 스테이지 개수
            ret &= Serialize((byte)data.Stages.Count);
            
            for (int stageIndex = 0; stageIndex < data.Stages.Count; stageIndex++)
            {
                //이 스테이지의 몬스터 개수
                ret &= Serialize((byte)data.Stages[stageIndex].MonsterSpawnData.Count);

                for (int monsterIndex = 0; monsterIndex < data.Stages[stageIndex].MonsterSpawnData.Count; monsterIndex++)
                {
                    ret &= Serialize(data.Stages[stageIndex].MonsterSpawnData[monsterIndex].MonsterId);
                    ret &= Serialize(data.Stages[stageIndex].MonsterSpawnData[monsterIndex].MonsterLevel);
                    ret &= Serialize(data.Stages[stageIndex].MonsterSpawnData[monsterIndex].MonsterNum);
                }
            }

            return ret;
        }

        public bool Deserialize(ref DungeonLevelData element)
        {
            if (GetDataSize() == 0)
            {
                // 데이터가 설정되지 않았다.
                return false;
            }

            bool ret = true;
            byte stageNum = 0;
            byte monsterKind = 0;
            byte monsterId = 0;
            byte monsterLevel = 0;
            byte monsterNum = 0;

            ret &= Deserialize(ref stageNum);

            for (int stageIndex = 0; stageIndex < stageNum; stageIndex++)
            {
                ret &= Deserialize(ref monsterKind);

                for(int monsterIndex = 0; monsterIndex < monsterKind; monsterIndex++)
                {
                    ret &= Deserialize(ref monsterId);
                    ret &= Deserialize(ref monsterLevel);
                    ret &= Deserialize(ref monsterNum);
                }

                element.AddStage(new Stage(stageIndex));
                element.GetStage(stageIndex).AddMonster(monsterId, monsterLevel, monsterNum);
            }

            return ret;
        }
    }

    public MonsterSpawnListPacket(DungeonLevelData data) // 데이터로 초기화(송신용)
    {
        m_data = data;
    }

    public MonsterSpawnListPacket(byte[] data) // 패킷을 데이터로 변환(수신용)
    {
        m_data = new DungeonLevelData();
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