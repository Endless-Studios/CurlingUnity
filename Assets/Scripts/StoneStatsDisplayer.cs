using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StoneStatsDisplayer : MonoBehaviour
{
    public GameObject stoneObject;
    public GameObject statDisplayObject;

    private Stone stone;
    private Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = statDisplayObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (stone != null)
        {
            text.text = FormatStatsString();
        }
    }

    string FormatStatsString()
    {
        string statsString = "Spin Direction: ";
        if (stone.RotationDirection > 0)
        {
            statsString += "inner" + Environment.NewLine;
        }
        else
        {
            statsString += "outer" + Environment.NewLine;
        }
        statsString += "Velocity: " + System.Math.Round(Mathf.Abs(stone.InstantaneousVelocity), 2) + Environment.NewLine;
        statsString += "Stone Center Position: " + System.Math.Round(stone.Position.x, 2) + ", " + System.Math.Round(stone.Position.y, 2) + ", " + System.Math.Round(stone.Position.z, 2) + Environment.NewLine;
        statsString += "Target Position: " + System.Math.Round(stone.TargetPosition.x, 2) + ", " + System.Math.Round(stone.TargetPosition.y, 2) + ", " + System.Math.Round(stone.TargetPosition.z, 2) + Environment.NewLine;
        statsString += "Closest Pont to Target: " + System.Math.Round(stone.ClosestPointToTarget.x, 2) + ", " + System.Math.Round(stone.ClosestPointToTarget.y, 2) + ", " + System.Math.Round(stone.ClosestPointToTarget.z, 2) + Environment.NewLine;
        statsString += "Distance from target: " + System.Math.Round(stone.DistanceFromTarget, 2) + Environment.NewLine;
        statsString += "Deviation from aimed course: " + System.Math.Round(stone.TotalDeflection, 2);
        return statsString;
    }

    public void TrackStone(GameObject stoneToTrack)
    {
        stoneObject = stoneToTrack;
        stone = stoneObject.GetComponent<Stone>();
    }

    public void DetachStone()
    {
        stoneObject = null;
        stone = null;
    }

    public void OverrideDisplayText(string textToDisplay)
    {
        text.text = textToDisplay;
    }

    public void ToggleStoneTrails()
    {
        TrailRenderer[] allTrails = FindObjectsOfType<TrailRenderer>();
        foreach (TrailRenderer item in allTrails)
        {
            item.enabled = !item.enabled;
        }
    }
}
