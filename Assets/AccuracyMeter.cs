using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccuracyMeter : MonoBehaviour
{

    public int maxValue = 100;
    public int minValue = 0;
    public float scrollSpeed = 5f;

    private GameObject meterObject;
    private GameObject cursorObject;
    private GameObject targetObject;
    private RectTransform meterRectTransform;
    private RectTransform cursorRectTransform;
    private RectTransform targetRectTransform;
    private bool isActive = false;

    private Vector3 meterTopPosition;
    private Vector3 meterBottomPosition;

    public bool IsActive
    {
        get
        {
            return isActive;
        }
    }

    void Awake()
    {
        meterObject = this.gameObject.transform.GetChild(0).gameObject;
        cursorObject = this.gameObject.transform.GetChild(1).gameObject;
        targetObject = this.gameObject.transform.GetChild(2).gameObject;
        meterRectTransform = meterObject.GetComponent<RectTransform>();
        cursorRectTransform = cursorObject.GetComponent<RectTransform>();
        targetRectTransform = targetObject.GetComponent<RectTransform>();

        //For debugging
        StartCursorCycle();
    }

    public void StartCursorCycle()
    {
        meterTopPosition = new Vector3(meterRectTransform.localPosition.x, meterRectTransform.localPosition.y + (meterRectTransform.sizeDelta.y / 2) - (cursorRectTransform.sizeDelta.y / 2), meterRectTransform.localPosition.z);
        meterBottomPosition = new Vector3(meterTopPosition.x, meterTopPosition.y - meterRectTransform.sizeDelta.y + cursorRectTransform.sizeDelta.y, meterTopPosition.z);
        StopCoroutine("CycleCursor");
        StartCoroutine(CycleCursor(meterBottomPosition, meterTopPosition));
    }

    private float ComputeYDelta(Vector3 Position1, Vector3 Position2)
    {
        return Mathf.Abs(Position1.y - Position2.y);
    }

    private IEnumerator CycleCursor(Vector3 startPosition, Vector3 endPosition)
    {
        cursorRectTransform.localPosition = startPosition;
        Vector3 localStartPosition = startPosition;
        Vector3 localEndPosition = endPosition;
        float elapsedTime = 0f;
        Vector3 currentPosition = startPosition;
        float yDistanceTraveled = 0f;
        float yDistanceToTravel = ComputeYDelta(localStartPosition, localEndPosition);
        while (true)
        {
            while (yDistanceTraveled < yDistanceToTravel)
            {
                elapsedTime += Time.deltaTime;
                currentPosition = Vector3.Lerp(localStartPosition, localEndPosition, elapsedTime / scrollSpeed);
                cursorRectTransform.localPosition = currentPosition;
                yDistanceTraveled = ComputeYDelta(localStartPosition, currentPosition);
                yield return new WaitForEndOfFrame();
            }
            localEndPosition = localStartPosition;
            localStartPosition = currentPosition;
            yDistanceTraveled = 0f;
            elapsedTime = 0f;

            }
        }
    }
