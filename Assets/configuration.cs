using System.IO;
using UnityEngine;

public class configuration
{
    public string path;
    public string secondsToWait;
    public int rate; // in seconds

    public static configuration readConfiguration()
    {
        using (StreamReader r = new StreamReader("Assets/json-schema1.json"))
        {
            string config = r.ReadToEnd();
            return JsonUtility.FromJson<configuration>(config);
        }
    }

}