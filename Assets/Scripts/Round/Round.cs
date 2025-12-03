using System.Collections;
using UnityEngine;

public class Round : MonoBehaviour
{
    public enum TurnState
    {
        BallRound, // 现在是 BallRound
        EnemyRound, // 现在是 EnemyRound
        GameOver
    }
    public float ballTime = 1f;
    public float enemyTime = 0.3f;
    public TurnState currentTurnState = TurnState.BallRound;

    public SlingshotBall ballScript; // 控制小球的脚本
    public EnemyAI enemyScript; // 控制敌人的脚本

    void Start()
    {
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
                    yield return new WaitForSeconds(ballTime);
                    EndBallRound(); // 结束 BallRound
                    break;

                case TurnState.EnemyRound: // 敌人回合
                    StartEnemyRound(); // 启动 EnemyRound
					yield return new WaitUntil(() => enemyScript != null && enemyScript.isMyTurn == false);
					yield return new WaitForSeconds(enemyTime);
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

        if (enemyScript != null)
            enemyScript.enabled = false; // 禁用敌人脚本，防止敌人在 BallRound 内移动
    }

    // 结束 BallRound
    void EndBallRound()
    {
        currentTurnState = TurnState.EnemyRound; // 转到敌人回合
    }

    // 启动 EnemyRound
    void StartEnemyRound()
    {
        // 启用敌人控制脚本
        if (enemyScript != null)
        {
			enemyScript.BeginTurn();
			enemyScript.enabled = true;
		}
            

        if (ballScript != null)
            ballScript.enabled = false; // 禁用小球控制脚本，防止玩家在 EnemyRound 内操作
    }

    // 结束 EnemyRound
    void EndEnemyRound()
    {
        currentTurnState = TurnState.BallRound; // 转到玩家回合
    }
}