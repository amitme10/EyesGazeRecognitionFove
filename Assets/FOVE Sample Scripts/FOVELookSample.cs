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

    public Light attachedLight;
    public FoveInterfaceBase foveInterface;

    private Collider my_collider;
    private Material material;
    private bool light_attached = false;

    bool stopper = false;
    Stopwatch sw = new Stopwatch();
    Stopwatch countForLocations = new Stopwatch();
    TimeSpan General;

    string firstTimeOnCollider;

    List<collID> knownColliders = new List<collID>();

    void Start()
    {
        firstTimeOnCollider = DateTime.Now.ToString("HH:mm:ss tt");
        my_collider = GetComponent<Collider>();

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

    void Update()
    {
        if (!electron.done)
        {
            if (foveInterface.Gazecast(my_collider))
            {
                ///logger////
                logger.timeStamp = DateTime.Now.ToString("HH:mm:ss tt");
                logger.objectIndactor = my_collider.ToString();
                logger.Write();
                /// 


                //// new points
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

                if (electron.tresPass)
                {
                    Debug.Log("trespass");
                    electron.tresPass = false;
                    sw.Stop();
                    StartCoroutine(sendCorrectness(sw.Elapsed, this.GetComponent<Renderer>().name, electron.Step - 2, true));
                    sw.Reset();
                    sw.Start();
                }
            }
            else
            {
                if (stopper)
                {
                    stopper = false;

                    // sendColliders was canceled due to sendCorrectness is the same plus the correctness issue
                    StartCoroutine(sendCorrectness(sw.Elapsed, this.GetComponent<Renderer>().name, electron.Step, false));
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
        }
        /////new points
        else if (!once)
        {
            once = true;
            foreach (var cor in list)
                StartCoroutine(sendingFunctions.sendLocationsPerStep(cor));
            if (electron.finishedSendingCordinates)
            {
                if (this.GetComponent<Renderer>().name == "Gazable Cube") // this option will be [0] and the opposite will be [1]
                {
                    if (electron.samplesFinishedSigns[1]) // asking about "Gazable Cube1" if he finished
                        Application.Quit();                    // if so - we will finish the game
                    electron.samplesFinishedSigns[0] = true; // it will be usefull if he hasnt done 
                }
                else
                {
                    if (electron.samplesFinishedSigns[0]) // asking about "Gazable Cube1" if he finished
                        Application.Quit();                    // if so - we will finish the game
                    electron.samplesFinishedSigns[1] = true; // it will be usefull if he hasnt done 
                }
            }
        }

        ///////
    }

    public class colliderDue
    {
        public string time;
        public int collider;
    }

    public class timeLine
    {
        public string Step;
        public int colliderPerStep;
        public string specificTime;
    }

    public class correctColliderDuring
    {
        public string Step;
        public string totalTime;
    }


    public IEnumerator sendCorrectness(TimeSpan elapsed, string colliderName, int step, bool tresPassTrace)
    {
        bool correctObj = false;

        if (electron.clipper)
        {
            electron.clipper = false;
            General = TimeSpan.Zero;
        }

        General += elapsed;
        colliderDue myObject = new colliderDue();
        //HERE I MADE THE TRANSFROM OF THE COLLIDERCHART FROM LONF FORMAT TO SECONDS.
        myObject.time = General.TotalSeconds.ToString();
        if (colliderName == "Gazable Cube") // the left object
            myObject.collider = step;
        else
            myObject.collider = step + 1;

        bool sign = false;
        Guid guid = Guid.NewGuid();
        Guid guidCorrectness = Guid.NewGuid();
       
        foreach (var key in knownColliders)
            if (key.getCollider() == myObject.collider)
            {
                guid = key.getGuid();
                sign = true;
            }

        if (!sign)
            knownColliders.Add(new collID(myObject.collider, guid));
        ////////// finished deal with any collider

        ////////// starting with the correct

        if (electron.corrects[electron.realStep - 1] == myObject.collider)
        {
            correctObj = true;
        }

        // send for any coollider
        yield return sendingFunctions.sendFirebase("collidersChart---", electron.GameTime, guid, myObject);

        // send for the correct collider
        if (correctObj)
        {
            correctColliderDuring meObjection = new correctColliderDuring();
            if (tresPassTrace)
            {
                meObjection.Step = (electron.realStep - 1).ToString();
                meObjection.totalTime = myObject.time;

                yield return sendingFunctions.sendFirebase("correctCollidersChart---", electron.GameTime, guidCorrectness, meObjection);

            }
            else
            {
                meObjection.Step = electron.realStep.ToString();
                meObjection.totalTime = myObject.time;

                yield return sendingFunctions.sendFirebase("correctCollidersChart---", electron.GameTime, guidCorrectness, meObjection);

            }

            
        }
        // in the case of tresPass
        if (tresPassTrace)
            General = TimeSpan.Zero;

    }
    

}
