using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour {


	public float freezeMotionAfterSeconds = .2f;
	public float freezeMotionVelocity = .1f;
	public AnimationCurve CurlRatio;
	public AnimationCurve[] CurlRatios;

	private Rigidbody stoneRB;
	private float[] weightTable;
	private int minRotations = 2;
	private int maxRotations = 3;
	private float hogToHogSecondsForRotation = 10f;
	private bool isInMotionFreezeTest = false;
	private float distanceSinceLaunch = 0f;
	private int directionOfRotation = 1;


	// Use this for initialization
	void Awake () {
		stoneRB = this.GetComponentInChildren<Rigidbody> ();
		PopulateWeightTable ();
	}

	private void PopulateWeightTable()
	{
		weightTable = new float[13];
		weightTable [0] = 1.73f;
		weightTable [1] = 1.76f;
		weightTable [2] = 1.8f;
		weightTable [3] = 1.86f;
		weightTable [4] = 1.88f;
		weightTable [5] = 1.9f;
		weightTable [6] = 1.91f;
		weightTable [7] = 1.92f;
		weightTable [8] = 1.94f;
		weightTable [9] = 1.96f;
		weightTable [10] = 1.98f;
		weightTable [11] = 2.04f;
		weightTable [12] = 3f;


	}
	
	private void LaunchInstant(Vector3 instantForce, int rotationDirection = 1)
	{
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
