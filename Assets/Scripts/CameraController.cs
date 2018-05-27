using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	Vector3 focusPoint;
	Vector3 groundOffset;
	// how high above the ground during the photo
	// these are the numbers I need to add by to get the desired end result
	Vector3 photoGroundOffset = new Vector3(0, 0.15f, -10);
	float nearZ = -3;
	float farZ = 10;
	bool tookPicture = false;

	// this is the desired end result
	// (x 0, y 1, z -7 to 4)
	// y == 1 is a decent height
	// z == -7 shows the whole house or most of the house
	// z == 4 makes the 4 players take up most of the screen
	// an angle of -7 shows most of the height of the house
	// partially cuts off the players' feet to center their heads
	float photoAngle = -7;
	//float timeToMoveToEnd = 4;
	//float endGameTime;

	bool gameOver = false;

	float baseMoveSpeed = 1;
	float currentMoveSpeed;

	//Vector3 velocity = Vector3.zero;

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

		currentMoveSpeed = baseMoveSpeed;
		focusPoint = transform.position;
	}
	
	// Update is called once per frame
	void Update () {

		//transform.position = (transform.position + focusPoint) * 0.5f;

		// smooooth
		//transform.position = Vector3.SmoothDamp (transform.position, focusPoint, ref velocity, 1);

		if (gameOver) {
			groundOffset = Vector3.Lerp(groundOffset, photoGroundOffset, Time.deltaTime * currentMoveSpeed);
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(photoAngle, 0, 0), Time.deltaTime * currentMoveSpeed);
			if(transform.position.z < (-2))
			{
				// when the camera gets close to the far position
				// Which is about -2 z
				// sits at -2 z
				// at the far position
				// swap to the near position
				photoGroundOffset = new Vector3 (photoGroundOffset.x, photoGroundOffset.y, nearZ);
			}
			if (photoGroundOffset.z == nearZ && transform.position.z > 4.9f && !tookPicture) {
				// comes to rest for the near at about 4.9
				// take a picture
				tookPicture = true;
				StartCoroutine (TakePicture ());
			}
		}
			// moves the first chunk faster than subsequent chunks
			// lurch and slide
			// moves 10% of the distance, then 10% of the remaining distance, then 10% of the.....
			transform.position = Vector3.Lerp (transform.position, focusPoint, Time.deltaTime * currentMoveSpeed);
		

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
						//Debug.Log (hit.point);
					}
				}
			}
		}
	}

	/// A point in the world for the camera to focus on
	public void FocusOn(Vector3 focalPoint)
	{
		FocusOn (focalPoint, baseMoveSpeed);
	}

	/// A point in the world for the camera to focus on
	public void FocusOn(Vector3 focalPoint, float speed)
	{
		currentMoveSpeed = speed;
		focusPoint = focalPoint + groundOffset;
	}

	public void PhotoshootPosition()
	{
		gameOver = true;
		//endGameTime = Time.time;
	}

	IEnumerator TakePicture()
	{
		yield return new WaitForEndOfFrame ();

		//ScreenCapture
		Texture2D screenImage = new Texture2D(Screen.width, Screen.height);
		screenImage.ReadPixels (new Rect (0, 0, Screen.width, Screen.height), 0, 0);
		screenImage.Apply ();

		byte[] imageBytes = screenImage.EncodeToPNG ();
		System.IO.File.WriteAllBytes ("Screenshots/Screenshot.png", imageBytes);
	}
}
