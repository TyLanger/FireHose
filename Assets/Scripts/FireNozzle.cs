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

	/*
	public override void PickUp (Transform parent)
	{
		base.PickUp (parent);
		ForcedMovementStarted ();
	}

	public override void Drop ()
	{
		base.Drop ();
		ToolFinished ();
	}

	public override void StopUse ()
	{
		if (spraying) {
			spraying = false;
			StopCoroutine ("Spray");
			particles.Stop ();
		}
	}
	*/
}
