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

	LinkedList<Vector3> points;

	IEnumerator CheckHose()
	{
		RaycastHit hit;
		points = new LinkedList<Vector3> ();
		points.AddFirst (anchorPoint);


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

			// Draw the line of where you've been
			LinkedListNode<Vector3> prev = points.First;
			while (prev.Next != null) {
				Debug.DrawLine (prev.Value, prev.Next.Value, Color.blue);
				prev = prev.Next;
			}


			//Debug.DrawRay (anchorPoint, (target.position - anchorPoint), Color.blue);
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
					anchorPoint = anchorPoint + (lastPoint - anchorPoint).normalized * (hit.distance+0.1f);
					points.AddLast (anchorPoint);
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
