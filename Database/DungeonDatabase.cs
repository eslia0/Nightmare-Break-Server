using System;
using System.Collections.Generic;

public enum DungeonId
{
    LostTeddyBear,
}

public class DungeonDatabase
{
    private static DungeonDatabase instance;

    public static DungeonDatabase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new DungeonDatabase();
            }

            return instance;
        }
    }

    List<DungeonBaseData> dungeonList;

    public void InitializeDungeonDatabase()
    {
        instance = this;
        dungeonList = new List<DungeonBaseData>();
        
        AddDungeonData(new DungeonBaseData((int)DungeonId.LostTeddyBear, "잃어버린 곰"));

        for (int i = 1; i < 11; i++)
        {
            #region 잃어버린 곰 던전 1 ~ 10레벨

            DungeonLevelData dungeonLevelData = new DungeonLevelData(i);

            Stage missingBearStage0 = new Stage(0);
            Stage missingBearStage1 = new Stage(1);
            Stage missingBearStage2 = new Stage(2);
            Stage missingBearStage3 = new Stage(3);

            missingBearStage0.AddMonster((int)MonsterId.BlackBear, 1, 1);
            //missingBearStage0.AddMonster((int)MonsterId.Duck, i, 4);
            //missingBearStage0.AddMonster((int)MonsterId.Rabbit, i, 1);

            missingBearStage1.AddMonster((int)MonsterId.Frog, i, 4);
            missingBearStage1.AddMonster((int)MonsterId.Duck, i, 4);
            missingBearStage1.AddMonster((int)MonsterId.Bear, i, 1);

            missingBearStage2.AddMonster((int)MonsterId.Frog, i, 3);
            missingBearStage2.AddMonster((int)MonsterId.Duck, i, 3);
            missingBearStage2.AddMonster((int)MonsterId.Rabbit, i, 3);

            missingBearStage3.AddMonster((int)MonsterId.Rabbit, i, 5);
            missingBearStage3.AddMonster((int)MonsterId.Duck, i, 4);
            missingBearStage3.AddMonster((int)MonsterId.BlackBear, i, 1);

            dungeonLevelData.AddStage(missingBearStage0);
            dungeonLevelData.AddStage(missingBearStage1);
            dungeonLevelData.AddStage(missingBearStage2);
            dungeonLevelData.AddStage(missingBearStage3);

            dungeonList[(int)DungeonId.LostTeddyBear].AddLevelData(dungeonLevelData);
            #endregion
        }
    }

    public bool AddDungeonData(DungeonBaseData newDungeonBaseData)
    {
        try
        {
            dungeonList.Add(newDungeonBaseData);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("DungeonDatabase::AddBaseData.Add 에러 " + e.Message);
            return false;
        }
    }

    public DungeonBaseData GetDungeonBaseData(int id)
    {
        for (int i = 0; i < dungeonList.Count; i++)
        {
            if (dungeonList[i].Id == id)
            {
                return dungeonList[i];
            }
        }

        return null;
    }
}

public class DungeonBaseData
{
    int id;
    string name;
    List<DungeonLevelData> dungeonLevelData;

    public int Id { get { return id; } }
    public string Name { get { return name; } }
    public List<DungeonLevelData> DungeonLevelData { get { return dungeonLevelData; } }

    public DungeonBaseData()
    {
        id = 0;
        name = "";
        dungeonLevelData = new List<DungeonLevelData>();
    }

    public DungeonBaseData(int _id, string _name)
    {
        id = _id;
        name = _name;
        dungeonLevelData = new List<DungeonLevelData>();
    }

    public DungeonLevelData GetLevelData(int level)
    {
        for (int index = 0; index < dungeonLevelData.Count; index++)
        {
            if (dungeonLevelData[index].Level== level)
            {
                return dungeonLevelData[index];
            }
        }

        return null;
    }

    public bool AddLevelData(DungeonLevelData newDungeonLevelData)
    {
        try
        {
            dungeonLevelData.Add(newDungeonLevelData);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool RemoveLevelData(int index)
    {
        try
        {
            dungeonLevelData.Remove(GetLevelData(index));
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class DungeonLevelData
{
    int level;
    List<Stage> stages;

    public int Level { get { return level; } }
    public List<Stage> Stages { get { return stages; } }

    public DungeonLevelData()
    {
        level = 0;
        stages = new List<Stage>();
    }

    public DungeonLevelData(int newLevel)
    {
        level = newLevel;
        stages = new List<Stage>();
    }

    public Stage GetStage(int index)
    {
        for(int i=0; i< stages.Count; i++)
        {
            if (stages[i].StageNum == index)
            {
                return stages[i];
            }
        }

        return null;
    }

    public bool AddStage(Stage newStage)
    {
        try
        {
            stages.Add(newStage);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool RemoveLevelData(int index)
    {
        try
        {
            stages.Remove(GetStage(index));
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public int GetMonsterNum()
    {
        int count = 0;

        for (int i = 0; i < stages.Count; i++)
        {
            count += stages[i].MonsterSpawnData.Count;
        }

        return count;
    }
}

public class Stage
{
    int stageNum;
    List<MonsterSpawnData> monsterSpawnData;

    public int StageNum { get { return stageNum; } }
    public List<MonsterSpawnData> MonsterSpawnData { get { return monsterSpawnData; } }

    public Stage()
    {
        stageNum = 0;
        monsterSpawnData = new List<MonsterSpawnData>();
    }

    public Stage(int newStageNum)
    {
        stageNum = newStageNum;
        monsterSpawnData = new List<MonsterSpawnData>();
    }

    public void AddMonster(int monsterId, int monsterLevel, int monsterNum)
    {
        try
        {
            monsterSpawnData.Add(new MonsterSpawnData((byte)monsterId, (byte)monsterLevel,(byte)monsterNum));            
        }
        catch
        {
            Console.WriteLine("DungeonDatabase::Stage.AddStage 에러");
        }
    }

    public bool RemoveMonster(int index)
    {
        try
        {
            monsterSpawnData.Remove(monsterSpawnData[index]);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public int GetMonsterNum()
    {
        int count = 0;

        for (int i = 0; i < monsterSpawnData.Count; i++)
        {
            count += monsterSpawnData[i].MonsterNum;
        }

        return count;
    }
}

public class MonsterSpawnData
{
    byte monsterId;
    byte monsterLevel;
    byte monsterNum;

    public byte MonsterId { get { return monsterId; } }
    public byte MonsterLevel { get { return monsterLevel; } }
    public byte MonsterNum { get { return monsterNum; } }

    public MonsterSpawnData()
    {
        monsterId = 0;
        monsterLevel = 0;
        monsterNum = 0;
    }

    public MonsterSpawnData(byte newMonsterId, byte newMonsterLevel, byte newMonsterNum)
    {
        monsterId = newMonsterId;
        monsterLevel = newMonsterLevel;
        monsterNum = newMonsterNum;
    }
}