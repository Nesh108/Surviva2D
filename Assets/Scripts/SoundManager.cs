using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{

	public AudioSource efxSource;
	public AudioSource musicSource;
	public static SoundManager instance = null;

	// Random pitch fluctuation for making audio different
	public float lowPitchRange = .95f;
	public float highPighRange = 1.05f;

	// Use this for initialization
	void Start ()
	{
		// Make sure there exists exactly one SoundManager and no more (singleton)
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);
	}

	public void PlaySingle (AudioClip clip)
	{
		efxSource.clip = clip;
		efxSource.Play ();
	}

	public void RandomizeSfx (params AudioClip[] clips)
	{
		int randomIndex = Random.Range (0, clips.Length);
		float randomPitch = Random.Range (lowPitchRange, highPighRange);

		efxSource.pitch = randomPitch;
		efxSource.clip = clips [randomIndex];
		efxSource.Play ();
	}

}
