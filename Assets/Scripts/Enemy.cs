using UnityEngine;
using System.Collections;

public class Enemy : MovingObject
{
	public int hp;
	public int playerDamage;
	private Animator _animator;
	private Transform target;
	private bool _skipMove;		// Make the enemies move every other turn
	
	public AudioClip chopSound1;
	public AudioClip chopSound2;
	public AudioClip enemyAttack1;
	public AudioClip enemyAttack2;

	protected override void Start ()
	{
		// Add itself to the GameManager so that it can interact with each enemy
		GameManager.instance.AddEnemyToList(this);
		_animator = GetComponent<Animator> ();

		// Set player as enemy's target
		target = GameObject.FindGameObjectWithTag ("Player").transform;

		// Start the basic class (MovingObject)
		base.Start ();
	}

	protected override void AttemptMove <T> (int xDir, int yDir)
	{
		if (_skipMove) {
			_skipMove = false;
			return;
		}

		base.AttemptMove<T> (xDir, yDir);

		_skipMove = true;
	}

	public void MoveEnemy ()
	{
		int xDir = 0;
		int yDir = 0;

		// Are the player and the enemy in the same column?
		if (Mathf.Abs (target.position.x - transform.position.x) < float.Epsilon)
			yDir = target.position.y > transform.position.y ? 1 : -1;		// Move up if player is above, move down if below # It can never be on the same column and row
		else
			xDir = target.position.x > transform.position.x ? 1 : -1;

		AttemptMove<Player> (xDir, yDir);
	}

	protected override void OnCantMove <T> (T component)
	{
		Player hitPlayer = component as Player;

		hitPlayer.LoseFood (playerDamage);

		_animator.SetTrigger("EnemyAttack");

		SoundManager.instance.RandomizeSfx (enemyAttack1, enemyAttack2);
	}

	public void DamageEnemy(int dmg) {
		
		SoundManager.instance.RandomizeSfx (chopSound1, chopSound2);
	
		// Remove the dealt damage 
		hp -= dmg;
		
		// If the hp drop below 0, remove the enemy
		if(hp <= 0)
		{
			GameManager.instance.RemoveEnemyFromList(this);
			Destroy(gameObject);
		}
	}
}
