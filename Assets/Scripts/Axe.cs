using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : Tool {

	int numDegrees = 90;
	public int numSwings = 4;

	public int breakStrength = 5;

	bool swinging = false;

	Collider axeTrigger;

	protected override void Start()
	{
		base.Start ();
		axeTrigger = GetComponent<Collider> ();
	}

	public IEnumerator SwingAxe()
	{
		swinging = true;

		// the axe is now pulling the player
		ForcedMovementStarted();
		for (int s = 0; s < numSwings; s++) {
			axeTrigger.enabled = true;
			for (int i = 0; i < numDegrees / 9; i++) {

				//swing axe down
				transform.RotateAround (transform.position, transform.right, 9);

				yield return null;
			}
			axeTrigger.enabled = false;
			for (int j = 0; j < numDegrees / 9; j++) {
				// pull axe back up
				transform.RotateAround (transform.position, transform.right, -9);

				yield return null;
			}
		}
		swinging = false;
		// the ability is over
		// run this so anything subscribed to the action will know
		ToolFinished ();
	}

	public override void Use ()
	{
		base.Use ();
		if (!swinging) {
			//GetComponent<Collider> ().isTrigger = true;
			StartCoroutine (SwingAxe ());
		}
	}

	public override void StopUse()
	{

	}

	public override Vector3 MoveTowards(Vector3 current, Vector3 input, float baseSpeed)
	{
		// get the axe's parent's forward vector (the hand)
		// could get axe's parent's parent's (player) forward
		// is this better than the player just passing it in?
		// player can rotate their character to rotate the axe

		return Vector3.MoveTowards (current, current + transform.parent.transform.forward, baseSpeed * speedMultiplier);
	}

	protected override void ForcedMovementStarted ()
	{
		base.ForcedMovementStarted ();
		canDrop = false;
	}

	protected override void ToolFinished ()
	{
		canDrop = true;
		base.ToolFinished ();
	}

	void OnTriggerEnter(Collider col)
	{
		if (swinging) {
			if(col.GetComponent<BuildingBlock>() != null) {
			//if (col.tag == "Door") {
				//Destroy (col.gameObject);
				col.GetComponent<BuildingBlock> ().Break (breakStrength);
			}
            if(col.tag == "Furniture")
            {
                Destroy(col.gameObject);
            }
		}
	}

}
