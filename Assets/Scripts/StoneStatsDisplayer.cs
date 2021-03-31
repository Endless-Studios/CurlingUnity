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
        statsString += "Velocity: " + Mathf.Abs(stone.InstantaneousVelocity) + Environment.NewLine;
        statsString += "Stone Center Position: " + stone.Position.x + ", " + stone.Position.y + ", " + stone.Position.z + Environment.NewLine;
        statsString += "Target Position: " + stone.TargetPosition.x + ", " + stone.TargetPosition.y + ", " + stone.TargetPosition.z + Environment.NewLine;
        statsString += "Closest Pont to Target: " + stone.ClosestPointToTarget.x + ", " + stone.ClosestPointToTarget.y + ", " + stone.ClosestPointToTarget.z + Environment.NewLine;
        statsString += "Distance from target: " + stone.DistanceFromTarget;
        return statsString;
    }

    public void TrackStone(GameObject stoneToTrack)
    {
        stoneObject = stoneToTrack;
        stone = stoneObject.GetComponent<Stone>();
    }

    public void OverrideDisplayText(string textToDisplay)
    {
        text.text = textToDisplay;
    }

}
