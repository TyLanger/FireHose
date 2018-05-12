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

	bool spraying = false;

	public ParticleSystem particles;

	protected override void Start()
	{
		base.Start ();	
		currentFuelSeconds = fuelSeconds;
		//particles = GetComponent<ParticleSystem> ();
	}

	IEnumerator Spray()
	{
		spraying = true;
		ForcedMovementStarted ();

		timeStartedSpraying = Time.time;
		// constantly spawn foam particles
		while (currentFuelSeconds > 0) {
			if (Time.time - timeStartedSpraying <= timeToFullSpray) {
				// increase spray slowly
				currentFuelRate = Mathf.Lerp(minFuelRate, maxFuelRate, (Time.time - timeStartedSpraying) / timeToFullSpray);
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
				Debug.DrawLine(transform.position, transform.position + direction * sprayDistance, Color.blue);
				if (Physics.Raycast (transform.position, direction, out hit, sprayDistance)) {
					if (hit.collider.tag == "Fire") {
						hit.collider.GetComponentInParent<BuildingBlock> ().PutOutFire (currentFuelRate * Time.fixedDeltaTime * douseStrength);
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
			ToolFinished ();
			particles.Stop ();
		}
		// recharges slowly when not in use
		StartCoroutine ("Recharge");
	}

	public override void Drop ()
	{
		ToolFinished ();
		base.Drop ();
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
