using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class FOVELookSample : MonoBehaviour
{
    // new points ///
    private Configuration conf = new Configuration();
    private Logger log = new Logger();
    private List<location> list = new List<location>();
    private bool once = false;


    [SerializeField]
    public Direction whichEye;

    /// ///////////

    public Light attachedLight;
    public FoveInterfaceBase foveInterface;

    private Collider collider;
    private Material material;
    private bool light_attached = false;
    bool stopper = false;
    Stopwatch sw = new Stopwatch();
    Stopwatch countForLocations = new Stopwatch();

    string firstTimeOnCollider;

    FirebaseLoggingService firebaseLoggingService = new FirebaseLoggingService();

    private void initilize()
    {
        firstTimeOnCollider = DateTime.Now.ToString("HH:mm:ss tt");
        collider = GetComponent<Collider>();

        if (attachedLight == null)
            attachedLight = transform.GetComponentInChildren<Light>();

        if (attachedLight)
        {
            light_attached = true;
            attachedLight.enabled = false;
        }
        material = gameObject.GetComponent<Renderer>().material;

        if (material == null)
            gameObject.SetActive(false);

        countForLocations.Start();
    }

    private void handleGame()
    {
        if (foveInterface.Gazecast(collider))
        {
            gazeOnCollider();
        }
        else // gaze is not on object
        {
            gazeNotOnCollider();
        }
    }

    private void gazeNotOnCollider()
    {
        if (stopper)
        {
            stopper = false;

            // sendColliders was canceled due to sendCorrectness is the same plus the correctness issue
            StartCoroutine(firebaseLoggingService.sendTimesOfCorrectObject(sw.Elapsed, this.GetComponent<Renderer>().name, CommonData.Step, false));
            sw.Stop();
            sw.Reset();
        }
        gameObject.GetComponent<Renderer>().material.color = Color.white;
        material.DisableKeyword("_EMISSION");

        if (light_attached)
        {
            attachedLight.enabled = false;
            DynamicGI.SetEmissive(GetComponent<Renderer>(), Color.black);
        }
    }

    private void gazeOnCollider()
    {
        ///logger////
        log.timeStamp = DateTime.Now.ToString("HH:mm:ss tt");
        log.objectIndactor = collider.ToString();
        log.Write();
        /// 

        //// new points
        FoveInterfaceBase.EyeRays rays = foveInterface.GetGazeRays();

        Ray r = whichEye == Direction.Left ? rays.left : rays.right;

        RaycastHit hit;
        Physics.Raycast(r, out hit, Mathf.Infinity);
        int rate = conf.getConfiguration().rate;
        if ((hit.point != Vector3.zero) && (countForLocations.Elapsed.TotalSeconds > rate))
        {
            list.Add(new location(hit.point.x, hit.point.y, CommonData.realStep));
            countForLocations.Stop();
            countForLocations.Reset();
            countForLocations.Start();
        }

        ///

        if (!stopper)
        {
            stopper = true;
            sw.Start();
        }
        material.EnableKeyword("_EMISSION");

        if (light_attached)
        {
            material.SetColor("_EmissionColor", attachedLight.color);
            attachedLight.enabled = true;
            DynamicGI.SetEmissive(GetComponent<Renderer>(), attachedLight.color);
        }

        if (CommonData.tresPass)
        {
            Debug.Log("trespass");
            CommonData.tresPass = false;
            sw.Stop();
            StartCoroutine(firebaseLoggingService.sendTimesOfCorrectObject(sw.Elapsed, this.GetComponent<Renderer>().name, CommonData.Step - 2, true));
            sw.Reset();
            sw.Start();
        }
    }

    private void handleGameOver()
    {
        once = true;
        foreach (var cor in list)
            StartCoroutine(firebaseLoggingService.sendLocationsPerStep(cor));
        if (CommonData.finishedSendingCordinates)
        {
            if (this.GetComponent<Renderer>().name == "Gazable Cube") // this option will be [0] and the opposite will be [1]
            {
                if (CommonData.samplesFinishedSigns[1]) // asking about "Gazable Cube1" if he finished
                    Application.Quit();                    // if so - we will finish the game
                CommonData.samplesFinishedSigns[0] = true; // it will be usefull if he hasnt done 
            }
            else
            {
                if (CommonData.samplesFinishedSigns[0]) // asking about "Gazable Cube1" if he finished
                    Application.Quit();                    // if so - we will finish the game
                CommonData.samplesFinishedSigns[1] = true; // it will be usefull if he hasnt done 
            }
        }
    }

    void Start()
    {
        initilize();
    }

    void Update()
    {
        if (!CommonData.done) // while the game is not over
        {
            handleGame();
        }
        else if (!once)
        {
            handleGameOver();
        }
    }
    
}
