using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour {

    public Button button1;
    public Button button2;

    public Menu menu;

    public void StartHouse(int num)
    {
        menu.StartGame(num);
    }
}
