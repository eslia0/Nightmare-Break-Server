using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public class AccountDatabase
{
    public const string accountDataFile = "AccountData.data";
    public const string heroNameDatafile = "HeroNameData.data";

    private static AccountDatabase instance;

    public static AccountDatabase Instance
    {
        get
        {
            if(instance == null)
            {
                return new AccountDatabase();
            }
            else
            {
                return instance;
            }
        }
    }

    Hashtable accountData;
    List<string> heroNameData;
    Hashtable userData;
    FileStream fs;
    BinaryFormatter bin;

    public Hashtable AccountData
    {
        get
        {
            if (heroNameData == null)
            {
                fs.Close();

                fs = new FileStream(accountDataFile, FileMode.OpenOrCreate);

                try
                {
                    if (fs.Length > 0)
                    {
                        return (Hashtable)bin.Deserialize(fs);
                    }
                    else
                    {
                        return new Hashtable();
                    }
                }
                catch
                {
                    Console.WriteLine("Database::GetData 에러");
                    return null;
                }
            }
            else
            {
                return accountData;
            }
        }
    }
    public List<string> HeroNameData
    {
        get
        {
            if (heroNameData == null)
            {
                fs.Close();

                fs = new FileStream(heroNameDatafile, FileMode.OpenOrCreate);

                try
                {
                    if (fs.Length > 0)
                    {
                        return (List<string>)bin.Deserialize(fs);
                    }
                    else
                    {
                        return new List<string>();
                    }
                }
                catch
                {
                    Console.WriteLine("Database::GetData 에러");
                    return null;
                }
            }
            else
            {
                return heroNameData;
            }
        }
    }
    public Hashtable UserData { get { return userData; } }

    //초기화
    public AccountDatabase()
    {
        bin = new BinaryFormatter();
        fs = new FileStream(accountDataFile, FileMode.OpenOrCreate);

        accountData = AccountData;
        heroNameData = HeroNameData;
        userData = new Hashtable();
    }

    //가입시 아이디 추가
    public bool AddAccountData(string Id, string Pw)
    {
        try
        {
            if (!accountData.Contains(Id))
            {
                accountData.Add(Id, new AccountData(Id, Pw));
                FileSave(accountDataFile, accountData);

                FileSave(Id + ".data", new UserData(Id));

                return true;
            }
            else
            {
                Console.WriteLine("이미 존재하는 아이디");
                return false;
            }
        }
        catch
        {
            Console.WriteLine("Database::AddUserData.Add 에러");
            return false;
        }
    }

    //탈퇴시 아이디 삭제
    public Result DeleteAccountData(string Id, string Pw)
    {
        try
        {
            if (accountData.Contains(Id))
            {
                if (((AccountData)accountData[Id]).Password == Pw)
                {
                    accountData.Remove(Id);
                    FileSave(accountDataFile, accountData);

                    FileInfo file = new FileInfo(Id + ".data");
                    file.Delete();

                    return Result.Success;
                }
                else
                {
                    Console.WriteLine("잘못된 비밀번호");
                    return Result.Fail;
                }
            }
            else
            {
                Console.WriteLine("존재하지 않는 아이디");
                return Result.Fail;
            }
        }
        catch
        {
            Console.WriteLine("Database::DeleteAccountData.Contains 에러");
            return Result.Fail;
        }
    }

    public UserData GetUserData(string id)
    {
        if (userData.Contains(id))
        {
            return (UserData)userData[id];
        }
        else
        {
            return null;
        }
    }

    public UserData AddUserData(string id)
    {
        fs.Close();
        //파일이 있으면 가져오고 없으면 새로 만듬
        try
        {
            fs = new FileStream(id + ".data", FileMode.OpenOrCreate);
        }
        catch
        {
            Console.WriteLine("Database::GetAccountData.FileOpenOrCreate 에러");
            return null;
        }

        UserData newUserData = null;

        if (fs.Length > 0)
        {
            newUserData = (UserData)bin.Deserialize(fs);
        }
        else
        {
            Console.WriteLine("Database::GetAccountData.Deserialize 에러");
            return null;
        }

        //데이터를 유저리스트 테이블에 추가한 뒤 반환
        userData.Add(id, newUserData);

        return newUserData;
    }
    
    //영웅 데이터 반환
    public HeroData GetHeroData(string id, int characterId)
    {
        UserData userData = GetUserData(id);
        return userData.HeroData[characterId];
    }

    //파일 저장
    public bool FileSave(string path, object data)
    {
        try
        {   //FileMode.Create 로 덮어쓰기
            fs.Close();
            fs = new FileStream(path, FileMode.Create);
            Console.WriteLine("저장 경로 : " + path);
        }
        catch
        {
            Console.WriteLine("Database::FileSave.FileMode.Create 에러");
            return false;
        }

        try
        {
            bin.Serialize(fs, data);
            Console.WriteLine(null == fs);
            Console.WriteLine(null == data);
        }
        catch (Exception e)
        {
            Console.WriteLine("Database::FileSaveSerialize 에러" + e.Message);
            return false;
        }

        return true;
    }
}