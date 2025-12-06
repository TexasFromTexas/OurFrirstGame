using UnityEngine;

public class MusicManager : MonoBehaviour
{
	public static MusicManager Instance;

	public AudioSource audioSource;

	private void Awake()
	{
		// 单例（如果已经存在，就删除重复对象）
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject); // 切场景不销毁
	}

	public void PlayMusic(AudioClip clip, float volume = 1f)
	{
		if (clip == null) return;

		audioSource.clip = clip;
		audioSource.volume = volume/2;
		audioSource.loop = true;
		audioSource.Play();
	}


	public void StopMusic()
	{
		audioSource.Stop();
	}
}
