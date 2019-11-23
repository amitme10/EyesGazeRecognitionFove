using UnityEngine;


public class FOVE3DCursor : MonoBehaviour
{

    [SerializeField]
    public Direction whichEye;
    public FoveInterfaceBase foveInterface;

    Logger log;
    // Use this for initialization
    void Start()
    {
    }

    // Latepdate ensures that the object doesn't lag behind the user's head motion
    void Update()
    {
        FoveInterfaceBase.EyeRays rays = foveInterface.GetGazeRays();

        Ray r = whichEye == Direction.Left ? rays.left : rays.right;

        RaycastHit hit;
        Physics.Raycast(r, out hit, Mathf.Infinity);

        if (whichEye == Direction.Left)
        {
            log.leftX = hit.point.x.ToString();
            log.leftY = hit.point.y.ToString();
        }
        else
        {
            log.rightX = hit.point.x.ToString();
            log.rightY = hit.point.y.ToString();
        }


        if (hit.point != Vector3.zero) // Vector3 is non-nullable; comparing to null is always false
        {
            transform.position = hit.point;
        }
        else
        {
            transform.position = r.GetPoint(3.0f);
        }
    }

}
