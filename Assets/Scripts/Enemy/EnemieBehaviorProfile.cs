using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using static EnemyAI;

public enum EnemyKind { 
    Grunt = 0,   //常规杂兵
    Brute,       //力大难缠型
    Assassin,    //刺客型
    Boss         //Boss战
}
[CreateAssetMenu(fileName = "EnemieBehaviorProfile", menuName = "EnemieBehaviorProfile")]
public class EnemieBehaviorProfile : ScriptableObject {
    [Header("基础属性")]
    public EnemyKind kind;

	[Header("状态与行为参数")]
	[Tooltip("感知距离：超过这个距离就基本不鸟玩家")]
	public float detectRange = 10f;

	[Tooltip("追击距离阈值：距离在 fleeDistance ~ chaseRange 之间会追上去")]
	public float chaseRange = 7f;

	[Tooltip("逃跑阈值：距离过近会优先远离玩家")]
	public float fleeDistance = 3f;

	[Tooltip("冲刺触发距离：进入这个范围，有概率发动 Dash 强力冲刺")]
	public float dashDistance = 5f;

	[Tooltip("普通追击/逃跑的冲量大小（越大越远）")]
	public float moveImpulse = 5f;

	[Tooltip("冲刺时的冲量大小")]
	public float dashImpulse = 8f;

	[Tooltip("AI 思考时间（回合开始到真正行动的延迟）")]
	public float thinkTime = 0.25f;

	[Tooltip("回合结束判断：速度小于该阈值时认为“动完了”")]
	public float endTurnSpeedThreshold = 0.05f;

	[Tooltip("安全上限：一个回合内最多移动多久（防止卡死）")]
	public float maxTurnDuration = 2.0f;

	[Header("性格系数 0~1")]
	[Range(0f, 1f)] public float aggressiveness = 0.5f; // 爱冲上去程度
	[Range(0f, 1f)] public float cowardness = 0.0f;     // 害怕程度
	[Range(0f, 1f)] public float backstabby = 0.0f;     // 喜欢绕屁股打

	[Tooltip("Dash 的随机概率（0~1），在距离合适时才会用到")]
	[Range(0f,1f)]
	public float dashProbability = 0.4f;
	
}
