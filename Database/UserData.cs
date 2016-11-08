using System;

[Serializable]
public class UserData
{
    string Id;
    int[] heroClass;
    int[] heroLevel;
    
    public UserData(string newId)
    {
        Id = newId;
    }

    
}