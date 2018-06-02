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
    public float aveMoveMultiplier = 10;

	public float force = 200;

    public int segmentNumber;
    public int totalSegments;

    static int numberCanMove = 0;
    public int debugInt;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        debugInt = numberCanMove;

        Vector3 newPos = Vector3.MoveTowards(transform.position, next.position, moveSpeed * Time.fixedDeltaTime);
        if (Vector3.Distance(newPos, prev.position) <= maxSeparation && (Vector3.Distance(newPos, next.position) >= minSeparation))
        {

            // move
            transform.position = newPos;


        }
        else
        {

            // only move to the average position if the segments ahead of you have
            /*
            if (numberCanMove >= (totalSegments - segmentNumber))
            {
                if (numberCanMove < totalSegments)
                {
                    //numberCanMove++;
                    numberCanMove += 1 + 10 * (segmentNumber / totalSegments);
                }
            }*/

            //Vector3 newPos3 = Vector3.MoveTowards(transform.position, (next.position-prev.position).normalized*(maxSeparation-0.1f), aveMoveMultiplier * moveSpeed * Time.fixedDeltaTime);
            //Debug.DrawLine(transform.position, newPos3, Color.blue);

            // if you can't get close to the next in line, check if you can get closer to the next, next in line
            //Vector3 newPosNext = Vector3.MoveTowards(transform.position, next.GetComponent<HoseSegment>().next.position, moveSpeed * Time.fixedDeltaTime);
            // if you can't get closer, move towards the center of the prev and next
            //((float)(segmentNumber+20) / (float)totalSegments) *
            Vector3 newPosAve = Vector3.MoveTowards(transform.position, (next.position * 0.5f) + (prev.position * 0.5f), aveMoveMultiplier * moveSpeed * Time.fixedDeltaTime);
            Debug.DrawLine(transform.position, newPosAve, Color.blue);
            if (Vector3.Distance(newPosAve, prev.position) <= maxSeparation && Vector3.Distance(newPosAve, next.position) >= minSeparation)
            {
                // move
                //Debug.Log("Average Move");
                transform.position = newPosAve;
            }


        }

        if(hoseType == HoseType.End)
        {
            // part attached to the nozzle
            if (Vector3.Distance(transform.position, next.position) > maxSeparation)
            {
                next.GetComponentInParent<Rigidbody>().AddForce((transform.position - next.position) * force);
            }
        }
        /*
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
			// part attached to the nozzle
			if (Vector3.Distance (transform.position, next.position) > maxSeparation) {
				next.GetComponentInParent<Rigidbody> ().AddForce ((transform.position - next.position) * force);
			} 
			if (Vector3.Distance (transform.position, prev.position) <= maxSeparation && Vector3.Distance (transform.position, next.position) >= minSeparation) {
				// 
				transform.position = Vector3.MoveTowards (transform.position, next.position, moveSpeed * Time.fixedDeltaTime);
			}
			break;
		case HoseType.Start:
			// part attached to the hydrant
			if (Vector3.Distance (transform.position, prev.position) > maxSeparation) {
				prev.GetComponentInParent<Rigidbody> ().AddForce ((transform.position - prev.position) * force);
				//prev.GetComponentInParent<Rigidbody> ().velocity = (transform.position - prev.position) * force;
			}
			if ( Vector3.Distance (transform.position, next.position) >= minSeparation) {
				// Vector3.Distance (transform.position, prev.position) <= maxSeparation &&

				float distToPrev = Vector3.Distance (transform.position, prev.position);
				float speedMultiplier = 1;
				// Doesn't really work
				if (distToPrev > maxSeparation) {
					//speedMultiplier = 0.5f;
					// multiplier will get to 0, then it will never move again to change away from 0
					//speedMultiplier = 1 - ((distToPrev - maxSeparation) * (distToPrev - maxSeparation));
				} 

				transform.position = Vector3.MoveTowards (transform.position, next.position, moveSpeed * Time.fixedDeltaTime * speedMultiplier);
			}
			break;
		}
        */
    }
}
