using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Victim : Tool {


	public float throwForce = 100;
	// what position in the z direction the victim needs to be below to be considered saved
	float rescueLine = 0;

	public event System.Action Rescued;

	protected override void  Start()
	{
		base.Start();
		StartCoroutine (RescueCheck ());
	}

	IEnumerator RescueCheck()
	{
		while (true) {
			// check if you're rescued
			if (isRescued ()) {
				Debug.Log ("Victim Rescued");
				if (Rescued != null) {
					Rescued ();
				}
				break;
			}

			// only need to check every 1 second
			// could lower this if it's an issue
			// running this more often will probably not affect performance at all. Would need >100 victims probably
			yield return new WaitForSeconds (1);
		}
	}

	public override void PickUp(Transform parent)
	{
		base.PickUp (parent);
		// tell the player that the tool is influencing their movement
		ForcedMovementStarted ();
	}

	public override void Drop()
	{
		// tell the player the tool is done influencing their movement
		ToolFinished ();
		base.Drop ();
	}

	public override void Use()
	{
		base.Use ();

		// Tell the player to drop this tool
		if (GetComponentInParent<Player> () != null) {
			GetComponentInParent<Player> ().DropTool ();
		}

		// stop the player from being slowed
		// and remove this object from the player's object
		Drop();

		StartCoroutine (Toss ());
	}

	IEnumerator Toss()
	{
		if (physicsObject != null) {
			physicsObject.GetComponent<Rigidbody>().AddForce ((transform.forward + new Vector3 (0, 1, 0)) * throwForce);
		}

		yield return new WaitForFixedUpdate();
	}

	public bool isRescued()
	{
		return (transform.position.z < rescueLine);
	}
}
