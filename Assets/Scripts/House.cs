using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class House : MonoBehaviour {

	public int xSize = 20;
	public int zSize = 15;
	public float gridSpacing = 1;

	//public enum BuildingBlock { Floor, Wall, Door};

	int numRoomsPerSide = 3;

	public GameObject Wall;
	public GameObject Floor;
	public GameObject Door;
	public GameObject Victim;

	GameObject[,] groundFloor;
	GameObject BlockParent;

	public int numFiresStarted = 0;
	public int numFiresPutOut = 0;
	public int numBlocksDestroyedByFire = 0;
	public float maxDestructionPercent = 0.75f;
	bool gameLost = false;

	public event Action<int, int, int> AllFiresPutOut;
	public event Action AllVictimsRescued;

	int numVictims = 3;
	int numVictimsRescued = 0;

	void Initialize()
	{
		if (BlockParent != null) {
			Destroy (BlockParent);
		}
		BlockParent = new GameObject ("BlockParent");

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

				var copy = Instantiate (groundFloor [x,z], bottomLeftPos + new Vector3 (x * gridSpacing, 0, z * gridSpacing), transform.rotation, parent);
				copy.GetComponentInChildren<BuildingBlock> ().Initialize (ref groundFloor, x, z);
				groundFloor [x, z] = copy;

				// set up actions
				copy.GetComponentInChildren<BuildingBlock>().SetAlight += NewFireStarted;
				copy.GetComponentInChildren<BuildingBlock> ().DestroyedByFire += BlockDestroyedByFire;
				copy.GetComponentInChildren<BuildingBlock>().FireQuenched += FirePutOut;

			}
		}
	}

	public void CreateNewHouse()
	{
		
		Initialize ();
		SetRooms ();
		SetOuterWalls ();
		SetDoors ();

		BuildHouse (transform.position, BlockParent.transform);
		//RotateWalls ();
		PlaceRandomVictims();

		StartFire ();
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

	/// Set the front door
	void SetDoors()
	{
		groundFloor [xSize / 2, 0] = Door;
	}

	/// Set up interior rooms
	void SetRooms()
	{
		int roomDepth = zSize / numRoomsPerSide;
		// build x rooms
		for (int i = 0; i < numRoomsPerSide; i++) {
			// half of the rooms on the left side of the house
			// half on the right side

			// place interior walls
			// simple design of equal sized rooms mirrored on either side
			for (int x = 0; x < xSize; x++) {
				groundFloor [x, roomDepth * i] = Wall;
			}
		}

		int hallWidth = 5;
		// left side of the hall
		int hallXPos = (xSize / 2) - (hallWidth / 2);
		// place hallway in the middle
		for (int w = 0; w < hallWidth; w++) {
			for (int z = 0; z < zSize; z++) {
				if (w == 0 || w == hallWidth - 1) {
					groundFloor [hallXPos + w, z] = Wall;
				} else {
					groundFloor [hallXPos + w, z] = Floor;
				}
			}
		}


		// place doors
		for (int i = 0; i < numRoomsPerSide; i++) {
			// one door on the left room, one door on the right room
			groundFloor [hallXPos, roomDepth * i + (roomDepth/2)] = Door;
			groundFloor [hallXPos + hallWidth-1, roomDepth * i + (roomDepth/2)] = Door;

		}
	}

	/// Rotate walls to the correct orientation
	void RotateWalls()
	{
		for (int x = 0; x < xSize; x++) {
			for (int z = 0; z < zSize; z++) {
				if (groundFloor [x, z].GetComponentInChildren<BuildingBlock> ().blockType == BlockType.Wall) {
					// check neighbours
				}
			}
		}
	}

	void StartFire()
	{
		int x = UnityEngine.Random.Range (0, xSize);
		int z = UnityEngine.Random.Range (0, zSize);
		Debug.Log ("Starting fire at " + x + ", " + z);
		groundFloor [x,z].GetComponentInChildren<BuildingBlock> ().Burn (100);
		//groundFloor[x,z].GetC
	}

	void PlaceRandomVictims()
	{
		for (int i = 0; i < numVictims; i++) {
			int x = UnityEngine.Random.Range (0, xSize);
			int z = UnityEngine.Random.Range (0, zSize);

			if (groundFloor [x, z].GetComponentInChildren<BuildingBlock> ().blockType == BlockType.Floor) {
				// only spawn victims on the floor

				var copy = Instantiate (Victim, transform.position + new Vector3 (x * gridSpacing, 1, z * gridSpacing), transform.rotation);
				Victim v = copy.GetComponentInChildren<Victim> ();
				if (v != null) {
					// subscribe to an action to know when the victim is safe
					v.Rescued += VictimRescued;
				}
			} else {
				// tried to spawn in a wall
				// try again
				i--;
			}

		}
	}

	void NewFireStarted()
	{
		numFiresStarted++;
	}

	void FirePutOut()
	{
		numFiresPutOut++;
		if ((numFiresPutOut + numBlocksDestroyedByFire) == numFiresStarted) {
			// all fires put out
			if (AllFiresPutOut != null) {
				AllFiresPutOut (numFiresStarted, numFiresPutOut, numBlocksDestroyedByFire);
			}
		}
	}

	void VictimRescued()
	{
		numVictimsRescued++;
		if (numVictimsRescued == numVictims) {

			if (AllVictimsRescued != null) {
				AllVictimsRescued ();
			}
		}
	}

	void BlockDestroyedByFire()
	{
		numBlocksDestroyedByFire++;
		if (numBlocksDestroyedByFire > (xSize * zSize) * maxDestructionPercent && !gameLost) {
			gameLost = true;
			Debug.Log("You lose. "+numBlocksDestroyedByFire + " blocks destroyed by fire");
		}
	}

	void BlockDestroyed()
	{

	}
}
