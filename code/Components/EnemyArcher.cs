using Sandbox;
using Sandbox.Citizen;
using Sandbox.VR;
using System;
using System.Numerics;

public sealed class EnemyArcher : Component
{

	[Property]
	public CharacterController Controller { get; set; }

	[Property]
	public GameObject Player { get; set; }

	[Property]
	public CitizenAnimationHelper AnimationHelper { get; set; }

	[Property]
	public GameObject ArrowPrefab { get; set; }

	[Property]
	public GameObject ArrowSpawnLocation { get; set; }


	public float backingDistance = 300f; // Distance at which AI starts backing up
	public float backingSpeed = 40f; // Speed at which AI backs up
	public float backingInterval = new Random().Next( 5, 15 ); // Interval at which AI checks for backing up

	public float forwardDistance = 300f; // Distance at which AI starts backing up
	public float forwardSpeed = 60f; // Speed at which AI backs up
	public float forwardInterval = new Random().Next( 5, 15 ); // Interval at which AI checks for backing up

	public float attackInterval = 3f;
	public float arrowSpeed = 1000f;
	public Vector3 arrowSpawnOffset = new Vector3(-20f, 3f, 54f);

	private Vector3 initialPosition; // Initial position of the AI
	private bool isBacking = false; // Flag to indicate if AI is currently backing up
	private float backingTimer = 0f; // Timer for backing interval

	private bool isForwarding = false; // Flag to indicate if AI is currently backing up
	private float forwardTimer = 0f; // Timer for backing interval

	TimeSince timeSinceAdjustYaw;

	TimeSince timeSinceArrowShot;

	protected override void OnUpdate()
	{

		if ( !Components.Get<UnitInfo>().IsAlive ) return;
		float distanceToPlayer = Transform.Position.Distance( Player.Transform.Position );
		if ( distanceToPlayer > 2000f ) return;

		Vector3 targetToPlayerDisplacement = (Transform.Position - Player.Transform.Position);
		if ( timeSinceAdjustYaw > new Random().Next( 5, 30 ) )
		{
			Transform.Rotation = Rotation.Lerp( Transform.Rotation, (targetToPlayerDisplacement.Normal * -1).EulerAngles.WithPitch( 0f ).WithRoll( 0f ), 0.1f );
			float angleDiff = Transform.Rotation.Distance( (targetToPlayerDisplacement.Normal * -1).EulerAngles.WithPitch( 0f ).WithRoll( 0f ) );
			if ( angleDiff < 1 ) timeSinceAdjustYaw = 0;
		}
		//AnimationHelper.WithLook( Transform.World.PointToLocal( Player.Transform.Position ).WithZ( Transform.World.PointToLocal( Player.Transform.Position ).z) );
		if ( Components.Get<UnitInfo>().Type == UnitType.Boss )
		{
			AnimationHelper.WithLook( (Player.Transform.Position - Transform.Position).WithZ( (Player.Transform.Position - Transform.Position).z - 100f ) );
		}
		else
		{
			AnimationHelper.WithLook( Player.Transform.Position - Transform.Position );
		}

	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( Controller == null ) return;
		if ( !Components.Get<UnitInfo>().IsAlive ) return;

		if ( Controller.IsOnGround )
		{
			float distanceToPlayer = Transform.Position.Distance( Player.Transform.Position );
			if ( distanceToPlayer > 2000f ) return;

			if ( CanSeePlayer() )
			{
				TryShootPlayer();
				Log.Info( "Can see player" );
				if ( distanceToPlayer < 2000f && distanceToPlayer >= forwardDistance - 5f )
				{
					approachBehavior(distanceToPlayer);
				}
				else if ( distanceToPlayer > backingDistance + 5f && distanceToPlayer < forwardDistance - 5f )
				{
					standStillBehavior();
				}
				else
				{
					retreatBehavior(distanceToPlayer);
				}
			}
		}
		else
		{
			Controller.Acceleration = 2.0f;
			Controller.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
		}

		Controller.Move();

		if ( AnimationHelper != null )
		{
			AnimationHelper.IsGrounded = Controller.IsOnGround;
			AnimationHelper.WithVelocity( Controller.Velocity );
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		initialPosition = Transform.Position;
	}

	bool CanSeePlayer()
	{
		bool hitPlayer = false;

		var traceToPlayer = Scene.Trace.Ray( Transform.Position.WithZ(Transform.Position.z + 45), Player.Transform.Position.WithZ(Player.Transform.Position.z + 45 )).Size( 3f ).IgnoreGameObjectHierarchy(GameObject).UseHitboxes( true ).Run();

		if ( traceToPlayer.Hitbox != null && traceToPlayer.Hitbox.GameObject.Components.TryGet( out Player _ ) )
		{
			hitPlayer = true;
		}

		return hitPlayer;
	}

	void approachBehavior(float distanceToPlayer)
	{
		// Check if it's time to go forward
		forwardTimer += Time.Delta;
		if ( forwardTimer >= forwardInterval )
		{

			forwardInterval = new Random().Next( 2, 5 );

			forwardTimer = 0f;

			// Check if AI should start going forward
			if ( distanceToPlayer >= forwardDistance && !isForwarding )
			{
				isForwarding = true;
			}

			// Check if AI should stop going forward
			if ( distanceToPlayer < forwardDistance && isForwarding )
			{
				isForwarding = false;
			}
		}
		Vector3 targetVelocity;
		// Perform go forward behavior
		if ( isForwarding )
		{

			// Calculate direction to move towards from player
			Vector3 direction = (Transform.Position - Player.Transform.Position).Normal;
			targetVelocity = -direction * forwardSpeed;
		}
		else
		{
			targetVelocity = Vector3.Zero;
		}

		Controller.Accelerate( targetVelocity );
		Controller.Acceleration = 10.0f;
		Controller.ApplyFriction( 5.0f, 0f );
	}
	void standStillBehavior()
	{
		return;
	}

	void retreatBehavior(float distanceToPlayer)
	{
		// Check if it's time to back up
		backingTimer += Time.Delta;
		if ( backingTimer >= backingInterval )
		{

			backingInterval = new Random().Next( 1, 3 );

			backingTimer = 0f;

			// Check if AI should start backing up
			if ( distanceToPlayer <= backingDistance && !isBacking )
			{
				isBacking = true;
			}

			// Check if AI should stop backing up
			if ( distanceToPlayer > backingDistance && isBacking )
			{
				isBacking = false;
			}
		}
		Vector3 targetVelocity;
		// Perform backing up behavior
		if ( isBacking )
		{

			// Calculate direction to move away from player
			Vector3 direction = (Transform.Position - Player.Transform.Position).Normal;
			targetVelocity = direction * backingSpeed;
		}
		else
		{
			targetVelocity = Vector3.Zero;
		}

		Controller.Accelerate( targetVelocity );
		Controller.Acceleration = 10.0f;
		Controller.ApplyFriction( 5.0f, 0f );
	}

	void TryShootPlayer()
	{
		//Log.Info( "Tried" );
		if ( timeSinceArrowShot > attackInterval)
		{
			ShootPlayer();
			timeSinceArrowShot = 0f;
		}
	}

	void ShootPlayer()
	{
		Vector3 directionToPlayer = (((Player.Transform.Position - arrowSpawnOffset) + new Vector3(0,0,45)) - Transform.Position).Normal;
		CreateArrow( directionToPlayer * arrowSpeed );
	}

	void CreateArrow(Vector3 launchVector)
	{
		GameObject arrow = ArrowPrefab.Clone(ArrowSpawnLocation.Transform.Position);

		arrow.Components.Get<Arrow>().owner = GameObject;
		arrow.Components.Get<Arrow>().initialVelocity = launchVector;

		arrow.Components.Get<Rigidbody>().Velocity = launchVector;

		Angles launchAngles = launchVector.EulerAngles;
		Rotation launchRotation = Rotation.From( launchAngles );

		arrow.Transform.Rotation = launchRotation.RotateAroundAxis(Transform.LocalRotation.Up, 90);

		//arrow.Transform.LocalRotation = arrow.Transform.LocalRotation.Angles().WithYaw( arrow.Transform.LocalRotation.Yaw() + 90 );

		arrow.Components.Get<Rigidbody>().Velocity = launchVector;
	}
}
