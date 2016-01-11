using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour {

	public Sprite damagedSprite;
	public int hp = 4;

	private SpriteRenderer _spriteRenderer;
	
	public AudioClip chopSound1;
	public AudioClip chopSound2;

	void Awake () {
		_spriteRenderer = GetComponent<SpriteRenderer>();	
	}

	public void DamageWall(int dmg) {
	
		SoundManager.instance.RandomizeSfx (chopSound1, chopSound2);

		// Whenever the wall is damaged, switch the sprite to the damaged one
		_spriteRenderer.sprite = damagedSprite;

		// Remove the dealt damage 
		hp -= dmg;

		// If the hp drop below 0, remove the wall
		if(hp <= 0)
			gameObject.SetActive(false);
	}

}
