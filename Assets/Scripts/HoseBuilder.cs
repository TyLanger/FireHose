﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoseBuilder : MonoBehaviour {

	public Transform nozzle;
    public float nozzleHeight;
	public Transform hydrant;
    public float hydrantHeight;


    public int numSegments = 10;

	public float maxSeparation = 0.4f;
	public float minSeparation = 0.1f;

	public float moveSpeed = 0.1f;

	public HoseSegment hoseSegment;

    HoseSegment first;
    HoseSegment last;


    // Mesh
    Mesh mesh;
    MeshFilter meshFilter;
    float hoseWidth = 0.2f;

	// Use this for initialization
	void Start () {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        //InitMesh();

        HoseSegment previous = hoseSegment;
		HoseSegment copy = hoseSegment;
        bool top = true;
        Vector3 position = Vector3.zero;
		for (int i = 0; i < numSegments; i++) {

            if(i%3==0)
            {
                top = !top;
            }
            if(top)
            {
                position = new Vector3((i + 1) * (minSeparation - 0.1f), 0.05f, 0.5f);
            }
            else
            {
                position = new Vector3((i + 1) * (minSeparation - 0.1f), 0.05f, 0);
            }
            // new Vector3 ((i+1) * (minSeparation+0.1f), 0.05f, 0)
            copy = Instantiate (hoseSegment, hydrant.position + position, Quaternion.identity);
			copy.maxSeparation = this.maxSeparation;
			copy.minSeparation = this.minSeparation;
			copy.moveSpeed = this.moveSpeed;
            // keep track of where in the line a segment is
            copy.segmentNumber = i+1;
            copy.totalSegments = numSegments;
			if (i == 0) {
				// first in list
				previous = copy;
				previous.prev = hydrant;
				previous.hoseType = HoseSegment.HoseType.Start;
                first = previous;
			} else {
				copy.prev = previous.transform;
				previous.next = copy.transform;
				previous = copy;
			}

		}
        last = copy;
		copy.hoseType = HoseSegment.HoseType.End;
		copy.next = nozzle;
        // Nozzle is larger than segments; give it more space
        copy.minSeparation *= 2;
        nozzle.parent.transform.position = copy.transform.position + new Vector3((minSeparation + 0.1f) * 2, 0.6f, 0);


        FindObjectOfType<GameManager>().GameOver += DetachNozzle;
	}
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.identity;
        UpdateMesh();
        
    }

    void DetachNozzle()
    {
        // when the players get teleprted, if one has the nozzle, it causes problems
        // let them keep the nozzle, but remove the hose
        // The hose stays where it is with this fake transform for it to point to
        GameObject fakeTrans = new GameObject("Fake");
        fakeTrans.transform.position = nozzle.position;
        // change nozzle to fake to stop drawing the mesh to the real nozzle
        nozzle = fakeTrans.transform;
        // change first.next to fake to stop the segements pulling the player
        last.GetComponent<HoseSegment>().next = fakeTrans.transform;
    }

    void UpdateMesh()
    {
        // each segment has 2 vertices
        // Plus 1 for the nozzle
        Vector3[] verts = new Vector3[(numSegments+1) * 2];
        // Each segment has 2 triangles towards the next segment
        // except the first one.
        // Each triangle has 3 points
        // Plus 1 for the nozzle
        int[] tris = new int[(numSegments+1 - 1) * 6];

        

        
        int trisIndex = 0;
        int trisCount = 0;
        HoseSegment current = first.next.GetComponent<HoseSegment>();
        Vector3 previousPos;
        Vector3 direction = (current.transform.position - first.transform.position);

        verts[0] = (first.transform.position ) + Vector3.up * hydrantHeight + new Vector3(-direction.z, 0, direction.x).normalized * hoseWidth;
        verts[1] = (first.transform.position) + Vector3.up * hydrantHeight + new Vector3(-direction.z, 0, direction.x).normalized * -hoseWidth;

        int vertIndex = 2;


        while (current.hoseType != HoseSegment.HoseType.End)
        {
            if (vertIndex == 2 || vertIndex == 4)
            {
                // make the verts near the hydrant higher
                verts[vertIndex] = (current.transform.position) + Vector3.up * hydrantHeight/vertIndex + new Vector3(-direction.z, 0, direction.x).normalized * hoseWidth;
                verts[vertIndex + 1] = (current.transform.position) + Vector3.up * hydrantHeight/vertIndex + new Vector3(-direction.z, 0, direction.x).normalized * -hoseWidth;
            }
            else
            {
                verts[vertIndex] = (current.transform.position) + new Vector3(-direction.z, 0, direction.x).normalized * hoseWidth;
                verts[vertIndex + 1] = (current.transform.position) + new Vector3(-direction.z, 0, direction.x).normalized * -hoseWidth;
            }
            vertIndex += 2;

            tris[trisIndex] = trisCount + 0;
            tris[trisIndex+1] = trisCount + 2;
            tris[trisIndex+2] = trisCount + 1;

            tris[trisIndex+3] = trisCount + 1;
            tris[trisIndex+4] = trisCount + 2;
            tris[trisIndex+5] = trisCount + 3;

            trisCount += 2;
            trisIndex += 6;

            

            previousPos = current.transform.position;
            current = current.next.GetComponent<HoseSegment>();
            direction = (current.transform.position - previousPos);
        }

        // Attach the mesh to the last Hose Segment 
        verts[vertIndex] = (current.transform.position) + Vector3.up * nozzleHeight*0.5f + new Vector3(-direction.z, 0, direction.x).normalized * hoseWidth;
        verts[vertIndex + 1] = (current.transform.position ) + Vector3.up * nozzleHeight*0.5f + new Vector3(-direction.z, 0, direction.x).normalized * -hoseWidth;
        vertIndex += 2;

        tris[trisIndex] = trisCount + 0;
        tris[trisIndex + 1] = trisCount + 2;
        tris[trisIndex + 2] = trisCount + 1;

        tris[trisIndex + 3] = trisCount + 1;
        tris[trisIndex + 4] = trisCount + 2;
        tris[trisIndex + 5] = trisCount + 3;

        trisCount += 2;
        trisIndex += 6;

        verts[vertIndex] = nozzle.position + Vector3.up * nozzleHeight + new Vector3(-direction.z, 0, direction.x).normalized * hoseWidth;
        verts[vertIndex + 1] = nozzle.position + Vector3.up * nozzleHeight + new Vector3(-direction.z, 0, direction.x).normalized * -hoseWidth;

        tris[trisIndex] = trisCount + 0;
        tris[trisIndex + 1] = trisCount + 2;
        tris[trisIndex + 2] = trisCount + 1;

        tris[trisIndex + 3] = trisCount + 1;
        tris[trisIndex + 4] = trisCount + 2;
        tris[trisIndex + 5] = trisCount + 3;

        mesh.vertices = verts;
        mesh.triangles = tris;

        mesh.RecalculateNormals();
        // keeps the mesh visible even when this game object is off the screen
        mesh.RecalculateBounds();
    }
}
