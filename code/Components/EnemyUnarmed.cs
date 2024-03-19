using Sandbox;
using Sandbox.Citizen;
using Sandbox.VR;
using System;
using System.Numerics;

public sealed class EnemyUnarmed : Component
{

	[Property]
	public CharacterController Controller { get; set; }

	[Property]
	public GameObject Player { get; set; }

	[Property]
	public CitizenAnimationHelper AnimationHelper { get; set; }

	public float backingDistance = 100f; // Distance at which AI starts backing up
	public float backingSpeed = 25f; // Speed at which AI backs up
	public float backingInterval = new Random().Next( 5, 15 ); // Interval at which AI checks for backing up

	private Vector3 initialPosition; // Initial position of the AI
	private bool isBacking = false; // Flag to indicate if AI is currently backing up
	private float backingTimer = 0f; // Timer for backing interval

	TimeSince timeSinceAdjustYaw;

	protected override void OnUpdate()
	{

		if ( !Components.Get<UnitInfo>().IsAlive ) return;

		float distanceToPlayer = Transform.Position.Distance( Player.Transform.Position );

		if ( distanceToPlayer > 800f ) return;

		if ( Components.Get<UnitInfo>().Health == Components.Get<UnitInfo>().MaxHealth ) return;
		Vector3 targetToPlayerDisplacement = (Transform.Position - Player.Transform.Position);
		if ( timeSinceAdjustYaw > new Random().Next( 5, 30 ) )
		{
			Transform.Rotation = Rotation.Lerp( Transform.Rotation, (targetToPlayerDisplacement.Normal * -1).EulerAngles.WithPitch( 0f ).WithRoll( 0f ), 0.1f );
			float angleDiff = Transform.Rotation.Distance( (targetToPlayerDisplacement.Normal * -1).EulerAngles.WithPitch( 0f ).WithRoll( 0f ) );
			if ( angleDiff < 1 ) timeSinceAdjustYaw = 0;
		}
		//AnimationHelper.WithLook( Transform.World.PointToLocal( Player.Transform.Position ).WithZ( Transform.World.PointToLocal( Player.Transform.Position ).z) );
		if ( Components.Get<UnitInfo>().Type == UnitType.Boss)
		{
			AnimationHelper.WithLook( (Player.Transform.Position - Transform.Position).WithZ( (Player.Transform.Position - Transform.Position).z - 100f) );
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

			//Log.Info( backingTimer );

			float distanceToPlayer = Transform.Position.Distance(Player.Transform.Position);

			//Log.Info( "Distance: " + distanceToPlayer );

			// Check if it's time to back up
			backingTimer += Time.Delta;
			if ( backingTimer >= backingInterval )
			{

				backingInterval = new Random().Next( 5, 8 );

				backingTimer = 0f;

				// Calculate distance to player
				//distanceToPlayer = Vector3.DistanceBetween( Transform.Position, Player.Transform.Position );

				//Log.Info( "Distance: " + distanceToPlayer );

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
				Log.Info( "BACKING UP" );

				// Calculate direction to move away from player
				Vector3 direction = (Transform.Position - Player.Transform.Position).Normal;
				targetVelocity = direction * backingSpeed;
			} 
			else
			{
				targetVelocity = Vector3.Zero;
			}

			//Log.Info( targetVelocity );

			Controller.Accelerate( targetVelocity );
			Controller.Acceleration = 10.0f;
			Controller.ApplyFriction( 5.0f, 0f );
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
}
