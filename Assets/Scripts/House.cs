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
		// reduce the time to burn of the floor below
		public int reducedBurnTime;
		public int width;
		public int height;
	}

	public int xSize = 20;
	public int zSize = 15;
	public float gridSpacing = 1;

	//public enum BuildingBlock { Floor, Wall, Door};

	int numRoomsPerSide = 3;

    public GameObject Boundary;
    public float boundaryHeight;
    // distance to put the boundaries away from the house
    public int sideBuffers = 3;
    public int topBuffer = 5;
    public int bottomBuffer = 10;
    public GameObject FoundationPlane;
    public GameObject GrassPlane;
    public GameObject Wall;
	public GameObject Floor;
	public GameObject Door;
	public GameObject Victim;

	GameObject[,] groundFloor;
	GameObject BlockParent;

    public GameObject[] tools;

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

    AudioSource audioSource;
    float minPitch = 0.6f;
    float maxPitch = 1.5f;
    //public AudioClip fireSmoulder;

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
        //audioSource = gameObject.AddComponent<AudioSource>();
        audioSource = GetComponent<AudioSource>();
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

    void BuildBoundaries(Transform parent)
    {
        // build boundaries to the left and right
        // build them sideBuffer tiles away
        // center is based on the size of the house and the size of the top and bottom buffers
        // so the boundaries are all encompassing
        // for even xSizes, the left boundary is 3, the right is 4; Oh well
        var leftB = Instantiate(Boundary, transform.position + new Vector3(-sideBuffers * gridSpacing, 0, (zSize+topBuffer-bottomBuffer)/2 * gridSpacing), Quaternion.identity, parent);
        leftB.transform.localScale = new Vector3(1, boundaryHeight, zSize + topBuffer + bottomBuffer);

        var rightB = Instantiate(Boundary, transform.position + new Vector3((xSize +sideBuffers) * gridSpacing, 0, (zSize + topBuffer - bottomBuffer) / 2 * gridSpacing), Quaternion.identity, parent);
        rightB.transform.localScale = new Vector3(1, boundaryHeight, zSize + topBuffer + bottomBuffer);

        var topB = Instantiate(Boundary, transform.position + new Vector3((xSize) / 2 * gridSpacing, 0, (zSize + topBuffer) * gridSpacing), Quaternion.identity, parent);
        topB.transform.localScale = new Vector3(xSize + sideBuffers*2, boundaryHeight, 1);

        var bottomB = Instantiate(Boundary, transform.position + new Vector3((xSize) / 2 * gridSpacing, 0, -bottomBuffer * gridSpacing), Quaternion.identity, parent);
        bottomB.transform.localScale = new Vector3(xSize + sideBuffers * 2, boundaryHeight, 1);

    }

    void MovePlanes()
    {
        // house is at 0.5 in the y. Counteract that
        FoundationPlane.transform.position = transform.position + new Vector3((xSize-1) * gridSpacing * 0.5f, -0.5f, (zSize-1)* gridSpacing * 0.5f);
        // this is the scaling for planes. They are already ~10x larger than other things
        FoundationPlane.transform.localScale = new Vector3(xSize, 1, zSize) * 0.1f;
        // move down 0.01 so it is below foundation
        GrassPlane.transform.position = transform.position + new Vector3(xSize* gridSpacing * 0.5f, -0.51f, zSize * gridSpacing * 0.5f);
        //GrassPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(20, 20);
        //GrassPlane.GetComponent<Renderer>().material.mainTexture.
    }

    void PlaceTools()
    {
        for (int i = 0; i < tools.Length; i++)
        {
            float spawnX = 0;
            switch(i)
            {
                case 0:
                    spawnX = 3 * gridSpacing;
                    break;
                case 1:
                    spawnX = 6 * gridSpacing;
                    break;
                case 2:
                    spawnX = (xSize - 7) * gridSpacing ;
                    break;
                case 3:
                    spawnX = (xSize - 4) * gridSpacing;
                    break;

            }
            for (int j = 0; j < 4; j++)
            {
                var t = Instantiate(tools[i], transform.position + new Vector3(spawnX, 0, -4 -(j*2)), Quaternion.identity);
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

        BuildBoundaries(BlockParent.transform);
        MovePlanes();

        PlaceTools();
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
							float angle = GetDegreesFromAlpha (c.a);
							Instantiate (furniture.item, bottomLeft + new Vector3 (i * gridSpacing, 0, j * gridSpacing), Quaternion.AngleAxis (angle, Vector3.up), parent);
							if (furniture.reducedBurnTime != 0) {
								for (int x = 0; x < furniture.width; x++) {
									for (int y = 0; y < furniture.height; y++) {
										// reduce the light time off all tiles
										// the tiles to be changed changes with the rotation
										switch((int)angle)
										{
										case 0:
											groundFloor [i + x, j + y].GetComponentInChildren<BuildingBlock> ().ReduceLightTime (furniture.reducedBurnTime);
											break;
										case 90:
											groundFloor [i - y, j + x].GetComponentInChildren<BuildingBlock> ().ReduceLightTime (furniture.reducedBurnTime);
											break;
										case 180:
											groundFloor [i - x, j - y].GetComponentInChildren<BuildingBlock> ().ReduceLightTime (furniture.reducedBurnTime);
											break;
										case 270:
											groundFloor [i + y, j - x].GetComponentInChildren<BuildingBlock> ().ReduceLightTime (furniture.reducedBurnTime);
											break;
										}
									}
								}

							}
						}
					}
				}
			}
		}
	}

	bool ColoursEqual(Color a, Color b)
	{

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
        audioSource.Play();
    }

	void PlaceRandomVictims()
	{
		for (int i = 0; i < numVictims; i++) {
			int x = UnityEngine.Random.Range (0, xSize);
			int z = UnityEngine.Random.Range (0, zSize);

			if (useImage) {
				Color c = furnitureLayout.GetPixel (x, z);
				if (c.a > 0) {
					// some furniiture exists here; do not spawn
					// but some spawn locations are also bad
					// the bed only takes up 1 pixel, but has 6-12 bad spawn spots
					// could put in the input image where to not spawn...
				}
			}

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

    void AdjustFireSound()
    {
        // raise the pitch more as more fires are started
        // up to the max when the number of current fires equals the number of blocks needed to be destroyed
        audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, (float)(numFiresStarted-numFiresPutOut)/ ((xSize * zSize) * maxDestructionPercent));
    }

	void NewFireStarted()
	{
		numFiresStarted++;
        AdjustFireSound();

    }

	void FirePutOut()
	{
		numFiresPutOut++;
        AdjustFireSound(); ;
        if ((numFiresPutOut + numBlocksDestroyedByFire) == numFiresStarted) {
            // all fires put out
            audioSource.Stop();
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
        /*
        if(numBlocksDestroyedByFire == 1)
        {
            audioSource.Play();
        }*/
		if (numBlocksDestroyedByFire > (xSize * zSize) * maxDestructionPercent && !gameLost) {
			gameLost = true;
			Debug.Log("You lose. "+numBlocksDestroyedByFire + " blocks destroyed by fire at "+Time.time);
			if (HouseDestroyedByFire != null) {
				HouseDestroyedByFire (numBlocksDestroyedByFire, xSize*zSize);
			}
		}
	}

	void BlockDestroyed()
	{

	}
}
