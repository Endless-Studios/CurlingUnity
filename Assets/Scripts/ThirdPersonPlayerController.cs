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
	public Camera camera;
	public GameObject curlingStoneObject;
	public GameObject virtualStoneObject;
	public GameObject virtualStoneLine;
	public GameObject virtualLineSourceObject;
    public GameObject hogCamera;

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

	void ProcessConjuring()
	{
		string overrideText;
		if (Input.GetKeyDown (KeyCode.Space) && crouched) {

			stoneConjuringMode = !stoneConjuringMode;
			summoningUI.SetActive(!summoningUI.activeSelf);
			stoneStatsUI.SetActive(!stoneStatsUI.activeSelf);
            if (stoneConjuringMode)
            {
                //Debug.Log("Conjuring set camera to hog");
                stoneCam.ReturnCameraToHog();
				
				if (spinDirection > 0)
				{
					overrideText = "Spin Direction: inner";
				}
				else
				{
					overrideText = "Spin Direction: outer";
				}
				statsDisplayer.OverrideDisplayText(overrideText);
			}

		}

		if (stoneConjuringMode) {
			virtualStoneObject.GetComponentInChildren<MeshRenderer> ().enabled = true;
			virtualStoneLineRenderer.enabled = true;
			RenderDirectionLine ();
            
			if (Input.GetKeyDown(KeyCode.I)) {
				spinDirection = -1 * spinDirection;
				statsDisplayer.DetachStone();
				if (spinDirection > 0)
				{
					overrideText = "Spin Direction: inner";
				}
				else
				{
					overrideText = "Spin Direction: outer";
				}
				statsDisplayer.OverrideDisplayText(overrideText);
			}

			if (Input.GetKeyDown (KeyCode.Period)) {
				GameObject droppedStone = DropStone ();
				statsDisplayer.TrackStone(droppedStone);
			}

            if (Input.GetKeyDown(KeyCode.Delete))
            {
				DestroyAllStones();
            }

			if (Input.GetKeyDown(KeyCode.Alpha1)) {
				GameObject droppedStone = DropStone ();
				statsDisplayer.TrackStone(droppedStone);
				droppedStone.GetComponent<Stone> ().Launch (0, this.gameObject.transform.forward, spinDirection);
                StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
                StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
			}

			if (Input.GetKeyDown(KeyCode.Alpha2)) {
				GameObject droppedStone = DropStone ();
				statsDisplayer.TrackStone(droppedStone);
				droppedStone.GetComponent<Stone> ().Launch (1, this.gameObject.transform.forward, spinDirection);
                StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
                StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
            }

			if (Input.GetKeyDown(KeyCode.Alpha3)) {
				GameObject droppedStone = DropStone ();
				statsDisplayer.TrackStone(droppedStone);
				droppedStone.GetComponent<Stone> ().Launch (2, this.gameObject.transform.forward, spinDirection);
                StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
                StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
            }

			if (Input.GetKeyDown(KeyCode.Alpha4)) {
				GameObject droppedStone = DropStone ();
				statsDisplayer.TrackStone(droppedStone);
				droppedStone.GetComponent<Stone> ().Launch (3, this.gameObject.transform.forward, spinDirection);
                StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
                StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
            }

			if (Input.GetKeyDown(KeyCode.Alpha5)) {
				GameObject droppedStone = DropStone ();
				statsDisplayer.TrackStone(droppedStone);
				droppedStone.GetComponent<Stone> ().Launch (4, this.gameObject.transform.forward, spinDirection);
                StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
                StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
            }

			if (Input.GetKeyDown(KeyCode.Alpha6)) {
				GameObject droppedStone = DropStone ();
				statsDisplayer.TrackStone(droppedStone);
				droppedStone.GetComponent<Stone> ().Launch (5, this.gameObject.transform.forward, spinDirection);
                StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
                StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
            }

			if (Input.GetKeyDown(KeyCode.Alpha7)) {
				GameObject droppedStone = DropStone ();
				statsDisplayer.TrackStone(droppedStone);
				droppedStone.GetComponent<Stone> ().Launch (6, this.gameObject.transform.forward, spinDirection);
                StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
                StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
            }

			if (Input.GetKeyDown(KeyCode.Alpha8)) {
				GameObject droppedStone = DropStone ();
				statsDisplayer.TrackStone(droppedStone);
				droppedStone.GetComponent<Stone> ().Launch (7, this.gameObject.transform.forward, spinDirection);
                StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
                StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
            }

			if (Input.GetKeyDown(KeyCode.Alpha9)) {
				GameObject droppedStone = DropStone ();
				statsDisplayer.TrackStone(droppedStone);
				droppedStone.GetComponent<Stone> ().Launch (8, this.gameObject.transform.forward, spinDirection);
                StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
                StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
            }

			if (Input.GetKeyDown(KeyCode.Alpha0)) {
				GameObject droppedStone = DropStone ();
				statsDisplayer.TrackStone(droppedStone);
				droppedStone.GetComponent<Stone> ().Launch (9, this.gameObject.transform.forward, spinDirection);
                StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
                StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
            }

			if (Input.GetKeyDown(KeyCode.Minus)) {
				GameObject droppedStone = DropStone ();
				statsDisplayer.TrackStone(droppedStone);
				droppedStone.GetComponent<Stone> ().Launch (10, this.gameObject.transform.forward, spinDirection);
                StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
                StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
            }

			if (Input.GetKeyDown(KeyCode.H)) {
				GameObject droppedStone = DropStone ();
				statsDisplayer.TrackStone(droppedStone);
				droppedStone.GetComponent<Stone> ().Launch (11, this.gameObject.transform.forward, spinDirection);
                StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
                StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
            }

			if (Input.GetKeyDown(KeyCode.T)) {
				GameObject droppedStone = DropStone ();
				statsDisplayer.TrackStone(droppedStone);
				droppedStone.GetComponent<Stone> ().Launch (12, this.gameObject.transform.forward, spinDirection);
                StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
                StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
            }

			if (Input.GetKeyDown(KeyCode.P))
			{
				GameObject droppedStone = DropStone();
				statsDisplayer.TrackStone(droppedStone);
				droppedStone.GetComponent<Stone>().Launch(13, this.gameObject.transform.forward, spinDirection);
				StopCoroutine(stoneCam.InitiateFollowCam(droppedStone));
				StartCoroutine(stoneCam.InitiateFollowCam(droppedStone));
			}

		} else {
			virtualStoneObject.GetComponentInChildren<MeshRenderer> ().enabled = false;
			virtualStoneLineRenderer.enabled = false;
		}
	}

	void ProcessMoveAnimations()
	{
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) {

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
		if (Input.GetKeyDown(KeyCode.C) && !cameraScript.freeLook && !stoneConjuringMode) {
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

		//Zero out animation to idle
		actions.Stay ();
		ProcessConjuring ();
		ProcessMoveAnimations ();

	}


	void FixedUpdate()
	{

		if (!cameraScript.freeLook) {
			//Avatar facing direction control
			float sensitivityScale = 1f;
			if (Input.GetKey(KeyCode.LeftControl))
            {
				sensitivityScale *= highSensitivityScalingFactor;
            }
			rb.AddTorque (0, Input.GetAxis ("Mouse X") * rotationalSensitivity * sensitivityScale, 0);
		}

		//Process WASD movement
		if (Input.GetKey(KeyCode.W) && !cameraScript.freeLook) {
			//actions.Walk ();
			rb.AddForce (this.gameObject.transform.forward * moveSpeed);
		}

		if (Input.GetKey(KeyCode.S) && !cameraScript.freeLook) {
			//actions.Walk ();
			rb.AddForce (-this.gameObject.transform.forward * moveSpeed);
		}

		if (Input.GetKey(KeyCode.A) && !cameraScript.freeLook) {
			//actions.Walk ();
			rb.AddForce (-this.gameObject.transform.right * moveSpeed);
		}

		if (Input.GetKey(KeyCode.D) && !cameraScript.freeLook) {
			//actions.Walk ();
			rb.AddForce (this.gameObject.transform.right * moveSpeed);
		}

		//Enforce appropriate speed limit for running or walking
		//TODO: Handle case of releasing shift and decelerating from a run gradually rather than instantaneously dropping to walk speed
		if (rb.velocity.x > maxAllowedSpeed) {
			rb.velocity = new Vector3 (maxAllowedSpeed, rb.velocity.y, rb.velocity.z);
		}

		if (rb.velocity.x < -maxAllowedSpeed) {
			rb.velocity = new Vector3 (-maxAllowedSpeed, rb.velocity.y, rb.velocity.z);
		}

		if (rb.velocity.z > maxAllowedSpeed) {
			rb.velocity = new Vector3 (rb.velocity.x, rb.velocity.y, maxAllowedSpeed);
		}

		if (rb.velocity.z < -maxAllowedSpeed) {
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

}
