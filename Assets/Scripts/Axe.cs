using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : Tool {

	int numDegrees = 90;
	int numSwings = 3;

	public IEnumerator SwingAxe()
	{
		for (int s = 0; s < numSwings; s++) {
			for (int i = 0; i < numDegrees / 2; i++) {

				//swing axe down
				transform.RotateAround (transform.position, transform.right, 2);

				yield return null;
			}
			for (int j = 0; j < numDegrees / 3; j++) {
				// pull axe back up
				transform.RotateAround (transform.position, transform.right, -3);

				yield return null;
			}
		}

	}

	public override void Use ()
	{
		base.Use ();
		StartCoroutine (SwingAxe());
	}

	public override void StopUse()
	{

	}

}
