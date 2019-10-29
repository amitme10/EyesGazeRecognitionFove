using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Diagnostics;
using System.IO;

public class neon : MonoBehaviour
{
    /// wall points

    List<location> list = new List<location>();
    bool once = false;

    public enum LeftOrRight
    {
        Left,
        Right
    }

    [SerializeField]
    public LeftOrRight whichEye;

    /// ///////////

    public Texture[] textures;
    public GameObject LeftObj;
    public GameObject RightObj;
    TimeSpan ezer = TimeSpan.Parse("00:00:00");
    int whCollider = 0;

    private Collider wall;
    public FoveInterfaceBase foveInterface;

    bool Stop = false;
    Stopwatch sw = new Stopwatch();

    Stopwatch countForLocations = new Stopwatch();

    string firstTimeOnCollider;
    bool stillHere = false;

    void Start()
    {
        firstTimeOnCollider = DateTime.Now.ToString("HH:mm:ss tt"); // for the empty at the begining
        // wal points
        wall = GetComponent<Collider>();
        // wall points
        LeftObj = GameObject.Find("Gazable Cube");
        RightObj = GameObject.Find("Gazable Cube1");

        LeftObj.GetComponent<Renderer>().material.mainTexture = textures[whCollider++];
        RightObj.GetComponent<Renderer>().material.mainTexture = textures[whCollider++];
        sw.Start();
        countForLocations.Start();
    }

    void Update()
    {
        logger.timeStamp = DateTime.Now.ToString("HH:mm:ss tt");
        //if ((sw.Elapsed > ezer + TimeSpan.Parse("00:00:10")) && !Stop)
        if ((sw.Elapsed > ezer + TimeSpan.Parse(configuration.readConfiguration().secondsToWait)) && !Stop)
        {
            if ((foveInterface.Gazecast(LeftObj.GetComponent<Collider>())) || (foveInterface.Gazecast(RightObj.GetComponent<Collider>())))
            {
                electron.tresPass = true;
                electron.clipper = true;
                electron.timePass = true;
            }

            electron.realStep++;
            electron.Step += 2;
            LeftObj.GetComponent<Renderer>().material.mainTexture = textures[whCollider++];
            RightObj.GetComponent<Renderer>().material.mainTexture = textures[whCollider++];
            ezer = sw.Elapsed;

            if (whCollider + 1 > textures.Length)
            {
                Stop = true;
            }
        }
        //else if (sw.Elapsed > ezer + TimeSpan.Parse("00:00:30")) // for the final step
        else if (sw.Elapsed > ezer + TimeSpan.Parse(configuration.readConfiguration().secondsToWait)) // for the final step  
        {
            electron.done = true;
            /// wall points
            /// 
            if (!once)
            {
                once = true;
                foreach (var cor in list)
                    StartCoroutine(sendingFunctions.sendLocationsPerStep(cor));
            }
            electron.finishedSendingCordinates = true;
        }
        if (foveInterface.Gazecast(wall))
        {
            if (!electron.done)
            {
                FoveInterfaceBase.EyeRays rays = foveInterface.GetGazeRays();

                Ray r = whichEye == LeftOrRight.Left ? rays.left : rays.right;

                RaycastHit hit;
                Physics.Raycast(r, out hit, Mathf.Infinity);
                int rate = configuration.readConfiguration().rate;
                //if ((hit.point != Vector3.zero) && (countForLocations.Elapsed.TotalSeconds > 0.5))
                if ((hit.point != Vector3.zero) && (countForLocations.Elapsed.TotalSeconds > rate))
                {
                    list.Add(new location(hit.point.x, hit.point.y, electron.realStep));
                    countForLocations.Stop();
                    countForLocations.Reset();
                    countForLocations.Start();
                }
            }
        }

        if ((foveInterface.Gazecast(LeftObj.GetComponent<Collider>())) || (foveInterface.Gazecast(RightObj.GetComponent<Collider>())))
        {
            if (!stillHere) // make sure not to do loop in the same object
            {
                // if we came here send for the empty
                StartCoroutine(sendingFunctions.sendTimes(firstTimeOnCollider, electron.realStep, "empty"));
                stillHere = true;
                firstTimeOnCollider = DateTime.Now.ToString("HH:mm:ss tt"); // for the object
                if (foveInterface.Gazecast(LeftObj.GetComponent<Collider>())) // make sure which one the curser on
                    electron.currentObject = LeftObj.GetComponent<Collider>().name;
                else
                    electron.currentObject = RightObj.GetComponent<Collider>().name;
            }

        }
        else // we are on the empty area so send the last object we were on OR IT IS THE BEGING AND THEN DONT SEND
        {
            if (electron.currentObject == null) // is it the first time
                firstTimeOnCollider = DateTime.Now.ToString("HH:mm:ss tt"); // for the object
            else if (stillHere)
            {
                stillHere = false;
                if (electron.timePass)
                {
                    electron.timePass = false;
                    stillHere = true;
                    StartCoroutine(sendingFunctions.sendTimes(firstTimeOnCollider, electron.realStep - 1, electron.currentObject));
                }
                else
                    StartCoroutine(sendingFunctions.sendTimes(firstTimeOnCollider, electron.realStep, electron.currentObject));
                firstTimeOnCollider = DateTime.Now.ToString("HH:mm:ss tt"); // for the empty
            }
        }

    }
}

