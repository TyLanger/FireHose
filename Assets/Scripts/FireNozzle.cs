using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireNozzle : Extinguisher {


	// similar to the fire extinguisher
	// Differences:
	// infinite
	// limited range (the hose has limited length)
	// stronger
	// can be dropped while spraying?
	// spray would slow to a stop

	public float weightSpeedMultiplier = 0.7f;

	Vector3 anchorPoint;
	float maxHoseLength;
	float currentHoseLength = 10;
	bool moving = false;
	bool canSee = false;
	Transform carrierTrans;
	public Transform hitTrans;

	public Hose hose;

	protected override void Start()
	{
		base.Start ();
		anchorPoint = transform.position;
	}

	IEnumerator CheckHose()
	{
		RaycastHit hit;
		while (moving) {
			// hose runs from where it is attached to the truck
			// fire a ray from the start to the player holding the nozzle
			// if the ray doesn't hit, that means there's something in the way
			// ignore other players, fire, other tools
			// create a hose bend at the last point the nozzle was
			// do the raycast from this new point
			// subtract the length from the available hoseLength

			// how do I ignore everything but this object?
			// is raycasting backwards easier (from tool to anchor)

			// raycast ignores the object casting it...
			// That's the object I want to hit...

			Debug.DrawRay (anchorPoint, (transform.position - anchorPoint), Color.blue);
			if (Physics.Raycast (anchorPoint, (transform.position - anchorPoint), out hit)) {
				hitTrans = hit.transform;
				if (hit.transform == transform.parent.parent) {
					canSee = true;
				} else {
					canSee = false;
				}
			}

			yield return new WaitForFixedUpdate ();
		}
	}

	public override Vector3 MoveTowards (Vector3 current, Vector3 forward, Vector3 input, float baseSpeed)
	{
		// movement is a blend of the push from the extinguisher and the slow movement of carrying the victim
		// the player can move with the hose, but if they stand still, they will be pushed back

		if (input.sqrMagnitude > 0) {
			// player is pressing their movement buttons

			return Vector3.MoveTowards (current, current + input, baseSpeed * weightSpeedMultiplier);
		} else {
			// GetSpeedMultiplier() accounts for the currentFuelRate
			return Vector3.MoveTowards (current, current + forward, -1 * baseSpeed * GetSpeedMultiplier() * weightSpeedMultiplier);
		}
	}

	public override void PickUp (Transform parent)
	{
		base.PickUp (parent);
		moving = true;
		hose.SetTarget (parent.parent);
		hose.StartMoving ();
		//StartCoroutine (CheckHose ());
		//Debug.Log ("Started coroutine");
	}

	public override void Drop ()
	{
		base.Drop ();
		moving = false;
		// setting moving to false will stop the CheckHose coroutine
		hose.StopMoving();
	}
}
