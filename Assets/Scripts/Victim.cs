using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Victim : Tool {

	float timeOfNextQuip = 0;
	float timeBetweenQuips = 4;
	public TextController textMesh;
	Vector3 textSpawnPoint = Vector3.up;
	static int lastHelpIndex;
	string[] helpMessages = {
		"Help",
		"Help me",
		"Hurry",
		"Please help me",
		"Put the fire out",
		"My house is on fire",
		"*Cough*",
		"*Cough* *Cough*",
		"Got smoke in my eyes",
		"Please, organic water only",
		"Save me"
	};

	string[] carryMessages = {
		"Woah",
		"You're strong",
		"Be gentle",
		"I can see my house from up here",
		"Put me down!",
		"Ya, horsie!",
		"Please don't drop me",
		"Wow",
		"Woah you're strong",
		"You sure are strong",
		"Do you even lift, bro?"

	};

	string[] rescueMessages = {
		"Thank you",
		"Thanks",
		"You're so kind",
		"My hero",
		"Thank you, kind sir",
		"I wanna be a fireman when I grow up",
		"You're the best",
		"It's cold out here"
	};

	bool beingCarried = false;

	public float throwForce = 100;
	// what position in the z direction the victim needs to be below to be considered saved
	float rescueLine = 0;

	public event System.Action Rescued;

	protected override void  Start()
	{
		base.Start();
		timeOfNextQuip = Random.Range (0, timeBetweenQuips);
		StartCoroutine (RescueCheck ());
	}

	IEnumerator RescueCheck()
	{
		while (true) {
			if (Time.time > timeOfNextQuip) {
				timeOfNextQuip = Time.time + timeBetweenQuips + Random.Range (0.0f, 2.0f);
				if (beingCarried) {
					SayMessage (carryMessages [Random.Range (0, carryMessages.Length)]);
				} else {
					int r;
					do {
						r = Random.Range (0, helpMessages.Length);
						// if r is equal to lastHelpIndex (the last message played by any victim)
						// choose another
					} while(r == lastHelpIndex);
					//Debug.Log("static: "+lastHelpIndex+"->"+r);
					lastHelpIndex = r;
					SayMessage (helpMessages [r]);
				}
			}

			// check if you're rescued
			if (isRescued ()) {
				Debug.Log ("Victim Rescued");
				if (Time.time > timeOfNextQuip - (timeBetweenQuips - 1)) {
					// if you haven't said anything in a little while, say a rescue message
					SayMessage(rescueMessages [Random.Range (0, rescueMessages.Length)]);

				}
				if (Rescued != null) {
					Rescued ();
				}
				break;
			}

			// only need to check every 1 second
			// could lower this if it's an issue
			// running this more often will probably not affect performance at all. Would need >100 victims probably
			yield return new WaitForSeconds (1);
		}
	}

	public override void PickUp(Transform parent)
	{
		base.PickUp (parent);
		beingCarried = true;
		// tell the player that the tool is influencing their movement
		ForcedMovementStarted ();
	}

	public override void Drop()
	{
		// tell the player the tool is done influencing their movement

		base.Drop ();
		beingCarried = false;
		ToolFinished ();
	}

	public override void Use()
	{
		base.Use ();

		// Tell the player to drop this tool
		if (GetComponentInParent<Player> () != null) {
			GetComponentInParent<Player> ().DropTool ();
		}

		// stop the player from being slowed
		// and remove this object from the player's object
		Drop();

		StartCoroutine (Toss ());
	}

	IEnumerator Toss()
	{
		if (physicsObject != null) {
			physicsObject.GetComponent<Rigidbody>().AddForce ((transform.forward + new Vector3 (0, 1, 0)) * throwForce);
		}

		yield return new WaitForFixedUpdate();
	}

	public bool isRescued()
	{
		return (transform.position.z < rescueLine);
	}

	void SayMessage(string message)
	{
		//Debug.Log (message);
		//Instantiate(text3D);
		TextController t = Instantiate(textMesh, transform.position + textSpawnPoint, FindObjectOfType<Camera>().transform.rotation) as TextController;
		t.SetText (message);
	}
}
