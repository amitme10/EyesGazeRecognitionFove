using System.IO;
using UnityEngine;

public class Configuration
{
    public string path;
    public string secondsToWait;
    public int rate; // in seconds

    public Configuration getConfiguration()
    {
        return JsonUtility.FromJson<Configuration>(CommonData.configuration);
    }

    public void readConfoguration()
    {
        using (StreamReader r = new StreamReader("Assets/json-schema1.json"))
        {
            CommonData.configuration=r.ReadToEnd();
        }
    }

}