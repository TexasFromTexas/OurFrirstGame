using System;
using System.Collections;
using UnityEngine;
public class BossAI : EnemyAI
{
	[Header("阶段配置（每阶段一个 BehaviorProfile）")]
	public EnemieBehaviorProfile phase1Profile;
	public EnemieBehaviorProfile phase2Profile;
	public EnemieBehaviorProfile enragedProfile;
	public enum BossPhase { Phase1, Phase2, Enraged }

	public BossPhase phase = BossPhase.Phase1;

	private HealthSystem_New health;   // ★ 从这里拿血量
	private int turnCount = 0;
	private bool isChargingBigSkill = false;
	private Vector2 lastPlayerDir;

	protected override void Awake()
	{
		base.Awake();                        // 一定要保留父类的 rb / visual 初始化
		health = GetComponent<HealthSystem_New>();

		if (health == null)
			Debug.LogError("[BossAI] 没找到 HealthSystem_New，记得给 Boss 挂上！");
	}

	public override void BeginTurn(System.Action onTurnEndCallback = null)
	{
		turnCount++;

		UpdatePhaseByHP();   // ★ 用 health 决定阶段

		ApplyPhaseProfile(); // 用当前阶段的行为配置覆盖 EnemyAI 参数

		base.BeginTurn(onTurnEndCallback);
	}

	private void UpdatePhaseByHP()
	{
		if (health == null) return;

		float max = health.GetMaxHealth();
		float cur = health.GetCurrentHealth();
		float ratio = (max > 0) ? cur / max : 1f;

		if (ratio > 0.7f) phase = BossPhase.Phase1;
		else if (ratio > 0.3f) phase = BossPhase.Phase2;
		else phase = BossPhase.Enraged;
	}

	// 血量不再自己维护，而是从 health 拿。
	private void ApplyPhaseProfile()
	{
		EnemieBehaviorProfile p = null;

		switch (phase)
		{
			case BossPhase.Phase1: p = phase1Profile; Debug.Log("Boss Phase 1"); break;

			case BossPhase.Phase2: p = phase2Profile; Debug.Log("Boss Phase 2"); break;
			case BossPhase.Enraged: p = enragedProfile; Debug.Log("Boss Enraged"); break;
		}

		if (p == null) return;

		// 这里根据你自己 EnemyAI 里字段的命名来映射
		detectRange = p.detectRange;
		chaseRange = p.chaseRange;
		dashDistance = p.dashDistance;
		fleeDistance = p.fleeDistance;
		moveImpulse = p.moveImpulse;
		dashImpulse = p.dashImpulse;
		thinkTime = p.thinkTime;
		endTurnSpeedThreshold = p.endTurnSpeedThreshold;
		maxTurnDuration = p.maxTurnDuration;
		dashProbability = p.dashProbability;
		// 如果你有 aggressiveness / cowardness / backstabby 等，也可以存下来
		aggressiveness = p.aggressiveness;
		cowardness = p.cowardness;
		backstabby = p.backstabby;
	}
}
