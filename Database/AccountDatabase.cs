using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

public class AccountDatabase
{
    public const string accountDataFile = "AccountData.data";

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
    Hashtable userData;
    FileStream fs;
    BinaryFormatter bin;

    public Hashtable AccountData { get { return accountData; } }
    public Hashtable UserData { get { return userData; } }

    public AccountDatabase()
    {
        bin = new BinaryFormatter();
        fs = new FileStream(accountDataFile, FileMode.OpenOrCreate);

        accountData = GetData(accountDataFile);

        userData = new Hashtable();
    }

    public Hashtable GetData(string path)
    {
        fs.Close();
        fs = new FileStream(path, FileMode.OpenOrCreate);

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
            return new Hashtable();
        }
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
    public DataHandler.Result DeleteAccountData(string Id, string Pw)
    {
        try
        {
            if (accountData.Contains(Id))
            {
                if (((AccountData)accountData[Id]).PW == Pw)
                {
                    accountData.Remove(Id);
                    FileSave(accountDataFile, accountData);

                    FileInfo file = new FileInfo(Id + ".data");
                    file.Delete();

                    return DataHandler.Result.Success;
                }
                else
                {
                    Console.WriteLine("잘못된 비밀번호");
                    return DataHandler.Result.Fail;
                }
            }
            else
            {
                Console.WriteLine("존재하지 않는 아이디");
                return DataHandler.Result.Fail;
            }
        }
        catch
        {
            Console.WriteLine("Database::DeleteAccountData.Contains 에러");
            return DataHandler.Result.Fail;
        }
    }

    public UserData GetAccountData(LoginCharacter Id)
    {
        if (userData.Contains(Id))
        {
            return (UserData)userData[Id];
        }
        else
        {
            return null;
        }
    }

    public UserData AddUserData(LoginCharacter Id)
    {
        fs.Close();
        //파일이 있으면 가져오고 없으면 새로 만듬
        try
        {
            fs = new FileStream(Id + ".data", FileMode.OpenOrCreate);
        }
        catch
        {
            Console.WriteLine("Database::GetAccountData.FileOpenOrCreate 에러");
        }

        UserData newUserData;

        //원래 있는 경우에는 그 파일의 데이터를 가져오고
        if (fs.Length > 0)
        {
            newUserData = (UserData)bin.Deserialize(fs);
        }
        //없을 경우에는 새로 만들어서 가져옴
        else
        {
            newUserData = new UserData(Id.Id);
        }

        //데이터를 유저리스트 테이블에 추가한 뒤 반환
        userData.Add(Id, newUserData);
        return newUserData;
    }

    public bool FileSave(string path, object data)
    {
        try
        {
            fs.Close();
            fs = new FileStream(path, FileMode.Create);
        }
        catch
        {
            Console.WriteLine("Database::FileSave.FileMode.Create 에러");
            return false;
        }

        try
        {
            bin.Serialize(fs, data);
        }
        catch (Exception e)
        {
            Console.WriteLine("Database::FileSaveSerialize 에러" + e.Message);
            return false;
        }

        return true;
    }
}

[Serializable]
public class AccountData
{
    string Id;
    string Pw;

    public string ID { get { return Id; } }
    public string PW { get { return Pw; } }

    public AccountData(string newId, string newPw)
    {
        Id = newId;
        Pw = newPw;
    }
}