using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
	public Transform player;

	public float chaseSpeed = 5f;		  //追逐速度
	public float moveForce = 10f;		  //移动速度
	public float dashForce = 20f;		  //冲刺力
	public float attackRange = 5f;		  //攻击范围
	public float safeEdgeDistance = 2f;   // 离边缘小于这个就危险
	public float arenaRadius = 20f;       // 场地半径

	public float dashCooldownTime = 2f;   //冲刺时间
	private float dashCooldown = 0f;

	private Rigidbody rb;
	void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	void Update()
	{
		if (dashCooldown > 0f)
			dashCooldown -= Time.deltaTime;
	}

	void FixedUpdate()
	{
		DecideAndAct();
	}

	void DecideAndAct()
	{
		if (player == null) return;

		// 平面上的位置（忽略 y）
		Vector3 myPos = transform.position;
		Vector3 playerPos = player.position;

		Vector3 toPlayer = (playerPos - myPos);
		toPlayer.y = 0;
		float distToPlayer = toPlayer.magnitude;

		// 到场地中心和边缘的距离
		Vector2 myPos2D = new Vector2(myPos.x, myPos.z);
		float distFromCenter = myPos2D.magnitude;
		float distToEdge = arenaRadius - distFromCenter;

		// 体型比较：用 scale.x 估算
		bool playerBigger = player.localScale.x > transform.localScale.x * 1.1f;
		bool canDash = dashCooldown <= 0f;

		// ====== 决策树开始 ======

		// 1. 边缘很危险 + 玩家比我大 → 往中心跑
		if (distToEdge < safeEdgeDistance && playerBigger)
		{
			Vector3 dirToCenter = -new Vector3(myPos.x, 0, myPos.z).normalized;
			Move(dirToCenter);
			return;
		}

		// 2. 我更大或差不多 + 距离不远 + 冲撞CD好 → 冲撞
		if (!playerBigger && distToPlayer < attackRange && canDash)
		{
			Dash(toPlayer.normalized);
			return;
		}

		// 3. 玩家太远 → 追击
		float chaseMinDistance = 3f;
		if (distToPlayer > chaseMinDistance)
		{
			Move(toPlayer.normalized);
			return;
		}

		// 4. 既不该逃也不该冲又不该追 → 绕圈一点（横向位移）
		Vector3 right = Vector3.Cross(Vector3.up, toPlayer.normalized);
		Move(right.normalized * 0.7f + toPlayer.normalized * 0.3f);
	}

	void Move(Vector3 direction)
	{
		// 简单用力推动，也可以改成 rb.MovePosition
		rb.AddForce(direction.normalized * moveForce, ForceMode.Acceleration);
	}

	void Dash(Vector3 direction)
	{
		rb.AddForce(direction.normalized * dashForce, ForceMode.VelocityChange);
		dashCooldown = dashCooldownTime;
	}
}
