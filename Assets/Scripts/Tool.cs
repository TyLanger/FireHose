﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ToolType {None, Axe, Hose, Extinguisher, Victim };

public class Tool : MonoBehaviour {

	// something the player can pick up

	//Vector3 parentPos;
	//public Vector3 offset;

	bool canPickUp = true;
	protected bool canDrop = true;

	Collider pickupCollider;
	//GameObject visuals;
	protected GameObject physicsObject;

	public ToolType toolType;
	public float speedMultiplier = 1;
	public float turnMultiplier = 1;

	public event Action ToolFinishedAction;
	public event Action ForcedMovement;

    protected AudioSource[] audioSources;
    // only 1 clip per source atm
    public AudioClip[] clips;
    public int numSoundEffects;

    // Use this for initialization
    protected virtual void Start () {
		//pickupCollider = GetComponent<SphereCollider> ();
		pickupCollider = GetComponentInParent<SphereCollider> ();
		physicsObject = GetComponentInParent<Rigidbody> ().gameObject;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//transform.position = parentPos + offset; 
	}

    protected virtual void AddAudioSources(int number)
    {
        audioSources = new AudioSource[number];
        for (int i = 0; i < number; i++)
        {
            // The documentation has the as AudioSource. Seems a little redundant
            audioSources[i] = gameObject.AddComponent<AudioSource>() as AudioSource;
            audioSources[i].playOnAwake = false;
        }
    }

	public bool CanPickup()
	{
		return canPickUp;
	}

	public bool CanDrop()
	{
		return canDrop;
	}

	public virtual void PickUp(Transform parent)
	{
		if (canPickUp) {
			// player uses this to pick it up
			// parent is used to tell this where to be
			//Debug.Log("Picked up");

			transform.position = parent.position;
			transform.rotation = parent.rotation;
			transform.parent = parent;

			// new system
			// rigidbody component stays behind
			// visuals get given to the player
			/*
			visuals.transform.position = parent.position;
			visuals.transform.rotation = parent.rotation;
			visuals.transform.parent = parent;
			// does setting this gameobject inactive make the script not work?
			// probably
			// should the script go on the visuals or the rigidbody?
			gameObject.SetActive (false);
			*/

			// new system 2
			// script is attached to the visuals
			// Would need to change player's pickup code
			// swap GetComponent for GetComponentInChild would probably do the trick

			transform.position = parent.position;
			transform.rotation = parent.rotation;
			transform.parent = parent;
			physicsObject.SetActive (false);


			canPickUp = false;
			//GetComponent<Rigidbody> ().useGravity = false;
			pickupCollider.enabled = false;
		}
	}

	public virtual void Drop()
	{
		transform.parent = null;
		// has nobody holding it
		// just leave it where it is

		// new split system
		/*
		transform.position = visuals.transform.position;
		transform.rotation = visuals.transform.rotation;
		visuals.transform.parent = gameObject;
		gameObject.SetActive(true);
		*/

		// new system 2
		// script is attached to the visuals
		// Move physics object to where visuals currently are
		// change visuals back to being child of physics object
		// Need to set active first for the Victim object
		// it uses an IEnumerator to calculate its position to tell if it's rescued
		// attaching it to an inactive object (the rigidbody object) stops the coroutine from running
		// If this breaks other stuff, Victim could use FixedUpdate or maybe start the coroutine in Awake?
		physicsObject.SetActive (true);
		physicsObject.transform.position = transform.position;
		physicsObject.transform.rotation = transform.rotation;
		transform.parent = physicsObject.transform;




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
		
	protected virtual void ForcedMovementStarted()
	{
		if (ForcedMovement != null) {
			ForcedMovement ();
		}
	}

	protected virtual void ToolFinished()
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
