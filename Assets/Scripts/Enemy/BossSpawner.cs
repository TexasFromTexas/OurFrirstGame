using System.Collections;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
	[Header("场景里提前摆好的 Boss 对象（Hierarchy 里的 Boss）")]
	public GameObject bossObject;     // 直接把 Hierarchy 里的 Boss 拖进来
	public GameObject bossBar;        // Boss 的血条 UI
	public SceneBGM bgm;              // 场景音乐控制
	[Header("Boss 音乐")]
	public AudioClip bossMusic;

	[Header("回合管理器")]
	public Round round;

	[Header("出场动画（可选）")]
	public Animator spawnAnimator;
	public string spawnTriggerName = "Spawn";
	public float spawnDelay = 1f;

	private bool spawned = false;

	private void Awake()
	{
		// 开局强制隐藏 Boss 和血条
		if (bossObject != null)
			bossObject.SetActive(false);
		if (bossBar != null)
			bossBar.SetActive(false);
	}

	public EnemyAI SpawnBoss()
	{
		if (spawned)
			return bossObject != null ? bossObject.GetComponent<EnemyAI>() : null;

		spawned = true;
		StartCoroutine(SpawnRoutine());
		return bossObject != null ? bossObject.GetComponent<EnemyAI>() : null;
	}

	private IEnumerator SpawnRoutine()
	{
		if (spawnAnimator != null && !string.IsNullOrEmpty(spawnTriggerName))
		{
			spawnAnimator.SetTrigger(spawnTriggerName);
		}

		if (spawnDelay > 0f)
			yield return new WaitForSeconds(spawnDelay);

		if (bossObject == null)
		{
			Debug.LogError("[BossSpawner] bossObject 为空，请把场景里的 Boss 拖进来！");
			yield break;
		}

		bossObject.SetActive(true);
		if (bossBar != null)
			bossBar.SetActive(true);

		if (bgm != null && bossMusic != null)
		{
			bgm.FadeMusic(bossMusic);
		}

		if (round == null)
			round = FindObjectOfType<Round>();

		if (round != null)
		{
			var bossAI = bossObject.GetComponent<EnemyAI>();
			if (bossAI != null)
			{
				// 通知 Round：Boss 已经刷新
				round.NotifyBossSpawned(bossAI);
				Debug.Log("[BossSpawner] Boss 已加入 Round 的敌人列表");
			}
			else
			{
				Debug.LogError("[BossSpawner] Boss 上没有 EnemyAI 组件！");
			}
		}
		else
		{
			Debug.LogError("[BossSpawner] 找不到 Round 管理器！");
		}
	}

	[ContextMenu("测试生成 Boss")]
	private void EditorTestSpawnBoss()
	{
		if (!Application.isPlaying)
		{
			Debug.LogWarning("请在 Play 模式下使用测试按钮");
			return;
		}

		if (round == null)
			round = FindObjectOfType<Round>();

		SpawnBoss();
	}
}
