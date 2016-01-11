using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
	public static GameManager instance = null;
	public BoardManager boardScript;
	public int playerFoodPoints = 100;
	[HideInInspector] public bool playersTurn = true;

	private int _curLevel = 3;

	void Awake ()
	{
		// Make sure there exists exactly one GameManager and no more (singleton)
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		// Keep the GameManager between scenes/levels
		DontDestroyOnLoad(gameObject);

		boardScript = GetComponent<BoardManager> ();
		InitGame ();
	}

	void InitGame ()
	{
		boardScript.SetupScene (_curLevel);
	}

	public void GameOver(){
		enabled = false;
	}

	// Update is called once per frame
	void Update ()
	{
	
	}
}
