using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StoneColor
{
	Yellow,
	Red
}

public class Stone : MonoBehaviour {

	public StoneColor stoneColor = StoneColor.Yellow;
	public float freezeMotionAfterSeconds = .2f;
	public float freezeMotionVelocity = .1f;
	public AnimationCurve CurlRatio;
	public AnimationCurve[] CurlRatios;
	private int directionOfRotation = 1;

	private Rigidbody stoneRB;
	private float[] weightTable;
	private int minRotations = 2;
	private int maxRotations = 3;
	private float hogToHogSecondsForRotation = 10f;
	private bool isInMotionFreezeTest = false;
	private float distanceSinceLaunch = 0f;
	private GameObject targetRing;
	private Vector3 launchPosition;
	private Vector3 launchForce;

	public float InstantaneousVelocity
    {
		get
        {
			return stoneRB.velocity.z;
        }
    }

	public int RotationDirection
    {
        get
        {
			return directionOfRotation;
        }
    }

	public Vector3 Position
    {
        get
        {
			MeshCollider curColider = GetComponentInChildren<MeshCollider>();
			return curColider.transform.position;
        }
    }

	public Vector3 TargetPosition
    {
		get
        {
			Vector3 targetWorldspace = targetRing.transform.position;
			return new Vector3(targetWorldspace.x, targetWorldspace.y, targetWorldspace.z);
        }
    }

	public Vector3 ClosestPointToTarget
    {
        get
        {
			MeshCollider stoneCollider = this.GetComponentInChildren<MeshCollider>();
			return stoneCollider.ClosestPoint(TargetPosition);
		}
    }

	public float DistanceFromTarget
	{
		get
		{
			return Vector3.Distance(ClosestPointToTarget, TargetPosition);
		}
	}

	// Use this for initialization
	void Awake () 
	{
		stoneRB = this.GetComponentInChildren<Rigidbody> ();
		PopulateWeightTable ();
		targetRing = GameObject.FindGameObjectWithTag("CenterRing");
		SetStoneColorVisuals();
	}

	public void SetStoneColor(StoneColor desiredColor)
    {
		stoneColor = desiredColor;
		SetStoneColorVisuals();
    }

	private void SetStoneColorVisuals()
    {
		TrailRenderer stoneTrailRenderer = GetComponentInChildren<TrailRenderer>();
		MeshRenderer[] handleCandidates = this.gameObject.GetComponentsInChildren<MeshRenderer>();
		MeshRenderer handle = null;
        foreach (MeshRenderer item in handleCandidates)
        {
            if (item.gameObject.CompareTag("StoneHandle"))
            {
				handle = item;
				break;
            }
        }
		Color targetColor = Color.yellow;
        if (stoneColor == StoneColor.Red)
        {
			targetColor = Color.red;
        }
		stoneTrailRenderer.startColor = targetColor;
		stoneTrailRenderer.endColor = targetColor;
		handle.materials[3].color = targetColor;
    }

	private void PopulateWeightTable()
	{
		weightTable = new float[15];
		weightTable [0] = 1.73f; // hog
		weightTable [1] = 1.76f; // high guard
		weightTable [2] = 1.8f; // low guard
		weightTable [3] = 1.86f; // 12 ft
		weightTable [4] = 1.88f; // 10ft circle
		weightTable [5] = 1.9f; // 8 ft
		weightTable [6] = 1.91f; // 6 ft
		weightTable [7] = 1.92f;  // 4 ft
		weightTable [8] = 1.94f; // tee / button
		weightTable [9] = 1.96f; // tee + 4
		weightTable [10] = 1.98f; // tee + 8
		weightTable [11] = 2.04f; // hack
		weightTable [12] = 2.1f; // board
		weightTable [13] = 3f; // take out / normal
		weightTable [14] = 5f; // peel


	}
	
	private void LaunchInstant(Vector3 instantForce, int rotationDirection = 1)
	{
		launchPosition = this.transform.position;
		launchForce = instantForce;
		stoneRB.velocity = Vector3.zero;
		directionOfRotation = rotationDirection;
		stoneRB.AddForce (instantForce, ForceMode.VelocityChange);
		StopCoroutine ("RotateStone");
		StartCoroutine ("RotateStone");
		StopCoroutine ("CurlStone");
		StartCoroutine ("CurlStone");
	}

	public void Launch(Vector3 launchForce, int rotationDirection = 1)
	{
		LaunchInstant (launchForce, rotationDirection);
	}

	public void Launch(float forceMPS, Vector3 direction, int rotationDirection = 1)
	{
		LaunchInstant (direction * forceMPS, rotationDirection);
	}

	public void Launch(int weightIndex, Vector3 direction, int rotationDirection = 1)
	{
		LaunchInstant (direction * weightTable [weightIndex], rotationDirection);
	}

	public float TotalDeflection
    {
        get
        {
			Vector3 normalizedCourse = launchForce.normalized;
			Vector3 lhs = this.Position - launchPosition;
			float dotP = Vector3.Dot(lhs, normalizedCourse);
			Vector3 nearestPoint = launchPosition + normalizedCourse * dotP;
			return Vector3.Distance(this.Position, nearestPoint);
        }
    }

	void FixedUpdate()
	{
		if (!stoneIsMoving() && !isInMotionFreezeTest)
		{
			isInMotionFreezeTest = true;
			StopCoroutine ("MotionFreezeTest");
			StartCoroutine ("MotionFreezeTest");
		}
		if (stoneIsMoving() && isInMotionFreezeTest) {
			StopCoroutine ("MotionFreezeTest");
		}
	}


	private bool stoneIsMoving()
	{
		if (Mathf.Abs(stoneRB.velocity.z) < freezeMotionVelocity) {
			return false;	
		} else
		{
			return true;
		}
	}

	IEnumerator MotionFreezeTest()
	{
		yield return new WaitForSeconds (freezeMotionAfterSeconds);
		if (!stoneIsMoving()) 
		{
			stoneRB.velocity = Vector3.zero;
			isInMotionFreezeTest = false;
		}
	}

	IEnumerator RotateStone()
	{
		Quaternion fromAngle = stoneRB.gameObject.transform.rotation;
		Vector3 randomRotation = new Vector3(0f, Random.Range (360 * minRotations, 360 * maxRotations), 0f) * directionOfRotation;
		bool isFirstTime = true;
		do 
		{
			Quaternion toAngle = Quaternion.Euler (randomRotation / (hogToHogSecondsForRotation / Time.deltaTime));
			stoneRB.gameObject.transform.rotation = stoneRB.gameObject.transform.rotation * toAngle;
			if (isFirstTime)
			{
				yield return new WaitForSeconds(freezeMotionAfterSeconds);
				isFirstTime = false;
			} else {
			yield return new WaitForEndOfFrame();
			}
		} while (stoneIsMoving());

		//TODO: When stone falls below minimum velocity, slow rotation to a stop, don't just kill it.
	}

	IEnumerator CurlStone()
	{
		float instantaneousVelocity;
		float instantaneousCurlRatio;
		Vector3 deflection;
		yield return new WaitForSeconds (freezeMotionAfterSeconds);
		do 
		{
			instantaneousVelocity = Mathf.Abs(stoneRB.velocity.z);
			instantaneousCurlRatio = CurlRatio.Evaluate(instantaneousVelocity);
			deflection = new Vector3(-instantaneousCurlRatio * Time.deltaTime * instantaneousVelocity, 0f, 0f) * directionOfRotation;
			//Adding the force rather than relative force is probably going to break stuff but for some reason relative force doesn't cause anything to happen. The Iternet guesses it's because of surface friction.
			stoneRB.AddForce(deflection, ForceMode.VelocityChange);
			yield return new WaitForEndOfFrame();
		} while (stoneIsMoving());
	}
		
}
