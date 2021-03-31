using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneFollowCam : MonoBehaviour
{

    public float fastCameraMoveTime = 2f;
    public float slowCameraMoveTime = 5f;
    public float frozenStoneVelocity = .1f;
    public float frozenStoneTime = 2f;
    public int frozenStoneTestSamples = 30;
    public float accelerationDelay = 1f;
    public float closenessTolerance = .5f;
    private Vector3 initialPosition;
    private float accelerationCounter;
    private float cameraVelocity;
    private bool inMotionTest = false;
    // for cameraState, -1 idle after initial positioning before tracking, 0=idle, 1=repositioning, 2=tracking, 3=initial positioning before tracking starts
    private int cameraState;
    private GameObject followObject;
    private Vector3 targetCameraPosition;
    private float targetCameraMoveTime;
    private float moveElapsedTime;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = this.transform.position;
        //string iniitialPositionLog = "Initial position is " + initialPosition.x + ", " + initialPosition.y + ", " + initialPosition.z + ".";
        //Debug.Log(iniitialPositionLog);
        accelerationCounter = 0f;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 oldPosition = this.transform.position;
        Vector3 newPosition = oldPosition;
        //not idle
        if (cameraState > 0)
        {
            moveElapsedTime += Time.deltaTime;
        }
        //repositioning (either simple or in preparation to follow)
        if (cameraState == 1 || cameraState == 3)
        {
            if (cameraState == 3)
            {
                targetCameraPosition = new Vector3(initialPosition.x, initialPosition.y, followObject.transform.position.z);
            }
            if (Mathf.Abs(this.transform.position.z - targetCameraPosition.z) > closenessTolerance)
            {
                newPosition = Vector3.Slerp(this.transform.position, targetCameraPosition, moveElapsedTime / targetCameraMoveTime);
                newPosition.y = initialPosition.y;
                this.transform.position = newPosition;
          //      string repoLogText = "Repositioning to target " + targetCameraPosition.x + ", " + targetCameraPosition.y + ", " + targetCameraPosition.z + ". Z distance to target is " + Mathf.Abs(newPosition.z - targetCameraPosition.z) + ".";
          //      Debug.Log(repoLogText);
            }
            else
            {
                switch (cameraState)
                {
                    case 1:
                        StopCameraMovement(0);
                        break;
                    case 3:
                        StopCameraMovement(-1);
                        break;
                    default:
                        break;
                }
            }
        }
        if (cameraState == 2)
        {
            //Debug.Log("Now tracking...");
            targetCameraPosition = new Vector3(initialPosition.x, initialPosition.y, followObject.transform.position.z);
            if (moveElapsedTime > accelerationDelay)
            {
                if (Mathf.Abs(targetCameraPosition.z) >= Mathf.Abs(initialPosition.z))
                {
              //      Debug.Log("Hog cam reached initial position and stopped tracking.");
                    StopCameraMovement(0);
                    StartCoroutine(InitiateSmoothReposition(initialPosition, fastCameraMoveTime, 1));
                    return;
                }
                if (Mathf.Abs(cameraVelocity) < frozenStoneVelocity && !inMotionTest)
                {
                    inMotionTest = true;
                    StopCoroutine("StoppedStoneTest");
                    StartCoroutine("StoppedStoneTest");
                    return;
                }
            }
            this.transform.position = targetCameraPosition;
            newPosition = this.transform.position;
        }
        if (cameraState > 0)
        {
            cameraVelocity = (newPosition.z - oldPosition.z) / Time.deltaTime;
            //string trackingLogText = "Current camera position is " + newPosition.x + ", " + newPosition.y + ", " + newPosition.z + ". Camera velocity is " + cameraVelocity + ".";
            //Debug.Log(trackingLogText);
        }
        
    }

    public void ReturnCameraToHog()
    {
        //Debug.Log("Returning to hog");
        StopCameraMovement();
        StartCoroutine(InitiateSmoothReposition(initialPosition, fastCameraMoveTime, 1));
    }

    public IEnumerator InitiateFollowCam(GameObject objectToFollow)
    {
        //Debug.Log("Follow cam initiated, starting reposition.");
        followObject = objectToFollow.transform.GetChild(0).gameObject;
        Vector3 startingCameraPosition = new Vector3(initialPosition.x, initialPosition.y, followObject.transform.position.z);
        StopCameraMovement(0);
        StartCoroutine(InitiateSmoothReposition(startingCameraPosition, slowCameraMoveTime, 3));
        yield return new WaitUntil(() => cameraState == -1);
        //Debug.Log("Follow start reposition complete");
        cameraState = 2;
    }

    public void StopCameraMovement(int desiredIdleState = 0)
    {
        moveElapsedTime = 0f;
        cameraState = desiredIdleState;
        cameraVelocity = 0f;
        targetCameraMoveTime = fastCameraMoveTime;
        if (cameraState == 3 && desiredIdleState == 0)
        {
            StopCoroutine("InitiateFollowCam");
          //  Debug.Log("Killed initial follow cam repositioning due to StopCameraMovement call.");
        }
        //Debug.Log("Stopped camera movement with idle state " + desiredIdleState);
    }

    IEnumerator InitiateSmoothReposition(Vector3 newPosition, float cameraMoveTime, int desiredMoveState = 1)
    {
        cameraState = desiredMoveState;
        //Debug.Log("Started smoothly repositioning with move state " + desiredMoveState);
        moveElapsedTime = 0f;
        targetCameraPosition = newPosition;
        targetCameraMoveTime = cameraMoveTime;
        yield return null;   
    }

    IEnumerator StoppedStoneTest()
    {
        //Debug.Log("Started motion test");
        int actuallyFrozenSamples = 0;
        for (int i = 0; i < frozenStoneTestSamples; i++)
        {
            yield return new WaitForEndOfFrame();
            if (Mathf.Abs(cameraVelocity) < frozenStoneVelocity)
            {
                actuallyFrozenSamples += 1;
            }
        }
        if (actuallyFrozenSamples >= (frozenStoneTestSamples/2))
        {
            StopCameraMovement();
            //Debug.Log("Stone stopped moving. Stopped following and returning hog camera to initial position.");
            yield return StartCoroutine(InitiateSmoothReposition(initialPosition, slowCameraMoveTime, 1));
        }
        inMotionTest = false;
    }
}
