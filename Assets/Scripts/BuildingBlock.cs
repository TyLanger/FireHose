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

	float distantBurnMultiplier = 0.65f;

	public event Action SetAlight;
	public event Action DestroyedByFire;
	public event Action FireQuenched;

	public GameObject fire;
	public GameObject rubble;
	public GameObject visuals;

	// fire particles
	public ParticleSystem fireParticles;
	public Material smoulderMat;
	public Color smoulderColour;
	ParticleSystem.EmissionModule fireEmission;
	ParticleSystem.MainModule fireMain;
	ParticleSystemRenderer fireRenderer;
	ParticleSystem.RotationOverLifetimeModule fireRotation;

	int smallEmission = 6;
	int largeEmission = 15;
	int smoulderEmission = 1;

	// don't change lifetime
	// change start speed
	// slower start speed means flames dont go as high
	float smallStartSpeed = 2;
	float largeStartSpeed = 4;
	float smoulderStartSpeed = 1;

	float smoulderStartSize = 0.7f;

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
		fireRenderer = fireParticles.GetComponent<ParticleSystemRenderer> ();
		fireRotation = fireParticles.rotationOverLifetime;

		// rotate doors
		if (blockType == BlockType.Door) {
			if (zPos > 0) {
				if (grid [xPos, zPos - 1].GetComponentInChildren<BuildingBlock> ().blockType == BlockType.Wall) {
					// the block below is a wall
					// rotate 90
					transform.RotateAround (transform.position, Vector3.up, 90);
				}
			}
		}
	}

	IEnumerator Burning()
	{
		// small fire particles
		fireEmission.rateOverTime = smallEmission;
		fireMain.startSpeed = smallStartSpeed;

		// some fires start out half grown
		// drops time to lose ~ 10 seconds
		//float r = UnityEngine.Random.Range (0, 0.75f);
		//currentFuelSeconds += r * r * growthTime;
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
		// turn off the collider so it can't set people on fire
		fire.GetComponent<CapsuleCollider> ().enabled = false;
		// Destroyed floor tiles may not have rubble
		if (rubble != null) {
			rubble.SetActive (true);
			rubble.transform.Rotate (Vector3.up, UnityEngine.Random.Range (0, 360));
		}
		DestroyBlock ();

		// smoulder particles
		fireEmission.rateOverTime = smoulderEmission;
		fireMain.startSpeed = smoulderStartSpeed;


		if (DestroyedByFire != null) {
			DestroyedByFire ();
		}

		// wait a bit to let the particles change to the new rate and speed
		yield return new WaitForSeconds(1);

		if (smoulderMat != null) {
			fireRenderer.material = smoulderMat;
			fireRenderer.material.color = smoulderColour;
			fireRotation.enabled = true;
			fireMain.startSize = smoulderStartSize;
		}
	}

	///  Burn neighbours (4 connected) plus spaces 2 away for 12 total
	void BurnNeighbours()
	{
		// add some randomness to the fire
		// with 0.8 to 1.5 squared, time to lose drops ~8 seconds (meaning you lose 8 seconds sooner if you don't put out any fires
		float r = UnityEngine.Random.Range (0.8f, 1.5f);
		r *= r;
		if (xPos + 1 < grid.GetLength (0)) {
			grid [xPos + 1, zPos].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * r);
		}
		if (xPos > 0) {
			grid [xPos - 1, zPos].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * r);
		}
		if (zPos + 1 < grid.GetLength (1)) {
			grid [xPos, zPos + 1].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * r);
		}
		if (zPos > 0) {
			grid [xPos, zPos - 1].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * r);
		}


		// burn 2 distance away
		if (xPos + 2 < grid.GetLength (0)) {
			grid [xPos + 2, zPos].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * r * distantBurnMultiplier);
		}
		if (xPos-1 > 0) {
			grid [xPos - 2, zPos].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * r * distantBurnMultiplier);
		}
		if (zPos + 2 < grid.GetLength (1)) {
			grid [xPos, zPos + 2].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * r * distantBurnMultiplier);
		}
		if (zPos-1 > 0) {
			grid [xPos, zPos - 2].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * r * distantBurnMultiplier);
		}


		// diagonal
		if (xPos + 1 < grid.GetLength (0) && zPos + 1 < grid.GetLength (1)) {
			grid [xPos + 1, zPos+1].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * r * distantBurnMultiplier);
		}
		if (xPos > 0 && zPos + 1 < grid.GetLength (1)) {
			grid [xPos - 1, zPos+1].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * r * distantBurnMultiplier);
		}
		if (xPos + 1 < grid.GetLength (0) && zPos > 0) {
			grid [xPos+1, zPos - 1].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * r * distantBurnMultiplier);
		}
		if (xPos > 0 && zPos > 0) {
			grid [xPos-1, zPos - 1].GetComponentInChildren<BuildingBlock> ().Burn (Time.fixedDeltaTime * r * distantBurnMultiplier);
		}
	}

	public void ReduceLightTime(int time)
	{
		// Called by furniture being on the tile
		// reduce the time to light and grow to a big fire
		// this makes the fire spread faster on these tiles
		// but raise the destroy time so the total time stays the same.
		lightTime -= time;
		growthTime -= time;
		destroyTime += time * 2;
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
				//transform.FindChild ("Fire").gameObject.SetActive (true);
				fire.SetActive (true);
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
				// after the fire is put out, take double the time to relight
				currentFuelSeconds = - lightTime;
				// temporary to visually debug
				//transform.position = transform.position + new Vector3 (0, -0.25f, 0);
				//transform.FindChild ("Fire").gameObject.SetActive (false);
				fire.SetActive (false);

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
				//GetComponent<Collider> ().enabled = false;
				//GetComponent<MeshRenderer>().enabled = false;
				DestroyBlock ();
			}
		}
	}

	void DestroyBlock()
	{
		// hides the visuals and disables the collider

		// this the renderer for the boxCollider
		// walls and floor may keep this.
		// doors and furniture will probably use models instead
		if (blockType != BlockType.Floor) {
			// floor tiles just get a scorch mark on top of them
			GetComponent<Collider> ().enabled = false;
			GetComponent<MeshRenderer> ().enabled = false;
		}
		if (visuals != null) {
			// this would be the door model
			visuals.SetActive (false);
		}
	}
		

}
