using UnityEngine;
using System.Collections;

public class Player : MovingObject
{

	public int wallDamage = 1;
	public int pointsPerFood = 10;
	public int pointsPerSoda = 20;
	public float restartLevelDelay = 1f;
	private Animator _animator;
	private int _curFood;

	// Use this for initialization
	protected override void Start ()
	{
		_animator = GetComponent<Animator> ();

		// Used to store and retrieve food points between levels
		_curFood = GameManager.instance.playerFoodPoints;

		// Start the basic class (MovingObject)
		base.Start ();

	}

	private void OnDisable ()
	{

		// Save current food before at the end of the level
		GameManager.instance.playerFoodPoints = _curFood;
	}
	// Update is called once per frame
	void Update ()
	{
		if (!GameManager.instance.playersTurn)
			return;

		int horizontal = 0;
		int vertical = 0;
		
		horizontal = (int)Input.GetAxisRaw ("Horizontal");
		vertical = (int)Input.GetAxisRaw ("Vertical");

		// Prevent player from moving diagonally
		if (horizontal != 0)
			vertical = 0;

		// If the player wants to move
		if (horizontal != 0 || vertical != 0)
			AttemptMove<Wall> (horizontal, vertical);	// Assuming that the wall is the component the player will interact with
	}

	protected override void AttemptMove<T> (int xDir, int yDir)
	{
		// Remove food for the action
		_curFood--;

		base.AttemptMove<T> (xDir, yDir);

		RaycastHit2D hit;

		Move (xDir, yDir, out hit);

		// Check if the game has ended after the move
		CheckGameOver ();

		GameManager.instance.playersTurn = false;
	}

	private void OnTriggerEnter2D (Collider2D obj)
	{
		// If the player hits the exit tile, wait 1 second and start a new level
		// Otherwise add the points to the food points and remove the item
		if(obj.tag == "Exit") {
			Invoke("Restart", restartLevelDelay);
			enabled = false;
		}
		else if(obj.tag == "Food"){
			_curFood += pointsPerFood;
			obj.gameObject.SetActive(false);
		}
		else if(obj.tag == "Sode"){
			_curFood += pointsPerSoda;
			obj.gameObject.SetActive(false);
		}
	}

	protected override void OnCantMove<T> (T component)
	{
		Wall hitWall = component as Wall;
		hitWall.DamageWall (wallDamage);

		_animator.SetTrigger ("PlayerChop");
	}

	private void Restart ()
	{
		// Re-load the last scene, as there is only one scene
		Application.LoadLevel (Application.loadedLevel);
	}

	public void LoseFood (int dmg)
	{
		_animator.SetTrigger ("PlayerHit");
		_curFood -= dmg;
		CheckGameOver ();

	}

	private void CheckGameOver ()
	{
		if (_curFood <= 0)
			GameManager.instance.GameOver ();
	}
}
