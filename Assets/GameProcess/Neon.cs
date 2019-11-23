using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Diagnostics;
using System.IO; 

public class Neon : MonoBehaviour
{

    [SerializeField]
    private Direction whichEye;
    private Texture[] textures;
    private GameObject LeftObj;
    private GameObject RightObj;
    private FoveInterfaceBase foveInterface;
    private Configuration conf = new Configuration();
    private FirebaseLoggingService firebaseLoggingService = new FirebaseLoggingService();
    private Logger log = new Logger();
    private List<location> locationList = new List<location>();
    private TimeSpan elapsed = TimeSpan.Parse("00:00:00");
    private int whCollider = 0;
    private Collider wall;
    private bool Stop = false;
    private Stopwatch elapsedStopwatch = new Stopwatch();
    private Stopwatch countForLocations = new Stopwatch();
    private string firstTimeOnCollider;
    private bool gazeOnObject = false;
    private bool once = false;

    private void initilze()
    {
        firstTimeOnCollider = DateTime.Now.ToString("HH:mm:ss tt"); // for the empty at the begining
        conf.readConfoguration();
        // wal points
        wall = GetComponent<Collider>();
        // wall points
        LeftObj = GameObject.Find("Gazable Cube");
        RightObj = GameObject.Find("Gazable Cube1");

        LeftObj.GetComponent<Renderer>().material.mainTexture = textures[whCollider++];
        RightObj.GetComponent<Renderer>().material.mainTexture = textures[whCollider++];
        elapsedStopwatch.Start();
        countForLocations.Start();
    }

    private void handleStep()
    {
        if ((foveInterface.Gazecast(LeftObj.GetComponent<Collider>())) || (foveInterface.Gazecast(RightObj.GetComponent<Collider>())))
        {
            CommonData.tresPass = true;
            CommonData.clipper = true;
            CommonData.timePass = true;
        }

        CommonData.realStep++;
        CommonData.Step += 2;
        LeftObj.GetComponent<Renderer>().material.mainTexture = textures[whCollider++];
        RightObj.GetComponent<Renderer>().material.mainTexture = textures[whCollider++];
        elapsed = elapsedStopwatch.Elapsed;

        if (whCollider + 1 > textures.Length)
        {
            Stop = true;
        }
    }

    private void handleFinalStep()
    {
        CommonData.done = true;
        if (!once)
        {
            once = true;
            foreach (var cor in locationList)
                StartCoroutine(firebaseLoggingService.sendLocationsPerStep(cor));
        }
        CommonData.finishedSendingCordinates = true;
    }

    private void gazeOnWall()
    {
        FoveInterfaceBase.EyeRays rays = foveInterface.GetGazeRays();

        Ray r = whichEye == Direction.Left ? rays.left : rays.right;

        RaycastHit hit;
        Physics.Raycast(r, out hit, Mathf.Infinity);
        int rate = conf.getConfiguration().rate;
        if ((hit.point != Vector3.zero) && (countForLocations.Elapsed.TotalSeconds > rate))
        {
            locationList.Add(new location(hit.point.x, hit.point.y, CommonData.realStep));
            countForLocations.Stop();
            countForLocations.Reset();
            countForLocations.Start();
        }
    }

    private void logInTheEndIfGazeNotOnObject()
    {
        // we are on the empty area so send the last object we were on OR IT IS THE BEGING AND THEN DONT SEND
        if (CommonData.currentObject == null) // is it the first time
            firstTimeOnCollider = DateTime.Now.ToString("HH:mm:ss tt"); // for the object
        else if (gazeOnObject)
        {
            gazeOnObject = false;
            if (CommonData.timePass)
            {
                CommonData.timePass = false;
                gazeOnObject = true;
                StartCoroutine(firebaseLoggingService.sendTimes(firstTimeOnCollider, CommonData.realStep - 1, CommonData.currentObject));
            }
            else
                StartCoroutine(firebaseLoggingService.sendTimes(firstTimeOnCollider, CommonData.realStep, CommonData.currentObject));
            firstTimeOnCollider = DateTime.Now.ToString("HH:mm:ss tt"); // for the empty
        }
    }

    private void logInTheEndIfGazeOnObject()
    {
        if (!gazeOnObject) // make sure not to do loop in the same object
        {
            // if we came here send for the empty
            StartCoroutine(firebaseLoggingService.sendTimes(firstTimeOnCollider, CommonData.realStep, "empty"));
            gazeOnObject = true;
            firstTimeOnCollider = DateTime.Now.ToString("HH:mm:ss tt"); // for the object
            if (foveInterface.Gazecast(LeftObj.GetComponent<Collider>())) // make sure which one the curser on
                CommonData.currentObject = LeftObj.GetComponent<Collider>().name;
            else
                CommonData.currentObject = RightObj.GetComponent<Collider>().name;
        }
    }

    void Start()
    {
        initilze();
    }

    void Update()
    {
        log.timeStamp = DateTime.Now.ToString("HH:mm:ss tt");
        if ((elapsedStopwatch.Elapsed > elapsed + TimeSpan.Parse(conf.getConfiguration().secondsToWait)) && !Stop)
        {
            handleStep();
        }
        else if (elapsedStopwatch.Elapsed > elapsed + TimeSpan.Parse(conf.getConfiguration().secondsToWait)) // for the final step  
        {
            handleFinalStep();        
        }
        if (foveInterface.Gazecast(wall) && (!CommonData.done))
        {
            gazeOnWall();
        }

        if ((foveInterface.Gazecast(LeftObj.GetComponent<Collider>())) || (foveInterface.Gazecast(RightObj.GetComponent<Collider>())))
        {
            logInTheEndIfGazeOnObject();
        }
        else
        {
            logInTheEndIfGazeNotOnObject();
        }

    }
}

