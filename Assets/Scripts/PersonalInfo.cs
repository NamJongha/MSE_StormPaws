[System.Serializable]
public class PersonalInfo
{
    public string id;
    public string email;
    public string name;
}

[System.Serializable]
public class PersonalInfoWrapper
{
    public bool success;
    public string message;
    public PersonalInfo data;
}