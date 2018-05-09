using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extinguisher : Tool {


	// degrees
	public float sprayCone = 60;

	// how many seconds you can spray for
	public float fuelSeconds = 6;

	public float timeToFullSpray = 2;

	IEnumerator Spray()
	{
		ForcedMovementStarted ();
		// constantly spawn foam particles
		yield return null;
	}

	public override void Use()
	{
		base.Use ();
		StartCoroutine ("Spray");
	}

	public override void StopUse()
	{
		StopCoroutine ("Spray");
		ToolFinished ();
	}

}
