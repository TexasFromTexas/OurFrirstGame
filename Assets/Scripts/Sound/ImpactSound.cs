using UnityEngine;

public class ImpactSound : MonoBehaviour
{
	public AudioSource sfx;
	public AudioClip fastClip;
	public AudioClip slowClip;
	public AudioClip volumeClip;

	public float impactThreshold = 4f;
	private Vector2 lastVelocity;

	public Rigidbody2D rb;

	void Update()
	{
		lastVelocity = rb.velocity;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		float impactForce = (lastVelocity - rb.velocity).magnitude;

		if (impactForce > impactThreshold)
		{
			float volume = Mathf.Clamp01(impactForce / 10f); // Ô½´ó×²»÷Ô½Ïì
			sfx.PlayOneShot(fastClip, volume);
		}
		else if (impactForce < 2f && impactForce > 0.1f)
		{
			sfx.PlayOneShot(slowClip, 1.5f);
		}else if (impactForce > 2f&& impactForce < 4f) { 
			sfx.PlayOneShot(slowClip, 1f);
		}
	}
}
