using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MovingObject
{
	public int maxBombs = 3;
	public int weaponDamage = 2;
	public int wallDamage = 1;
	public int pointsPerWeapon = 5;
	public int maxDurability = 10;
	public float restartLevelDelay = 1f;
	public Text scoreText;
	public Text weaponText;
	public Text foodText;
	public GameObject[] bombs;
	public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;
	public AudioClip gameOverSound;
	private Animator _animator;
	private int _curFood;
	private int _curScore;
	private int _curWeaponDurability;
	private int _curBombs;
	private const int MAX_DAMAGE = 100;			// Used to insta-kill stuck enemies

	protected override void Start ()
	{
		_animator = GetComponent<Animator> ();

		// Used to store and retrieve food and weapons points between levels
		_curFood = GameManager.instance.playerFoodPoints;
		_curScore = GameManager.instance.playerScore;
		_curWeaponDurability = GameManager.instance.weaponDurability;
		_curBombs = GameManager.instance.playerBombs;

		foodText.text = "Food: " + _curFood;
		weaponText.text = "Weapon: " + _curWeaponDurability;
		scoreText.text = "Score: " + _curScore;

		// Handle bombs UI
		checkBombs ();

		// Start the basic class (MovingObject)
		base.Start ();

	}

	private void OnDisable ()
	{

		// Save current food, weapons and score before at the end of the level
		GameManager.instance.playerFoodPoints = _curFood;
		GameManager.instance.weaponDurability = _curWeaponDurability;
		GameManager.instance.playerScore = _curScore;
		GameManager.instance.playerBombs = _curBombs;
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

		if(Input.GetKeyUp(KeyCode.X)){
			if(_curBombs > 0)
				ExplodeBomb();
		}

		// Prevent player from moving diagonally
		if (horizontal != 0)
			vertical = 0;

		//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

		#endif //End of mobile platform dependendent compilation section started above with #elif

		// If the player wants to move
		if (horizontal != 0 || vertical != 0) {
			AttemptMove<Wall> (horizontal, vertical);	// Assuming that the wall is the component the player will interact with
			AttemptMove<Enemy> (horizontal, vertical);	// Assuming that the enemy is the other component the player will interact with
		
			// Remove food for the action
			_curFood--;
			foodText.text = "Food: " + _curFood;
		
		}
	}

	protected override void AttemptMove<T> (int xDir, int yDir)
	{
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

			_curFood += obj.GetComponent<Food> ().foodPoints;
			foodText.text = "+" + obj.GetComponent<Food> ().foodPoints + " Food: " + _curFood;

			// If the food is an edible, make eating sounds, else drinking sounds
			if (obj.GetComponent<Food> ().type == FoodType.Edible)
				SoundManager.instance.RandomizeSfx (eatSound1, eatSound2);
			else
				SoundManager.instance.RandomizeSfx (drinkSound1, drinkSound2);

			Destroy (obj.gameObject);


		} else if (obj.tag == "Weapon") {
			_curWeaponDurability += pointsPerWeapon;

			// Check if the current weapon durability is not higher than the max
			_curWeaponDurability = Mathf.Min (maxDurability, _curWeaponDurability);

			weaponText.text = "Weapon: " + _curWeaponDurability;
			Destroy (obj.gameObject);

		} else if (obj.tag == "Bomb") {
			if (_curBombs < maxBombs) {
				_curBombs ++;
				checkBombs ();
				Destroy (obj.gameObject);
			} else
				Debug.Log ("Currently TOO MANY BOMBS");
		}
	}

	protected override void OnCantMove<T> (T component)
	{
		if (component is Wall) {
			Wall hitWall = component as Wall;
			hitWall.DamageWall (wallDamage);

			_animator.SetTrigger ("PlayerChop");

		} else if (component is Enemy) {
			Enemy hitEnemy = component as Enemy;

			if (_curWeaponDurability > 0) {

				if (hitEnemy.DamageEnemy (weaponDamage)) {
					_curScore += hitEnemy.score;
					scoreText.text = "Score: " + _curScore;
				}
			
				_animator.SetTrigger ("PlayerChop");

				// Remove durability from the weapon
				_curWeaponDurability--;
				weaponText.text = "Weapon: " + _curWeaponDurability;


			} else if (hitEnemy.transform.position == transform.position) {	// Enemy is right over the player [kind of a bug]
				// Remove food from the player according to current enemy HP
				int lostFood = hitEnemy.hp * 5;

				// Allow the user to destroy the enemy for food
				if (hitEnemy.DamageEnemy (MAX_DAMAGE)) {
					_curScore += hitEnemy.score;
					scoreText.text = "Score: " + _curScore;
				}
				
				_animator.SetTrigger ("PlayerChop");

				Debug.Log ("Special move: " + lostFood + " food lost.");
				_curFood -= lostFood;
				foodText.text = "-" + lostFood + " Food: " + _curFood;
			}

		}
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
			GameManager.instance.playerScore = _curScore;
			GameManager.instance.GameOver ();
		}
	}

	private void ExplodeBomb ()
	{

		// Remove 1 bomb
		_curBombs--;

		// Check collisions around player

		// Temporarily disable to avoid clashing with component's own collider
		this.GetComponent<BoxCollider2D> ().enabled = false;

		// Check up - right - left - down first
		RaycastHit2D hitU = Physics2D.Linecast (transform.position, new Vector3 (transform.position.x, transform.position.y + 1), blockingLayer);
		RaycastHit2D hitL = Physics2D.Linecast (transform.position, new Vector3 (transform.position.x - 1, transform.position.y), blockingLayer);
		RaycastHit2D hitR = Physics2D.Linecast (transform.position, new Vector3 (transform.position.x + 1, transform.position.y), blockingLayer);
		RaycastHit2D hitD = Physics2D.Linecast (transform.position, new Vector3 (transform.position.x, transform.position.y - 1), blockingLayer);


		checkHits (new RaycastHit2D[]{hitU, hitR, hitL, hitD});

		// Check if a collision is detected in the diagonals
		RaycastHit2D hitUL = Physics2D.Linecast (transform.position, new Vector3 (transform.position.x - 1, transform.position.y + 1), blockingLayer);
		RaycastHit2D hitUR = Physics2D.Linecast (transform.position, new Vector3 (transform.position.x + 1, transform.position.y + 1), blockingLayer);

		RaycastHit2D hitDL = Physics2D.Linecast (transform.position, new Vector3 (transform.position.x - 1, transform.position.y - 1), blockingLayer);
		RaycastHit2D hitDR = Physics2D.Linecast (transform.position, new Vector3 (transform.position.x + 1, transform.position.y - 1), blockingLayer);
		
		checkHits (new RaycastHit2D[]{hitUL, hitUR, hitDL, hitDR});


		// Re-enable collider
		this.GetComponent<BoxCollider2D> ().enabled = true;

		// Handle bombs UI
		checkBombs ();
		
	}

	void checkHits (RaycastHit2D[] hits)
	{

		foreach (RaycastHit2D hit in hits) {
			// Something got hit
			if (hit.transform != null) {
				// if the hit object was a wall
				if (hit.transform.GetComponent<Wall> () != null) {
					//	Debug.Log ("Hit a wall at " + hit.transform.position.x + "," + hit.transform.position.y);

					// Destroy wall
					(hit.transform.GetComponent<Wall> () as Wall).DamageWall (MAX_DAMAGE);
				} else if (hit.transform.GetComponent<Enemy> () != null) {
					// Destroy enemy
					(hit.transform.GetComponent<Enemy> () as Enemy).DamageEnemy (MAX_DAMAGE);
				}
			}
		}

	}

	void checkBombs ()
	{
		// Disable not available bombs
		for (int i = 3; i > _curBombs; i--)
			bombs [i - 1].SetActive (false);

		// Enable available bombs
		for (int i = 0; i < _curBombs; i++)
			bombs [i].SetActive (true);

	}
}