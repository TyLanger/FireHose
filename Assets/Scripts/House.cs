using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class House : MonoBehaviour {

	[System.Serializable]
	public struct ColorMapObject
	{
		public Color colour;
		public GameObject item;
		public int numBlocks;
	}

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

	public bool useImage = false;
	public Texture2D floorLayout;
	public Texture2D furnitureLayout;
	public ColorMapObject[] mappings;
	public ColorMapObject[] furnitureMappings;

	public int numFiresStarted = 0;
	public int numFiresPutOut = 0;
	public int numBlocksDestroyedByFire = 0;
	float maxDestructionPercent = 0.75f;
	bool gameLost = false;

	public event Action<int, int, int> AllFiresPutOut;
	public event Action AllVictimsRescued;
	public event Action<int, int> HouseDestroyedByFire;

	int numVictims = 3;
	int numVictimsRescued = 0;

	void Initialize()
	{
		if (BlockParent != null) {
			Destroy (BlockParent);
		}
		BlockParent = new GameObject ("BlockParent");
		if (useImage) {
			xSize = floorLayout.width;
			zSize = floorLayout.height;
		}
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
				copy.GetComponentInChildren<BuildingBlock> ().SetAlight += NewFireStarted;
				copy.GetComponentInChildren<BuildingBlock> ().DestroyedByFire += BlockDestroyedByFire;
				copy.GetComponentInChildren<BuildingBlock> ().FireQuenched += FirePutOut;

			}
		}
	}

	public void CreateNewHouse(float maxDestroyPercent)
	{
		// if this percent of blocks destroyed by fire is reached, the house is considered destroyed by fire
		maxDestructionPercent = maxDestroyPercent;

		Initialize ();
		if (useImage) {
			// build houses using a layout image
			SetHouseFromImage();
		} else {
			// build houses procedurally
			SetRooms ();
			SetOuterWalls ();
			SetDoors ();
		}

		BuildHouse (transform.position, BlockParent.transform);
		if (useImage) {
			PlaceFurnitureFromImage (transform.position, BlockParent.transform);
		}

		PlaceRandomVictims();

		StartFire ();
	}

	// Places furniture based on an input image
	void PlaceFurnitureFromImage(Vector3 bottomLeft, Transform parent)
	{
		for (int i = 0; i < furnitureLayout.width; i++) {
			for (int j = 0; j < furnitureLayout.height; j++) {
				Color c = furnitureLayout.GetPixel (i, j);
				// ignore empty tiles in input image
				if (c.a > 0) {
					foreach (var furniture in furnitureMappings) {
						// furniture.colour.Equals (c) || 

						if (ColoursEqual (furniture.colour, c)) {
							if (furniture.numBlocks < 2) {
								// single block or default 0 if value isn't changed

						
								Instantiate (furniture.item, bottomLeft + new Vector3 (i * gridSpacing, 0, j * gridSpacing), Quaternion.AngleAxis (GetDegreesFromAlpha (c.a), Vector3.up), parent);
							} 
							/*
						else {
							// check neighbours to see what rotation it should have and where the center is
							switch (furniture.numBlocks) {
							case 2:
								if (i < furnitureLayout.width - 1) {
									Color right = furnitureLayout.GetPixel (i+1, j);
									if (furniture.colour.Equals (right)) {
										// item continues to the right
										// check where the wall is. If wall is above, don't rotate
										// if wall is below, rotate
										// but some things aren't against walls...
										// ignoring this for now
										// spawn in between the 2 tiles
										Instantiate (furniture.item, bottomLeft + new Vector3 ((i + 0.5f) * gridSpacing, 0, (j) * gridSpacing), Quaternion.identity, parent);
									} else {
										// pixel to the right didn't match; check above
										if (j < furnitureLayout.height - 1) {
											Color up = furnitureLayout.GetPixel (i, j + 1);
											if (furniture.colour.Equals (up)) {
												// pixel above matches
												// rotate
												Instantiate (furniture.item, bottomLeft + new Vector3 ((i) * gridSpacing, 0, (j + 0.5f) * gridSpacing), Quaternion.AngleAxis(90, Vector3.up), parent);
											}
										}
									}
								}
								// if above and right pixels don't match, that means you've already spwaned this
								break;
							}
						}
						*/

						}
					}
				}
			}
		}
	}

	bool ColoursEqual(Color a, Color b)
	{
		
		/*
		 * May have different colour channels hold different info
		 * ex. blue holds rotation
		if (a.r == b.r) {
			if (a.g == b.g) {
				if (a.b == b.b) {
					return true;
				}
			}
		}
		return false;
		*/

		// ignore alpha
		// alpha holds rotation
		return (a.r == b.r) && (a.g == b.g) && (a.b == b.b);
	}

	float GetDegreesFromAlpha(float alpha)
	{
		// alpha is 0.0 to 1.0 in unity
		// 0 to 255 in paint.net

		int a = Mathf.RoundToInt(255 * alpha);
		// alpha of 209 becomes 90 degree rotation
		return (a - 200) * 10;
	}

	/// Assigns the grid based on an input image
	void SetHouseFromImage()
	{
		for (int i = 0; i < floorLayout.width; i++) {
			for (int j = 0; j < floorLayout.height; j++) {
				Color c = floorLayout.GetPixel (i, j);
				//Debug.Log (c);
				// get the colour at that point in the image
				// check that colour against the available mapping objects
				foreach (var mapping in mappings) {
					if (mapping.colour.Equals (c)) {
						//Debug.Log ("Match colour: "+c);
						// if one matches, add the correct item to be created
						groundFloor [i, j] = mapping.item;
					}
				}
			}
		}
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
			if (HouseDestroyedByFire != null) {
				HouseDestroyedByFire (numBlocksDestroyedByFire, xSize*zSize);
			}
		}
	}

	void BlockDestroyed()
	{

	}
}
