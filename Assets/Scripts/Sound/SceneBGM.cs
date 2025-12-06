using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBGM: MonoBehaviour
{
	public AudioClip bgm;

	private void Start()
	{
		if(MusicManager.Instance!= null)
		{
			MusicManager.Instance.PlayMusic(bgm);
		}
	}

	public IEnumerator FadeMusic(AudioClip newClip)
	{
		float t = 0f;

		// µ­³ö
		while (t < 1f)
		{
			MusicManager.Instance.audioSource.volume = 1f - t;
			t += Time.deltaTime;
			yield return null;
		}


		MusicManager.Instance.PlayMusic(newClip);

		// µ­Èë
		t = 0f;
		while (t < 1f)
		{
			MusicManager.Instance.audioSource.volume = t;
			t += Time.deltaTime;
			yield return null;
		}
	}

}
