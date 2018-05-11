﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	// handles starting the game
	// setting up multiple players
	// ending the game when a win/loss condition is met

	public Player player;

	Player player1;
	Player player2;
	Player player3;
	Player player4;

	// default spawn points
	Vector3 p1Spawn = new Vector3(1, 1.5f, 0);
	Vector3 p2Spawn = new Vector3(3, 1.5f, 0);
	Vector3 p3Spawn = new Vector3(1, 1.5f, -2);
	Vector3 p4Spawn = new Vector3(3, 1.5f, -2);

	bool p1Joined = false;
	bool p2Joined = false;
	bool p3Joined = false;
	bool p4Joined = false;

	int numPlayers = 0;

	// Use this for initialization
	void Start () {
		SpawnPlayers ();
	}
	
	// Update is called once per frame
	void Update () {

		// Joysticks are in the order they were plugged in
		if (!p1Joined) {
			if (Input.GetButtonDown ("Pickup_P1")) {
				Debug.Log ("Player 1 joined");
				p1Joined = true;
				numPlayers++;
				// set up controls when you get input from the controller
				player1.Setup ("Horizontal_P1", "Vertical_P1", "Pickup_P1", "Use_P1");
			}
		}
		if (!p2Joined) {
			if (Input.GetButtonDown ("Pickup_P2")) {
				Debug.Log ("Player 2 joined");
				p2Joined = true;
				numPlayers++;

				player2.Setup ("Horizontal_P2", "Vertical_P2", "Pickup_P2", "Use_P2");
			}
		}
		if (!p3Joined) {
			if (Input.GetButtonDown ("Pickup_P3")) {
				Debug.Log ("Player 3 joined");
				p3Joined = true;
				numPlayers++;

				player3.Setup ("Horizontal_P3", "Vertical_P3", "Pickup_P3", "Use_P3");
			}
		}
		if (!p4Joined) {
			if (Input.GetButtonDown ("Pickup_P4")) {
				Debug.Log ("Player 4 joined");
				p4Joined = true;
				numPlayers++;

				player4.Setup ("Horizontal_P4", "Vertical_P4", "Pickup_P4", "Use_P4");
			}
		}

	}

	void SpawnPlayers()
	{
		// spawn all 4 characters at the start
		// they only start to move once their respective joystick has been moved
		// any characters not joined by the time the leve starts just get left behind at the fire hall
		// they still exist in the world, but they'll just be napping at a table or something
		player1 = Instantiate (player, p1Spawn, Quaternion.identity);
		player1.GetComponent<Renderer> ().material.color = Color.red;
		player2 = Instantiate (player, p2Spawn, Quaternion.identity);
		player2.GetComponent<Renderer> ().material.color = Color.blue;
		player3 = Instantiate (player, p3Spawn, Quaternion.identity);
		player3.GetComponent<Renderer> ().material.color = Color.green;
		player4 = Instantiate (player, p4Spawn, Quaternion.identity);
		player4.GetComponent<Renderer> ().material.color = Color.yellow;

	}
}