using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ToolType {None, Axe, Hose, Extinguisher };

public class Tool : MonoBehaviour {

	// something the player can pick up

	//Vector3 parentPos;
	//public Vector3 offset;

	bool canPickUp = true;

	Collider pickupCollider;

	public ToolType toolType;
	public float speedMultiplier = 1;
	public float turnMultiplier = 1;

	public event Action ToolFinishedAction;
	public event Action ForcedMovement;


	// Use this for initialization
	protected virtual void Start () {
		pickupCollider = GetComponent<SphereCollider> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//transform.position = parentPos + offset; 
	}

	public bool CanPickup()
	{
		return canPickUp;
	}

	public void PickUp(Transform parent)
	{
		if (canPickUp) {
			// player uses this to pick it up
			// parent is used to tell this where to be
			//Debug.Log("Picked up");

			transform.position = parent.position;
			transform.rotation = parent.rotation;
			transform.parent = parent;
			canPickUp = false;
			//GetComponent<Rigidbody> ().useGravity = false;
			pickupCollider.enabled = false;
		}
	}

	public void Drop()
	{
		transform.parent = null;
		// has nobody holding it
		// just leave it where it is
		canPickUp = true;
		//GetComponent<Rigidbody> ().useGravity = true;
		pickupCollider.enabled = true;
	}

	/// Start using the tool
	public virtual void Use()
	{
		//Debug.Log ("Used");
	}

	/// Stop using the tool
	public virtual void StopUse()
	{

	}
		
	protected void ForcedMovementStarted()
	{
		if (ForcedMovement != null) {
			ForcedMovement ();
		}
	}

	protected void ToolFinished()
	{
		if (ToolFinishedAction != null) {
			ToolFinishedAction ();
		}
	}

	public virtual float GetSpeedMultiplier()
	{
		return speedMultiplier;
	}

	public virtual float GetTurnMultiplier()
	{
		return turnMultiplier;
	}

	public virtual Vector3 MoveTowards(Vector3 current, Vector3 input, float baseSpeed)
	{
		return Vector3.MoveTowards (current, current + input, baseSpeed);
	}

	public virtual Vector3 MoveTowards(Vector3 current, Vector3 forward, Vector3 input, float baseSpeed)
	{
		return Vector3.MoveTowards (current, current + input, baseSpeed);
	}
}
