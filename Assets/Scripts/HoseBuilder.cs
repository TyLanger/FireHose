using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoseBuilder : MonoBehaviour {

	public Transform nozzle;
	public Transform hydrant;

	public int numSegments = 10;

	public float maxSeparation = 0.4f;
	public float minSeparation = 0.1f;

	public float moveSpeed = 0.1f;

	public HoseSegment hoseSegment;

	// Use this for initialization
	void Start () {
		HoseSegment previous = hoseSegment;
		HoseSegment copy = hoseSegment;
		for (int i = 0; i < numSegments; i++) {
			copy = Instantiate (hoseSegment, transform.position + new Vector3 (i * minSeparation, 0, 0), Quaternion.identity);
			copy.maxSeparation = this.maxSeparation;
			copy.minSeparation = this.minSeparation;
			copy.moveSpeed = this.moveSpeed;
			if (i == 0) {
				// first in list
				previous = copy;
				previous.prev = hydrant;
				previous.hoseType = HoseSegment.HoseType.Start;
			} else {
				copy.prev = previous.transform;
				previous.next = copy.transform;
				previous = copy;
			}

		}
		copy.hoseType = HoseSegment.HoseType.End;
		copy.next = nozzle;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
