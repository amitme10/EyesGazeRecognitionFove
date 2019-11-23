using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseLoggingService
{

    public IEnumerator sendLocationsPerStep(location cordinate)
    {
        LocationPerStep log = new LocationPerStep();
        log.x = cordinate.getX().ToString();
        log.y = cordinate.getY().ToString();
        log.Step = cordinate.getStep().ToString();

        Guid guid = Guid.NewGuid();
        return sendFirebase("ScatterMap---", CommonData.GameTime, guid, log);
        
    }

    public IEnumerator sendTimes(string time, int step, string colliderName)
    {
        ObjectsTimeLine log = new ObjectsTimeLine();
        if (colliderName == "empty")
            log.colliderPerStep = 1;
        else if (colliderName == "Gazable Cube") // the left object
            log.colliderPerStep = 0;
        else
            log.colliderPerStep = 2;
        log.Step = step.ToString();
        log.specificTime = time;

        Guid guid = Guid.NewGuid();
        return sendFirebase("collidersDuringtheGame---", CommonData.GameTime, guid, log);       
    }

    public IEnumerator sendTimesOfCorrectObject(TimeSpan elapsed, string colliderName, int step, bool tresPassTrace)
    {
        ColliderDue collider = new ColliderDue();
        TimeSpan General = TimeSpan.Zero;
        List<CollID> knownColliders = new List<CollID>();
        Guid guid = Guid.NewGuid();
        Guid guidCorrectness = Guid.NewGuid();
        bool sign = false;
        bool correctObj = false;

        if (CommonData.clipper)
        {
            CommonData.clipper = false;
            General = TimeSpan.Zero;
        }

        General += elapsed;
        //HERE I MADE THE TRANSFROM OF THE COLLIDERCHART FROM LONF FORMAT TO SECONDS.
        collider.time = General.TotalSeconds.ToString();
        if (colliderName == "Gazable Cube") // the left object
            collider.collider = step;
        else
            collider.collider = step + 1;

        foreach (var key in knownColliders)
            if (key.getCollider() == collider.collider)
            {
                guid = key.getGuid();
                sign = true;
            }

        if (!sign)
            knownColliders.Add(new CollID(collider.collider, guid));
        ////////// finished deal with any collider

        ////////// starting with the correct

        if (CommonData.corrects[CommonData.realStep - 1] == collider.collider)
            correctObj = true;

        // send for any coollider
        yield return sendFirebase("collidersChart---", CommonData.GameTime, guid, collider);

        // send for the correct collider
        if (correctObj)
        {
            CorrectColliderDuring correctColliderDuringObject = new CorrectColliderDuring();
            if (tresPassTrace)
                correctColliderDuringObject.Step = (CommonData.realStep - 1).ToString();
            else
                correctColliderDuringObject.Step = CommonData.realStep.ToString();
            
            correctColliderDuringObject.totalTime = collider.time;

            yield return sendFirebase("correctCollidersChart---", CommonData.GameTime, guidCorrectness, correctColliderDuringObject);
        }
        // in the case of tresPass
        if (tresPassTrace)
            General = TimeSpan.Zero;

    }

    public IEnumerator sendFirebase(string category, string GameTime, Guid guid , System.Object log)
    {
        Configuration c1 = new Configuration();
        using (UnityWebRequest www = UnityWebRequest.Put(c1.getConfiguration().path + category + GameTime + "/" + guid + "/.json", JsonUtility.ToJson(log)))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.Send();

            byte[] results = www.downloadHandler.data;
            Debug.Log(www.downloadHandler.text);
        }
    }

}
