using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour 
{
	static int POOL_SIZE = 3; 
	static float SHIP_WIDTH = 20f;

	public Asteroids asteroids;

	[SerializeField]
	RectTransform RemainingShipPrefab;
	[SerializeField]
	Transform remainingShipContainer;
	[SerializeField]
	Text scoreText;

	RectTransform[] shipPool;

	void Start()
	{
		shipPool = new RectTransform[POOL_SIZE];

		for (var i = 0; i < POOL_SIZE; i++)
		{
			var localPosition = new Vector3(-i * SHIP_WIDTH, 0, 0);

			shipPool[i] = Instantiate(RemainingShipPrefab, remainingShipContainer);
			shipPool[i].anchoredPosition = localPosition;
		}
	}

	void Update()
	{
		scoreText.text = $"{asteroids.score}";

		if (asteroids.numberOfLives < 0)
			return;

		for (var i = 0; i < asteroids.numberOfLives; i++)
		{
			shipPool[i].gameObject.SetActive(true);
		}
		for (var i = asteroids.numberOfLives; i < shipPool.Length; i++)
		{
			shipPool[i].gameObject.SetActive(false);
		}
	}
}