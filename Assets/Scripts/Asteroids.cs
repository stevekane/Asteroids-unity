using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public delegate void OnCollision(GameObject a, GameObject b);

public class Asteroids : MonoBehaviour 
{
	static int BULLET_COUNT = 100;
	static int LARGE_ASTEROID_COUNT = 50;
	static int MEDIUM_ASTEROID_COUNT = 100;
	static int SMALL_ASTEROID_COUNT = 200;
	static int LARGE_EXPLOSION_COUNT = 50;
	static int MEDIUM_EXPLOSION_COUNT = 100;
	static int SMALL_EXPLOSION_COUNT = 200;

	[Header("Prefabs")]
	[SerializeField]
	Ship ShipPrefab;
	[SerializeField]
	Bullet BulletPrefab;
	[SerializeField]
	Asteroid LargeAsteroidPrefab;
	[SerializeField]
	Asteroid MediumAsteroidPrefab;
	[SerializeField]
	Asteroid SmallAsteroidPrefab;
	[SerializeField]
	Explosion LargeExplosionPrefab;
	[SerializeField]
	Explosion MediumExplosionPrefab;
	[SerializeField]
	Explosion SmallExplosionPrefab;
	[SerializeField]
	Explosion ShipExplosion;

	[Header("Score Config")]
	public int LargeAsteroidPointValue = 20;
	public int MediumAsteroidPointValue = 25;
	public int SmallAsteroidPointValue = 30;
	public int UFOPointValue = 100;

	[Header("Input config")]
	public float fireRate = .1f;
	public float bulletSpeed = 300f;
	public float bulletDuration = 1f;
	public float maxShipSpeed = 100f;
	public float turnRate = 1f;
	public float thrust = 1f;
	public float largeAsteroidSpawnRate = 2000f;

	[Header("Playfield")]
	public Vector2 min = new Vector2(-400f, 400f);
	public Vector2 max = new Vector2(-300f, 300f);

	[Header("State")]
	Ship ship;
	Bullet[] bullets;
	Asteroid[] largeAsteroids;
	Asteroid[] mediumAsteroids;
	Asteroid[] smallAsteroids;
	Explosion[] largeExplosions;
	Explosion[] mediumExplosions;
	Explosion[] smallExplosions;
	Explosion shipExplosion;

	int bulletIndex = 0;
	int largeAsteroidIndex = 0;
	int mediumAsteroidIndex = 0;
	int smallAsteroidIndex = 0;
	int largeExplosionIndex = 0;
	int mediumExplosionIndex = 0;
	int smallExplosionIndex = 0;

	int largeAsteroidKills = 0;
	int mediumAsteroidKills = 0;
	int smallAsteroidKills = 0;
	int UFOKills = 0;
	float lastFireTime = 0f;
	float lastLargeAsteroidSpawnTime = 0f;

	public int numberOfLives = 3;
	public int score {
		get { 
			return 
				largeAsteroidKills * LargeAsteroidPointValue + 
				mediumAsteroidKills * MediumAsteroidPointValue +
				smallAsteroidKills * SmallAsteroidPointValue +
				UFOKills * UFOPointValue;
		}
	}

	void Start()
	{
		ship = Instantiate(ShipPrefab);
		shipExplosion = Instantiate(ShipExplosion);

		bullets = new Bullet[BULLET_COUNT];
		for (var i = 0; i < BULLET_COUNT; i++)
		{
			bullets[i] = Instantiate(BulletPrefab);
			bullets[i].transform.position = ship.transform.position;
			bullets[i].gameObject.SetActive(false);
		}

		largeAsteroids = new Asteroid[LARGE_ASTEROID_COUNT];
		for (var i = 0; i < LARGE_ASTEROID_COUNT; i++)
		{
			largeAsteroids[i] = Instantiate(LargeAsteroidPrefab);
			largeAsteroids[i].gameObject.SetActive(false);
			largeAsteroids[i].onCollision = onTrigger;
		}

		mediumAsteroids = new Asteroid[MEDIUM_ASTEROID_COUNT];
		for (var i = 0; i < MEDIUM_ASTEROID_COUNT; i++)
		{
			mediumAsteroids[i] = Instantiate(MediumAsteroidPrefab);
			mediumAsteroids[i].gameObject.SetActive(false);
			mediumAsteroids[i].onCollision = onTrigger;
		}
		
		smallAsteroids = new Asteroid[SMALL_ASTEROID_COUNT];
		for (var i = 0; i < SMALL_ASTEROID_COUNT; i++)
		{
			smallAsteroids[i] = Instantiate(SmallAsteroidPrefab);
			smallAsteroids[i].gameObject.SetActive(false);
			smallAsteroids[i].onCollision = onTrigger;
		}

		largeExplosions = new Explosion[LARGE_EXPLOSION_COUNT];
		for (var i = 0; i < LARGE_EXPLOSION_COUNT; i++)
		{
			largeExplosions[i] = Instantiate(LargeExplosionPrefab);
		}

		mediumExplosions = new Explosion[MEDIUM_EXPLOSION_COUNT];
		for (var i = 0; i < MEDIUM_EXPLOSION_COUNT; i++)
		{
			mediumExplosions[i] = Instantiate(MediumExplosionPrefab);
		}
				
		smallExplosions = new Explosion[SMALL_EXPLOSION_COUNT];
		for (var i = 0; i < SMALL_EXPLOSION_COUNT; i++)
		{
			smallExplosions[i] = Instantiate(SmallExplosionPrefab);
		}
	}	

	void FixedUpdate() 
	{
		var now = Time.time;

		// check losing conditions 
		// TODO: add some kind of "button to restart mechanic"
		if (numberOfLives <= 0)
			SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

		// kill dead bullets
		for (var i = 0; i < bullets.Length; i++)
		{
			if (now > bullets[i].deathTime)
				bullets[i].gameObject.SetActive(false);
		}

		// spawn new large asteroid if appropriate
		if (now > largeAsteroidSpawnRate + lastLargeAsteroidSpawnTime)
		{
			var position = new Vector2(max.x, max.y);
			var index = Random.Range(0, 2);
			var posForIndex = Random.Range(min[index], max[index]);
			var velocity = Vector2.zero;

			velocity.x = Random.Range(-200f, 200f);
			velocity.y = Random.Range(-200f, 200f);
			position[index] = posForIndex;
			largeAsteroids[largeAsteroidIndex].gameObject.SetActive(true);
			largeAsteroids[largeAsteroidIndex].transform.position = position;
			largeAsteroids[largeAsteroidIndex].rigidBody.velocity = velocity;
			largeAsteroidIndex = largeAsteroidIndex + 1 >= LARGE_ASTEROID_COUNT ? 0 : largeAsteroidIndex + 1;
			lastLargeAsteroidSpawnTime = now;
		}

		// wrap position of all entities
		wrapPosition(ship.gameObject);
		for (var i = 0; i < bullets.Length; i++)
		{
			wrapPosition(bullets[i].gameObject);
		}
		for (var i = 0; i < largeAsteroids.Length; i++)
		{
			wrapPosition(largeAsteroids[i].gameObject);
		}
		for (var i = 0; i < mediumAsteroids.Length; i++)
		{
			wrapPosition(mediumAsteroids[i].gameObject);
		}
		for (var i = 0; i < smallAsteroids.Length; i++)
		{
			wrapPosition(smallAsteroids[i].gameObject);
		}

		// process inputs
		if (Input.GetKey("left"))
			ship.transform.Rotate(0f, 0f, turnRate);

		if (Input.GetKey("right"))
			ship.transform.Rotate(0f, 0f, -turnRate);

		if (Input.GetKey("up"))
		{
			ship.rigidBody.AddForce(ship.transform.up * thrust, ForceMode2D.Impulse);
			if (!ship.audioSource.isPlaying)
				ship.audioSource.Play();
		}

		if (Input.GetKey("space") && Time.time > lastFireTime + fireRate)
		{
			var velocity = ship.rigidBody.velocity;
			var bullet = bullets[bulletIndex];

			velocity.x += ship.transform.up.x * bulletSpeed;
			velocity.y += ship.transform.up.y * bulletSpeed;

			bullet.gameObject.SetActive(true);
			bullet.deathTime = now + bulletDuration;
			bullet.audioSource.volume = Random.Range(.8f, 1f);
			bullet.audioSource.pitch = Random.Range(1f, 1.2f);
			bullet.audioSource.Play();
			bullet.transform.position = ship.transform.position;
			bullet.rigidBody.velocity = velocity;
			bulletIndex = bulletIndex + 1 >= BULLET_COUNT ? 0 : bulletIndex + 1;
			lastFireTime = Time.time;
		}
	}

	// Stupid boilerplate collision handling... a should always be an enemy
	void onTrigger(GameObject a, GameObject b)
	{
		if (b.tag == "ship")
		{
			var ship = b.GetComponent<Ship>();
			var asteroid = a.GetComponent<Asteroid>();

			onShipCollision(asteroid, ship);
		}
		else if (b.tag == "bullet")
		{
			var bullet = b.GetComponent<Bullet>();
			var asteroid = a.GetComponent<Asteroid>();

			if (a.tag == "large")
			{
				onLargeCollision(asteroid, bullet);
			}
			else if (a.tag == "medium")
			{
				onMediumCollision(asteroid, bullet);
			}
			else if (a.tag == "small")
			{
				onSmallCollision(asteroid, bullet);
			}
			else
			{
				Debug.Log("You hit an unknown object");
			}
		}
		else
		{
			Debug.Log("You hit an unknown object");
		}
	}

	void onLargeCollision(Asteroid a, Bullet b)
	{
		// spawn explosion
		var e = largeExplosions[largeExplosionIndex];

		e.transform.position = a.transform.position;
		e.gameObject.SetActive(true);
		e.audioSource.Play();
		e.particleSystem.Stop();
		e.particleSystem.Play();
		largeExplosionIndex = largeExplosionIndex + 1 >= LARGE_EXPLOSION_COUNT ? 0 : largeExplosionIndex + 1;

		// spawn medium asteroids
		var asteroidSpeed = a.rigidBody.velocity.magnitude;
		var asteroidPosition = a.transform.position;

		for (var i = 0; i < 2; i++)
		{
			var ma = mediumAsteroids[mediumAsteroidIndex];
			var velocity = Random.insideUnitCircle * asteroidSpeed * 1.2f;
			
			ma.gameObject.SetActive(true);
			ma.transform.position = asteroidPosition;
			ma.rigidBody.velocity = velocity;
			mediumAsteroidIndex = mediumAsteroidIndex + 1 >= MEDIUM_ASTEROID_COUNT ? 0 : mediumAsteroidIndex + 1;
		}

		// update kill count
		largeAsteroidKills++;

		// disable the large asteroid and bullet
		a.gameObject.SetActive(false);
		b.gameObject.SetActive(false);
	}

	void onMediumCollision(Asteroid a, Bullet b)
	{
		// spawn explosion
		var e = mediumExplosions[mediumExplosionIndex];

		e.transform.position = a.transform.position;
		e.gameObject.SetActive(true);
		e.audioSource.Play();
		e.particleSystem.Stop();
		e.particleSystem.Play();
		mediumExplosionIndex = mediumExplosionIndex + 1 >= MEDIUM_EXPLOSION_COUNT ? 0 : mediumExplosionIndex + 1;

		// spawn small asteroids
		var asteroidSpeed = a.rigidBody.velocity.magnitude;
		var asteroidPosition = a.transform.position;

		for (var i = 0; i < 2; i++)
		{
			var ma = smallAsteroids[smallAsteroidIndex];
			var velocity = Random.insideUnitCircle * asteroidSpeed * 1.2f;
			
			ma.gameObject.SetActive(true);
			ma.transform.position = asteroidPosition;
			ma.rigidBody.velocity = velocity;
			smallAsteroidIndex = smallAsteroidIndex + 1 >= SMALL_ASTEROID_COUNT ? 0 : smallAsteroidIndex + 1;
		}

		// update kill count
		mediumAsteroidKills++;

		// disable the medium asteroid and bullet
		a.gameObject.SetActive(false);
		b.gameObject.SetActive(false);
	}
		
	void onSmallCollision(Asteroid a, Bullet b)
	{
		// spawn explosion
		var e = smallExplosions[smallExplosionIndex];

		e.transform.position = a.transform.position;
		e.gameObject.SetActive(true);
		e.audioSource.Play();
		e.particleSystem.Stop();
		e.particleSystem.Play();
		smallExplosionIndex = smallExplosionIndex + 1 >= SMALL_EXPLOSION_COUNT ? 0 : smallExplosionIndex + 1;

		// update kill count
		smallAsteroidKills++;

		// disable the small asteroid and bullet
		a.gameObject.SetActive(false);
		b.gameObject.SetActive(false);
	}

	void onShipCollision(Asteroid asteroid, Ship ship)
	{
		// play explosion
		shipExplosion.transform.position = ship.transform.position;
		shipExplosion.particleSystem.Stop();
		shipExplosion.particleSystem.Play();
		shipExplosion.audioSource.Play();

		// remove asteroid
		asteroid.gameObject.SetActive(false);

		// reset ship
		ship.transform.position = Vector3.zero;
		ship.transform.rotation = Quaternion.identity;
		ship.rigidBody.velocity = Vector2.zero;

		// lower remaining lives
		numberOfLives--;
	}

	void wrapPosition(GameObject g)
	{
		var position = g.transform.position;

		if (position.x < min.x)
			position.x = max.x;
		else if (position.x > max.x)
			position.x = min.x;
		else
			position.x = position.x;

		if (position.y < min.y)
			position.y = max.y;
		else if (position.y > max.y)
			position.y = min.y;
		else
			position.y = position.y;

		g.transform.position = position;
	} 
}