using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hose : MonoBehaviour {

	Vector3 anchorPoint;
	public Vector3 lastPoint;
	public Transform target;
	float maxLength = 10;
	float currentLength;
	bool moving = true;


	public Transform hitTrans;
	public bool canSee = false;


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

			Debug.DrawRay (anchorPoint, (target.position - anchorPoint), Color.blue);
			Debug.DrawRay (lastPoint, Vector3.forward, Color.red);
			if (Physics.Raycast (anchorPoint, (target.position - anchorPoint), out hit)) {
				hitTrans = hit.transform;
				if (hit.transform == target) {
					canSee = true;
					lastPoint = hit.point;
				} else {
					//Debug.Break ();
					canSee = false;
					//anchorPoint = lastPoint;
					anchorPoint = anchorPoint + (lastPoint - anchorPoint).normalized * (hit.distance+0.01f);
				}
			}

			yield return new WaitForFixedUpdate ();
		}
	}

	public void SetTarget(Transform t)
	{
		target = t;
	}

	public void StartMoving()
	{
		moving = true;
		StartCoroutine (CheckHose ());
	}

	public void StopMoving()
	{
		moving = false;
	}
}
