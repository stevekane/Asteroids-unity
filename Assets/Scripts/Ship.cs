using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(AudioSource))]
public class Ship : MonoBehaviour 
{
	public Rigidbody2D rigidBody;
	public AudioSource audioSource;
}