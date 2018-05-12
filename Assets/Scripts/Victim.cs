using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Victim : Tool {


	public float throwForce = 100;

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
}
