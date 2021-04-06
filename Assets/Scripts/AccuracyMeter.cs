﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccuracyMeter : MonoBehaviour
{

    public float scrollSpeed = 5f;

    private GameObject meterObject;
    private GameObject cursorObject;
    private GameObject targetObject;
    private RectTransform meterRectTransform;
    private RectTransform cursorRectTransform;
    private RectTransform targetRectTransform;
    private bool isActive = false;
    private float targetValue = .8f;

    private Vector3 meterTopPosition;
    private Vector3 meterBottomPosition;

    public bool IsActive
    {
        get
        {
            return isActive;
        }
    }

    public void SetTargetValue(float desiredValue)
    {
        targetValue = desiredValue;
        SetMeterPositions();
        float meterTotalHeight = ComputeYDelta(meterTopPosition, meterBottomPosition);
        float meterRelativeHeight = meterTotalHeight * desiredValue;
        Vector3 targetDesiredPosition = new Vector3(targetRectTransform.localPosition.x, meterBottomPosition.y + meterRelativeHeight, targetRectTransform.localPosition.z);
        targetRectTransform.localPosition = targetDesiredPosition;
    }

    void Awake()
    {
        meterObject = this.gameObject.transform.GetChild(0).gameObject;
        cursorObject = this.gameObject.transform.GetChild(1).gameObject;
        targetObject = this.gameObject.transform.GetChild(2).gameObject;
        meterRectTransform = meterObject.GetComponent<RectTransform>();
        cursorRectTransform = cursorObject.GetComponent<RectTransform>();
        targetRectTransform = targetObject.GetComponent<RectTransform>();
        SetTargetValue(.8f);
        SetMeterPositions();

        //For debugging
        //StartCursorCycle()
        //Debug.Log("Target value: " + targetValue.ToString());
        //Debug.Log("Cursor absolute height: " + GetCursorAbsoluteLocalHeight().ToString());
        //Debug.Log("Target absolute hieght: " + targetRectTransform.localPosition.y.ToString());
        //Debug.Log("Cursor height above baseline: " + GetCursorHeightAboveBaseline().ToString());
        //Debug.Log("Target height above baseline: " + (targetRectTransform.localPosition.y - meterBottomPosition.y).ToString());
        //Debug.Log("Cursor scaled height: " + GetCursorScaledHeight().ToString());
        //Debug.Log("Cursor percentage of target: " + GetCursorPercentageOfTarget().ToString());
        //Debug.Log("Cursor deviation from target: " + GetCursorDeviationFromTarget().ToString());
        //Debug.Log("Cursor absolute deviation from target: " + GetCursorAbsoluteDeviationFromTarget().ToString());
        //Debug.Log("Cursor percentage of deviation from target: " + GetCursorPercentageOfDeviationFromTarget().ToString());
        //Debug.Log("Cursor absolute percentage of deviation from target: " + GetCursorAbsolutePercentageOfDeviationFromTarget().ToString());
    }

    private void SetMeterPositions()
    {
        meterTopPosition = new Vector3(meterRectTransform.localPosition.x, meterRectTransform.localPosition.y + (meterRectTransform.sizeDelta.y / 2) - (cursorRectTransform.sizeDelta.y / 2), meterRectTransform.localPosition.z);
        meterBottomPosition = new Vector3(meterTopPosition.x, meterTopPosition.y - meterRectTransform.sizeDelta.y + cursorRectTransform.sizeDelta.y, meterTopPosition.z);

    }

    public void StartCursorCycle()
    {
        SetMeterPositions();
        StopCoroutine("CycleCursor");
        StartCoroutine(CycleCursor(meterBottomPosition, meterTopPosition));
    }

    public void StopCursorCycle()
    {
        StopCoroutine("CycleCursor");
    }

    private float ComputeYDelta(Vector3 Position1, Vector3 Position2)
    {
        return Mathf.Abs(Position1.y - Position2.y);
    }

    public float GetCursorAbsoluteLocalHeight()
    {
        // returns the cursor's current y-axis position in local units
        return cursorRectTransform.localPosition.y;
    }

    public float GetCursorHeightAboveBaseline()
    {
        // returns the number of local units the cursor's current y-axis position is above the bottom of the meter
        return GetCursorAbsoluteLocalHeight() - meterBottomPosition.y;
    }

    public float GetCursorScaledHeight()
    {
        //returns the cursor's current y-axis position as a % of the meter's total heigh, with 0 being all the way at the bottom and 1 all the way at the top
        float cursorLocalHeight = GetCursorHeightAboveBaseline();
        float meterTotalHeight = ComputeYDelta(meterTopPosition, meterBottomPosition);
        return cursorLocalHeight / meterTotalHeight;
    }

    public float GetCursorPercentageOfTarget()
    {
        // returns the cursor's current y-axis position as a percentage of the target value.
        // if the cursor is perfectly on target, this returns 1
        // if the cursor is half the target height, this returns .5
        // if the cursor is 4 times higher than the target height, this return 4
        float cursorLocalHeight = GetCursorHeightAboveBaseline();
        float targetLocalHeight = ComputeYDelta(meterTopPosition, meterBottomPosition) * targetValue;
        return cursorLocalHeight / targetLocalHeight;
    }

    public float GetCursorDeviationFromTarget()
    {
        // returns the number of local units above or below the target the cursor's current y-axis position is
        // if the cursor is right on target, this returns 0
        // if the cursor is 10 units above the target, this returns 10
        // if the cursor is 43 units below the target, this returns -43
        float cursorLocalHeight = GetCursorHeightAboveBaseline();
        float targetLocalHeight = ComputeYDelta(meterTopPosition, meterBottomPosition) * targetValue;
        return (cursorLocalHeight - targetLocalHeight);
    }

    public float GetCursorAbsoluteDeviationFromTarget()
    {
        // returns the number of local units off from target the cursor's current y-axis position is
        // if the cursor is right on targey, this returns 0
        // if the cursor is 10 units above the target, this returns 10
        // if the cursor is 43 units below the target, this returns 43
        float cursorLocalHeight = GetCursorHeightAboveBaseline();
        float targetLocalHeight = ComputeYDelta(meterTopPosition, meterBottomPosition) * targetValue;
        return Mathf.Abs(cursorLocalHeight - targetLocalHeight);
    }

    public float GetCursorPercentageOfDeviationFromTarget()
    {
        // returns a percentage indicating how far above or below the cursor's current y-axis position is from the target value
        // if the cursor is perfectly on target, this returns 0
        // if the cursor is 20% lower than the target, this returns -.2
        // if the cursor is 37% higher than the target, this returns .37
        return GetCursorDeviationFromTarget() / (ComputeYDelta(meterTopPosition, meterBottomPosition) * targetValue);
    }

    public float GetCursorAbsolutePercentageOfDeviationFromTarget()
    {
        // returns a percentage indicating how far off the cursor's current y-axis position is from the target value
        // if the cursor is perfectly on target, this returns 0
        // if the cursor is 20% lower than the target, this returns .2
        // if the cursor is 37% higher than the target, this returns .37
        return GetCursorAbsoluteDeviationFromTarget() / (ComputeYDelta(meterTopPosition, meterBottomPosition) * targetValue);
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
