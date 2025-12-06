using System.Collections;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
	[Header("场景里提前摆好的 Boss 对象")]
	public GameObject bossObject;       // 直接把场景里的 Boss 拖进来（不是 prefab）
	public GameObject bossBar;

	[Header("回合管理器")]
	public Round round;                 // 你的 Round 脚本

	[Header("出场动画（可选）")]
	public Animator spawnAnimator;      // 比如魔法阵 / 门的 Animator
	public string spawnTriggerName = "Spawn";
	public float spawnDelay = 1f;       // 播动画多久后，让 Boss 真正出现

	private bool spawned = false;

	private void Awake()
	{
		// ★ 不管 Inspector 里是不是勾上，代码强制一开局就藏起来
		if (bossObject != null)
		{
			bossBar.gameObject.SetActive(false);
			bossObject.SetActive(false);
		}
	}

	/// <summary>
	/// 外部调用：生成 Boss（带可选动画）
	/// </summary>
	public void SpawnBoss()
	{
		if (spawned) return;
		spawned = true;

		StartCoroutine(SpawnBossRoutine());
	}

	private IEnumerator SpawnBossRoutine()
	{
		// 1）播出场动画（如果有）
		if (spawnAnimator != null && !string.IsNullOrEmpty(spawnTriggerName))
		{
			spawnAnimator.SetTrigger(spawnTriggerName);
		}

		// 2）等一小段时间，让动画演一下
		if (spawnDelay > 0f)
		{
			yield return new WaitForSeconds(spawnDelay);
		}

		// 3）真正让 Boss 出现在场景中
		if (bossObject != null)
		{
			bossObject.SetActive(true);   // ★ 这一步才是“出现”
			bossBar.gameObject.SetActive(true);
		}
		else
		{
			Debug.LogError("[BossSpawner] bossObject 为空，你要把场景里的 Boss 拖进来！");
			yield break;
		}

		// 4）把 Boss 注册到 Round 的敌人列表，让它参与回合
		if (round != null)
		{
			EnemyAI bossAI = bossObject.GetComponent<EnemyAI>();
			if (bossAI != null)
			{
				round.RegisterEnemy(bossAI);
				Debug.Log("[BossSpawner] Boss 已加入 Round 敌人列表");
			}
			else
			{
				Debug.LogError("[BossSpawner] Boss 上没有 EnemyAI/BossAI 组件！");
			}
		}

		Debug.Log("[BossSpawner] Boss 出场完成");
	}

	// 方便测试：在 Inspector 右键这个脚本 → 生成 Boss（测试）
	[ContextMenu("生成 Boss（测试）")]
	private void EditorTestSpawnBoss()
	{
		if (!Application.isPlaying)
		{
			Debug.LogWarning("请在 Play 模式下使用这个测试按钮");
			return;
		}

		// 找一下 Round（如果你没拖的话）
		if (round == null)
		{
			round = FindObjectOfType<Round>();
		}

		SpawnBoss();
	}
}

