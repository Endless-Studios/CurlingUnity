using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public UnityEngine.UI.Text RedScoreLabel;
    public UnityEngine.UI.Text YellowScoreLabel;
    public UnityEngine.UI.Text RedStonesInPlayLabel;
    public UnityEngine.UI.Text YellowStonesInPlayLabel;
    
    private List<Stone> redScoringStones;
    private List<Stone> yellowScoringStones;
    private float closestRedDistance;
    private float closestYellowDistance;
    private int redScore;
    private int yellowScore;
    private int redStonesInPlay;
    private int yellowStonesInPlay;

    public int RedScore
    {
        get
        {
            return redScore;
        }
    }

    public int YellowScore
    {
        get
        {
            return yellowScore;
        }
    }

    public int RedStonesInPlay
    {
        get
        {
            return redStonesInPlay;
        }
    }

    public int YellowStonesInPlay
    {
        get
        {
            return yellowStonesInPlay;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void LateUpdate()
    {
        ComputeScore();
        if (RedScoreLabel != null)
        {
            RedScoreLabel.text = redScore.ToString();
        }
        if (YellowScoreLabel != null)
        {
            YellowScoreLabel.text = yellowScore.ToString();
        }
        if (RedStonesInPlayLabel != null)
        {
            RedStonesInPlayLabel.text = redStonesInPlay.ToString();
        }
        if (YellowStonesInPlayLabel != null)
        {
            YellowStonesInPlayLabel.text = yellowStonesInPlay.ToString();
        }
    }

    private void ComputeScore()
    {
        closestRedDistance = 999f;
        closestYellowDistance = 99f;
        redScore = 0;
        yellowScore = 0;
        redStonesInPlay = 0;
        yellowStonesInPlay = 0;
        redScoringStones = new List<Stone>();
        yellowScoringStones = new List<Stone>();

        GameObject[] stoneObjects = GameObject.FindGameObjectsWithTag("Stone");
        Stone curStone;
        foreach (GameObject item in stoneObjects)
        {
            curStone = item.GetComponent<Stone>();
            if (curStone.stoneColor == StoneColor.Red)
            {
                redStonesInPlay++;
            }
            if (curStone.stoneColor == StoneColor.Yellow)
            {
                yellowStonesInPlay++;
            }
            if (curStone.stoneColor == StoneColor.Red && curStone.DistanceFromTarget <= 1.8288f)
            {
                redScoringStones.Add(curStone);
            }
            if (curStone.stoneColor == StoneColor.Yellow && curStone.DistanceFromTarget <= 1.8288f)
            {
                yellowScoringStones.Add(curStone);
            }
        }

        foreach (Stone item in redScoringStones)
        {
            if (item.DistanceFromTarget < closestRedDistance)
            {
                closestRedDistance = item.DistanceFromTarget;
            }
        }

        foreach (Stone item in yellowScoringStones)
        {
            if (item.DistanceFromTarget < closestYellowDistance)
            {
                closestYellowDistance = item.DistanceFromTarget;
            }
        }

        foreach (Stone item in redScoringStones)
        {
            if (item.DistanceFromTarget < closestYellowDistance)
            {
                redScore++;
            }
        }

        foreach (Stone item in yellowScoringStones)
        {
            if (item.DistanceFromTarget < closestRedDistance)
            {
                yellowScore++;
            }
        }
    }
}
