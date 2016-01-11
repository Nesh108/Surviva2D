using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public float levelStartDelay = 2f;	// Time before starting level in seconds
	public float turnDelay = 0.1f;
	public static GameManager instance = null;
	public BoardManager boardScript;
	public int playerFoodPoints = 100;
	[HideInInspector]
	public bool playersTurn = true;
	private Text _levelText;
	private GameObject _levelImage;
	private int _curLevel = 1;
	private List<Enemy> _enemies;
	private bool _enemiesMoving;
	private bool _doingSetup;

	void Awake ()
	{
		// Make sure there exists exactly one GameManager and no more (singleton)
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		// Keep the GameManager between scenes/levels
		DontDestroyOnLoad (gameObject);

		_enemies = new List<Enemy> ();

		boardScript = GetComponent<BoardManager> ();
		InitGame ();
	}

	public void OnLevelWasLoaded (int index)
	{
		_curLevel++;
		InitGame ();
	}

	void InitGame ()
	{
		// To prevent the player from moving
		_doingSetup = true;

		_levelImage = GameObject.Find ("LevelImage");
		_levelText = GameObject.Find ("LevelText").GetComponent<Text>();

		_levelText.text = "Day " + _curLevel;

		_levelImage.SetActive(true);

		Invoke ("HideLevelImage", levelStartDelay);

		_enemies.Clear ();
		boardScript.SetupScene (_curLevel);
	}

	private void HideLevelImage(){

		_levelImage.SetActive(false);

		// To allow the player to move
		_doingSetup = false;
	}

	public void GameOver ()
	{
		// Show Game Over screen
		_levelText.text = "After " + _curLevel + " days, you starved. \n\nDaymn son...";
		_levelImage.SetActive(true);

		enabled = false;
	}

	void Update ()
	{
		if (playersTurn || _enemiesMoving || _doingSetup)
			return;

		StartCoroutine (MoveEnemies ());
	}

	public void AddEnemyToList (Enemy script)
	{
		_enemies.Add (script);
	}

	IEnumerator MoveEnemies ()
	{
		// Enemies turn
		_enemiesMoving = true;

		yield return new WaitForSeconds (turnDelay);

		if (_enemies.Count == 0) 
			yield return new WaitForSeconds (turnDelay);

		for (int i = 0; i< _enemies.Count; i++) {

			_enemies [i].MoveEnemy ();
			yield return new WaitForSeconds (_enemies [i].moveTime);
		}

		playersTurn = true;
		_enemiesMoving = false;
	}
}
