using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	Vector3 focusPoint;
	Vector3 groundOffset;
    Vector3 bottomOffset = Vector3.zero;
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

    float minFOV = 45;
    Vector3[] CornerPoints = new Vector3[4];

	bool gameOver = false;

	float baseMoveSpeed = 1;
	float currentMoveSpeed;

    //Vector3 velocity = Vector3.zero;
    Camera cameraComponent;

	// Use this for initialization
	void Start () {

        cameraComponent = GetComponent<Camera>();
		// find the center of the screen
		RaycastHit hitCenter;
		if (Physics.Raycast (GetComponent<Camera>().transform.position, GetComponent<Camera>().transform.forward, out hitCenter)) {
			if(hitCenter.collider != null)
			{
				Debug.Log("Center: " +hitCenter.point);
                
				groundOffset = transform.position - hitCenter.point;
			}
		}
        CalculateRelativeCorners();

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
						Debug.Log (hit.point);
					}
				}
			}
		}
	}

    void CalculateRelativeCorners()
    {
        Vector3[] frustumCorners = new Vector3[4];
        cameraComponent.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cameraComponent.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

        for (int i = 0; i < 4; i++)
        {
            var worldSpaceCorner = cameraComponent.transform.TransformVector(frustumCorners[i]);
            //Debug.DrawRay(camera.transform.position, worldSpaceCorner, Color.blue);
            RaycastHit hit;
            if (Physics.Raycast(cameraComponent.transform.position, worldSpaceCorner, out hit, LayerMask.GetMask("Ground")))
            {
                if (hit.collider != null)
                {
                    //Debug.Log(hit.point);
                    // calculate the corners
                    // they are relative to the center of the screen
                    // (transform.position - groundOffset) is the center on the ground
                    // hit.point is the world point where the corner of the view hits the ground
                    // cornerPoints is the different between these 2.
                    // If the center is 10, 10, and the bottom left is 5, 5 world points
                    // cornerPoints[0] will be -5, -5. 5 down and 5 left of the center, regardless of where the center is
                    CornerPoints[i] = hit.point - (transform.position - groundOffset);
                }
            }
        }

    }

    public void AdjustFOV(Vector3 minPosition, Vector3 maxPosition)
    {
        // Don't wait until the player is right off the screen.
        // This gives a little padding
        // start growing at this
        minPosition += new Vector3(-3, 0, -3);
        maxPosition += new Vector3(3, 0, 5);

        bool growFOV = false;
        bool shrinkFOV = false;

        // if any of these are true, a player is outside the screen
        // Could have done this in one if with a ton of ORs, but it wouldn't be readable.
        if(minPosition.z < ((transform.position - groundOffset)+ CornerPoints[0]).z)
        {
            // corner[0] is bottom left
            // minPoint is below the screen
            //Debug.Log("Below");
            //growFOV = true;
            // adjusting the FOV alone isn't enough when off the bottom
            // move the focus point down too
            //focusPoint += Vector3.down;
            bottomOffset += Vector3.back * 0.1f;
        }
        if(maxPosition.z > ((transform.position - groundOffset) + CornerPoints[1]).z)
        {
            // corner[1] is top left
            // maxPoint is above the screen
            growFOV = true;
            //Debug.Log("Above");

        }
        else if(IsPointLeft((transform.position - groundOffset) + CornerPoints[0], (transform.position - groundOffset) + CornerPoints[1], minPosition))
        {
            // min position is outside the screen to the left
            growFOV = true;
            //Debug.Log("Left");

        }
        else if (!IsPointLeft((transform.position - groundOffset) + CornerPoints[3], (transform.position - groundOffset) + CornerPoints[2], maxPosition))
        {
            // max position is outside the screen to the right
            // Could have done corner[2], corner[3], then not had the !, (the line would have been updside down) but this way seems more readable
            growFOV = true;
            //Debug.Log("Right");

        }

        // start shrinking when this isn't true
        minPosition -= new Vector3(2, 0, 2);
        maxPosition -= new Vector3(-2, 0, -2);
        shrinkFOV = true;

        if (minPosition.z < ((transform.position - groundOffset) + CornerPoints[0]).z)
        {
            // still too far out of bounds to shrink back
            //shrinkFOV = false;
        }
        else
        {
            if (bottomOffset.z < 0)
            {
                bottomOffset = Vector3.Lerp(bottomOffset, Vector3.zero, 0.1f);
            }
        }
        if (maxPosition.z > ((transform.position - groundOffset) + CornerPoints[1]).z)
        {
            // still too far out of bounds to shrink back
            shrinkFOV = false;
        }
        else if (IsPointLeft((transform.position - groundOffset) + CornerPoints[0], (transform.position - groundOffset) + CornerPoints[1], minPosition))
        {

            shrinkFOV = false;

        }
        else if (!IsPointLeft((transform.position - groundOffset) + CornerPoints[3], (transform.position - groundOffset) + CornerPoints[2], maxPosition))
        {

            shrinkFOV = false;

        }



        if (growFOV && !gameOver)
        {
            // if the game is over, stop adjusting the FOV
            // return the FOV go back to normal
            // if outside, make the FOV larger
            cameraComponent.fieldOfView += 0.25f;
            CalculateRelativeCorners();
        }
        else if(shrinkFOV || gameOver)
        {
            // if not outside, make the FOV go back to normal
            if (cameraComponent.fieldOfView > minFOV)
            {
                cameraComponent.fieldOfView -= 0.1f;
                //cameraComponent.fieldOfView = Mathf.Lerp(cameraComponent.fieldOfView, minFOV, 0.1f);
                // if FOV dips below the minimum, put it equal to the min
                cameraComponent.fieldOfView = Mathf.Max(cameraComponent.fieldOfView, minFOV);
                
                CalculateRelativeCorners();
            }
            
            //cameraComponent.fieldOfView = minFOV;
        }
        
    }

    bool IsPointLeft(Vector3 linePointA, Vector3 linePointB, Vector3 pointC)
    {
        // number > 0 is to the left
        // number < 0 is to the right
        // number == 0 is on the line
        return ((linePointB.x - linePointA.x)*(pointC.z - linePointA.z) - (linePointB.z - linePointA.z)*(pointC.x - linePointA.x)) > 0;
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
		focusPoint = focalPoint + groundOffset + bottomOffset;
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
