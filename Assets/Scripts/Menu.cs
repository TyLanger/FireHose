using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


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

    bool menuAnim = true;
    public int houseHumber = 1;

    public Texture2D screenshot1Tex;
    public Texture2D screenshot2Tex;

    public SpriteRenderer house1Image;
    public SpriteRenderer house2Image;

    static bool alreadySpawned = false;
    public static Menu originalMenu;

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        if(alreadySpawned)
        {
            // don't spawn 2 menus

            // reset things in the original
            originalMenu.player1 = this.player1;
            originalMenu.player2 = this.player2;
            originalMenu.player3 = this.player3;
            originalMenu.player4 = this.player4;

            originalMenu.p1Joined = false;
            originalMenu.p2Joined = false;
            originalMenu.p3Joined = false;
            originalMenu.p4Joined = false;

            originalMenu.menuAnim = true;

            originalMenu.house1Image = this.house1Image;
            originalMenu.house2Image = this.house2Image;

            FindObjectOfType<MenuButton>().menu = originalMenu;

            Destroy(gameObject);
        }
        else
        {
            alreadySpawned = true;
            originalMenu = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (menuAnim)
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

    public void StartGame(int houseNum)
    {
        menuAnim = false;
        houseHumber = houseNum;
        SceneManager.LoadScene("Building");
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
        Invoke("ApplyTexture", 0.5f);
    }

    public void SetScreenShot(Texture2D image)
    {
        if (houseHumber == 1)
        {
            screenshot1Tex = image;
        }
        else
        {
            screenshot2Tex = image;

        }


    }

    void ApplyTexture()
    {
        if (screenshot1Tex != null)
        {
            Sprite tempSprite = Sprite.Create(screenshot1Tex, new Rect(0, 0, screenshot1Tex.width, screenshot1Tex.height), new Vector2(0, 0));
            house1Image.sprite = tempSprite;
        }
        if (screenshot2Tex != null)
        {
            Sprite tempSprite = Sprite.Create(screenshot2Tex, new Rect(0, 0, screenshot2Tex.width, screenshot2Tex.height), new Vector2(0, 0));
            house2Image.sprite = tempSprite;
        }
    }
}
