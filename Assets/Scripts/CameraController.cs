using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	Vector3 focusPoint;
	Vector3 groundOffset;

	Vector3 velocity = Vector3.zero;

	// Use this for initialization
	void Start () {
		// find the center of the screen
		RaycastHit hitCenter;
		if (Physics.Raycast (GetComponent<Camera>().transform.position, GetComponent<Camera>().transform.forward, out hitCenter)) {
			if(hitCenter.collider != null)
			{
				Debug.Log("Center: " +hitCenter.point);

				groundOffset = transform.position - hitCenter.point;
			}
		}

		focusPoint = transform.position;
	}
	
	// Update is called once per frame
	void Update () {

		//transform.position = (transform.position + focusPoint) * 0.5f;

		// smooooth
		//transform.position = Vector3.SmoothDamp (transform.position, focusPoint, ref velocity, 1);

		// moves the first chunk faster than subsequent chunks
		// lurch and slide
		// moves 10% of the distance, then 10% of the remaining distance, then 10% of the.....
		transform.position = Vector3.Lerp (transform.position, focusPoint, Time.deltaTime);

		if (Input.GetButtonDown ("Jump")) {

			var camera = GetComponent<Camera> ();
			Vector3[] frustumCorners = new Vector3[4];
			camera.CalculateFrustumCorners (new Rect (0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

			for (int i = 0; i < 4; i++) {
				var worldSpaceCorner = camera.transform.TransformVector (frustumCorners [i]);
				Debug.DrawRay (camera.transform.position, worldSpaceCorner, Color.blue);
				RaycastHit hit;
				if (Physics.Raycast (camera.transform.position, worldSpaceCorner, out hit)) {
					if (hit.collider != null) {
						Debug.Log (hit.point);
					}
				}
			}
			FocusOn(new Vector3(12, 0, 6));

		}
	}

	/// A point in the world for the camera to focus on
	public void FocusOn(Vector3 focalPoint)
	{
		// this doesn't work if it is called every frame
		focusPoint = focalPoint + groundOffset;
	}
}
