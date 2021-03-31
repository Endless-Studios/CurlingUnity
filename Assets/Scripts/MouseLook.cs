using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour {

	public float mouseSensitivity = 100.0f;
	public float clampAngle = 30.0f;
	public bool invertY = false;
	public bool freeLook = false;

	private float rotY = 0.0f; // rotation around the up/y axis
	private float rotX = 0.0f;
	private Quaternion originalCameraPosition;
	private bool startLookFlag = false;



	public void StartFreeLook () {
		Vector3 rot = this.gameObject.transform.localRotation.eulerAngles;
		rotY = rot.y;
		rotX = rot.x;
		originalCameraPosition = this.gameObject.transform.localRotation;
		freeLook = true;
	}

	public void StopFreeLook() {
		//TODO: Lerp back to original position
		freeLook = false;
		this.gameObject.transform.localRotation = originalCameraPosition;
	}


	// Update is called once per frame
	void LateUpdate () {
		if (freeLook) {
			
			float mouseY = Input.GetAxis ("Mouse Y");
			float mouseX = Input.GetAxis ("Mouse X");

			rotY += mouseX * mouseSensitivity * Time.deltaTime;
			if (invertY) {
				rotX += mouseY * mouseSensitivity * Time.deltaTime;
			} else {
				rotX -= mouseY * mouseSensitivity * Time.deltaTime;
			}

			rotX = Mathf.Clamp (rotX, -clampAngle, clampAngle);

			Quaternion localRotation = Quaternion.Euler (rotX, rotY, 0);
			this.gameObject.transform.localRotation = localRotation;
		}
	}
}
