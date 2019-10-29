using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class sendingFunctions
{

    public class Royal
    {
        public string x;
        public string y;
        public string Step;
    }

    public class timeLine
    {
        public string Step;
        public int colliderPerStep;
        public string specificTime;
    }

    public static IEnumerator sendLocationsPerStep(location cordinate)
    {
        Royal myObject = new Royal();
        myObject.x = cordinate.getX().ToString();
        myObject.y = cordinate.getY().ToString();
        myObject.Step = cordinate.getStep().ToString();

        Guid guid = Guid.NewGuid();
        return sendFirebase("ScatterMap---", electron.GameTime, guid, myObject);
        
    }

    public static IEnumerator sendTimes(string time, int step, string colliderName)
    {
        timeLine myObject = new timeLine();
        if (colliderName == "empty")
            myObject.colliderPerStep = 1;
        else if (colliderName == "Gazable Cube") // the left object
            myObject.colliderPerStep = 0;
        else
            myObject.colliderPerStep = 2;
        myObject.Step = step.ToString();
        myObject.specificTime = time;

        Guid guid = Guid.NewGuid();
        return sendFirebase("collidersDuringtheGame---", electron.GameTime, guid, myObject);       
    }
    
    public static IEnumerator sendFirebase(string category, string GameTime, Guid guid , System.Object myObject)
    {
        //using (UnityWebRequest www = UnityWebRequest.Put("https://just-1adb5.firebaseio.com/" + category + GameTime + "/" + guid + "/.json", JsonUtility.ToJson(myObject)))
        configuration c1 = configuration.readConfiguration();
        using (UnityWebRequest www = UnityWebRequest.Put(c1.path + category + GameTime + "/" + guid + "/.json", JsonUtility.ToJson(myObject)))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.Send();

            byte[] results = www.downloadHandler.data;
            Debug.Log(www.downloadHandler.text);
        }
        //Debug.Log(myObject);
    }

}
