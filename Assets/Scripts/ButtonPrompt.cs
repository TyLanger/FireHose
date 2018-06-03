using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPrompt : MonoBehaviour {

    Transform cameraTrans;
    public Vector3 offset;

    Renderer spriteRenderer;

    public float sphereRadius = 0.6f;
    int numPlayersNear = 0;

	// Use this for initialization
	void Start () {
        cameraTrans = FindObjectOfType<Camera>().transform;
        transform.rotation = cameraTrans.rotation;
        spriteRenderer = GetComponent<Renderer>();
        SetVisible(false);
        if (GetComponent<SphereCollider>() != null)
        {
            // for the prompt to pick up tools
            GetComponent<SphereCollider>().radius = sphereRadius;
        }
        else
        {
            // for the b prompt on being set on fire
            SetVisible(true);
        }
	}
	
	// Update is called once per frame
	void Update () {
        //transform.LookAt(cameraTrans, Vector3.up);
        transform.rotation = cameraTrans.rotation;
        transform.position = transform.parent.position + offset;
    }

    void SetVisible(bool isVisible)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = isVisible;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.tag.Equals("Player"))
        {
            // keep track of how many players nearby
            numPlayersNear++;
            if (numPlayersNear == 1)
            {
                // only need to set visible for the first player to enter
                // when more players enter, it will already be visible

                SetVisible(true);
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.tag.Equals("Player"))
        {
            numPlayersNear--;
            if(numPlayersNear <= 0)
            {
                // make invisible once all players have left
                SetVisible(false);
            }
        }
    }
}
