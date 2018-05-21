using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hose : MonoBehaviour {

	Vector3 anchorPoint;
	public Vector3 lastPoint;
	public Transform target;
	float maxLength = 100;
	float currentLength;
	bool moving = true;


	public Transform hitTrans;
	public bool canSee = false;

	LinkedList<Vector3> points;


	// Hose mesh
	Mesh mesh;
	MeshFilter meshFilter;
	float hoseWidth = 0.2f;

	void Start()
	{
		points = new LinkedList<Vector3> ();
		points.AddFirst (anchorPoint);

		meshFilter = GetComponent<MeshFilter> ();
		InitMesh ();
	}

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

			// Draw the line of where you've been
			LinkedListNode<Vector3> prev = points.First;
			while (prev.Next != null) {
				Debug.DrawLine (prev.Value, prev.Next.Value, Color.blue);
				prev = prev.Next;
			}
			Debug.DrawLine (points.Last.Value, target.position, Color.blue);

			//Debug.DrawRay (anchorPoint, (target.position - anchorPoint), Color.blue);
			Debug.DrawRay (lastPoint, Vector3.forward, Color.red);
			if (Physics.Raycast (anchorPoint, (target.position - anchorPoint), out hit)) {
				hitTrans = hit.transform;
				if (hit.transform == target) {
					canSee = true;
					lastPoint = hit.point;
					if (currentLength + hit.distance > maxLength) {
						Debug.Log ("Max length reached, can see.");
					}
				} else {
					//Debug.Break ();
					canSee = false;
					//anchorPoint = lastPoint;
					anchorPoint = anchorPoint + (lastPoint - anchorPoint).normalized * (hit.distance+0.1f);
					points.AddLast (anchorPoint);
					currentLength += (hit.distance + 0.1f);
					UpdateMesh (anchorPoint);
					if (currentLength > maxLength) {
						Debug.Log ("Max length reached");
					}
				}
			}

			yield return new WaitForFixedUpdate ();
		}
	}

	void InitMesh()
	{
		mesh = new Mesh ();
		meshFilter.mesh = mesh;

		Vector3[] verts = new Vector3[4];
		int[] tris = new int[6];

		verts [0] = anchorPoint + new Vector3 (-hoseWidth, 0, 0);
		verts [1] = anchorPoint + new Vector3 (hoseWidth, 0, 0);
		verts [2] = transform.position + new Vector3 (-hoseWidth, 0, 0);
		verts [3] = transform.position + new Vector3 (hoseWidth, 0, 0);

		tris [0] = 0;
		tris [1] = 2;
		tris [2] = 1;
		tris [3] = 1;
		tris [4] = 2;
		tris [5] = 3;

		mesh.vertices = verts;
		mesh.triangles = tris;

		mesh.RecalculateNormals ();

	}

	void UpdateMesh(Vector3 newPoint)
	{
		LinkedListNode<Vector3> lastPointNode = points.Last;
		Vector3 thisPoint = lastPointNode.Value;
		Vector3 prevPoint;
		// if the list has 2 or more items
		if (lastPointNode.Previous != null) {
			prevPoint = lastPointNode.Previous.Value;
		} else {
			// if only 1 item in list
			prevPoint = anchorPoint;
		}

		//mesh.vertices
		//mesh.triangles = int[] // 0, 1, 2, 1,2,3, 2,3,4, 3,4,5
		// each time, add 2 vertices and 2 triangles

		Vector3[] verts = mesh.vertices;
		int[] tris = mesh.triangles;

		Vector3[] newVerts = new Vector3[verts.Length + 2];
		int[] newTris = new int[tris.Length + 6];

		for (int i = 0; i < verts.Length; i++) {
			newVerts [i] = verts [i];
		}
		for (int i = 0; i < tris.Length; i++) {
			newTris [i] = tris [i];
		}

		// add new vertices based on the input point.
		// just add them to the left and right for now
		Vector3 direction = (thisPoint - prevPoint);

		// swapping x and z and making z negative is the perpendicular vector to the direction from the last point to this point.
		newVerts [newVerts.Length - 2] = thisPoint + new Vector3 (-direction.z, 0, direction.x).normalized * hoseWidth;
		newVerts [newVerts.Length - 1] = thisPoint + new Vector3 (-direction.z, 0, direction.x).normalized * -hoseWidth;

		// assign triangles
		// example: have 8 verts already
		int numVerts = verts.Length; // 8
		newTris [newTris.Length - 6] = numVerts - 2; // 6
		newTris [newTris.Length - 5] = numVerts;	 // 8
		newTris [newTris.Length - 4] = numVerts - 1; // 7

		newTris [newTris.Length - 3] = numVerts - 1; // 7
		newTris [newTris.Length - 2] = numVerts; 	 // 8
		newTris [newTris.Length - 1] = numVerts + 1; // 9



		mesh.vertices = newVerts;
		mesh.triangles = newTris;

		mesh.RecalculateNormals ();
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
