using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(ParticleSystem))]
public class Explosion : MonoBehaviour 
{
	public AudioSource audioSource;
	public ParticleSystem particleSystem;
}