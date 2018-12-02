using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(AudioSource))]
public class Bullet : MonoBehaviour 
{
	public Rigidbody2D rigidBody;
	public AudioSource audioSource;
	public float deathTime = 0f;
}