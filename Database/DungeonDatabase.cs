//using System;
//using System.Collections.Generic;

//public enum DungeonId
//{
//    None = 0,
//    MissingBear,
//}

//public class DungeonDatabase
//{
//    private static DungeonDatabase instance;

//    public static DungeonDatabase Instance
//    {
//        get
//        {
//            if (instance == null)
//            {
//                instance = new DungeonDatabase();
//            }

//            return instance;
//        }
//    }

//    List<DungeonData> dungeonData;

//    public void InitializeDungeonDatabase()
//    {
//        dungeonData = new List<DungeonData>();

//        AddBaseData(new DungeonData((int)DungeonId.None, "None"));
//        AddBaseData(new DungeonData((int)DungeonId.MissingBear, "잃어버린 곰"));

//        dungeonData[(int)DungeonId.MissingBear].AddMonsterSpawnData(new StageData(MonsterId.Bear));

//    }

//    public bool AddBaseData(DungeonData newDungeonData)
//    {
//        try
//        {
//            dungeonData.Add(newDungeonData);
//            return true;
//        }
//        catch (Exception e)
//        {
//            Console.WriteLine("DungeonDatabase::AddBaseData.Add 에러 " + e.Message);
//            return false;
//        }

//    }

//    public DungeonData GetBaseData(int Id)
//    {
//        foreach (DungeonBaseData baseData in dungeonData)
//        {
//            if (baseData.Id == Id)
//            {
//                return baseData;
//            }
//        }

//        return null;
//    }
//}

//public class DungeonData
//{
//    int id;
//    string name;
//    int stageNum;
//    List<Stage> stages;

//    public int Id { get { return id; } }
//    public string Name { get { return name; } }
//    public StageData StageData { get { return stageData; } }

//    public DungeonData()
//    {
//        id = 0;
//        name = "";
//        maxStageNum = 0;
//        stages = new List<Stage>();
//    }

//    public DungeonData(int _id, string _name)
//    {
//        id = _id;
//        name = _name;
//        maxStageNum = newMaxStageNum;
//        stages = newStages;
//    }

//    public Stage GetStageData(int stageNum)
//    {
//        for (int index = 0; index < stageData.Stages.Count; index++)
//        {
//            if (stageData.Stages[index].StageNum == stageNum)
//            {
//                return stageData.Stages[index];
//            }
//        }

//        return null;
//    }

//    public bool AddStageData(StageData newStageData)
//    {
//        try
//        {
//            stageData.Add(newStageData);
//            return true;
//        }
//        catch
//        {
//            return false;
//        }
//    }
//}

//public class StageData
//{
//    public StageData()
//    {
//    }

//    public StageData(int newMaxStageNum, List<Stage> newStages)
//    {
//    }
//}

//public class Stage
//{
//    int stageNum;
//    MonsterSpawnData monsterSpawnData;

//    public int StageNum { get { return StageNum; } }
//    public MonsterSpawnData MonsterSpawnData { get { return MonsterSpawnData; } }

//    public Stage()
//    {
//        stageNum = 0;
//        monsterSpawnData = new MonsterSpawnData();
//    }

//    public Stage(int newStageNum, MonsterSpawnData newMonsterSpawnData)
//    {
//        stageNum = newStageNum;
//        monsterSpawnData = newMonsterSpawnData;
//    }
//}