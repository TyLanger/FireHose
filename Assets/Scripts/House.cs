using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour {

	public int xSize = 20;
	public int zSize = 15;
	public float gridSpacing = 1;

	//public enum BuildingBlock { Floor, Wall, Door};

	public GameObject Wall;
	public GameObject Floor;
	public GameObject Door;

	GameObject[,] groundFloor;

	void Update()
	{
		if (Input.GetButtonDown ("Jump")) {
			CreateNewHouse ();
		}
	}

	void Initialize()
	{
		groundFloor = new GameObject[xSize,zSize];
		for (int x = 0; x < xSize; x++) {
			for (int z = 0; z < zSize; z++) {
				groundFloor [x, z] = Floor;
			}
		}
	}

	/// Instantiates all the objects to make the house
	void BuildHouse(Vector3 bottomLeftPos, Transform parent)
	{
		for (int x = 0; x < xSize; x++) {
			for (int z = 0; z < zSize; z++) {

				Instantiate (groundFloor [x,z], bottomLeftPos + new Vector3 (x * gridSpacing, 0, z * gridSpacing), transform.rotation, parent);


			}
		}
	}

	public void CreateNewHouse()
	{
		Initialize ();
		SetOuterWalls ();

		PlaceDoors ();

		BuildHouse (transform.position, transform);
	}

	void SetOuterWalls()
	{
		for (int x = 0; x < xSize; x++) {
			for (int z = 0; z < zSize; z++) {
				if(x == 0 || z == 0 || x == xSize-1 || z == zSize-1)
				{
					groundFloor [x,z] = Wall;
				}
			}
		}
	}

	void PlaceDoors()
	{
		groundFloor [xSize / 2, 0] = Door;
	}
}
