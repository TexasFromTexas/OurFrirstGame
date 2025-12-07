using System;
using UnityEngine;

public class SceneBGM : MonoBehaviour
{
	public AudioClip defaultMusic;   // 场景默认 BGM
	void Start()
	{
		if (MusicManager.Instance != null && defaultMusic != null)
		{
			// 进这个场景时淡入默认 BGM
			MusicManager.Instance.PlayMusicWithFade(defaultMusic, 1.0f, 0.7f);
		}
	}

	// BossSpawner 会调用这个，把 Boss 的 BGM 传进来
	public void FadeMusic(AudioClip newMusic, float fadeTime = 1.0f, float targetVolume = 0.7f)
	{
		if (MusicManager.Instance != null && newMusic != null)
		{
			MusicManager.Instance.PlayMusicWithFade(newMusic, fadeTime, targetVolume);
		}
	}

	public void FadeBackToNormal()
	{
		if (MusicManager.Instance != null)
		{
			MusicManager.Instance.PlayMusicWithFade(defaultMusic, 1.0f, 0.8f);
		}
	}
}
