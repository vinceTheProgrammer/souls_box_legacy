using Sandbox;
using Sandbox.Citizen;
using Sandbox.Diagnostics;
using Sandbox.Utility;
using System;
using System.Numerics;

public sealed class Player : Component
{
	[Property]
	public GameObject Camera { get; set; }

	[Property]
	public CharacterController Controller { get; set; }

	[Property]
	public CitizenAnimationHelper AnimationHelper { get; set; }

	[Property]
	public SkinnedModelRenderer SkinnedModelRenderer { get; set; }

	[Property]
	public GameObject Sword { get; set; }

	[Property]
	public GameObject SwordTip { get; set; }

	[Property]
	public GameObject ShieldIKTarget { get; set; }

	[Property]
	public float WalkSpeed { get; set; } = 200.0f;

	[Property]
	public float RunSpeed { get; set; } = 500.0f;

	[Property]
	public float JumpForce { get; set; } = 300.0f;

	[Property]
	public float RollForce { get; set; } = 400.0f;

	[Property]
	public float RollDistance { get; set; } = 400.0f;

	[Property]
	public float BackstepForce { get; set; } = 400.0f;

	[Property]
	public Vector3 CameraOffset { get; set; } = new Vector3( -215f, 0f, 66f );

	[Property]
	public Vector3 OrbitOrigin { get; set; } = new Vector3( 0f, 0f, 54f );

	[Property]
	public Vector3 LockOnTarget { get; set; } = new Vector3( 0.0f, 0.0f, 0.0f );

	public int LockedOnIndex { get; set; }

	[Property]
	public bool LockedOn { get; set; } = false;

	[Property]
	public List<LockOnAble> LockOnAbles { get; set; }

	public Angles ForwardAngles { get; set; }

	public Angles LastMoveDirection { get; set; }

	Vector3 rollStartPoint = new Vector3(0, 0, 69420);

	Transform _initialCameraTransform;

	private bool IsRolling;

	TimeSince timeSince = 0;

	bool AttackHit = false;

	bool IsHitboxActive = false;

	bool IsAttacking = false;

	protected override void OnUpdate()
	{
		if ( Camera != null && Controller != null) 
		{
			if ( LockedOn )
			{
				Vector3 targetToPlayerDisplacement = (Transform.Position - LockOnTarget);
				Vector3 targetToCameraDisplacement = ((targetToPlayerDisplacement.Length + 215f) * targetToPlayerDisplacement.Normal);
				Camera.Transform.Position = Camera.Transform.Position.LerpTo( LockOnTarget + targetToCameraDisplacement, 0.05f);
				Camera.Transform.Position = Camera.Transform.Position.WithZ(GameObject.Transform.Position.z + CameraOffset.z * 2);
				Vector3 lockedOnTargetDisplacementNormalized = (LockOnTarget - Camera.Transform.Position).Normal;
				Angles lockedOnTargetAngles = lockedOnTargetDisplacementNormalized.EulerAngles;
				Camera.Transform.Rotation = Rotation.Lerp(Camera.Transform.Rotation, lockedOnTargetAngles, 0.05f);
				var cameraTrace = Scene.Trace.Ray( LockOnTarget, Camera.Transform.World.Position ).Size( 5f ).WithoutTags( "player" ).WithoutTags("character").Run();
				Camera.Transform.Position = cameraTrace.EndPosition;
				if ( !IsRolling && Controller.Velocity.Abs().Length <= RunSpeed / 2f )
				{
					Transform.Rotation = Rotation.Lerp( Transform.Rotation, (targetToPlayerDisplacement.Normal * -1).EulerAngles.WithPitch( 0f ).WithRoll( 0f ), 0.2f );
				} 
				else if ( Input.Down("Run"))
				{
					Transform.Rotation = Rotation.Lerp( Transform.Rotation, (Input.AnalogMove.Normal * Camera.Transform.Rotation).EulerAngles.WithPitch( 0f ).WithRoll( 0f ), 0.2f );
				}
				ForwardAngles = Camera.Transform.Rotation;
				//Camera.Transform.Position = cameraTrace.EndPosition;
			}
			else
			{
				ForwardAngles += Input.AnalogLook;
				ForwardAngles = ForwardAngles.WithPitch( MathX.Clamp( ForwardAngles.pitch, -30.0f, 60.0f ) );
				_initialCameraTransform.Position = (Transform.Position + CameraOffset);
				Camera.Transform.World = _initialCameraTransform.RotateAround( Transform.Position + OrbitOrigin, ForwardAngles );
				var cameraTrace = Scene.Trace.Ray( Transform.Position + OrbitOrigin, Camera.Transform.World.Position ).Size( 5f ).WithoutTags( "player" ).Run();
				Camera.Transform.Position = cameraTrace.EndPosition;
				Transform.Rotation = Rotation.Lerp( Transform.Rotation, Rotation.FromYaw( LastMoveDirection.yaw ), 0.2f );
			}
			
		}
	}

	protected override void OnFixedUpdate()
	{
		

		base.OnFixedUpdate();

		String list = String.Join( ",", LockOnAbles );

		Log.Info(list);

		//Log.Info( LockOnTarget.ToScreen().x + ", " + LockOnTarget.ToScreen().y );


		if ( Controller == null ) return;
		if (Camera == null ) return;

		if ( Input.Pressed( "Lock_On" ) )
		{
			if ( LockedOn )
			{
				LockedOn = !LockedOn;
			}
			else
			{
				if ( LockOnAbles.Count > 0 )
				{
					if ( LockedOnIndex > LockOnAbles.Count - 1 || LockedOnIndex < 0) LockedOnIndex = 0;
					Log.Info( LockedOnIndex );
					LockOnTarget = LockOnAbles[LockedOnIndex].LockOnNode.Transform.Position;
					LockedOn = !LockedOn;
				}
			}

		}

		if ( LockedOn && Input.MouseWheel.y > 0 )
		{
			if ( LockOnAbles.Count == LockedOnIndex + 1 )
			{
				LockedOnIndex = 0;
			}
			else
			{
				LockedOnIndex++;
			}
			Log.Info( LockedOnIndex );
			LockOnTarget = LockOnAbles[LockedOnIndex].LockOnNode.Transform.Position;
		}
		else if ( LockedOn && Input.MouseWheel.y < 0 )
		{
			if ( LockedOnIndex == 0 )
			{
				LockedOnIndex = LockOnAbles.Count - 1;
			}
			else
			{
				LockedOnIndex--;
			}
			Log.Info( LockedOnIndex );
			LockOnTarget = LockOnAbles[LockedOnIndex].LockOnNode.Transform.Position;
		}

		float targetSpeed = Input.Down( "Run" ) && !Input.Down("Guard") ? RunSpeed : WalkSpeed;

		var targetVelocity = Input.AnalogMove.Normal * targetSpeed * Camera.Transform.Rotation;

		//Log.Info(swordOffset );

		float swordOffset = GameObject.Transform.World.ToLocal( SwordTip.Transform.World ).Position.x;
		if ( swordOffset > 0 )
		{
			var animationCollisionTrace = Scene.Trace.Ray( GameObject.Transform.World.Position.WithZ( GameObject.Transform.World.Position.z + 30 ), GameObject.Transform.World.Position.WithZ( GameObject.Transform.World.Position.z + 30 ) + (Transform.Rotation.Forward * 100f) ).Size( 25f ).WithoutTags( "player" ).WithTag( "Character" ).Run();
			if ( animationCollisionTrace.Distance < swordOffset ) Transform.Position += ( -animationCollisionTrace.Direction * MathF.Pow((swordOffset - animationCollisionTrace.Distance) / 32f, 3));
		}

		if ( IsAttacking ) {
			
			//GameObject.Components.Get<BoxCollider>().Enabled = true;
			if (IsHitboxActive)
			{
				if ( Sword.Components.TryGet<Collider>( out Collider collider ) )
				{
					foreach ( var item in collider.Touching )
					{
						Log.Info( item );
						if ( item.Components.TryGet<UnitInfo>( out UnitInfo info ) )
						{
							if ( !AttackHit )
							{
								info.Damage( new Random().Next( 12, 30 ), Sword, GameObject);
								AttackHit = true;
							}
						}
					}
				}
			}
			return;
		};

		if ( IsRolling && timeSince > 0.5)
		{
			IsRolling = false;
			AnimationHelper.SpecialMove = CitizenAnimationHelper.SpecialMoveStyle.None;
			//rollStartPoint = new Vector3( 0, 0, 69420 );

		}

		//Log.Info( AnimationHelper.Tags.Has( "Idle (not moving)" ) );

		//Log.Info( AnimationHelper.Tags.Has( "Walking" ) );

		//Log.Info( AnimationHelper.Tags.Has("Attacking") );


		if ( IsRolling )
		{
			Controller.Punch( RollForce * Transform.Rotation.Forward );
			Transform.Rotation = Rotation.Lerp( Transform.Rotation, LastMoveDirection.WithPitch( 0f ).WithRoll( 0f ), 0.8f );
			targetVelocity = Vector3.Zero;
		}

		if ( Controller.IsOnGround && !IsRolling)
		{
			
			
			if ( targetVelocity.Length > 0 ) LastMoveDirection = targetVelocity.EulerAngles;
			Controller.Accelerate( targetVelocity );

			Controller.Acceleration = 10.0f;
			Controller.ApplyFriction( 5.0f );

			if ( Input.Pressed( "Jump" ))
			{
				if ( Controller.Velocity.Abs().Length >= RunSpeed / 2f && !IsRolling )
				{
						Controller.Punch( JumpForce * Vector3.Up );
						if ( AnimationHelper != null ) AnimationHelper.TriggerJump();
				}
				else if ( Input.AnalogMove.Normal.Length > 0 )
				{

					if (!IsRolling) 
					{
						AnimationHelper.SpecialMove = CitizenAnimationHelper.SpecialMoveStyle.Roll;
						IsRolling = true;
						timeSince = 0;
					}
				}
				else
				{
					Controller.Punch( BackstepForce * Transform.Rotation.Backward );
				}
			}

			//Log.Info( Input.MouseWheel );

			if ( Input.Pressed( "Light_Attack" ) )
			{
				Log.Info( "Light Attack" );
				AnimationHelper.Target.Set( "b_light_attack", true);
			}

			if ( Input.Pressed( "Heavy_Attack" ) )
			{
				Log.Info( "Heavy Attack" );
			}	

			if ( Input.Down("Guard") && !IsRolling )
			{
				Log.Info( AnimationHelper.DuckLevel );
				AnimationHelper.IkLeftHand = ShieldIKTarget;
				AnimationHelper.DuckLevel = 0.3f;
				WalkSpeed = 100f;
			} else
			{
				AnimationHelper.IkLeftHand = null;
				AnimationHelper.DuckLevel = 0f;
				WalkSpeed = 150f;
			}

		} 
		else if (!IsRolling)
		{
			Controller.Acceleration = 2.0f;
			Controller.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
		}

		Controller.Move();

		if ( AnimationHelper != null )
		{
			AnimationHelper.IsGrounded = Controller.IsOnGround;
			AnimationHelper.WithVelocity( Controller.Velocity );
			AnimationHelper.WithLook( targetVelocity );
		}
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( Camera != null ) _initialCameraTransform = Camera.Transform.World;

		ClothingContainer clothing = ClothingContainer.CreateFromLocalUser();
		clothing.Apply( SkinnedModelRenderer );

		AnimationHelper.Target.OnGenericEvent = ( SceneModel.GenericEvent genericEvent ) =>
		{
			switch( genericEvent.String )
			{
				case "attack_start":
					IsAttacking = true;
					break;
				case "attack_end":
					IsAttacking = false;
					AttackHit = false;
					break;
				case "hitbox_active":
					IsHitboxActive = true;
					break;
				case "hitbox_inactive":
					IsHitboxActive = false;
					break;
			}
		};
	}

	protected override void DrawGizmos()
	{
		Gizmo.Draw.LineSphere( OrbitOrigin, 10f );
	}
}
