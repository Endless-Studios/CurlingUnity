using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ThirdPersonPlayerController : MonoBehaviour {

    public float accelerationFactor = 70f;
	public float runThreshhold = 2.5f;
	public float maxSpeed = 5f;
	public float rotationalSensitivity = 30f;
	public float highSensitivityScalingFactor = .5f;
	public float percentTopSpeedCrouched = .7f;
	public float autoTurnTime = 1f;
	public float horizontalAimClamping = 3.814f;
	public float fullSweepCPS = 6f;
	public AnimationCurve throwWeightAccuracyCurve;
	public Camera camera;
	public GameObject curlingStoneObject;
	public GameObject virtualStoneObject;
	public GameObject virtualStoneLine;
	public GameObject virtualLineSourceObject;
    public GameObject hogCamera;
	public StoneColor nextStoneColor = StoneColor.Yellow;

	private Actions actions;
    private Rigidbody rb;
	private Vector3 cameraPositionOffset;
	private bool crouched = false;
	private float moveSpeed;
	private float maxAllowedSpeed;
	private MouseLook cameraScript;
	private LineRenderer virtualStoneLineRenderer;
	private bool stoneConjuringMode = false;
    private StoneFollowCam stoneCam;
	private StoneStatsDisplayer statsDisplayer;
	private GameObject debugUI;
	private GameObject crouchUI;
	private GameObject summoningUI;
	private GameObject stoneStatsUI;
	private int spinDirection = 1;
	private Vector3 launchPosition;
	private Quaternion launchRotation;
	private bool inAutomove = false;
	private AccuracyMeter accuracyMeter;
	private bool inRealThrow = false;
	private UnityEngine.UI.Text statusDisplay;
	private List<KeyCode> weightKeyCodes;
	private float sweepRate = 0f;

	// Use this for initialization
	void Start () {
        actions = GetComponent<Actions>();
        rb = GetComponent<Rigidbody>();
		cameraPositionOffset = new Vector3 (0, 1f, -.7f);
		moveSpeed = accelerationFactor;
		maxAllowedSpeed = runThreshhold;
		virtualStoneObject.GetComponentInChildren<MeshRenderer>().enabled = false;
		cameraScript = camera.GetComponent<MouseLook> ();
		virtualStoneLineRenderer = virtualStoneLine.GetComponent<LineRenderer> ();
		virtualStoneLineRenderer.enabled = false;
        stoneCam = hogCamera.GetComponent<StoneFollowCam>();
		statsDisplayer = FindObjectOfType<StoneStatsDisplayer>();
		debugUI = GameObject.FindGameObjectWithTag("DebugUI");
		crouchUI = GameObject.FindGameObjectWithTag("CrouchControls");
		summoningUI = GameObject.FindGameObjectWithTag("SummoningControls");
		stoneStatsUI = GameObject.FindGameObjectWithTag("StoneStatsControls");
		debugUI.SetActive(false);
		crouchUI.SetActive(false);
		summoningUI.SetActive(false);
		stoneStatsUI.SetActive(false);
		launchPosition = this.gameObject.transform.position;
		launchRotation = this.gameObject.transform.rotation;
		accuracyMeter = GameObject.FindGameObjectWithTag("AccuracyMeter").GetComponent<AccuracyMeter>();
		statusDisplay = GameObject.FindGameObjectWithTag("StatusDisplay").GetComponent<UnityEngine.UI.Text>();
		statusDisplay.enabled = false;
		weightKeyCodes = new List<KeyCode>();
		PopulateWeightKeyCodes();
	}

	private void PopulateWeightKeyCodes()
    {
		weightKeyCodes.Add(KeyCode.Alpha1);
		weightKeyCodes.Add(KeyCode.Alpha2);
		weightKeyCodes.Add(KeyCode.Alpha3);
		weightKeyCodes.Add(KeyCode.Alpha4);
		weightKeyCodes.Add(KeyCode.Alpha5);
		weightKeyCodes.Add(KeyCode.Alpha6);
		weightKeyCodes.Add(KeyCode.Alpha7);
		weightKeyCodes.Add(KeyCode.Alpha8);
		weightKeyCodes.Add(KeyCode.Alpha9);
		weightKeyCodes.Add(KeyCode.Alpha0);
		weightKeyCodes.Add(KeyCode.Minus);
		weightKeyCodes.Add(KeyCode.H);
		weightKeyCodes.Add(KeyCode.B);
		weightKeyCodes.Add(KeyCode.T);
		weightKeyCodes.Add(KeyCode.P);
	}

	private KeyCode GetWeightKeycode()
    {
        foreach (KeyCode item in weightKeyCodes)
        {
            if (Input.GetKey(item))
            {
				return item;
            }
        }
		return KeyCode.None;
    }

	private IEnumerator RunToLaunchPosition()
    {
		inAutomove = true;
		Vector3 startPosition = this.gameObject.transform.position;
		Vector3 relativePosition = -(this.gameObject.transform.position - launchPosition);
		Quaternion startRotation = this.gameObject.transform.rotation;
		Quaternion walkToRotation = Quaternion.LookRotation(relativePosition);
		float distanceToLaunch = Vector3.Distance(this.gameObject.transform.position, launchPosition);
		float timeToTraverse = distanceToLaunch / maxAllowedSpeed;
		float elapsedTime = 0f;
		Vector3 newPosition;
		Quaternion newRotation;
		//actions.Walk();
        while (elapsedTime < autoTurnTime)
        {
			elapsedTime += Time.deltaTime;
			newRotation = Quaternion.Lerp(startRotation, walkToRotation, elapsedTime / autoTurnTime);
			rb.MoveRotation(newRotation);
			yield return new WaitForEndOfFrame();
        }
		elapsedTime = 0f;
		//actions.Run();
		while (elapsedTime < timeToTraverse)
        {
			elapsedTime += Time.deltaTime;
			newPosition = Vector3.Lerp(startPosition, launchPosition, elapsedTime / timeToTraverse);
			newPosition = new Vector3(newPosition.x, this.gameObject.transform.position.y, newPosition.z);
			rb.MovePosition(newPosition);
			yield return new WaitForEndOfFrame();
        }
		elapsedTime = 0f;
		startRotation = this.transform.rotation;
		//actions.Walk();
		while (elapsedTime < autoTurnTime)
        {
			elapsedTime += Time.deltaTime;
			newRotation = Quaternion.Lerp(startRotation, launchRotation, elapsedTime / autoTurnTime);
			rb.MoveRotation(newRotation);
			yield return new WaitForEndOfFrame();
		}
		//actions.Stay();
		inAutomove = false;
    }

	private void DisplayStatus(string textToDisplay)
    {
		statusDisplay.text = textToDisplay;
		statusDisplay.enabled = true;
    }

	void RenderDirectionLine() {

		RaycastHit hit;
		if (Physics.Raycast(virtualLineSourceObject.transform.position, virtualLineSourceObject.transform.forward, out hit)) {
			virtualStoneLineRenderer.positionCount = 2;
			virtualStoneLineRenderer.SetPosition (0, virtualLineSourceObject.transform.position);
			virtualStoneLineRenderer.SetPosition (1, hit.point);
		}
	}

	void PositionCamera()
	{
		
		camera.transform.localPosition = cameraPositionOffset;
		Quaternion oldCameraRotation = camera.transform.localRotation;
		camera.transform.localRotation = oldCameraRotation;
	}

	void ProcessTimescale()
    {
        switch (Time.timeScale)
        {
			case 1f:
				Time.timeScale = 2f;
				break;
			case 2f:
				Time.timeScale = 4f;
				break;
			case 4f:
				Time.timeScale = 1f;
				break;
			default:
                break;
        }
    }

	void ManageFreeLook()
	{
		if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)) {
			if (!cameraScript.freeLook) {
				cameraScript.StartFreeLook ();
				//camera.transform.LookAt (this.gameObject.transform.position);
			}
		}

		if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt)) {
			cameraScript.StopFreeLook ();
		}
	}

	private void OverrideNextStoneDisplayText()
    {
		string overrideText;
		statsDisplayer.DetachStone();
		overrideText = "Stone Color: " + nextStoneColor.ToString() + System.Environment.NewLine;
		if (spinDirection > 0)
		{
			overrideText += "Curl Direction: right";
		}
		else
		{
			overrideText += "Curl Direction: left";
		}
		overrideText += System.Environment.NewLine;
		overrideText += "Timescale: " + System.Math.Round(Time.timeScale, 0) + "x";
		statsDisplayer.OverrideDisplayText(overrideText);
	}

	private float getWeightByKeycode(KeyCode weightKeyCode)
    {
		Stone referenceStone = GameObject.Instantiate(curlingStoneObject.GetComponent<Stone>());
		float[] weightTable = referenceStone.WeightTable;
		float desiredWeight = 0f;

		if (weightKeyCode == KeyCode.Alpha1)
		{
			desiredWeight = weightTable[0];
		}
		if (weightKeyCode == KeyCode.Alpha2)
		{
			desiredWeight = weightTable[1];
		}
		if (weightKeyCode == KeyCode.Alpha3)
		{
			desiredWeight = weightTable[2];
		}
		if (weightKeyCode == KeyCode.Alpha4)
		{
			desiredWeight = weightTable[3];
		}
		if (weightKeyCode == KeyCode.Alpha5)
		{
			desiredWeight = weightTable[4];
		}
		if (weightKeyCode == KeyCode.Alpha6)
		{
			desiredWeight = weightTable[5];
		}
		if (weightKeyCode == KeyCode.Alpha7)
		{
			desiredWeight = weightTable[6];
		}
		if (weightKeyCode == KeyCode.Alpha8)
		{
			desiredWeight = weightTable[7];
		}
		if (weightKeyCode == KeyCode.Alpha9)
		{
			desiredWeight = weightTable[8];
		}
		if (weightKeyCode == KeyCode.Alpha0)
		{
			desiredWeight = weightTable[9];
		}
		if (weightKeyCode == KeyCode.Minus)
		{
			desiredWeight = weightTable[10];
		}
		if (weightKeyCode == KeyCode.H)
		{
			desiredWeight = weightTable[11];
		}
		if (weightKeyCode == KeyCode.B)
		{
			desiredWeight = weightTable[12];
		}
		if (weightKeyCode == KeyCode.T)
		{
			desiredWeight = weightTable[13];
		}
		if (weightKeyCode == KeyCode.P)
		{
			desiredWeight = weightTable[14];
		}
		GameObject.Destroy(referenceStone.gameObject);
		return desiredWeight;

    }

	private IEnumerator ProcessRealThrow()
    {
		inRealThrow = true;
		inAutomove = true;
		crouched = true;
		float intendedWeight = 0f;
		bool isWeighted = false;
		bool isWeightAcc = false;
		bool isAimAcc = false;
		DisplayStatus("Select shot weight");
        yield return new WaitForEndOfFrame();
        while (!isWeighted)
        {
			KeyCode returnedCode = GetWeightKeycode();
			if (inRealThrow && returnedCode != KeyCode.None)
            {
				intendedWeight = getWeightByKeycode(returnedCode);
				DisplayStatus("Intended weight is " + intendedWeight.ToString() + ". Click to set weight accuracy.");
				isWeighted = true;
            }
            else
            {
				yield return new WaitForEndOfFrame();
            }
        }
		accuracyMeter.SetTargetValue(.5f);
		accuracyMeter.StartCursorCycle();
		float weightAccuracyMultiplier = 1f;
		yield return new WaitForEndOfFrame();
        while (!isWeightAcc)
        {
            if (Input.GetMouseButton(0) && inRealThrow)
            {
				accuracyMeter.StopCursorCycle();
				yield return new WaitForEndOfFrame();
				float cursorAccuracy = accuracyMeter.GetCursorPercentageOfTarget();
				weightAccuracyMultiplier = throwWeightAccuracyCurve.Evaluate(cursorAccuracy);
				intendedWeight *= weightAccuracyMultiplier;
                DisplayStatus("Raw weight accuracy was " + System.Math.Round(cursorAccuracy, 2).ToString() + ". Adjusted weight accuracy was " + System.Math.Round(weightAccuracyMultiplier, 3).ToString() + ". Click to set aim accuracy.");
				yield return new WaitForSeconds(1f);
				isWeightAcc = true;
            }
			else
            {
				yield return new WaitForEndOfFrame();
            }
        }
		accuracyMeter.StartCursorCycle();
		float aimAccuracyMultiplier = 0f;
		yield return new WaitForEndOfFrame();
		Quaternion deviatedAim = new Quaternion();
		while (!isAimAcc)
        {
			if (Input.GetMouseButton(0) && inRealThrow)
			{
				accuracyMeter.StopCursorCycle();
				yield return new WaitForEndOfFrame();
				aimAccuracyMultiplier = accuracyMeter.GetCursorPercentageOfDeviationFromTarget();
				deviatedAim = CalculateDeviatedAimRotation(aimAccuracyMultiplier);
				DisplayStatus("Aim deviation was " + aimAccuracyMultiplier.ToString() + ". Launching!");
				yield return new WaitForSeconds(1f);
				isAimAcc = true;
			}
			else
			{
				yield return new WaitForEndOfFrame();
			}
		}
		Vector3 deviatedLaunchVector = deviatedAim * this.transform.forward;
		LaunchStoneByRawWeight(intendedWeight, deviatedLaunchVector);
		inAutomove = false;
		inRealThrow = false;
    }

	private Quaternion CalculateDeviatedAimRotation(float aimDeviation)
    {
		Quaternion currentAim = this.transform.rotation;
		aimDeviation *= horizontalAimClamping;
		Quaternion newAim = new Quaternion();
		newAim = Quaternion.Euler(0, aimDeviation, 0);
		return newAim;
    }

	private void LaunchStoneByRawWeight(float rawLaunchWeight, Vector3 launchDirection)
    {
		GameObject droppedStone = DropStone();
		droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
		statsDisplayer.TrackStone(droppedStone);
		droppedStone.GetComponent<Stone>().Launch(rawLaunchWeight, launchDirection, spinDirection);
		StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
		StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
	}

	void ProcessConjuring()
	{
		if (Input.GetKeyDown (KeyCode.Space) && crouched) {

			stoneConjuringMode = !stoneConjuringMode;
			summoningUI.SetActive(!summoningUI.activeSelf);
			stoneStatsUI.SetActive(!stoneStatsUI.activeSelf);
			rb.velocity = Vector3.zero;
            if (stoneConjuringMode)
            {
                //Debug.Log("Conjuring set camera to hog");
                stoneCam.ReturnCameraToHog();
				
				OverrideNextStoneDisplayText();
			}

		}

		if (stoneConjuringMode) {
			virtualStoneObject.GetComponentInChildren<MeshRenderer> ().enabled = true;
			virtualStoneLineRenderer.enabled = true;
			RenderDirectionLine ();

            if (Input.GetKeyDown(KeyCode.O))
            {
                if (nextStoneColor == StoneColor.Red)
                {
					nextStoneColor = StoneColor.Yellow;
                }
				else
                {
					nextStoneColor = StoneColor.Red;
                }
				OverrideNextStoneDisplayText();
            }
            
			if (Input.GetKeyDown(KeyCode.I)) {
				spinDirection = -1 * spinDirection;
				OverrideNextStoneDisplayText();
			}

			if (Input.GetKeyDown (KeyCode.Period)) {
				GameObject droppedStone = DropStone ();
				droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
				statsDisplayer.TrackStone(droppedStone);
			}

            if (Input.GetKeyDown(KeyCode.Delete))
            {
				DestroyAllStones();
            }

            if (Input.GetKeyDown(KeyCode.Quote))
            {
				ProcessTimescale();
				OverrideNextStoneDisplayText();
            }

            if (Input.GetKeyDown(KeyCode.Z) && !inRealThrow)
            {
				StopCoroutine("ProcesRealThrow");
				inRealThrow = true;
				StartCoroutine("ProcessRealThrow");
            }

			if (!inRealThrow && GetWeightKeycode() != KeyCode.None)
			{
				if (Input.GetKeyDown(KeyCode.Alpha1))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(0, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				}

				if (Input.GetKeyDown(KeyCode.Alpha2))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(1, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				}

				if (Input.GetKeyDown(KeyCode.Alpha3))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(2, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				}

				if (Input.GetKeyDown(KeyCode.Alpha4))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(3, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				}

				if (Input.GetKeyDown(KeyCode.Alpha5))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(4, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				}

				if (Input.GetKeyDown(KeyCode.Alpha6))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(5, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				}

				if (Input.GetKeyDown(KeyCode.Alpha7))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(6, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				}

				if (Input.GetKeyDown(KeyCode.Alpha8))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(7, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				}

				if (Input.GetKeyDown(KeyCode.Alpha9))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(8, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StopCoroutine("TrackSweepRate");
					StartCoroutine("TrackSweepRate");
				}

				if (Input.GetKeyDown(KeyCode.Alpha0))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(9, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				}

				if (Input.GetKeyDown(KeyCode.Minus))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(10, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				}

				if (Input.GetKeyDown(KeyCode.H))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(11, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				}

				if (Input.GetKeyDown(KeyCode.B))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(12, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				}

				if (Input.GetKeyDown(KeyCode.T))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(13, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				}

				if (Input.GetKeyDown(KeyCode.P))
				{
					GameObject droppedStone = DropStone();
					droppedStone.GetComponent<Stone>().SetStoneColor(nextStoneColor);
					statsDisplayer.TrackStone(droppedStone);
					droppedStone.GetComponent<Stone>().Launch(14, this.gameObject.transform.forward, spinDirection);
					StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
					StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				}
			}

		} else 
		{
			virtualStoneObject.GetComponentInChildren<MeshRenderer> ().enabled = false;
			virtualStoneLineRenderer.enabled = false;
		}
	}

	void ProcessMoveAnimations()
	{
		if (((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) & !inAutomove) || (inAutomove & !inRealThrow)) {

			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
				actions.Run();
				maxAllowedSpeed = maxSpeed;
			} else {
				actions.Walk();
				maxAllowedSpeed = runThreshhold;
			}
		}
	}

	void LateUpdate()
	{
		if (!cameraScript.freeLook) {
			PositionCamera ();
		} 

		ManageFreeLook ();	
	}

	void Update() {

        //Handle displaying debug UI
        if (Input.GetKeyDown(KeyCode.F1))
        {
			debugUI.SetActive(!debugUI.activeSelf);
        }

		//Handle crouching
		if (Input.GetKeyDown(KeyCode.C) && !cameraScript.freeLook && !inAutomove) {
			actions.Sitting ();
			crouched = !crouched;
			crouchUI.SetActive(!crouchUI.activeSelf);
			if (crouched) {
				moveSpeed = accelerationFactor * percentTopSpeedCrouched;
			} else {
				moveSpeed = accelerationFactor;
			}
			return;
		}

		//Handle return to launch
		if (Input.GetKeyDown(KeyCode.L) && !inAutomove)
		{
			float curDistanceToLaunch = Mathf.Abs(Vector3.Distance(this.transform.position, launchPosition));
			if (curDistanceToLaunch > .5f)
			{
				StopCoroutine("RunToLaunchPosition");
				StartCoroutine("RunToLaunchPosition");
			}
			else
            {
				rb.MoveRotation(launchRotation);
				rb.MovePosition(launchPosition);
            }
		}


		//Zero out animation to idle
		actions.Stay ();
		ProcessConjuring ();
		ProcessMoveAnimations ();

	}


	void FixedUpdate()
	{

		if (!cameraScript.freeLook && !inAutomove) {
			//Avatar facing direction control
			float sensitivityScale = 1f;
			if (Input.GetKey(KeyCode.LeftControl))
            {
				sensitivityScale *= highSensitivityScalingFactor;
            }
			rb.AddTorque (0, Input.GetAxis ("Mouse X") * rotationalSensitivity * sensitivityScale, 0);
            if (Input.GetKey(KeyCode.RightArrow))
            {
				rb.AddTorque(0, 10, 0);
            }
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				rb.AddTorque(0, -10, 0);
			}
		}



		//Process WASD movement
		if (Input.GetKey(KeyCode.W) && !cameraScript.freeLook && !inAutomove) {
			//actions.Walk ();
			rb.AddForce (this.gameObject.transform.forward * moveSpeed);
		}

		if (Input.GetKey(KeyCode.S) && !cameraScript.freeLook && !inAutomove) {
			//actions.Walk ();
			rb.AddForce (-this.gameObject.transform.forward * moveSpeed);
		}

		if (Input.GetKey(KeyCode.A) && !cameraScript.freeLook && !inAutomove) {
			//actions.Walk ();
			rb.AddForce (-this.gameObject.transform.right * moveSpeed);
		}

		if (Input.GetKey(KeyCode.D) && !cameraScript.freeLook && !inAutomove) {
			//actions.Walk ();
			rb.AddForce (this.gameObject.transform.right * moveSpeed);
		}

		//Enforce appropriate speed limit for running or walking
		//TODO: Handle case of releasing shift and decelerating from a run gradually rather than instantaneously dropping to walk speed
		if (rb.velocity.x > maxAllowedSpeed && !inAutomove) {
			rb.velocity = new Vector3 (maxAllowedSpeed, rb.velocity.y, rb.velocity.z);
		}

		if (rb.velocity.x < -maxAllowedSpeed && !inAutomove) {
			rb.velocity = new Vector3 (-maxAllowedSpeed, rb.velocity.y, rb.velocity.z);
		}

		if (rb.velocity.z > maxAllowedSpeed && !inAutomove) {
			rb.velocity = new Vector3 (rb.velocity.x, rb.velocity.y, maxAllowedSpeed);
		}

		if (rb.velocity.z < -maxAllowedSpeed && !inAutomove) {
			rb.velocity = new Vector3 (rb.velocity.x, rb.velocity.y, -maxAllowedSpeed);
		}

	}

	private GameObject DropStone()
	{
		return Instantiate (curlingStoneObject, virtualStoneObject.transform.position, this.gameObject.transform.rotation);
	}

	private void DestroyAllStones()
    {
		GameObject[] AllStones = GameObject.FindGameObjectsWithTag("Stone");
        foreach (GameObject item in AllStones)
        {
			GameObject.Destroy(item);
        }
    }

	private IEnumerator TrackSweepRate()
    {
		float t1 = 0f;
		float t2 = 0f;
		float strokeDuration = 0f;
		bool applySweep = false;
		float applicationTimer = 0f;
		bool awaitingRelease = false;
		Stone activeStone = statsDisplayer.stoneObject.GetComponent<Stone>();
        while (statsDisplayer.stoneObject != null)
        {
            if (Input.GetMouseButton(0))
            {
				if (t2 == 0f)
				{
					if (t1 == 0f)
					{
						t1 = Time.time;
						awaitingRelease = true;
					}
					else
					{
						if (!awaitingRelease)
						{
							t2 = Time.time;
							strokeDuration = t2 - t1;
							sweepRate = Mathf.Clamp(1 / strokeDuration, 0f, fullSweepCPS);
							DisplayStatus("Sweep click rate is " + System.Math.Round(sweepRate, 2).ToString());
							activeStone.SweepRate = sweepRate / fullSweepCPS;
							applySweep = true;
							t1 = 0f;
							t2 = 0f;
						}
					}
				}
			}
            if (applySweep)
            {
				applicationTimer += Time.deltaTime;
                if (applicationTimer <= strokeDuration)
                {
					activeStone.isSwept = true;
                }
				else
                {
					activeStone.isSwept = false;
					applicationTimer = 0f;
					applySweep = false;
                }
            }
			yield return null;
            if (Input.GetMouseButtonUp(0))
            {
				awaitingRelease = false;
            }
		}
	}
}
