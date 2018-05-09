using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BuildingBlock : MonoBehaviour {


	public enum BlockType { Wall, Door, Floor, Furniture};

	// takes this many seconds to set a light
	// small fire takes this long to turn to a big fire
	// burns for this long before it's destroyed
	public float fuelSeconds = 10;
	float currentFuelSeconds = 0;
	bool onFire = false;

	float distantBurnMultiplier = 0.5f;

	public event Action SetAlight;
	public event Action DestroyedByFire;


	// doors and furniture can be destroyed by hand
	bool canBeDestroyedByHand = false;
	// hitpoints style
	// if over 10, cannot be destroyed by hand
	// otherwise, player needs to pull on it for that many seconds?
	// some structures > 20 cannot be destroyed by axe (concrete, steel)
	int hitPoints = 10;

	GameObject[,] grid;
	int xPos;
	int zPos;

	/// Tell the block about its neighbours
	public void Initialize(ref GameObject[,] _grid, int _xPos, int _zPos)
	{
		grid = _grid;
		xPos = _xPos;
		zPos = _zPos;
	}

	IEnumerator Burning()
	{
		while (currentFuelSeconds < fuelSeconds) {
			currentFuelSeconds += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}
		// now a big fire
		// temporary to see where the fire is
		transform.position = transform.position + new Vector3(0, 0.1f, 0);

		currentFuelSeconds = 0;
		while (currentFuelSeconds < fuelSeconds) {
			currentFuelSeconds += Time.fixedDeltaTime;
			BurnNeighbours ();
			yield return new WaitForFixedUpdate();
		}
		// destroyed
		if (DestroyedByFire != null) {
			DestroyedByFire ();
		}
	}

	///  Burn neighbours (4 connected)
	void BurnNeighbours()
	{
		
		if (xPos + 1 < grid.GetLength (0)) {
			grid [xPos + 1, zPos].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime);
		}
		if (xPos > 0) {
			grid [xPos - 1, zPos].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime);
		}
		if (zPos + 1 < grid.GetLength (1)) {
			grid [xPos, zPos + 1].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime);
		}
		if (zPos > 0) {
			grid [xPos, zPos - 1].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime);
		}


		// burn 2 distance away
		if (xPos + 2 < grid.GetLength (0)) {
			grid [xPos + 2, zPos].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * distantBurnMultiplier);
		}
		if (xPos-1 > 0) {
			grid [xPos - 2, zPos].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * distantBurnMultiplier);
		}
		if (zPos + 2 < grid.GetLength (1)) {
			grid [xPos, zPos + 2].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * distantBurnMultiplier);
		}
		if (zPos-1 > 0) {
			grid [xPos, zPos - 2].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * distantBurnMultiplier);
		}


		// diagonal
		if (xPos + 1 < grid.GetLength (0) && zPos + 1 < grid.GetLength (1)) {
			grid [xPos + 1, zPos+1].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * distantBurnMultiplier);
		}
		if (xPos > 0 && zPos + 1 < grid.GetLength (1)) {
			grid [xPos - 1, zPos+1].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * distantBurnMultiplier);
		}
		if (xPos + 1 < grid.GetLength (0) && zPos > 0) {
			grid [xPos+1, zPos - 1].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * distantBurnMultiplier);
		}
		if (xPos > 0 && zPos > 0) {
			grid [xPos-1, zPos - 1].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * distantBurnMultiplier);
		}
	}

	/// Try to set this object on fire
	public void Burn(float heat)
	{
		if (!onFire) {
			currentFuelSeconds += heat;
			if (currentFuelSeconds > fuelSeconds) {
				onFire = true;
				currentFuelSeconds = 0;

				// temporary to see where the fire is
				transform.position = transform.position + new Vector3(0, 0.1f, 0);
				if (SetAlight != null) {
					SetAlight ();
				}
				StartCoroutine (Burning ());
			}
		}
	}

	public void Break()
	{
		// break this object with fists or an axe
		GetComponent<Collider>().enabled = false;
	}
		

}
