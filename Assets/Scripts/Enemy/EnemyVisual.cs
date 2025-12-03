using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyVisual : MonoBehaviour
{
	private SpriteRenderer sr;

	public Color normalColor = Color.green;
	public Color fleeColor = Color.blue;
	public Color dashColor = Color.red;
	public Color chaseColor = Color.yellow;

	void Awake()
	{
		sr = GetComponent<SpriteRenderer>();
		SetNormal();
	}

	void SetColor(Color c)
	{
		sr.color = c;
	}

	public void SetNormal() => SetColor(normalColor);
	public void SetFlee() => SetColor(fleeColor);
	public void SetDash() => SetColor(dashColor);
	public void SetChase() => SetColor(chaseColor);
}
