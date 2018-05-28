using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextController : MonoBehaviour {

	float fadeTime = 3;
	float spawnTime;

	Color originalColour;
	Color fadeColour;

	TextMesh word;
	public TextMesh outline;

	float riseSpeed = 1.5f;

	// Use this for initialization
	void Awake () {
		word = GetComponent<TextMesh> ();
		spawnTime = Time.time;
		originalColour = word.color;
		// same colour, but with 0 alpha
		fadeColour = word.color + new Color (0, 0, 0, -word.color.a);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		/*
		 * Speed controlled method
		if (word.color.a < 0.05f) {
			// last 1/20th of the alpha
			DestroyText();
		}
		word.color = word.color + new Color (0, 0, 0, -Time.fixedDeltaTime);
		*/
		// time controlled method
		if ((Time.time - spawnTime) / fadeTime >= 0.9f) {
			DestroyText ();
		}
		word.color = Color.Lerp (originalColour, fadeColour, (Time.time - spawnTime) / fadeTime);
		outline.color = Color.Lerp(Color.black, Color.clear, (Time.time - spawnTime) / fadeTime);

		transform.position = Vector3.MoveTowards (transform.position, transform.position + Vector3.up, riseSpeed * Time.fixedDeltaTime);
	}


	void DestroyText()
	{
		Destroy (gameObject);
	}

	public void SetText(string text)
	{
		//string[] words = text.Split (" ");
		// I would need to know the size of each word to be able to space them out
		// It seems pretty tedious to do that for a result that's just going to be slightly better


		word.text = text;
		outline.text = text;
	}
}
