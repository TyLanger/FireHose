using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : Tool {

	int numDegrees = 90;
	public int numSwings = 4;

	public int breakStrength = 5;

	bool swinging = false;

	Collider axeTrigger;

    public AudioClip[] hitClips;
    public AudioClip[] swingClips;
    public int hitIndex = 0;
    public int swingIndex = 1;

    float hitVolume = 0.3f;
    float hitPitch = 1f;
    float swingVolume = 0.1f;
    float swingPitch = 0.6f;

	protected override void Start()
	{
		base.Start ();
		axeTrigger = GetComponent<Collider> ();
        AddAudioSources(2);
        audioSources[hitIndex].clip = hitClips[0];
        audioSources[hitIndex].pitch = hitPitch;
        audioSources[hitIndex].volume = hitVolume;

        audioSources[swingIndex].clip = swingClips[0];
        audioSources[swingIndex].pitch = swingPitch;
        audioSources[swingIndex].volume = swingVolume;


    }

    public IEnumerator SwingAxe()
	{
		swinging = true;
        audioSources[swingIndex].Play();
		// the axe is now pulling the player
		ForcedMovementStarted();
		for (int s = 0; s < numSwings; s++) {
			axeTrigger.enabled = true;
			for (int i = 0; i < numDegrees / 9; i++) {

				//swing axe down
				transform.RotateAround (transform.position, transform.right, 9);

				yield return null;
			}
			axeTrigger.enabled = false;
			for (int j = 0; j < numDegrees / 9; j++) {
				// pull axe back up
				transform.RotateAround (transform.position, transform.right, -9);

				yield return null;
			}
		}
		swinging = false;
		// the ability is over
		// run this so anything subscribed to the action will know
		ToolFinished ();
        audioSources[swingIndex].Stop();

    }

    public override void Use ()
	{
		base.Use ();
		if (!swinging) {
			//GetComponent<Collider> ().isTrigger = true;
			StartCoroutine (SwingAxe ());
		}
	}

	public override void StopUse()
	{

	}

	public override Vector3 MoveTowards(Vector3 current, Vector3 input, float baseSpeed)
	{
		// get the axe's parent's forward vector (the hand)
		// could get axe's parent's parent's (player) forward
		// is this better than the player just passing it in?
		// player can rotate their character to rotate the axe

		return Vector3.MoveTowards (current, current + transform.parent.transform.forward, baseSpeed * speedMultiplier);
	}

	protected override void ForcedMovementStarted ()
	{
		base.ForcedMovementStarted ();
		canDrop = false;
	}

	protected override void ToolFinished ()
	{
		canDrop = true;
		base.ToolFinished ();
	}

	void OnTriggerEnter(Collider col)
	{
		if (swinging) {
			if(col.GetComponent<BuildingBlock>() != null) {
			//if (col.tag == "Door") {
				//Destroy (col.gameObject);
				col.GetComponent<BuildingBlock> ().Break (breakStrength);
                if (col.GetComponent<BuildingBlock>().blockType != BlockType.Floor)
                {
                    audioSources[hitIndex].Play();
                }

            }
            if (col.tag == "Furniture")
            {
                Destroy(col.gameObject);
                audioSources[hitIndex].Play();

            }
        }
	}

}
