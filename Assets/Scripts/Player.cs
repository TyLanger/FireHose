using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    // random things to say when set on fire
    // only need to say 1 thing when you get set on fire probably?
    // maybe a second thing after you stay on fire?
    public TextController textMesh;
    Vector3 textSpawnPoint = Vector3.up;
    // if the player doesn't put themselves out, remind them this often
    float timeOfNextQuip = 0;
    float timeBetweenQuips = 4;
    string[] onFireMessages = {
		"Ah, I'm on fire",
		"Oh gee",
		"This is mighty inconvenient",
		"I swear, this never happens",
		"Stop, drop, and roll",
		"Put me out!",
		"It's getting hot in here",
		"Uh oh",
		"The fire has gotten me",
		"I am on fire",
		"I seem to be ablaze",
		"Anyone got a light?",
		"Those are flames on my arms",
		"Am I hot or what?",
		"Uh oh",
		"This isn't good",
		"I better put this out",
		"Halt, fall, and turn I think?",
		"Go, jump, and tumble?",
		"Um.. Stop, drop, and hmmm...",
		"I'll take a water, please",
        "Being on fire is the worst"
	};

	public float moveSpeed = 1;
	Vector3 moveInput;
	public float turnSpeed = 1;
	Vector3 lookDirection;

	ToolType currentUsing;

	// raise where the ray is fired from. Should make it easier to pick up objects on top of furniture
	Vector3 pickupPosition = new Vector3(0, 0.25f, 0);
	// may change to make it so you can hold multiple of some things
	bool holdingTool = false;
	Tool currentTool;
	public Transform hand;
	public Vector3 hatPosition;
	public Vector3 jacketPosition;

	public GameObject shirt;
    int headNum = 0;
    public GameObject[] heads;

	// the effectiveness of doing tasks without the right tools
	int unarmedDouseStrength = 35;
	int unarmedBreakStrength = 1;


	// Default to keyboard config for easy testing
	int playerNumber;
	string horizontalInput = "Horizontal";
	string verticalInput = "Vertical";
	string pickupInput = "Fire1";
	string useInput = "Fire2";
	public bool canMove = false;

	public GameObject fire;
	bool onFire = false;
	bool stopDropAndRoll = false;
	float rollMoveMultiplier = 0.8f;
	Vector3 rollStartPos;
	float distanceOfRoll = 1f;
	int numRollsToDouse = 6;
	int numRollsMade = 0;
    public float rollRotationMultiplier = 10;

	float baseFireResistance = 0;
	float currentFireResistance;
	float baseLightTime = 0;
	float lightTimeLeft;
	// the hp of the fire when you are set on fire
	float fireHp = 20;

	// Use this for initialization
	void Start () {
		currentUsing = ToolType.None;
		lookDirection = transform.forward;
		currentFireResistance = baseFireResistance;
		lightTimeLeft = baseLightTime;

        headNum = UnityEngine.Random.Range(0, heads.Length);

        for (int i = 0; i < heads.Length; i++)
        {
            if(i != headNum)
            {
                heads[i].SetActive(false);
            }
        }
        
        // either -1 or 1
        int facing = (Random.Range(0, 2)*2) - 1;
        // random chance for the hair to point left or right
        heads[headNum].transform.localScale = new Vector3(heads[headNum].transform.localScale.x * facing, heads[headNum].transform.localScale.y, heads[headNum].transform.localScale.z);
	}
	
	// Update is called once per frame
	void Update () {
		if (canMove) {
			if (Input.GetButtonDown (pickupInput)) {
				// left click
				// A button on snes controller
				//Debug.Log ("Fire1");
				PickUp ();
			}
			if (Input.GetButtonDown (useInput)) {
				// right click
				// B button on snes controller
				//Debug.Log ("Fire2");
				UseTool ();
			}
			if (Input.GetButtonUp (useInput)) {
				// right click
				// B button on snes controller
				//Debug.Log ("Fire2");
				StopTool ();
			}
			

			// wasd
			// left stick for snes controller
			moveInput = new Vector3 (Input.GetAxisRaw (horizontalInput), 0, Input.GetAxisRaw (verticalInput));
			if (moveInput.sqrMagnitude != 0) {
				// set lookDirection only if input is not 0
				lookDirection = moveInput;
			}

            if(onFire)
            {
                if(timeOfNextQuip < Time.time)
                {
                    timeOfNextQuip = Time.time + timeBetweenQuips + UnityEngine.Random.Range(0.0f, 2.0f);
                    SayMessage(onFireMessages[UnityEngine.Random.Range(0, onFireMessages.Length)]);
                }

            }
		}
	}

    void FixedUpdate()
    {
        switch (currentUsing)
        {
            case ToolType.None:
                if (stopDropAndRoll)
                {
                    // Stop Drop and Roll movement

                    // only get horizontal input
                    // this could be changed later to be able to roll up and down or on a diagonal instead of only left tot right
                    transform.position = Vector3.MoveTowards(transform.position, transform.position + new Vector3(moveInput.x, 0, 0), rollMoveMultiplier * moveSpeed * Time.fixedDeltaTime);
                    if (moveInput.x != 0)
                    {
                        // rotation speed should be based on movement speed too
                        transform.RotateAround(transform.position, Vector3.forward, moveInput.x * rollMoveMultiplier * moveSpeed * Time.fixedDeltaTime * rollRotationMultiplier);
                    }
                    if (onFire)
                    {
                        if (Mathf.Abs(transform.position.x - rollStartPos.x) > distanceOfRoll)
                        {
                            // if the difference between the current pos and start pos is greater than the distance of a roll, made a succesful roll
                            // made a roll
                            // Problem: once you hit the roll threshold, you can stay in that position and after the right number of updates, you will be put out
                            // Solution: set a new starting pos after every roll.
                            // Minor: this means you can just roll in one direction, you don't have to roll back and forth
                            // If you lay down in a position where you can't roll distanceOfRoll in either direction, you can't put yourself out
                            rollStartPos = transform.position;
                            numRollsMade++;
                            Debug.Log("Roll made");
                            if (numRollsMade == numRollsToDouse)
                            {
                                FirePutOut();
                            }
                        }
                    }
                }
                else
                {
                    // Normal Movement

                    // transform.forward*moveInput.magnitude makes the player have to do circles to turn around
                    // they always move forward and rely on turn speed to be able to turn. Multiplying by moveInput.magnitude makes it so the player doesn't move when not pressing anything
                    transform.position = Vector3.MoveTowards(transform.position, transform.position + moveInput, moveSpeed * Time.fixedDeltaTime);
                    // use lastMove input so it is never 0
                    // lookDirection is the last "actual" input, no 0 when not pressing movement buttons
                    // makes character face where they are moving and stay facing that direction when no movement
                    transform.forward = Vector3.RotateTowards(transform.forward, lookDirection, turnSpeed * Time.fixedDeltaTime, 1);
                }
                break;
            case ToolType.Axe:
                transform.position = currentTool.MoveTowards(transform.position, lookDirection, moveSpeed * Time.fixedDeltaTime);
                //transform.position = Vector3.MoveTowards (transform.position, transform.position + transform.forward,  currentTool.GetSpeedMultiplier() * moveSpeed * Time.fixedDeltaTime);
                transform.forward = Vector3.RotateTowards(transform.forward, lookDirection, currentTool.GetTurnMultiplier() * turnSpeed * Time.fixedDeltaTime, 1);
                break;
            case ToolType.Hose:
                // move away from the water coming out of the hose
                transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, currentTool.GetSpeedMultiplier() * moveSpeed * Time.fixedDeltaTime);
                transform.forward = Vector3.RotateTowards(transform.forward, lookDirection, currentTool.GetTurnMultiplier() * turnSpeed * Time.fixedDeltaTime, 1);
                break;
            case ToolType.Extinguisher:
                // move backwards away from the direction the extinguisher is shooting
                transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, currentTool.GetSpeedMultiplier() * moveSpeed * Time.fixedDeltaTime);
                transform.forward = Vector3.RotateTowards(transform.forward, lookDirection, currentTool.GetTurnMultiplier() * turnSpeed * Time.fixedDeltaTime, 1);
                break;
            case ToolType.Victim:
                // can move normally
                // just slower
                transform.position = Vector3.MoveTowards(transform.position, transform.position + moveInput, currentTool.GetSpeedMultiplier() * moveSpeed * Time.fixedDeltaTime);
                transform.forward = Vector3.RotateTowards(transform.forward, lookDirection, currentTool.GetTurnMultiplier() * turnSpeed * Time.fixedDeltaTime, 1);
                break;
        }

        //transform.position = currentTool.MoveTowards (transform.position, lookDirection, moveSpeed * Time.fixedDeltaTime);

    }

	public void Setup(string horName, string vertName, string pickupName, string useName, int playerNum)
	{
		// could have only passed in playerNum and just appended like "Horizontal_P"+playerNum
		// but I already did it the long way
		playerNumber = playerNum;
		horizontalInput = horName;
		verticalInput = vertName;
		pickupInput = pickupName;
		useInput = useName;
		canMove = true;
	}

	public int GetPlayerNumber()
	{
		return playerNumber;
	}

	public void SetShirtColour(Color shirtColour)
	{
		// set the first material to the shirt colour
		// works as long as the shirt material is first
		shirt.GetComponent<Renderer> ().material.color = shirtColour;
	}

	public void GameOver()
	{
		// player can't input any more 
		canMove = false;
		// make the player face the camera
		lookDirection = Vector3.back;
		// stop any movement the player has
		moveInput = Vector3.zero;
		// stop tools that use a held button to be used
		StopTool ();

		// when running the last victim in, the player tps, then keeps running
		// I don;t want to drop tools. I want people to still be holding them.
	}

	void StopTool()
	{
		if (holdingTool) {
			currentTool.StopUse ();
		}
	}

	void UseTool()
	{
		if (holdingTool) {
			

			currentTool.Use ();
		} else {
			// figure out what task to attempt
			if (onFire && !stopDropAndRoll) {
				// go into stop drop and roll mode
				stopDropAndRoll = true;
                // lay down
                transform.rotation = Quaternion.Euler(-90, 0, 0);
				numRollsMade = 0;
				rollStartPos = transform.position;
			} else if (stopDropAndRoll) {
                // !onFire && 
                // able to get up whenever
                // you have now put yourself out, but are still rolling on the ground
                // press use again to stand up
                // move the player up a bit. They will rotate back into position automatically.
                // moving them above the floor makes it so they don't get bounced around by the physics
                transform.position = transform.position + Vector3.up;
                stopDropAndRoll = false;
			} else {
				// break down door
				RaycastHit hit;
				// aim the ray slightly above the player's center.
				// same height they try to pick up new tools from
				// this is to be able to put out fires over top of some furniture
				if (Physics.Raycast (transform.position + pickupPosition, transform.forward, out hit, 3)) {
					if (hit.collider.GetComponent<BuildingBlock> () != null) {
						if (hit.collider.GetComponent<BuildingBlock> ().blockType == BlockType.Door) {
							// hit a door
							// try to break it down
							hit.collider.GetComponent<BuildingBlock> ().Break (unarmedBreakStrength);
						}
					} else if (hit.collider.tag == "Fire") {
                        // players also have Fire on them
                        if (hit.collider.GetComponentInParent<BuildingBlock>() != null)
                        {
                            // stomp out fire
                            hit.collider.GetComponentInParent<BuildingBlock>().PutOutFire(unarmedDouseStrength * Time.fixedDeltaTime);
                        }
					}
				}
			}

		}
	}

	void ToolStarted()
	{
		currentUsing = currentTool.toolType;
	}

	void ToolFinished()
	{
		currentUsing = ToolType.None;
		if (onFire && holdingTool) {
			// the axe doesn't let the player drop it in the middle of its animation
			// being on fire makes you drop your tool
			// so this is here to get the axe to drop itself when it's done if you were set on fire during the axe use
			PickUp ();
		}
	}

	void PickUp()
	{
		// Drop current thing
		if (holdingTool) {
			// drop object
			if (currentTool != null) {
				if (currentTool.CanDrop ()) {
					// check if the tool lets you drop it at this moment
					// can't drop an axe while swinging
					DropTool ();
					//currentTool.Drop ();

				}
			}
		// Pick up new thing
		} else {
			// cannot pick up anything while on fire or rolling on the ground trying to put out the fire
			if (!onFire && !stopDropAndRoll) {
				// pick up object in front of you
				RaycastHit hit;
				Debug.DrawRay (transform.position + pickupPosition, transform.forward, Color.green);
				if (Physics.Raycast (transform.position + pickupPosition, transform.forward, out hit, 2, LayerMask.GetMask ("Tool"))) {
					//Debug.Log ("Hit something");
					if (hit.collider.GetComponentInChildren<Tool> () != null) {
						// hit a tool
						if (hit.collider.GetComponentInChildren<Tool> ().CanPickup ()) {
							currentTool = hit.collider.GetComponentInChildren<Tool> ();

							// set up Forced movement before you call tool.pickup()
							// this is for the people you have to carry that slow you down
							// get an action that tells the player when to start getting moved by the object
							currentTool.ForcedMovement -= ToolStarted;
							currentTool.ForcedMovement += ToolStarted;
							// get an action to tell the player when it can go back to regular movement
							currentTool.ToolFinishedAction -= ToolFinished;
							currentTool.ToolFinishedAction += ToolFinished;

							currentTool.PickUp (hand);
							holdingTool = true;
						}
					}
				}
			}
		}
	}

	public void DropTool()
	{
		holdingTool = false;
		// tell the tool you are dropping it
		// it may call ToolFinished
		// the victim does to tell the player it can move normally again
		currentTool.Drop ();
		// then unsubscribe from the actions
		// else another player can pick up that tool and force you to move
		currentTool.ForcedMovement -= ToolStarted;
		currentTool.ToolFinishedAction -= ToolFinished;

	}

	public void PutOutFire(float dousePower)
	{
		if (onFire) {
			fireHp -= dousePower;
			if (fireHp < 0) {
				FirePutOut();
			}
		}
	}

	void FirePutOut()
	{
		if (onFire) {
			onFire = false;
			fire.SetActive (false);
			lightTimeLeft = baseLightTime + currentFireResistance;
		}
	}

	public void SetFireResistance(float newFireRes)
	{
		// called when the player picks up some protective gear
		// adds new fireRes to old fireRes
		// this way the hat and jacket stack
		currentFireResistance += newFireRes;
		lightTimeLeft += newFireRes;
	}

	public void PutOnPPE(Transform ppe, float fireRes, bool isHat)
	{
		ppe.parent = transform;
		holdingTool = false;
		SetFireResistance (fireRes);
		if (isHat) {
			ppe.position = transform.position + hatPosition;
		} else {
			// if it's not the hat, then it's the jacket
			ppe.position = transform.position + jacketPosition;
		}
	}

	void StartOnFire()
	{
		if (!onFire) {
			// You are now on fire
			// can you still use tools?
			// probably not. Would like to force drop them, but that may mess up the axe.
			onFire = true;
			fire.SetActive (true);
			// if holding something, drop it. If it's the axe, you can't drop it
			PickUp ();
            SayMessage(onFireMessages[UnityEngine.Random.Range(0, onFireMessages.Length)]);
            timeOfNextQuip = Time.time + timeBetweenQuips + UnityEngine.Random.Range(0.0f, 2.0f);
		}

	}

    void SayMessage(string message)
    {
        // Ripped 1:1 from Victim
        //Debug.Log (message);
        //Instantiate(text3D);
        TextController t = Instantiate(textMesh, transform.position + textSpawnPoint, FindObjectOfType<Camera>().transform.rotation) as TextController;
        t.SetText(message);
    }

    void OnTriggerStay(Collider col)
	{
		if (col.tag == "Fire") {
			lightTimeLeft -= Time.fixedDeltaTime;
			if (lightTimeLeft < 0) {
				StartOnFire ();
			}
		}
	}
}
