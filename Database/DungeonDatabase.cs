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
        
        AddBaseData(new DungeonData((int)DungeonId.MissingBear, "잃어버린 곰"));

        Stage missingBearStage1 = new Stage(1);
        Stage missingBearStage2 = new Stage(2);
        Stage missingBearStage3 = new Stage(3);
        Stage missingBearStage4 = new Stage(4);

        missingBearStage1.AddMonster((int)MonsterId.Frog, 5);
        missingBearStage1.AddMonster((int)MonsterId.Duck, 4);
        missingBearStage1.AddMonster((int)MonsterId.Rabbit, 1);

        missingBearStage2.AddMonster((int)MonsterId.Frog, 4);
        missingBearStage2.AddMonster((int)MonsterId.Duck, 4);
        missingBearStage2.AddMonster((int)MonsterId.Bear, 1);

        missingBearStage3.AddMonster((int)MonsterId.Frog, 3);
        missingBearStage3.AddMonster((int)MonsterId.Duck, 3);
        missingBearStage3.AddMonster((int)MonsterId.Rabbit, 3);

        missingBearStage4.AddMonster((int)MonsterId.Rabbit, 5);
        missingBearStage4.AddMonster((int)MonsterId.Duck, 4);
        missingBearStage2.AddMonster((int)MonsterId.BlackBear, 1);

        dungeonData[(int)DungeonId.MissingBear].AddStage(missingBearStage1);
        dungeonData[(int)DungeonId.MissingBear].AddStage(missingBearStage2);
        dungeonData[(int)DungeonId.MissingBear].AddStage(missingBearStage3);
        dungeonData[(int)DungeonId.MissingBear].AddStage(missingBearStage4);
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

    public DungeonData GetDungeonData(int Id)
    {
        foreach (DungeonData baseData in dungeonData)
        {
            if (baseData.Id == Id)
            {
                return baseData;
            }
        }

        return null;
    }
}

public class DungeonData
{
    int id;
    string name;
    int stageNum;
    List<Stage> stages;

    public int Id { get { return id; } }
    public string Name { get { return name; } }
    public int StageNum { get { return stageNum; } }
    public List<Stage> StageData { get { return stages; } }

    public DungeonData()
    {
        id = 0;
        name = "";
        stageNum = 0;
        stages = new List<Stage>();
    }

    public DungeonData(int _id, string _name)
    {
        id = _id;
        name = _name;
        stageNum = 0;
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
            stageNum++;
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
            stageNum--;
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class Stage
{
    int stageNum;
    List<MonsterSpawnData> monsterSpawnData;

    public int StageNum { get { return stageNum; } }
    public List<MonsterSpawnData> MonsterSpawnData { get { return MonsterSpawnData; } }

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

    public void AddMonster(int monsterId, int monsterNum)
    {
        try
        {
            monsterSpawnData.Add(new MonsterSpawnData((byte)monsterId, (byte)monsterNum));            
        }
        catch
        {
            Console.WriteLine("DungeonDatabase::Stage.AddStage 에러");
        }
    }

    public bool RemoveState(int index)
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
}