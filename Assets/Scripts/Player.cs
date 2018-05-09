using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {



	public float moveSpeed = 1;
	Vector3 moveInput;
	public float turnSpeed = 1;
	Vector3 lookDirection;

	ToolType currentUsing;

	// may change to make it so you can hold multiple of some things
	bool holdingTool = false;
	Tool currentTool;
	public Transform hand;

	// Use this for initialization
	void Start () {
		currentUsing = ToolType.None;
		lookDirection = transform.forward;
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
		if (moveInput.sqrMagnitude != 0) {
			// set lookDirection only if input is not 0
			lookDirection = moveInput;
		}

	}

	void FixedUpdate () {
		switch(currentUsing)
		{
		case ToolType.None:
			// transform.forward*moveInput.magnitude makes the player have to do circles to turn around
			// they always move forward and rely on turn speed to be able to turn. Multiplying by moveInput.magnitude makes it so the player doesn't move when not pressing anything
			transform.position = Vector3.MoveTowards (transform.position, transform.position + moveInput, moveSpeed * Time.fixedDeltaTime);
			// use lastMove input so it is never 0
			// lookDirection is the last "actual" input, no 0 when not pressing movement buttons
			// makes character facec where they are moving
			transform.forward = Vector3.RotateTowards (transform.forward, lookDirection, turnSpeed * Time.fixedDeltaTime, 1);
			break;
		case ToolType.Axe:
			transform.position = Vector3.MoveTowards (transform.position, transform.position + transform.forward,  currentTool.GetSpeedMultiplier() * moveSpeed * Time.fixedDeltaTime);
			transform.forward = Vector3.RotateTowards (transform.forward, lookDirection, currentTool.GetTurnMultiplier() * turnSpeed * Time.fixedDeltaTime, 1);
			break;
		case ToolType.Hose:
			// move away from the water coming out of the hose
			transform.position = Vector3.MoveTowards (transform.position, transform.position + transform.forward,  currentTool.GetSpeedMultiplier() * moveSpeed * Time.fixedDeltaTime);
			transform.forward = Vector3.RotateTowards (transform.forward, lookDirection, currentTool.GetTurnMultiplier() * turnSpeed * Time.fixedDeltaTime, 1);
			break;
		case ToolType.Extinguisher:
			// move backwards away from the direction the extinguisher is shooting
			transform.position = Vector3.MoveTowards (transform.position, transform.position + transform.forward,  currentTool.GetSpeedMultiplier() * moveSpeed * Time.fixedDeltaTime);
			transform.forward = Vector3.RotateTowards (transform.forward, lookDirection, currentTool.GetTurnMultiplier() * turnSpeed * Time.fixedDeltaTime, 1);
			break;

		}

		//transform.position = currentTool.MoveTowards (transform.position, lookDirection, moveSpeed * Time.fixedDeltaTime);

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
			
			// get an action that tells the player when to start getting pushed by the object
			currentTool.ForcedMovement -= ToolStarted;
			currentTool.ForcedMovement += ToolStarted;
			// get an action to tell the player when it can go back to regular movement
			currentTool.ToolFinishedAction -= ToolFinished;
			currentTool.ToolFinishedAction += ToolFinished;
			currentTool.Use ();
		}
	}

	void ToolStarted()
	{
		currentUsing = currentTool.toolType;
	}

	void ToolFinished()
	{
		currentUsing = ToolType.None;
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
