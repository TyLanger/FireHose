using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public float moveSpeed = 1;
	Vector3 moveInput;
	public float turnSpeed = 1;
	Vector3 lastMoveInput;

	// may change to make it so you can hold multiple of some things
	bool holdingTool = false;
	Tool currentTool;
	public Transform hand;

	// Use this for initialization
	void Start () {
		lastMoveInput = transform.forward;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Fire1")) {
			// left click
			// A button on snes controller
			//Debug.Log ("Fire1");
			PickUp();
		}
		if (Input.GetButtonDown ("Fire2")) {
			// righ click
			// B button on snes controller
			//Debug.Log ("Fire2");
			UseTool();
		}
		if (Input.GetButtonUp ("Fire2")) {
			// righ click
			// B button on snes controller
			//Debug.Log ("Fire2");
			StopTool();
		}
		if (Input.GetButtonDown ("Fire3")) {
			// middle mouse button
			// not bound on snes
			Debug.Log ("Fire3");
		}

		// wasd
		// left stick for snes controller
		moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		if (moveInput.magnitude != 0) {
			// set lastMoveInput only if input is not 0
			lastMoveInput = moveInput;
		}

	}

	void FixedUpdate () {
		transform.position = Vector3.MoveTowards(transform.position, transform.position + moveInput, moveSpeed * Time.fixedDeltaTime);
		// use lastMove input so it is never 0
		// lastMoveInput is the last "actual" input, no 0 when not pressing movement buttons
		// makes character facec where they are moving
		transform.forward = Vector3.RotateTowards (transform.forward, lastMoveInput, turnSpeed * Time.fixedDeltaTime, 1);
	}

	void StopTool()
	{
		if (holdingTool) {
			currentTool.StopUse ();
		}
	}

	void UseTool()
	{
		if (holdingTool) {
			currentTool.Use ();
		}
	}

	void PickUp()
	{
		if (holdingTool) {
			// drop object
			if (currentTool != null) {
				currentTool.Drop ();
			}
			holdingTool = false;
		} else {
			// pick up object in front of you
			RaycastHit hit;
			if (Physics.Raycast (transform.position, transform.forward, out hit, 2)) {
				//Debug.Log ("Hit something");
				if (hit.collider.GetComponent<Tool> () != null) {
					// hit a tool
					currentTool = hit.collider.GetComponent<Tool>();
					currentTool.PickUp(hand);
					holdingTool = true;
				}
			}
		}
	}
}
