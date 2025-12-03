using System;
using System.Collections;
using UnityEngine;
using static EnemieBehaviorProfile;
/// <summary>
/// 敌人回合制 AI：
/// - BeginTurn() 在回合开始时由回合管理器调用
/// - 自动根据与玩家距离选择：追击 / 逃跑 / 冲刺 / 原地思考
/// - 等到移动基本停下时，自动结束回合并调用 OnTurnEnd 回调
/// 
/// 依赖组件：
/// - EnemyBallPhysics（拿刚体、质量等）
/// - EnemyVisual2D（切换颜色表示状态）
/// </summary>
[RequireComponent(typeof(EnemyBallPhysics))]
[RequireComponent(typeof(EnemyVisual2D))]
public class EnemyAI : MonoBehaviour
{
	public enum EnemyState
	{
		Idle,       // 发呆 / 待机
		Chase,      // 普通追击
		Flee,       // 远离玩家
		Dash        // 冲刺（强力一击）
	}

	[Header("基础引用")]
	[Tooltip("玩家物体（最好是玩家小球），如果不指定会按Tag=Player自动寻找")]
	public Transform player;

	private EnemyBallPhysics enemyPhysics;
	private EnemyVisual2D enemyVisual;
	private Rigidbody2D rb;
	public EnemieBehaviorProfile behavior;

	[Header("状态与行为参数")]
	[Tooltip("感知距离：超过这个距离就基本不鸟玩家")]
	private float detectRange;

	[Tooltip("追击距离阈值：距离在 fleeDistance ~ chaseRange 之间会追上去")]
	private float chaseRange;

	[Tooltip("逃跑阈值：距离过近会优先远离玩家")]
	private float fleeDistance;

	[Tooltip("冲刺触发距离：进入这个范围，有概率发动 Dash 强力冲刺")]
	private float dashDistance;

	[Tooltip("普通追击/逃跑的冲量大小（越大越远）")]
	private float moveImpulse;

	[Tooltip("冲刺时的冲量大小")]
	private float dashImpulse;

	[Tooltip("AI 思考时间（回合开始到真正行动的延迟）")]
	private float thinkTime;

	[Tooltip("回合结束判断：速度小于该阈值时认为“动完了”")]
	private float endTurnSpeedThreshold;

	[Tooltip("安全上限：一个回合内最多移动多久（防止卡死）")]
	private float maxTurnDuration;

	[Header("性格系数 0~1")]
	[Range(0f, 1f)] public float aggressiveness = 0.5f; // 爱冲上去程度
	[Range(0f, 1f)] public float cowardness = 0.0f;     // 害怕程度
	[Range(0f, 1f)] public float backstabby = 0.0f;     // 喜欢绕屁股打
	[Tooltip("Dash 的随机概率（0~1），在距离合适时才会用到")]
	[Range(0f, 1f)]
	private float dashProbability;

	[Header("调试")]
	public EnemyState currentState = EnemyState.Idle;
	[Tooltip("只读：当前是否是敌人的回合")]
	public bool isMyTurn;

	// 回合结束时，由外部回合管理器注册这个回调
	public Action OnTurnEnd;

	private Coroutine turnRoutine;

	private System.Random rng = new System.Random();

	private void Awake()
	{
		enemyPhysics = GetComponent<EnemyBallPhysics>();
		enemyVisual = GetComponent<EnemyVisual2D>();
		rb = enemyPhysics != null ? enemyPhysics.rb : null;


		if (player == null)
		{
			GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
			if (playerObj != null)
			{
				player = playerObj.transform;
			}
			else
			{
				Debug.LogWarning("[EnemyAI] 未找到 Tag 为 Player 的对象，请在 Inspector 手动指定 player 引用。");
			}
		}

		if(behavior != null)
		{
			detectRange = behavior.detectRange;
			chaseRange = behavior.chaseRange;
			dashDistance = behavior.dashDistance;
			fleeDistance = behavior.fleeDistance;
			moveImpulse = behavior.moveImpulse;
			dashImpulse = behavior.dashImpulse;
			dashProbability = behavior.dashProbability; // 越凶越爱 Dash
			aggressiveness = behavior.aggressiveness;
			cowardness= behavior.cowardness;
			backstabby = behavior.backstabby;

		    thinkTime = behavior.thinkTime;
			endTurnSpeedThreshold = behavior.endTurnSpeedThreshold;
			maxTurnDuration = behavior.maxTurnDuration;
		}
	}

	/// <summary>
	/// 在“敌人回合开始”时由回合管理器调用
	/// </summary>
	public void BeginTurn(Action onTurnEndCallback = null)
	{
		if (isMyTurn)
		{
			// 防止重复开始
			return;
		}

		isMyTurn = true;
		OnTurnEnd = onTurnEndCallback;

		// 保守一点：防止上一次 Coroutine 没停干净
		if (turnRoutine != null)
		{
			StopCoroutine(turnRoutine);
		}
		turnRoutine = StartCoroutine(TurnLogic());
	}

	/// <summary>
	/// 主回合逻辑：思考 → 选择行为 → 施加冲量 → 等待停下 → 结束回合
	/// </summary>
	private IEnumerator TurnLogic()
	{
		// 小小的“思考”时间，避免敌人像机器人一样瞬时行动
		yield return new WaitForSeconds(thinkTime);

		if (player == null)
		{
			// 找不到玩家，就随机晃悠一下
			ChooseStateWithoutPlayer();
		}
		else
		{
			DecideAction();
		}

		// 根据状态执行对应行动（施加冲量、切换颜色等）
		DoActionByState();

		// 等待敌人“动完了”
		float timer = 0f;
		while (timer < maxTurnDuration)
		{
			timer += Time.deltaTime;

			if (rb == null) break;

			if (rb.velocity.magnitude < endTurnSpeedThreshold)
			{
				break;
			}

			yield return null;
		}

		// 保险：强制刹车一下，防止残余速度导致下个回合误判
		if (rb != null)
		{
			rb.velocity = Vector2.zero;
			rb.angularVelocity = 0f;
		}

		EndTurn();
	}

	/// <summary>
	/// 根据与玩家的距离选择状态
	/// </summary>
	private void ChooseStateByDistance()
	{
		float dist = Vector2.Distance(transform.position, player.position);

		if (dist > detectRange)
		{
			// 玩家太远：基本当空气
			Debug.LogWarning("[EnemyAI] 玩家距离过远，敌人状态切换为 Idle");
			currentState = EnemyState.Idle;
		}
		else if (dist < fleeDistance)
		{
			// 太近了：优先远离
			currentState = EnemyState.Flee;
		}
		else if (dist < dashDistance)
		{
			// 进入冲刺范围，有一定概率 Dash
			float p = (float)rng.NextDouble();
			currentState = (p < dashProbability) ? EnemyState.Dash : EnemyState.Chase;
		}
		else if (dist < chaseRange)
		{
			// 中距离：跟上去
			currentState = EnemyState.Chase;
		}
		else
		{
			// 在感知范围里，但又不急，就当发呆
			currentState = EnemyState.Idle;
		}
	}

	/// <summary>
	/// 找不到玩家时的兜底：随机晃一晃
	/// </summary>
	private void ChooseStateWithoutPlayer()
	{
		// 约69%的概率原地发呆，31%随便动一下
		float p = (float)rng.NextDouble();
		if (p < 0.69f)
		{
			currentState = EnemyState.Idle;
		}
		else
		{
			currentState = EnemyState.Chase; // 当作“随便找个方向乱撞”
		}
	}

	/// <summary>
	/// 执行当前状态对应的动作（改变颜色 + 施加冲量）
	/// </summary>
	private void DoActionByState()
	{
		if (rb == null) return;

		Vector2 dir = Vector2.zero;

		switch (currentState)
		{
			case EnemyState.Idle:
				enemyVisual.SetNormal();
				// 轻微随机晃动一下，避免完全静止像假人
				dir = UnityEngine.Random.insideUnitCircle.normalized;
				rb.AddForce(dir * (moveImpulse * 0.3f), ForceMode2D.Impulse);
				break;

			case EnemyState.Chase:
				enemyVisual.SetChase();
				if (player != null)
				{
					dir = ((Vector2)(player.position - transform.position)).normalized;
				}
				else
				{
					dir = UnityEngine.Random.insideUnitCircle.normalized;
				}
				rb.AddForce(dir * moveImpulse, ForceMode2D.Impulse);
				break;

			case EnemyState.Flee:
				enemyVisual.SetFlee();
				if (player != null)
				{
					dir = ((Vector2)(transform.position - player.position)).normalized;
				}
				else
				{
					dir = UnityEngine.Random.insideUnitCircle.normalized;
				}
				rb.AddForce(dir * moveImpulse, ForceMode2D.Impulse);
				break;

			case EnemyState.Dash:
				enemyVisual.SetDash();
				if (player != null)
				{
					dir = ((Vector2)(player.position - transform.position)).normalized;
				}
				else
				{
					dir = UnityEngine.Random.insideUnitCircle.normalized;
				}
				rb.AddForce(dir * dashImpulse, ForceMode2D.Impulse);
				break;
		}
	}

	/// <summary>
	/// 结束敌人当前回合，并通知回合管理器
	/// </summary>
	private void EndTurn()
	{
		isMyTurn = false;

		// 回到普通颜色（正常状态）
		if (enemyVisual != null)
		{
			enemyVisual.SetNormal();
		}

		// 调用外部注册的回调
		OnTurnEnd?.Invoke();
		OnTurnEnd = null;

		// 清掉协程引用
		turnRoutine = null;
	}

	#region 实用公开接口

	/// <summary>
	/// 外部如果想强制切换状态（比如被某张卡牌嘲讽、恐惧等）可以调用这个。
	/// 下个回合开始时依旧会根据距离重新决策。
	/// </summary>
	public void ForceSetState(EnemyState newState)
	{
		currentState = newState;
	}

	/// <summary>
	/// 紧急打断当前回合（例如战斗结束、敌人死亡）
	/// </summary>
	public void ForceInterruptTurn()
	{
		if (turnRoutine != null)
		{
			StopCoroutine(turnRoutine);
			turnRoutine = null;
		}

		isMyTurn = false;
		OnTurnEnd = null;

		if (rb != null)
		{
			rb.velocity = Vector2.zero;
			rb.angularVelocity = 0f;
		}
	}

	public float GetSizeRatio()
	{
		if (!player) return 1f;
		float mySize = (transform.localScale.x + transform.localScale.y) * 0.5f;
		float pSize = (player.localScale.x + player.localScale.y) * 0.5f;
		if (mySize <= 0.0001f) return 1f;
		return pSize/mySize;
	}

	public float ScoreChase(float dist,float sizeRatio) {
		float s = 0f;
		// 距离在 chaseRange 附近最想追
		s += Mathf.InverseLerp(chaseRange, dashDistance, dist);
		// 距离越近越想冲刺
		s += Mathf.InverseLerp(dashDistance, fleeDistance, dist);
		// 大小比例越大越想冲刺
		s += Mathf.InverseLerp(1f, 2f, sizeRatio);
		// 越凶越爱 Dash
		if (behavior) s *= Mathf.Lerp(0.5f, 1.5f, behavior.aggressiveness);

		return s;
	}

	public float ScoreDash(float dist, float sizeRatio)
	{
		float s = 0f;
		// 只有在 dashDistance 内才考虑 Dash
		s += Mathf.InverseLerp(dashDistance * 1.5f, dashDistance * 0.5f, dist);
		// 玩家比我小 → 更敢 Dash
		s += Mathf.InverseLerp(1.5f, 0.5f, sizeRatio);
		if (behavior) s *= Mathf.Lerp(0.5f, 2.0f, behavior.aggressiveness);
		return s;
	}

	public float ScoreFlee(float dist, float sizeRatio) {
		float s = 0f;
		// 越近越想跑
		s += Mathf.InverseLerp(fleeDistance * 2f, fleeDistance * 0.3f, dist);
		// 玩家比我大 → 更想跑
		s += Mathf.InverseLerp(0.8f, 1.8f, sizeRatio);
		if (behavior) s *= Mathf.Lerp(0.5f, 2.0f, behavior.cowardness);
		return s;
	}

	public float ScoreBackstab(float dist, float sizeRatio)
	{
		if (!player || behavior == null) return 0f;
		if (behavior.backstabby <= 0f) return 0f;

		float s = 0f;
		// 偷袭：希望距离中等（不要太远，也不要贴脸）
		s += Mathf.InverseLerp(chaseRange, dashDistance, dist);

		// 玩家越大，越想绕屁股打
		s += Mathf.InverseLerp(0.8f, 1.5f, sizeRatio);

		// 乘上“阴险度”
		s *= behavior.backstabby;
		return s;
	}
	void DecideAction()
	{
		if (!player)
		{
			currentState = EnemyState.Idle;
			return;
		}
		float dist = Vector2.Distance(transform.position, player.position);
		float sizeRatio = GetSizeRatio();

		float chaseScore = ScoreChase(dist, sizeRatio);
		float dashScore = ScoreDash(dist, sizeRatio);
		float fleeScore = ScoreFlee(dist, sizeRatio);
		float backstabScore = ScoreBackstab(dist, sizeRatio);
		float idleScore = 0.2f; // 基本发呆

		// 针对不同种类再做一点加权
		if (behavior)
		{
			switch (behavior.kind)
			{
				case EnemyKind.Brute:
					dashScore *= 1.3f;
					fleeScore *= 0.6f;
					break;
				case EnemyKind.Assassin:
					backstabScore *= 1.5f;
					fleeScore *= 1.2f;
					break;
				case EnemyKind.Grunt:
					// 普通小兵，比较平均
					break;
			}
		}

		float maxScore = Mathf.Max(chaseScore, dashScore, fleeScore, backstabScore, idleScore);

		if (maxScore == dashScore)
			currentState = EnemyState.Dash;
		else if (maxScore == backstabScore)
			currentState = EnemyState.Chase;   // 行为上仍然是 Chase，但你可以在移动方向上做“绕后”处理
		else if (maxScore == fleeScore)
			currentState = EnemyState.Flee;
		else if (maxScore == chaseScore)
			currentState = EnemyState.Chase;
		else
			currentState = EnemyState.Idle;
	}
	#endregion
}
