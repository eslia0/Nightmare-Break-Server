using System;

[Serializable]
public class UserData
{
    public enum Gender
    {
        male = 0,
        female = 1,
    }

    public const int maxHeroNum = 4;

    string Id;
    HeroData[] heroData;

    public HeroData[] HeroData { get { return heroData; } }

    public UserData(string newId)
    {
        Id = newId;
        heroData = new HeroData[maxHeroNum];

        for (int i = 0; i < maxHeroNum; i++)
        {
            heroData[i] = new HeroData();
        }
    }

    public int FindEmptySlot()
    {
        for (int i = 0; i < maxHeroNum; i++)
        {
            if (heroData[i].LevelData.Level == 0)
            {
                return i;
            }
        }

        return -1;
    }

    public bool CreateHero(CreateCharacterData createData)
    {
        int index = FindEmptySlot();

        if (index < 0)
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

[Serializable]
public class HeroData
{
    public const int skillNum = 6;
    public const int equipNum = 4;
    public const int maxLevel = 20;

    string name;
    int gender;
    int hClass;
    int currentExp;
    HeroLevelData levelData;
    int skillPoint;
    int dreamStone;
    int[] skillLevel;
    int[] equipLevel;

    public string Name { get { return name; } }
    public int Gender { get { return gender; } }
    public int HClass { get { return hClass; } }
    public int CurrentExp { get { return currentExp; } }
    public HeroLevelData LevelData { get {return levelData; } }
    public int SkillPoint { get { return skillPoint; } }
    public int DreamStone { get { return dreamStone; } }
    public int[] SkillLevel { get { return skillLevel; } }
    public int[] EquipLevel { get { return equipLevel; } }
    public byte[] ByteSkillLevel
    {
        get
        {
            byte[] byteArray = new byte[skillNum];

            for (int i = 0; i < skillNum; i++)
            {
                byteArray[i] = (byte)skillLevel[i];
            }

            return byteArray;
        }
    }
    public byte[] ByteEquipLevel
    {
        get
        {
            byte[] byteArray = new byte[equipNum];

            for (int i = 0; i < equipNum; i++)
            {
                byteArray[i] = (byte)equipLevel[i];
            }

            return byteArray;
        }
    }

    public HeroData()
    {
        name = "Hero";
        gender = 0;
        hClass = 0;
        currentExp = 0;
        levelData = new HeroLevelData();
        skillPoint = 0;
        dreamStone = 0;
        skillLevel = new int[skillNum];
        equipLevel = new int[equipNum];
    }

    public HeroData(string newName, int newGender, int newClass)
    {   //차후 데이터 베이스 만들때 그 데이터를 가져와서 초기화 하도록 변경
        name = newName;
        gender = newGender;
        hClass = newClass;
        currentExp = 0;
        levelData = CharacterDatabase.Instance.GetBaseData(hClass).GetLevelData(1);
        skillPoint = 0;
        dreamStone = 0;
        skillLevel = new int[skillNum];
        equipLevel = new int[equipNum];

        for (int i = 0; i < skillNum; i++) { skillLevel[i] = 1; }
        for (int i = 0; i < equipNum; i++) { equipLevel[i] = 1; }
    }

    //경험치 획득 및 레벨업
    public void GainExp(int amount)
    {
        currentExp += amount;

        if (currentExp > LevelData.MaxExp)
        {
            levelData =new HeroLevelData(CharacterDatabase.Instance.GetBaseData(hClass).GetLevelData(levelData.Level + 1));
            currentExp -= LevelData.MaxExp;
            skillPoint++;
        }
    }

    //스킬 투자
    public void SkillUp(int index)
    {
        skillLevel[index]++;
        skillPoint--;
    }

    //장비 강화
    public void EquipUpgrade(int index)
    {
        dreamStone -= equipLevel[index] * 10;
        equipLevel[index]++;
    }
}