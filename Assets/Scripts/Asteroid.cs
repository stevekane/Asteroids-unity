using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Asteroid : MonoBehaviour 
{
	public Rigidbody2D rigidBody;
	public OnCollision onCollision;

	void OnTriggerEnter2D(Collider2D other)
	{
		onCollision(gameObject, other.gameObject);
	}
}