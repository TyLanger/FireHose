using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BlockType { Wall, Door, Floor, Furniture};

public class BuildingBlock : MonoBehaviour {


	public BlockType blockType;




	//public float fuelSeconds = 10;
	public float lightTime = 10;			// takes this many seconds to set a light
	public float growthTime = 10;			// small fire takes this long to turn to a big fire
	public float destroyTime = 20;			// burns for this long before it's destroyed
	float currentFuelSeconds = 0;
	bool onFire = false;
	bool destroyedByFire = false;

	float distantBurnMultiplier = 0.5f;

	public event Action SetAlight;
	public event Action DestroyedByFire;
	public event Action FireQuenched;


	// fire particles
	public ParticleSystem fireParticles;
	ParticleSystem.EmissionModule fireEmission;
	ParticleSystem.MainModule fireMain;

	int smallEmission = 6;
	int largeEmission = 15;
	int smoulderEmission = 5;

	// don't change lifetime
	// change start speed
	// slower start speed means flames dont go as high
	float smallStartSpeed = 2;
	float largeStartSpeed = 4;
	float smoulderStartSpeed = 1;


	// smoke particles


	// doors and furniture can be destroyed by hand, but not walls (they need an axe)
	public int hitPoints = 5;
	// strength needed to do any damage
	// if the strngth is higher than this, do damage
	public int breakThreshold = 0;

	GameObject[,] grid;
	int xPos;
	int zPos;

	/// Tell the block about its neighbours
	public void Initialize(ref GameObject[,] _grid, int _xPos, int _zPos)
	{
		grid = _grid;
		xPos = _xPos;
		zPos = _zPos;

		//fireParticles = GetComponentInChildren<ParticleSystem> ();
		fireEmission = fireParticles.emission;
		fireMain = fireParticles.main;
	}

	IEnumerator Burning()
	{
		// small fire particles
		fireEmission.rateOverTime = smallEmission;
		fireMain.startSpeed = smallStartSpeed;

		while (currentFuelSeconds < growthTime) {
			currentFuelSeconds += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}
		// now a big fire
		// adjust particles
		fireEmission.rateOverTime = largeEmission;
		fireMain.startSpeed = largeStartSpeed;


		//currentFuelSeconds = 0;
		while (currentFuelSeconds < destroyTime+growthTime) {
			currentFuelSeconds += Time.fixedDeltaTime;
			BurnNeighbours ();
			yield return new WaitForFixedUpdate();
		}
		// destroyed
		destroyedByFire = true;

		// smoulder particles
		fireEmission.rateOverTime = smoulderEmission;
		fireMain.startSpeed = smoulderStartSpeed;


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
			if (currentFuelSeconds > lightTime) {
				onFire = true;
				currentFuelSeconds = 0;

				// temporary to see where the fire is
				//transform.position = transform.position + new Vector3(0, 0.1f, 0);
				transform.FindChild ("Fire").gameObject.SetActive (true);
				if (SetAlight != null) {
					SetAlight ();
				}
				StartCoroutine ("Burning");
			}
		}
	}

	public void PutOutFire(float dousePower)
	{
		if (onFire && !destroyedByFire) {
			currentFuelSeconds -= dousePower;
			if (currentFuelSeconds < 0) {
				// fire is put out
				StopCoroutine ("Burning");
				onFire = false;
				currentFuelSeconds = 0;
				// temporary to visually debug
				//transform.position = transform.position + new Vector3 (0, -0.25f, 0);
				transform.FindChild ("Fire").gameObject.SetActive (false);

				if (FireQuenched != null) {
					FireQuenched ();
				}
			}
		}
	}

	public void Break(int breakStrength)
	{
		if (breakStrength > breakThreshold) {
			hitPoints -= breakStrength;
			if (hitPoints <= 0) {
				// break this object with fists or an axe
				GetComponent<Collider> ().enabled = false;
				GetComponent<MeshRenderer>().enabled = false;
			}
		}
	}
		

}
