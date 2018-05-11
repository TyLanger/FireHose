using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	// handles starting the game
	// setting up multiple players
	// ending the game when a win/loss condition is met

	public Player player;

	bool p1Spawned = false;
	bool p2Spawned = false;
	bool p3Spawned = false;
	bool p4Spawned = false;

	int numPlayers = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		// Joysticks are in the order they were plugged in
		if (!p1Spawned) {
			if (Input.GetButtonDown ("Pickup_P1")) {
				Debug.Log ("Player 1 joined");
				p1Spawned = true;
				numPlayers++;
				var copy = Instantiate (player, transform.position, Quaternion.identity);
				copy.Setup ("Horizontal_P1", "Vertical_P1", "Pickup_P1", "Use_P1");
			}
		}
		if (!p2Spawned) {
			if (Input.GetButtonDown ("Pickup_P2")) {
				Debug.Log ("Player 2 joined");
				p2Spawned = true;
				numPlayers++;
				var copy = Instantiate (player, transform.position + new Vector3 (1, 0, 0), Quaternion.identity);
				copy.Setup ("Horizontal_P2", "Vertical_P2", "Pickup_P2", "Use_P2");
			}
		}
		if (!p3Spawned) {
			if (Input.GetButtonDown ("Pickup_P3")) {
				Debug.Log ("Player 3 joined");
				p3Spawned = true;
				numPlayers++;
				var copy = Instantiate (player, transform.position + new Vector3 (2, 0, 0), Quaternion.identity);
				copy.Setup ("Horizontal_P3", "Vertical_P3", "Pickup_P3", "Use_P3");
			}
		}
		if (!p4Spawned) {
			if (Input.GetButtonDown ("Pickup_P4")) {
				Debug.Log ("Player 4 joined");
				p4Spawned = true;
				numPlayers++;
				var copy = Instantiate (player, transform.position + new Vector3 (3, 0, 0), Quaternion.identity);
				copy.Setup ("Horizontal_P4", "Vertical_P4", "Pickup_P4", "Use_P4");
			}
		}

	}
}
