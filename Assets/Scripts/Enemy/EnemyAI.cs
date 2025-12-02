using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI2D : MonoBehaviour
{	//获取角色坐标
	public Transform player;

	public float moveForce = 5f;          // 普通移动力度
	public float dashForce = 15f;         // 冲撞力度
	public float attackRange = 5f;        // 攻击距离
	public float chaseMinDistance = 2f;   // 认为“太远”开始追击的阈值

	public float arenaRadius = 10f;       // 场地半径（假设中心在 (0,0)）
	public float safeEdgeDistance = 10f;   // 距离边缘小于这个就认为危险

	public float dashCooldownTime = 2f;		// 冲撞冷却时间
	private float dashCooldown = 0f;		

	private Rigidbody2D rb;				   //刚体
	private EnemyVisual2D visual;		   //可视化

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		visual = GetComponent<EnemyVisual2D>();
	}

	void Update()
	{
		if (dashCooldown > 0f)//碰撞cd
			dashCooldown -= Time.deltaTime;
	}

	void FixedUpdate()
	{
		DecideAndAct();
	}

	void DecideAndAct()
	{	//角色死亡
		if (player == null) return;

		Vector2 myPos = rb.position;
		Vector2 playerPos = player.position;

		Vector2 toPlayer = (playerPos - myPos).normalized;
		float distToPlayer = toPlayer.magnitude;

		// 计算离场地中心和边缘的距离
		float distFromCenter = myPos.magnitude;                 // 如果场地中心在 (0,0)
		float distToEdge = arenaRadius - distFromCenter;        // 距离边界的剩余距离

		// 体型比较：用 x 轴缩放近似
		bool playerBigger = player.localScale.x > transform.localScale.x * 1.1f;
		bool canDash = dashCooldown <= 0f;

		// ======= 决策树 =======

		// 1. 我在边缘附近且打不过玩家 → 往中心跑
		if (distToEdge < safeEdgeDistance && playerBigger)
		{
			visual?.SetFlee();

			Vector2 dirToCenter = (-myPos).normalized; // 从我指向 (0,0)
			Move(dirToCenter);
			return;
		}

		// 2. 我不比他小太多 + 他在攻击范围内 + 冲撞CD好 → 冲撞
		if (!playerBigger && distToPlayer < attackRange && canDash)
		{
			visual?.SetDash();

			Dash(toPlayer.normalized);
			return;
		}

		// 3. 玩家太远 → 追击
		if (distToPlayer > chaseMinDistance)
		{
			visual?.SetChase();


			Move(toPlayer.normalized);
			return;
		}

		// 4. 既不该逃也不该冲又不该追 → 小范围绕圈骚扰
		visual?.SetNormal();

		Vector2 right = new Vector2(-toPlayer.y, toPlayer.x).normalized; // 垂直方向
		Vector2 moveDir = (right * 0.7f + toPlayer.normalized * 0.3f).normalized;
		Move(moveDir);
	}

	void Move(Vector2 direction)
	{
		rb.AddForce(direction * moveForce, ForceMode2D.Force);
	}

	void Dash(Vector2 direction)
	{
		rb.AddForce(direction * dashForce, ForceMode2D.Impulse);
		dashCooldown = dashCooldownTime;
	}
}
