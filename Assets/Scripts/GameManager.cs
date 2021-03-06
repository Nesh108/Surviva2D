﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public GameObject mainCamera;
	public int followPlayerEvery = 2;	// Amount of slots between camera and player before moving the camera
	public float levelStartDelay = 2f;	// Time before starting level in seconds
	public float turnDelay = 0.1f;
	public static GameManager instance = null;
	public BoardManager boardScript;
	public int playerFoodPoints = 100;
	public int weaponDurability = 0;
	public int playerScore = 0;
	public int playerBombs = 0;
	
	[HideInInspector]
	public bool playersTurn = true;
	public bool checkCamera = false;

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
		// Show Game Over screen and final score
		#if UNITY_STANDALONE || UNITY_WEBPLAYER
			_levelText.text = "After " + _curLevel + " days, you starved. \n\nDaymn son...\n\n\n\nFinal Score: " + (playerScore + 100 * (_curLevel-1)) + "\n\n\nPress R to restart.";
		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		_levelText.text = "After " + _curLevel + " days, you starved. \n\nDaymn son...\n\n\n\nFinal Score: " + (playerScore + 100 * (_curLevel-1)) + "\n\n\nTap to restart.";
		#endif

		_levelImage.SetActive(true);

		enabled = false;
	}

	void Update ()
	{
		CheckCamera();

		if (playersTurn || _enemiesMoving || _doingSetup)
			return;

		StartCoroutine (MoveEnemies ());
	}

	public void AddEnemyToList (Enemy script)
	{
		_enemies.Add (script);
	}

	public void RemoveEnemyFromList (Enemy script)
	{
		_enemies.Remove (script);
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

	void CheckCamera ()
	{
		if(mainCamera == null)
			// Find MainCamera
			mainCamera = GameObject.Find("Main Camera") as GameObject;

		//Debug.Log ("Current camera position is: " + mainCamera.transform.position.x + "," +  mainCamera.transform.position.y);

		GameObject player = GameObject.FindGameObjectWithTag("Player") as GameObject;
		
		//Debug.Log ("Current player position is: " + player.transform.position.x + "," +  player.transform.position.y);
		
		if(boardScript.CurColumns > 8){
			float moveX = 0f;
			float moveY = 0f;

			// X axis
			if(player.transform.position.x - mainCamera.transform.position.x > followPlayerEvery)
				moveX = 1f;
			else if(player.transform.position.x - mainCamera.transform.position.x < -followPlayerEvery)
				moveX = -1f; 

			// Y axis
			if(player.transform.position.y - mainCamera.transform.position.y > followPlayerEvery)
				moveY = 1f;
			else if(player.transform.position.y - mainCamera.transform.position.y <- followPlayerEvery)
				moveY = -1f;

			mainCamera.transform.position += new Vector3(moveX, moveY);
		}

		checkCamera = false;
	}

	public void RestartGame(){
		playerFoodPoints = 100;
		weaponDurability = 5;
		playerScore = 0;
		playerBombs = 0;
		_curLevel = 0;
		playersTurn = true;
		enabled = true;
		Application.LoadLevel (Application.loadedLevel);
	}
}
