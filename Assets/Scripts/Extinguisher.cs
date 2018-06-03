using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extinguisher : Tool {


	// degrees
	public float sprayCone = 60;
	public float sprayDistance = 6;

	// how many seconds you can spray for
	public float fuelSeconds = 6;
	public float currentFuelSeconds;
	public float rechargeRate = 0.25f;

	public float timeToFullSpray = 2;
	float timeStartedSpraying = 0;
	float minFuelRate = 0.1f;
	float maxFuelRate = 1;
	float currentFuelRate = 0;


	public float douseStrength = 30;

	protected bool spraying = false;

	public ParticleSystem particles;

    public AudioClip startSprayClip;
    public AudioClip maxSprayClip;
    public AudioClip endSprayClip;
    int startIndex = 0;
    int maxIndex = 1;
    int endIndex = 2;

    float startVolume = 0.7f;
    float maxVolume = 0.5f;
    float endVolume = 0.5f;
    float startPitch = 1;
    float maxPitch = 1;
    float endPitch = 1;

    float endTime = 1.5f;


    protected override void Start()
	{
		base.Start ();	
		currentFuelSeconds = fuelSeconds;
        //particles = GetComponent<ParticleSystem> ();
        AddAudioSources(3);

        audioSources[startIndex].clip = startSprayClip;
        audioSources[maxIndex].clip = maxSprayClip;
        //audioSources[maxIndex+1].clip = maxSprayClip;
        audioSources[endIndex].clip = endSprayClip;

        audioSources[startIndex].pitch = startPitch;
        audioSources[startIndex].volume = startVolume;

        audioSources[maxIndex].pitch = maxPitch;
        audioSources[maxIndex].volume = maxVolume;
        audioSources[maxIndex].loop = true;
        //audioSources[maxIndex+1].pitch = maxPitch;
        //audioSources[maxIndex+1].volume = maxVolume;
        //audioSources[maxIndex+1].loop = true;

        audioSources[endIndex].pitch = endPitch;
        audioSources[endIndex].volume = endVolume;
    }

    void MixSpraySound(float sprayPercent)
    {
        // as the spray gets stronger, fade out the volume
        audioSources[startIndex].volume = Mathf.Lerp(startVolume, 0, sprayPercent);
        // fade in the maxSpray sound
        audioSources[maxIndex].volume = Mathf.Lerp(0, maxVolume, sprayPercent);
        //audioSources[maxIndex+1].volume = Mathf.Lerp(0, maxVolume, sprayPercent);
    }

    IEnumerator FadeSprayOut(float fadeTime)
    {
        if (audioSources[startIndex].isPlaying)
        {
            audioSources[startIndex].Stop();
        }
        float originalFadeTime = fadeTime;
        audioSources[endIndex].Play();
        // check if it has started spraying again before this ended
        while (fadeTime > 0 && !spraying)
        {
            fadeTime -= Time.fixedDeltaTime;
            if (audioSources[maxIndex].volume > 0)
            {
                audioSources[maxIndex].volume -= Time.fixedDeltaTime;
            }
            /*
            if (audioSources[maxIndex+1].volume > 0)
            {
                audioSources[maxIndex+1].volume -= Time.fixedDeltaTime;
            }*/
            if (audioSources[endIndex].volume > 0)
            {
                audioSources[endIndex].volume -= Time.fixedDeltaTime;
            }

            yield return new WaitForFixedUpdate();
        }
        if (!spraying)
        {
            // may have already started again
            audioSources[maxIndex].Stop();
        }
        //audioSources[maxIndex+1].Stop();

    }

    IEnumerator Spray()
	{
		spraying = true;
		ForcedMovementStarted ();

        //StopCoroutine(FadeSprayOut());
        audioSources[startIndex].Play();
        audioSources[maxIndex].Play();
        //Invoke("PlaySecondSpraySound", audioSources[maxIndex].clip.length * 0.5f);

        timeStartedSpraying = Time.time;
		// constantly spawn foam particles
		while (currentFuelSeconds > 0) {
			if (Time.time - timeStartedSpraying <= timeToFullSpray) {
				// increase spray slowly
				currentFuelRate = Mathf.Lerp(minFuelRate, maxFuelRate, (Time.time - timeStartedSpraying) / timeToFullSpray);
                // only need to mix while the spray is amping up
                MixSpraySound(currentFuelRate / maxFuelRate);
            }

           

			if (!particles.isEmitting) {
				particles.Play ();
			}

			float angle;
			Vector3 direction;
			RaycastHit hit;

			for (int i = 0; i <= sprayCone * 0.2f; i++) {

				// every 5 degrees
				// Cos and sin use radians so convert to radians
				// iterate from +30 degrees to -30 degrees
				angle = (sprayCone * 0.5f - (5) * i) * Mathf.Deg2Rad;
				direction = new Vector3((transform.forward.x * Mathf.Cos(angle) - (transform.forward.z * Mathf.Sin(angle))), 0, transform.forward.x * Mathf.Sin(angle) + transform.forward.z * Mathf.Cos(angle));
				// fire a ray every 10 degrees
				//Ray r = new Ray(transform.position, direction);
				Debug.DrawLine(particles.transform.position, particles.transform.position + direction * sprayDistance, Color.blue);
				if (Physics.Raycast (particles.transform.position, direction, out hit, sprayDistance)) {
					if (hit.collider.tag == "Fire") {
						if (hit.collider.GetComponentInParent<BuildingBlock> () != null) {
							hit.collider.GetComponentInParent<BuildingBlock> ().PutOutFire (currentFuelRate * Time.fixedDeltaTime * douseStrength);
						} else if (hit.collider.GetComponentInParent<Player> () != null) {
							hit.collider.GetComponentInParent<Player> ().PutOutFire (currentFuelRate * Time.fixedDeltaTime * douseStrength);

						}
					}
				}
			}
			/*
			RaycastHit hit;
			//Debug.DrawLine (transform.position, transform.position + transform.forward * 6, Color.blue);
			if (Physics.Raycast (transform.position, transform.forward, out hit, sprayDistance)) {
				if (hit.collider.tag == "Fire") {
					hit.collider.GetComponentInParent<BuildingBlock> ().PutOutFire (currentFuelRate * Time.fixedDeltaTime * douseStrength);
				}
			}*/

			// fire extinguisher has fuelSeconds worth of fuel
			// this is how many secconds it can fire at full power
			// the fire extinguisher will take timeToFullSpray seconds to get to full spray
			// fuel is then only consumed at a fraction of the full power when not at full spray
			// multiply by Time.fixedDeltaTime because this method runs every Time.fixedDeltaTime seconds
			currentFuelSeconds -= currentFuelRate * Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate ();
		}

        
		// ran out of fuel
		spraying = false;
		OutOfFuel ();
	}

	IEnumerator Recharge()
	{
		while (currentFuelSeconds < fuelSeconds) {

			currentFuelSeconds += rechargeRate * Time.fixedDeltaTime;

			yield return new WaitForFixedUpdate ();
		}
			
	}

	void OutOfFuel()
	{
        StartCoroutine(FadeSprayOut(endTime));
		ToolFinished ();
		particles.Stop ();
	}

	public override void Use()
	{
		base.Use ();
		StartCoroutine ("Spray");
		// stop recharging while being used
		StopCoroutine ("Recharge");
	}

	public override void StopUse()
	{
		// only stop spraying if it is currently spraying
		// the exinguisher could have run out of fuel already and stopped itself
		if (spraying) {
			spraying = false;
			StopCoroutine ("Spray");
            StartCoroutine(FadeSprayOut(endTime));
            ToolFinished();
			particles.Stop ();
		}
		// recharges slowly when not in use
		StartCoroutine ("Recharge");
	}

	public override void Drop ()
	{
		base.Drop ();
		// check here too so I don't start the recharge coroutine twice
		if (spraying) {
			StopUse ();
		}
		ToolFinished ();
	}

	public override float GetSpeedMultiplier()
	{
		return speedMultiplier * currentFuelRate;
	}

	public override Vector3 MoveTowards(Vector3 current, Vector3 input, float baseSpeed)
	{
		// get the axe's parent's forward vector (the hand)
		// could get axe's parent's parent's (player) forward
		// is this better than the player just passing it in?
		// player can rotate their character to rotate the extinguisher

		return Vector3.MoveTowards (current, current + transform.parent.transform.forward, baseSpeed * speedMultiplier);
	}

}
