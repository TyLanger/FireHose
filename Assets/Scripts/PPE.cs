using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPE : Tool {
	// PPE = Personal Protective Equipment
	// wearables that protect the player from fire

	public float fireResistance = 1;

	public bool isHat = true;

	public override void Use ()
	{
		base.Use ();
		// put on the item

		GetComponentInParent<Player> ().PutOnPPE (transform, fireResistance, isHat);

		ToolFinished ();
	}
		

}
