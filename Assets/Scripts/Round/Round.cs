using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BossSpawner;
public class Round : MonoBehaviour
{
	public enum TurnState
	{
		BallRound, // 现在是 BallRound
		EnemyRound, // 现在是 EnemyRound
		GameOver
	}
	public float BallTime = 1f;
	public TurnState currentTurnState = TurnState.BallRound;

	public SlingshotBall ballScript; // 控制小球的脚本
	public List<EnemyAI> enemies = new List<EnemyAI>(); // 控制敌人的脚本
	private bool bossSpawned = false;

	[Header("Boss生成器")]
	public BossSpawner bossSpawner;

	void Start()
	{
		if (enemies.Count == 0) {
			enemies.AddRange(FindObjectsOfType<EnemyAI>());
		}

		StartCoroutine(HandleTurns()); // 启动回合控制协程
	}
	IEnumerator HandleTurns()
	{
		while (currentTurnState != TurnState.GameOver)
		{
			switch (currentTurnState)
			{
				case TurnState.BallRound: // 玩家回合
					StartBallRound(); // 启动 BallRound
					yield return new WaitUntil(() => ballScript.isStop);
					yield return new WaitForSeconds(BallTime);
					EndBallRound(); // 结束 BallRound
					break;

				case TurnState.EnemyRound: // 敌人回合
					yield return StartCoroutine(HandleEnemyRound());
					EndEnemyRound(); // 结束 EnemyRound
					break;
			}
		}
	}

	public void RegisterEnemy(EnemyAI boss)
	{
		if (boss != null && !enemies.Contains(boss))
		{
			enemies.Add(boss);
			Debug.Log("[Round] Boss 已加入敌人回合列表");
		}
	}


	// 启动 BallRound
	void StartBallRound()
	{
		// 启用小球控制脚本
		if (ballScript != null) 
			ballScript.enabled = true;
	}

	// 结束 BallRound
	void EndBallRound()
	{
		if (ballScript != null)ballScript.enabled = false;
		currentTurnState = TurnState.EnemyRound; // 转到敌人回合
	}

	// ⭐ 敌人回合：依次让每个敌人行动一次
	IEnumerator HandleEnemyRound()
	{
		Debug.Log("敌人回合开始");

		// 清理已经死掉/销毁的敌人
		enemies.RemoveAll(e => e == null);

		// 如果没有敌人了，可以直接 GameOver 或进入下一关
		if (enemies.Count == 0)
		{
			if (!bossSpawned && bossSpawner != null) {
				bossSpawned = true;

				//播放动画---还没做

				enemies.RemoveAll(e => e == null);
			}
			else
			{
				// 2）Boss 也已经死了 → 通关
				Debug.Log("[Turn] 所有敌人（包括 Boss）都死了，通关！");
				currentTurnState = TurnState.GameOver;
				yield break;
			}
			
		}
		// 再检查一次，防止生成 Boss 失败之类
		if (enemies.Count == 0)
		{
			currentTurnState = TurnState.GameOver;
			yield break;
		}
		// 逐个敌人轮着走
		foreach (var enemy in enemies)
		{
			if (enemy == null) continue;

			// 开启这个敌人的一回合
			enemy.BeginTurn();

			// 等这个敌人动完（isMyTurn 在 EnemyAI.EndTurn 里会变成 false）
			yield return new WaitUntil(() => enemy.isMyTurn == false);

			// 敌人之间稍微留个间隔
			yield return new WaitForSeconds(0.3f);
		}

		Debug.Log("敌人回合结束（全部行动完）");
	}
	// 结束 EnemyRound
	void EndEnemyRound()
	{
		currentTurnState = TurnState.BallRound; // 转到玩家回合
	}
}