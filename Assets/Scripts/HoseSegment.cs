using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoseSegment : MonoBehaviour {

	public enum HoseType { Normal, Start, End

	};

	public HoseType hoseType;

	public Transform next;
	public Transform prev;

	public float maxSeparation = 0.4f;
	public float minSeparation = 0.1f;

	public float moveSpeed = 0.1f;

	public float force = 200;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		switch(hoseType)
		{
		case HoseType.Normal:
			if (Vector3.Distance (transform.position, prev.position) <= maxSeparation && Vector3.Distance (transform.position, next.position) >= minSeparation) {
				// Vector3.Distance (transform.position, prev.position) < maxSeparation &&
				// only move if
				// not too far from prev
				// not too close to next
				transform.position = Vector3.MoveTowards (transform.position, next.position, moveSpeed * Time.fixedDeltaTime);
			}
			break;
		case HoseType.End:
			if (Vector3.Distance (transform.position, next.position) > maxSeparation) {
				next.GetComponentInParent<Rigidbody> ().AddForce ((transform.position - next.position) * force);
			} 
			if (Vector3.Distance (transform.position, prev.position) <= maxSeparation && Vector3.Distance (transform.position, next.position) >= minSeparation) {
				// 
				transform.position = Vector3.MoveTowards (transform.position, next.position, moveSpeed * Time.fixedDeltaTime);
			}
			break;
		case HoseType.Start:
			if (Vector3.Distance (transform.position, prev.position) > maxSeparation) {
				prev.GetComponentInParent<Rigidbody> ().AddForce ((transform.position - prev.position) * force);
				//prev.GetComponentInParent<Rigidbody> ().velocity = (transform.position - prev.position) * force;
			}
			if (Vector3.Distance (transform.position, prev.position) <= maxSeparation && Vector3.Distance (transform.position, next.position) >= minSeparation) {
				// only move if
				// not too far from prev
				// not too close to next
				transform.position = Vector3.MoveTowards (transform.position, next.position, moveSpeed * Time.fixedDeltaTime);
			}
			break;
		}
	}
}
