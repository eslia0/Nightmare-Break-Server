﻿using System;
using System.Collections.Generic;

public enum MonsterId
{
    Frog,
    Duck,
    Rabbit,
    Bear,
    BlackBear,
}

public class MonsterDatabase
{
    private static MonsterDatabase instance;

    public static MonsterDatabase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MonsterDatabase();
            }

            return instance;
        }
    }

    List<MonsterBaseData> monsterData;

    public void InitializeMonsterDatabase()
    {
        instance = this;
        monsterData = new List<MonsterBaseData>();

        AddBaseData(new MonsterBaseData((int)MonsterId.Frog, "Frog"));
        AddBaseData(new MonsterBaseData((int)MonsterId.Duck, "Duck"));
        AddBaseData(new MonsterBaseData((int)MonsterId.Rabbit, "Rabbit"));
        AddBaseData(new MonsterBaseData((int)MonsterId.Bear, "Bear"));
        AddBaseData(new MonsterBaseData((int)MonsterId.BlackBear, "BlackBear"));

        GetBaseData((int)MonsterId.Frog).AddLevelData(new MonsterLevelData(1, 2, 0, 30, 5));
        GetBaseData((int)MonsterId.Frog).AddLevelData(new MonsterLevelData(2, 3, 0, 40, 5));
        GetBaseData((int)MonsterId.Frog).AddLevelData(new MonsterLevelData(3, 5, 0, 50, 6));
        GetBaseData((int)MonsterId.Frog).AddLevelData(new MonsterLevelData(4, 7, 0, 70, 6));
        GetBaseData((int)MonsterId.Frog).AddLevelData(new MonsterLevelData(5, 9, 0, 90, 7));
        GetBaseData((int)MonsterId.Frog).AddLevelData(new MonsterLevelData(6, 11, 5, 110, 7));
        GetBaseData((int)MonsterId.Frog).AddLevelData(new MonsterLevelData(7, 15, 7, 150, 8));
        GetBaseData((int)MonsterId.Frog).AddLevelData(new MonsterLevelData(8, 20, 9, 200, 8));
        GetBaseData((int)MonsterId.Frog).AddLevelData(new MonsterLevelData(9, 24, 11, 250, 9));
        GetBaseData((int)MonsterId.Frog).AddLevelData(new MonsterLevelData(10, 30, 15, 350, 9));

        GetBaseData((int)MonsterId.Duck).AddLevelData(new MonsterLevelData(1, 3, 0, 35, 4));
        GetBaseData((int)MonsterId.Duck).AddLevelData(new MonsterLevelData(2, 4, 0, 45, 4));
        GetBaseData((int)MonsterId.Duck).AddLevelData(new MonsterLevelData(3, 5, 0, 55, 4));
        GetBaseData((int)MonsterId.Duck).AddLevelData(new MonsterLevelData(4, 6, 0, 75, 5));
        GetBaseData((int)MonsterId.Duck).AddLevelData(new MonsterLevelData(5, 7, 0, 95, 5));
        GetBaseData((int)MonsterId.Duck).AddLevelData(new MonsterLevelData(6, 8, 0, 115, 5));
        GetBaseData((int)MonsterId.Duck).AddLevelData(new MonsterLevelData(7, 10, 0, 145, 5));
        GetBaseData((int)MonsterId.Duck).AddLevelData(new MonsterLevelData(8, 12, 0, 175, 5));
        GetBaseData((int)MonsterId.Duck).AddLevelData(new MonsterLevelData(9, 14, 0, 205, 6));
        GetBaseData((int)MonsterId.Duck).AddLevelData(new MonsterLevelData(10, 16, 0, 235, 6));

        GetBaseData((int)MonsterId.Rabbit).AddLevelData(new MonsterLevelData(1, 5, 0, 75, 4));
        GetBaseData((int)MonsterId.Rabbit).AddLevelData(new MonsterLevelData(2, 7, 1, 100, 4));
        GetBaseData((int)MonsterId.Rabbit).AddLevelData(new MonsterLevelData(3, 9, 2, 125, 5));
        GetBaseData((int)MonsterId.Rabbit).AddLevelData(new MonsterLevelData(4, 12, 3, 175, 5));
        GetBaseData((int)MonsterId.Rabbit).AddLevelData(new MonsterLevelData(5, 15, 4, 225, 6));
        GetBaseData((int)MonsterId.Rabbit).AddLevelData(new MonsterLevelData(6, 18, 5, 275, 6));
        GetBaseData((int)MonsterId.Rabbit).AddLevelData(new MonsterLevelData(7, 21, 6, 325, 7));
        GetBaseData((int)MonsterId.Rabbit).AddLevelData(new MonsterLevelData(8, 25, 7, 400, 7));
        GetBaseData((int)MonsterId.Rabbit).AddLevelData(new MonsterLevelData(9, 29, 8, 475, 7));
        GetBaseData((int)MonsterId.Rabbit).AddLevelData(new MonsterLevelData(10, 33, 9, 550, 8));

        GetBaseData((int)MonsterId.Bear).AddLevelData(new MonsterLevelData(1, 10, 0, 200, 5));
        GetBaseData((int)MonsterId.Bear).AddLevelData(new MonsterLevelData(2, 15, 1, 300, 5));
        GetBaseData((int)MonsterId.Bear).AddLevelData(new MonsterLevelData(3, 21, 2, 400, 6));
        GetBaseData((int)MonsterId.Bear).AddLevelData(new MonsterLevelData(4, 27, 4, 500, 6));
        GetBaseData((int)MonsterId.Bear).AddLevelData(new MonsterLevelData(5, 33, 6, 6500, 7));
        GetBaseData((int)MonsterId.Bear).AddLevelData(new MonsterLevelData(6, 40, 8, 800, 7));
        GetBaseData((int)MonsterId.Bear).AddLevelData(new MonsterLevelData(7, 48, 12, 1000, 8));
        GetBaseData((int)MonsterId.Bear).AddLevelData(new MonsterLevelData(8, 57, 18, 1300, 8));
        GetBaseData((int)MonsterId.Bear).AddLevelData(new MonsterLevelData(9, 67, 24, 1600, 9));
        GetBaseData((int)MonsterId.Bear).AddLevelData(new MonsterLevelData(10, 78, 45, 2000, 10));

        GetBaseData((int)MonsterId.BlackBear).AddLevelData(new MonsterLevelData(1, 20, 0, 300, 5));
        GetBaseData((int)MonsterId.BlackBear).AddLevelData(new MonsterLevelData(2, 35, 2, 450, 5));
        GetBaseData((int)MonsterId.BlackBear).AddLevelData(new MonsterLevelData(3, 50, 4, 700, 6));
        GetBaseData((int)MonsterId.BlackBear).AddLevelData(new MonsterLevelData(4, 75, 8, 1000, 6));
        GetBaseData((int)MonsterId.BlackBear).AddLevelData(new MonsterLevelData(5, 105, 12, 1400, 7));
        GetBaseData((int)MonsterId.BlackBear).AddLevelData(new MonsterLevelData(6, 130, 16, 2000, 7));
        GetBaseData((int)MonsterId.BlackBear).AddLevelData(new MonsterLevelData(7, 160, 24, 2800, 8));
        GetBaseData((int)MonsterId.BlackBear).AddLevelData(new MonsterLevelData(8, 200, 36, 3800, 8));
        GetBaseData((int)MonsterId.BlackBear).AddLevelData(new MonsterLevelData(9, 250, 48, 5000, 9));
        GetBaseData((int)MonsterId.BlackBear).AddLevelData(new MonsterLevelData(10, 330, 90, 6400, 10));
    }

    public bool AddBaseData(MonsterBaseData newMonsterData)
    {
        try
        {
            monsterData.Add(newMonsterData);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("MonsterDatabase::AddBaseData.Add 에러 " + e.Message);
            return false;
        }

    }

    public MonsterBaseData GetBaseData(int Id)
    {
        foreach (MonsterBaseData baseData in monsterData)
        {
            if (baseData.Id == Id)
            {
                return baseData;
            }
        }

        return null;
    }
}

public class MonsterBaseData
{
    int id;
    string name;
    List<MonsterLevelData> monsterLevelData;

    public int Id { get { return id; } }
    public string Name { get { return name; } }
    public List<MonsterLevelData> MonsterLevelData { get { return monsterLevelData; } }

    public MonsterBaseData()
    {
        id = 0;
        name = "";
        monsterLevelData = new List<MonsterLevelData>();
    }

    public MonsterBaseData(int _id, string _name)
    {
        id = _id;
        name = _name;
        monsterLevelData = new List<MonsterLevelData>();
    }

    public MonsterBaseData(MonsterBaseData newMonsterBaseData)
    {
        id = newMonsterBaseData.Id;
        name = newMonsterBaseData.Name;
        monsterLevelData = new List<MonsterLevelData>();

        foreach (MonsterLevelData levelData in newMonsterBaseData.MonsterLevelData)
        {
            monsterLevelData.Add(new MonsterLevelData(levelData));
        }
    }

    public MonsterLevelData GetLevelData(int level)
    {
        for (int index = 0; index < monsterLevelData.Count; index++)
        {
            if (monsterLevelData[index].Level == level)
            {
                return monsterLevelData[index];
            }
        }

        return null;
    }

    public bool AddLevelData(MonsterLevelData newMonsterLevelData)
    {
        try
        {
            monsterLevelData.Add(newMonsterLevelData);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class MonsterLevelData
{
    int level;
    int attack;
    int defense;
    int healthPoint;
    int moveSpeed;

    public int Level { get { return level; } }
    public int Attack { get { return attack; } }
    public int Defense { get { return defense; } }
    public int HealthPoint { get { return healthPoint; } }
    public int MoveSpeed { get { return moveSpeed; } }

    public MonsterLevelData()
    {
        level = 1;
        attack = 0;
        defense = 0;
        healthPoint = 0;
        moveSpeed = 1;
    }

    public MonsterLevelData(MonsterLevelData newMonsterLevelData)
    {
        level = newMonsterLevelData.Level;
        attack = newMonsterLevelData.Attack;
        defense = newMonsterLevelData.Defense;
        healthPoint = newMonsterLevelData.HealthPoint;
        moveSpeed = newMonsterLevelData.MoveSpeed;
    }

    public MonsterLevelData(int newLevel, int newAttack, int newDefense, int newHealthPoint, int newMoveSpeed)
    {
        level = newLevel;
        attack = newAttack;
        defense = newDefense;
        healthPoint = newHealthPoint;
        moveSpeed = newMoveSpeed;
    }
}


