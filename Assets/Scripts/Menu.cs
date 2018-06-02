using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{


    public GameObject player1;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;


    public Vector3 p1Pos;
    public Vector3 p2Pos;
    public Vector3 p3Pos;
    public Vector3 p4Pos;

    bool p1Joined = false;
    bool p2Joined = false;
    bool p3Joined = false;
    bool p4Joined = false;

    float moveSpeed = 6;

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

        if (!p1Joined)
        {
            if (Input.GetButtonDown("Pickup_P1"))
            {
                p1Joined = true;
                player1.GetComponent<Player>().SetShirtColour(Color.red);
            }
        }
        if (p1Joined)
        {
            player1.transform.position = Vector3.MoveTowards(player1.transform.position, p1Pos, moveSpeed * Time.deltaTime);
        }

        if (!p2Joined)
        {
            if (Input.GetButtonDown("Pickup_P2"))
            {
                p2Joined = true;
                player2.GetComponent<Player>().SetShirtColour(Color.blue);

            }
        }
        if (p2Joined)
        {
            player2.transform.position = Vector3.MoveTowards(player2.transform.position, p2Pos, moveSpeed * Time.deltaTime);
        }

        if (!p3Joined)
        {
            if (Input.GetButtonDown("Pickup_P3"))
            {
                Debug.Log("P3");
                p3Joined = true;
                player3.GetComponent<Player>().SetShirtColour(Color.green);

            }
        }
        if (p3Joined)
        {
            player3.transform.position = Vector3.MoveTowards(player3.transform.position, p3Pos, moveSpeed * Time.deltaTime);
        }

        if (!p4Joined)
        {
            if (Input.GetButtonDown("Pickup_P4"))
            {
                p4Joined = true;
                player4.GetComponent<Player>().SetShirtColour(Color.yellow);

            }
        }
        if (p4Joined)
        {
            player4.transform.position = Vector3.MoveTowards(player4.transform.position, p4Pos, moveSpeed * Time.deltaTime);
        }
    }
}
