using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : Tool {

	int numDegrees = 90;
	int numSwings = 3;

	bool swinging = false;

	public IEnumerator SwingAxe()
	{
		swinging = true;
		for (int s = 0; s < numSwings; s++) {
			for (int i = 0; i < numDegrees / 9; i++) {

				//swing axe down
				transform.RotateAround (transform.position, transform.right, 9);

				yield return null;
			}
			for (int j = 0; j < numDegrees / 9; j++) {
				// pull axe back up
				transform.RotateAround (transform.position, transform.right, -9);

				yield return null;
			}
		}
		swinging = false;

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

	void OnTriggerEnter(Collider col)
	{
		if (swinging) {
			if (col.tag == "Door") {
				Destroy (col.gameObject);
			}
		}
	}

}
