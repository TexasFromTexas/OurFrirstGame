using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
	public enum TurnState
	{
		BallRound, // 现在是 BallRound
		EnemyRound, // 现在是 EnemyRound
		GameOver
	}
	public float time = 10f;
	public TurnState currentTurnState = TurnState.BallRound;

	public SlingshotBall ballScript; // 控制小球的脚本
	public List<EnemyAI> enemies = new List<EnemyAI>(); // 控制敌人的脚本

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
					yield return new WaitForSeconds(1f);
					EndBallRound(); // 结束 BallRound
					break;

				case TurnState.EnemyRound: // 敌人回合
					yield return StartCoroutine(HandleEnemyRound());
					EndEnemyRound(); // 结束 EnemyRound
					break;
			}
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
			yield return new WaitForSeconds(0.1f);
		}

		Debug.Log("敌人回合结束（全部行动完）");
	}
	// 结束 EnemyRound
	void EndEnemyRound()
	{
		currentTurnState = TurnState.BallRound; // 转到玩家回合
	}
}