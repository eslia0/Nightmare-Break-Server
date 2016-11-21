using System;

[Serializable]
public class UserData
{
    public const int maxHeroNum = 4;

    string Id;
    HeroData[] heroData;

    public HeroData[] HeroData { get { return heroData; } }
    
    public UserData(string newId)
    {
        Id = newId;
        heroData = new HeroData[maxHeroNum];

        for(int i =0; i< maxHeroNum; i++)
        {
            heroData[i] = new HeroData();
        }
    }

    public int FindEmptySlot()
    {
        for (int i = 0; i < maxHeroNum; i++)
        {
            if (heroData[i].Level == 0)
            {
                return i;
            }
        }

        return -1;
    }

    public bool CreateHero(CreateCharacterData createData)
    {
        int index = FindEmptySlot();

        if (index <= 0)
        {
            Console.WriteLine("영웅 최대 생성수를 초과하였습니다.");
            return false;
        }

        heroData[index] = new HeroData(createData.HName, createData.Gender, createData.HClass);

        return true;
    }

    public bool DeleteHero(int index)
    {
        try
        {
            heroData[index] = new HeroData();
            return true;
        }
        catch
        {
            return false;
        }        
    }
}

public class HeroData
{
    public const int skillNum = 6;
    public const int equipNum = 4;
    public const int maxLevel = 20;

    string name;
    int gender;
    int level;
    int hClass;
    int exp;
    int healthPoint;
    int magicPoint;
    int hpRegeneration;
    int mpRegeneration;
    int attack;
    int defense;
    float critical;
    int dreamStone;
    int[] skillLevel;
    int[] equipLevel;

    public string Name { get { return name; } }
    public int Level { get { return level; } }
    public int HClass { get { return hClass; } }
    public int Exp { get { return exp; } }
    public int HealthPoint { get { return healthPoint; } }
    public int MagicPoint { get { return magicPoint; } }
    public int HpRegeneration { get { return hpRegeneration; } }
    public int MpRegeneration { get { return mpRegeneration; } }
    public int Attack { get { return attack; } }
    public int Defense { get { return defense; } }
    public float Critical { get { return critical; } }
    public int DreamStone { get { return dreamStone; } }
    public int[] SkillLevel { get { return skillLevel; } }
    public int[] EquipLevel { get { return equipLevel; } }

    public HeroData()
    {
        name = "Hero";
        level = 0;
        hClass = 0;
        exp = 0;
        healthPoint = 0;
        magicPoint = 0;
        hpRegeneration = 0;
        mpRegeneration = 0;
        attack = 0;
        defense = 0;
        critical = 0;
        dreamStone = 0;
        skillLevel = new int[skillNum];
        equipLevel = new int[equipNum];
    }

    public HeroData(string newName, int newGender, int newClass)
    {   //차후 데이터 베이스 만들때 그 데이터를 가져와서 초기화 하도록 변경
        name = newName;
        level = 1;
        gender = newGender;
        hClass = newClass;
        exp = 0;
        healthPoint = 100;
        magicPoint = 20;
        hpRegeneration = 0;
        mpRegeneration = 0;
        attack = 0;
        defense = 0;
        critical = 0;
        dreamStone = 0;
        skillLevel = new int[skillNum];
        equipLevel = new int[equipNum];

        for (int i = 0; i < skillNum; i++) { skillLevel[i] = 1; }
        for (int i = 0; i < equipNum; i++) { equipLevel[i] = 1; }
    }
}