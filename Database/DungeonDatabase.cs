using System;
using System.Collections.Generic;

public enum DungeonId
{
    MissingBear,
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

    List<DungeonData> dungeonData;

    public void InitializeDungeonDatabase()
    {
        dungeonData = new List<DungeonData>();

        #region 잃어버린 곰 던전 1레벨
        AddBaseData(new DungeonData((int)DungeonId.MissingBear, 1, "잃어버린 곰"));

        Stage missingBearStage1 = new Stage(1);
        Stage missingBearStage2 = new Stage(2);
        Stage missingBearStage3 = new Stage(3);
        Stage missingBearStage4 = new Stage(4);

        missingBearStage1.AddMonster((int)MonsterId.Frog, 1, 5);
        missingBearStage1.AddMonster((int)MonsterId.Duck, 1, 4);
        missingBearStage1.AddMonster((int)MonsterId.Rabbit, 1, 1);

        missingBearStage2.AddMonster((int)MonsterId.Frog, 1, 4);
        missingBearStage2.AddMonster((int)MonsterId.Duck, 1, 4);
        missingBearStage2.AddMonster((int)MonsterId.Bear, 1, 1);

        missingBearStage3.AddMonster((int)MonsterId.Frog, 1, 3);
        missingBearStage3.AddMonster((int)MonsterId.Duck, 1, 3);
        missingBearStage3.AddMonster((int)MonsterId.Rabbit, 1, 3);

        missingBearStage4.AddMonster((int)MonsterId.Rabbit, 1, 5);
        missingBearStage4.AddMonster((int)MonsterId.Duck, 1, 4);
        missingBearStage4.AddMonster((int)MonsterId.BlackBear, 1, 1);

        dungeonData[(int)DungeonId.MissingBear].AddStage(missingBearStage1);
        dungeonData[(int)DungeonId.MissingBear].AddStage(missingBearStage2);
        dungeonData[(int)DungeonId.MissingBear].AddStage(missingBearStage3);
        dungeonData[(int)DungeonId.MissingBear].AddStage(missingBearStage4);
        #endregion
    }

    public bool AddBaseData(DungeonData newDungeonData)
    {
        try
        {
            dungeonData.Add(newDungeonData);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("DungeonDatabase::AddBaseData.Add 에러 " + e.Message);
            return false;
        }

    }

    public DungeonData GetDungeonData(int id, int level)
    {
        for (int i = 0; i < dungeonData.Count; i++)
        {
            if (dungeonData[i].Id == id && dungeonData[i].Level == level)
            {
                return dungeonData[i];
            }
        }

        return null;
    }
}

public class DungeonData
{
    int id;
    string name;
    int level;
    List<Stage> stages;

    public int Id { get { return id; } }
    public string Name { get { return name; } }
    public int Level { get { return level; } }
    public List<Stage> Stages { get { return stages; } }

    public DungeonData()
    {
        id = 0;
        name = "";
        level = 0;
        stages = new List<Stage>();
    }

    public DungeonData(int _id, int _level, string _name)
    {
        id = _id;
        name = _name;
        level = _level;
        stages = new List<Stage>();
    }

    public Stage GetStage(int stageNum)
    {
        for (int index = 0; index < stages.Count; index++)
        {
            if (stages[index].StageNum == stageNum)
            {
                return stages[index];
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

    public bool RemoveStage(int index)
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