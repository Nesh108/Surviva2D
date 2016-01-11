using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MovingObject
{

	public int wallDamage = 1;
	public int pointsPerFood = 10;
	public int pointsPerSoda = 20;
	public float restartLevelDelay = 1f;
	public Text foodText;
	public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;
	public AudioClip gameOverSound;
	private Animator _animator;
	private int _curFood;

	protected override void Start ()
	{
		_animator = GetComponent<Animator> ();

		// Used to store and retrieve food points between levels
		_curFood = GameManager.instance.playerFoodPoints;

		foodText.text = "Food: " + _curFood;

		// Start the basic class (MovingObject)
		base.Start ();

	}

	private void OnDisable ()
	{

		// Save current food before at the end of the level
		GameManager.instance.playerFoodPoints = _curFood;
	}

	void Update ()
	{
		if (!GameManager.instance.playersTurn)
			return;

		int horizontal = 0;
		int vertical = 0;

		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER

		horizontal = (int)Input.GetAxisRaw ("Horizontal");
		vertical = (int)Input.GetAxisRaw ("Vertical");

		// Prevent player from moving diagonally
		if (horizontal != 0)
			vertical = 0;

		//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

		#endif //End of mobile platform dependendent compilation section started above with #elif

		// If the player wants to move
		if (horizontal != 0 || vertical != 0)
			AttemptMove<Wall> (horizontal, vertical);	// Assuming that the wall is the component the player will interact with
	}

	protected override void AttemptMove<T> (int xDir, int yDir)
	{
		// Remove food for the action
		_curFood--;

		foodText.text = "Food: " + _curFood;

		base.AttemptMove<T> (xDir, yDir);

		RaycastHit2D hit;

		if (Move (xDir, yDir, out hit))
			SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);

		// Check if the game has ended after the move
		CheckGameOver ();

		GameManager.instance.playersTurn = false;
	}

	private void OnTriggerEnter2D (Collider2D obj)
	{

		// If the player hits the exit tile, wait 1 second and start a new level
		// Otherwise add the points to the food points and remove the item
		if (obj.tag == "Exit") {
			Invoke ("Restart", restartLevelDelay);
			enabled = false;
		} else if (obj.tag == "Food") {
			_curFood += pointsPerFood;
			foodText.text = "+" + pointsPerFood + " Food: " + _curFood;
			SoundManager.instance.RandomizeSfx (eatSound1, eatSound2);
			obj.gameObject.SetActive (false);
		} else if (obj.tag == "Soda") {
			_curFood += pointsPerSoda;
			foodText.text = "+" + pointsPerSoda + " Food: " + _curFood;
			SoundManager.instance.RandomizeSfx (drinkSound1, drinkSound2);
			obj.gameObject.SetActive (false);
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
		
		foodText.text = "-" + dmg + " Food: " + _curFood;
		CheckGameOver ();

	}

	private void CheckGameOver ()
	{
		if (_curFood <= 0) {
			SoundManager.instance.PlaySingle (gameOverSound);
			SoundManager.instance.musicSource.Stop ();
			GameManager.instance.GameOver ();
		}
	}
}
