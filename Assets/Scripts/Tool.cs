using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : MonoBehaviour {

	// something the player can pick up

	//Vector3 parentPos;
	//public Vector3 offset;

	bool canPickUp = true;

	Collider pickupCollider;

	// Use this for initialization
	void Start () {
		pickupCollider = GetComponent<SphereCollider> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//transform.position = parentPos + offset; 
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

	public virtual void Use()
	{
		//Debug.Log ("Used");
	}

	public virtual void StopUse()
	{

	}
}
