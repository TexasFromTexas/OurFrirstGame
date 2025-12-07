using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Round : MonoBehaviour
{
	public enum TurnState
	{
		BallRound,
		EnemyRound,
		GameOver
	}

	public TurnState currentTurnState = TurnState.BallRound;

	[Header("玩家")]
	public SlingshotBall ballScript;

	[Header("当前场上所有敌人（小怪 + Boss）")]
	public List<EnemyAI> enemies = new List<EnemyAI>();

	[Header("只用于判断小怪是否清空（不要把 Boss 塞进来）")]
	[SerializeField] private List<EnemyAI> minionEnemies = new List<EnemyAI>();

	[Header("Boss 刷新")]
	public BossSpawner bossSpawner;
	private bool bossSpawned = false;

	private void Start()
	{
		// 如果你懒得在 Inspector 里拖，就自动收集场景里的敌人（不包括一开始就隐藏的 Boss）
		if (enemies.Count == 0)
		{
			enemies.AddRange(FindObjectsOfType<EnemyAI>());
		}

		// 初始小怪列表：默认就是开局的这些敌人（不开启的 Boss 不会被算进去）
		if (minionEnemies.Count == 0)
		{
			minionEnemies = new List<EnemyAI>(enemies);
		}

		StartCoroutine(HandleTurns());
	}

	private IEnumerator HandleTurns()
	{
		while (currentTurnState != TurnState.GameOver)
		{
			switch (currentTurnState)
			{
				case TurnState.BallRound:
					StartBallRound();
					yield return new WaitUntil(() => ballScript != null && ballScript.isStop);
					yield return new WaitForSeconds(0.2f);
					EndBallRound();
					break;

				case TurnState.EnemyRound:
					yield return StartCoroutine(HandleEnemyRound());
					EndEnemyRound();
					break;
			}

			yield return null;
		}
	}

	// ---------- 玩家回合 ----------

	void StartBallRound()
	{
		if (ballScript != null)
		{
			ballScript.enabled = true;
			ballScript.StartNewRound();   // ✅ 统一在这里清状态
		}
	}


	private void EndBallRound()
	{
		if (ballScript != null)
			ballScript.enabled = false;

		currentTurnState = TurnState.EnemyRound;
	}

	// ---------- 敌人回合（所有敌人轮流行动） ----------

	private IEnumerator HandleEnemyRound()
	{
		Debug.Log("[Round] 敌人回合开始");

		// 清理已经真正被 Destroy 的敌人
		enemies.RemoveAll(e => e == null);

		// 如果场上没有任何敌人了：
		if (enemies.Count == 0)
		{
			// 1）Boss 已经刷出来又死了 → 通关
			if (bossSpawned)
			{
				Debug.Log("[Round] 所有敌人（包括 Boss）都死了，通关！");
				currentTurnState = TurnState.GameOver;
				yield break;
			}
			// 2）理论上这里小怪清空但 Boss 还没刷，这种情况 OnEnemyDead 会负责刷 Boss，
			//    这里就当这一轮敌人没人行动，直接结束这回合即可。
			Debug.Log("[Round] 当前回合没有敌人可行动");
			yield break;
		}

		// 还有敌人（可能包含 Boss），轮流行动
		var snapshot = new List<EnemyAI>(enemies); // 避免遍历时列表被修改

		foreach (var enemy in snapshot)
		{
			if (enemy == null) continue;

			enemy.BeginTurn();
			yield return new WaitUntil(() => enemy.isMyTurn == false);
			yield return new WaitForSeconds(0.1f);
		}

		Debug.Log("[Round] 敌人回合结束");
	}

	private void EndEnemyRound()
	{
		currentTurnState = TurnState.BallRound;
	}

	// ---------- 敌人注册 / 死亡回调 ----------

	/// <summary>
	/// BossSpawner 等地方生成新敌人时调用
	/// </summary>
	public void RegisterEnemy(EnemyAI enemy)
	{
		if (enemy != null && !enemies.Contains(enemy))
		{
			enemies.Add(enemy);
		}
	}

	/// <summary>
	/// 敌人死亡时调用（由 HealthSystem_New / EnemyDamageReceiver 通知）
	/// </summary>
	public void OnEnemyDead(EnemyAI enemy)
	{
		if (enemy == null) return;

		enemies.Remove(enemy);
		minionEnemies.Remove(enemy);

		// ★ 小怪全灭，但 Boss 还没刷出来 → 现在立刻刷 Boss
		if (!bossSpawned && bossSpawner != null && minionEnemies.Count == 0)
		{
			bossSpawned = true;
			bossSpawner.SpawnBoss();
			Debug.Log("[Round] 小怪清空，刷新 Boss！");
		}
	}

	/// <summary>
	/// 给 BossSpawner 调用，告诉 Round：Boss 已经正式生成
	/// </summary>
	public void NotifyBossSpawned(EnemyAI boss)
	{
		bossSpawned = true;
		RegisterEnemy(boss);
	}
}
