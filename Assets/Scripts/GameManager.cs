﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour {

	// handles starting the game
	// setting up multiple players
	// ending the game when a win/loss condition is met

	public CameraController cameraController;
	public Player player;

	Player player1;
	Player player2;
	Player player3;
	Player player4;

	// default spawn points
	Vector3 p1Spawn = new Vector3(11, 1.5f, 0);
	Vector3 p2Spawn = new Vector3(13, 1.5f, 0);
	Vector3 p3Spawn = new Vector3(11, 1.5f, -2);
	Vector3 p4Spawn = new Vector3(13, 1.5f, -2);

	bool p1Joined = false;
	bool p2Joined = false;
	bool p3Joined = false;
	bool p4Joined = false;

	int numPlayers = 0;

	public House house;
	[Range(0, 1)]
	public float maxDestructionPercent = 0.5f;

    public Texture2D map1Layout;
    public Texture2D map1Furniture;
    public Texture2D map2Layout;
    public Texture2D map2Furniture;


    bool allFiresOut = false;
	bool allRescued = false;

    Vector3 minPlayerPos;
    Vector3 maxPlayerPos;

    public event Action GameOver;

    // Use this for initialization
    void Start () {

		
		house.AllFiresPutOut += AllFiresPutOut;
		house.AllVictimsRescued += AllVictimsRescued;
		house.HouseDestroyedByFire += HouseDestroyedByFire;

        minPlayerPos = Vector3.zero;
        maxPlayerPos = Vector3.zero;

        Menu menu = FindObjectOfType<Menu>();
        numPlayers = menu.numPlayers;
        StartHouse(menu.houseHumber, numPlayers);

        p1Joined = menu.p1Joined;
        p2Joined = menu.p2Joined;
        p3Joined = menu.p3Joined;
        p4Joined = menu.p4Joined;
        SpawnPlayers();

        if (p1Joined)
        {
            player1.Setup("Horizontal_P1", "Vertical_P1", "Pickup_P1", "Use_P1", 1);
        }
        if (p2Joined)
        {
            player2.Setup("Horizontal_P2", "Vertical_P2", "Pickup_P2", "Use_P2", 2);
        }
        if (p3Joined)
        {
            player3.Setup("Horizontal_P3", "Vertical_P3", "Pickup_P3", "Use_P3", 3);
        }
        if (p4Joined)
        {
            player4.Setup("Horizontal_P4", "Vertical_P4", "Pickup_P4", "Use_P4", 4);
        }

    }

    // Update is called once per frame
    void Update () {

        /* Use the UI buttons
		if (Input.GetButtonDown ("Jump")) {
			house.CreateNewHouse (maxDestructionPercent);
		}
        */
        /*
		if (Input.GetButtonDown ("Fire3")) {
            //TeleportPlayers ();
            GameWon();
            //Debug.Break();
		}*/

		// Joysticks are in the order they were plugged in
        /*
		if (!p1Joined) {
			
			if (Input.GetButtonDown ("Pickup_P1")) {
				Debug.Log ("Player 1 joined");
				p1Joined = true;
				//numPlayers++;
				// set up controls when you get input from the controller
				player1.Setup ("Horizontal_P1", "Vertical_P1", "Pickup_P1", "Use_P1", 1);
			}
		}
		if (!p2Joined) {
			if (Input.GetButtonDown ("Pickup_P2")) {
				Debug.Log ("Player 2 joined");
				p2Joined = true;
				//numPlayers++;

				player2.Setup ("Horizontal_P2", "Vertical_P2", "Pickup_P2", "Use_P2", 2);
			}
		}
		if (!p3Joined) {
			if (Input.GetButtonDown ("Pickup_P3")) {
				Debug.Log ("Player 3 joined");
				p3Joined = true;
				//numPlayers++;

				player3.Setup ("Horizontal_P3", "Vertical_P3", "Pickup_P3", "Use_P3", 3);
			}
		}
		if (!p4Joined) {
			if (Input.GetButtonDown ("Fire1")) {
				Debug.Log ("Keyboard as P4");
				p4Joined = true;
				//numPlayers++;
				player4.Setup("Horizontal", "Vertical", "Fire1", "Fire2", 4);
			}
			if (Input.GetButtonDown ("Pickup_P4")) {
				Debug.Log ("Player 4 joined");
				p4Joined = true;
				//numPlayers++;

				player4.Setup ("Horizontal_P4", "Vertical_P4", "Pickup_P4", "Use_P4", 4);
			}
		}
        */

        // adjust position of camera
		cameraController.FocusOn (AveragePlayerLocation ());
        // adjust FOV of camera
        if (numPlayers > 0)
        {
            MinMaxPositions(out minPlayerPos, out maxPlayerPos);
            cameraController.AdjustFOV(minPlayerPos, maxPlayerPos);
        }
	}

	void SpawnPlayers()
	{
        // spawn all 4 characters at the start
        // they only start to move once their respective joystick has been moved
        // any characters not joined by the time the leve starts just get left behind at the fire hall
        // they still exist in the world, but they'll just be napping at a table or something
        if (p1Joined)
        {
            player1 = Instantiate(player, p1Spawn, Quaternion.identity);
            //player1.GetComponent<Renderer> ().material.color = Color.red;
            player1.SetShirtColour(Color.red);
        }
        if (p2Joined)
        {
            player2 = Instantiate(player, p2Spawn, Quaternion.identity);
            //player2.GetComponent<Renderer> ().material.color = Color.blue;
            player2.SetShirtColour(Color.blue);
        }
        if (p3Joined)
        {
            player3 = Instantiate(player, p3Spawn, Quaternion.identity);
            //player3.GetComponent<Renderer> ().material.color = Color.green;
            player3.SetShirtColour(Color.green);
        }
        if (p4Joined)
        {
            player4 = Instantiate(player, p4Spawn, Quaternion.identity);
            //player4.GetComponent<Renderer> ().material.color = Color.yellow;
            player4.SetShirtColour(Color.yellow);
        }
	}

	Vector3 AveragePlayerLocation()
	{
		Vector3 average = Vector3.zero;
		if (numPlayers > 0 && (p1Joined || p2Joined || p3Joined || p4Joined)) {
			
			if (p1Joined) {
				average += player1.transform.position;
			}
			if (p2Joined) {
				average += player2.transform.position;
			}
			if (p3Joined) {
				average += player3.transform.position;
			}
			if (p4Joined) {
				average += player4.transform.position;
			}

			average = average / numPlayers;
		} else {
            // this doesn't work anymore
            // now that players are only spawned if they pressed a in the menu
            /*
            average += player1.transform.position;
			average += player2.transform.position;
			average += player3.transform.position;
			average += player4.transform.position;
			average = average / 4;
            */
		}
		return average;
	}

    void MinMaxPositions(out Vector3 minPosition, out Vector3 maxPosition)
    {

        float minX = 100;
        float maxX = -100;
        float minZ = 100;
        float maxZ = -100;

        if(numPlayers > 0)
        {
            if (p1Joined)
            {
                minX = Mathf.Min(player1.transform.position.x, minX);
                maxX = Mathf.Max(player1.transform.position.x, maxX);
                minZ = Mathf.Min(player1.transform.position.z, minZ);
                maxZ = Mathf.Max(player1.transform.position.z, maxZ);

            }
            if (p2Joined)
            {
                minX = Mathf.Min(player2.transform.position.x, minX);
                maxX = Mathf.Max(player2.transform.position.x, maxX);
                minZ = Mathf.Min(player2.transform.position.z, minZ);
                maxZ = Mathf.Max(player2.transform.position.z, maxZ);
            }
            if (p3Joined)
            {
                minX = Mathf.Min(player3.transform.position.x, minX);
                maxX = Mathf.Max(player3.transform.position.x, maxX);
                minZ = Mathf.Min(player3.transform.position.z, minZ);
                maxZ = Mathf.Max(player3.transform.position.z, maxZ);
            }
            if (p4Joined)
            {
                minX = Mathf.Min(player4.transform.position.x, minX);
                maxX = Mathf.Max(player4.transform.position.x, maxX);
                minZ = Mathf.Min(player4.transform.position.z, minZ);
                maxZ = Mathf.Max(player4.transform.position.z, maxZ);
            }
        }

        minPosition = new Vector3(minX, 0, minZ);
        maxPosition = new Vector3(maxX, 0, maxZ);
    }

	void AllFiresPutOut(int firesStarted, int firesPutOut, int blocksDestroyedByFire)
	{
		Debug.Log ("All fires out. Total fires: "+firesStarted+", put out: "+firesPutOut+", blocks destroyed: "+blocksDestroyedByFire);
		allFiresOut = true;
		if (allFiresOut && allRescued) {
			GameWon ();
		}
	}

	void AllVictimsRescued()
	{
		Debug.Log ("All victims rescued");
		allRescued = true;
		if (allFiresOut && allRescued) {
			GameWon ();
		}
	}

	/// Teleports all active players to pose for a photo op in front of the house
	void TeleportPlayers()
	{
		Vector3 pos = house.transform.position + new Vector3 ((house.xSize/2) * house.gridSpacing, 0, -2);

        // tell the players to stop moving
        // if a player is using an axe when the game ends, they will keep moving....
        // Going to need some way to stop this
        if (p1Joined)
        {
            player1.GameOver();
        }
        if (p2Joined)
        {
            player2.GameOver();
        }
        if (p3Joined)
        {
            player3.GameOver();
        }
        if (p4Joined)
        {
            player4.GameOver();
        }

        // teleport the players
        if (p1Joined)
        {
            player1.transform.position = pos + new Vector3(-2, 0, 0);
        }
        if (p2Joined)
        {
            player2.transform.position = pos + new Vector3(-1, 0, 0);
        }
        if (p3Joined)
        {
            player3.transform.position = pos + new Vector3(0, 0, 0);
        }
        if (p4Joined)
        {
            player4.transform.position = pos + new Vector3(1, 0, 0);
        }
		//player1.transform.rotation = Quaternion.Euler (0, 180, 0);
		//player2.transform.rotation = Quaternion.Euler (0, 180, 0);
		//player3.transform.rotation = Quaternion.Euler (0, 180, 0);
		//player4.transform.rotation = Quaternion.Euler (0, 180, 0);

		cameraController.PhotoshootPosition ();


	}

    public void StartHouse(int houseNum, int numPlayers)
    {
        // start the game with the specified house setup
        if (houseNum == 1)
        {
            house.floorLayout = map1Layout;
            house.furnitureLayout = map1Furniture;
        }
        else if (houseNum == 2)
        {
            house.floorLayout = map2Layout;
            house.furnitureLayout = map2Furniture;
        }
        house.CreateNewHouse(maxDestructionPercent, numPlayers);
    }

	void GameWon()
	{
		Debug.Log ("Game Won");
        if(GameOver != null)
        {
            GameOver();
        }
		TeleportPlayers ();
	}

	void HouseDestroyedByFire(int blocksDestroyed, int totalBlocks)
	{
        if(GameOver != null)
        {
            GameOver();
        }
		Debug.Log ("Game Lost");
		TeleportPlayers ();

	}

    public void GoToMenu()
    {
        // the button calls this
        // the menu doesn't exist in the scene until the menu scene is run
        FindObjectOfType<Menu>().LoadMenu();
    }
}
