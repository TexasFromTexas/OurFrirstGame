using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
	public static MusicManager Instance;

	public AudioSource audioSource;
	public float defaultFadeTime = 1.0f;

	Coroutine currentFade;

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	// 硬切（不推荐 Boss 用这个）
	public void PlayMusic(AudioClip clip, float volume = 1f)
	{
		if (clip == null || audioSource == null) return;

		audioSource.clip = clip;
		audioSource.volume = volume;
		audioSource.loop = true;
		audioSource.Play();
	}

	// 淡出当前 → 淡入新 BGM
	public void PlayMusicWithFade(AudioClip newClip, float fadeTime = -1f, float targetVolume = 1f)
	{
		if (newClip == null || audioSource == null) return;

		if (fadeTime < 0f)
			fadeTime = defaultFadeTime;

		if (currentFade != null)
			StopCoroutine(currentFade);

		currentFade = StartCoroutine(FadeMusicRoutine(newClip, fadeTime, targetVolume));
	}

	private IEnumerator FadeMusicRoutine(AudioClip newClip, float duration, float targetVolume)
	{
		float startVolume = audioSource.isPlaying ? audioSource.volume : 0f;

		// 淡出
		float t = 0f;
		while (t < duration * 0.5f)
		{
			t += Time.unscaledDeltaTime;
			audioSource.volume = Mathf.Lerp(startVolume, 0f, t / (duration * 0.5f));
			yield return null;
		}

		audioSource.volume = 0f;
		audioSource.clip = newClip;
		audioSource.loop = true;
		audioSource.Play();

		// 淡入
		t = 0f;
		while (t < duration * 0.5f)
		{
			t += Time.unscaledDeltaTime;
			audioSource.volume = Mathf.Lerp(0f, targetVolume, t / (duration * 0.5f));
			yield return null;
		}

		audioSource.volume = targetVolume;
		currentFade = null;
	}
}
