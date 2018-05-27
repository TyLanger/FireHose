using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPE : Tool {
	// PPE = Personal Protective Equipment
	// wearables that protect the player from fire

	public float fireResistance = 1;

	public bool isHat = true;

	public Renderer visuals;
	public Material[] headNumberMats;


	public override void Use ()
	{
		if (isHat) {
			// can't use GetComponentInChildren<Renderer>() to access the model
			Material[] mats = visuals.materials;
			mats [1] = headNumberMats [GetComponentInParent<Player> ().GetPlayerNumber () - 1];
			visuals.materials = mats;

		}
		base.Use ();
		// put on the item



		GetComponentInParent<Player> ().PutOnPPE (transform, fireResistance, isHat);

		ToolFinished ();
	}
		

}
