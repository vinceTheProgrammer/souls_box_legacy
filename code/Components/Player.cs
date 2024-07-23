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

	[Property]
	public int FatigueAmount { get; set; }

	[Property]
	public int LightAttackFatigueAmount { get; set; }

	[Property]
	public int RollFatigueAmount { get; set; }

	[Property]
	public int JumpFatigueAmount { get; set; }

	[Property]
	public float FatiguePeriod { get; set; }

	TimeSince lastFatigue;

	public Angles ForwardAngles { get; set; }

	public Angles LastMoveDirection { get; set; }

	Vector3 rollStartPoint = new Vector3(0, 0, 69420);

	Transform _initialCameraTransform;

	public bool IsRolling = false;

	public bool IsRunning = false;

	public bool isGuarding = false;

	TimeSince timeSince = 0;

	bool AttackHit = false;

	bool AttackTired = false;

	bool IsHitboxActive = false;

	public bool IsAttacking = false;

	public bool IsCommittedToAttack = false;

	GameObject bone;

	enum LastAttack
	{
		ss_light_1,
		ss_light_2,
	}

	LastAttack lastAttack = LastAttack.ss_light_1;

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
				Vector3 rotateAround = bone.Transform.Position.WithZ(Transform.Position.z);
				ForwardAngles += Input.AnalogLook;
				ForwardAngles = ForwardAngles.WithPitch( MathX.Clamp( ForwardAngles.pitch, -30.0f, 60.0f ) );
				_initialCameraTransform.Position = (rotateAround + CameraOffset);
				Camera.Transform.World = _initialCameraTransform.RotateAround(rotateAround, ForwardAngles );
				var cameraTrace = Scene.Trace.Ray( rotateAround, Camera.Transform.World.Position ).Size( 5f ).WithoutTags( "player" ).Run();
				//Camera.Transform.Position = cameraTrace.EndPosition;
				Transform.Rotation = Rotation.Lerp( Transform.Rotation, Rotation.FromYaw( LastMoveDirection.yaw ), 0.2f );
			}
		}
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		String list = String.Join( ",", LockOnAbles );

		//Log.Info(list);

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
					//Log.Info( LockedOnIndex );
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
			//Log.Info( LockedOnIndex );
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
			//Log.Info( LockedOnIndex );
			LockOnTarget = LockOnAbles[LockedOnIndex].LockOnNode.Transform.Position;
		}

		float targetSpeed = Input.Down( "Run" ) && !Input.Down("Guard") ? RunSpeed : WalkSpeed;

		//Log.Info( targetSpeed + "   :   " + RunSpeed );

		if (targetSpeed == RunSpeed)
		{
			IsRunning = true;
			if (lastFatigue > FatiguePeriod)
			{
				//Log.Info( "Draining..." );
				Components.Get<UnitInfo>().Tire( FatigueAmount );
				lastFatigue = 0;
			}
		} else
		{
			IsRunning = false;
		}

		if ( Components.Get<UnitInfo>().Stamina == 0 ) targetSpeed = WalkSpeed;

		var targetVelocity = Input.AnalogMove.Normal * targetSpeed * Camera.Transform.Rotation;

		//Log.Info(swordOffset );

		float swordOffset = GameObject.Transform.World.ToLocal( SwordTip.Transform.World ).Position.x;
		if ( swordOffset > 0 )
		{
			var animationCollisionTrace = Scene.Trace.Ray( GameObject.Transform.World.Position.WithZ( GameObject.Transform.World.Position.z + 30 ), GameObject.Transform.World.Position.WithZ( GameObject.Transform.World.Position.z + 30 ) + (Transform.Rotation.Forward * 100f) ).Size( 25f ).WithoutTags( "player" ).WithTag( "Character" ).Run();
			if ( animationCollisionTrace.Distance < swordOffset ) Transform.Position += ( -animationCollisionTrace.Direction * MathF.Pow((swordOffset - animationCollisionTrace.Distance) / 32f, 3));
		}


		if ( Input.Pressed( "Light_Attack" ) && Components.Get<UnitInfo>().Stamina >= LightAttackFatigueAmount )
		{

			if ( lastAttack == LastAttack.ss_light_1 && IsCommittedToAttack == false && IsAttacking == true )
			{
				AnimationHelper.Target.Set( "b_light_attack_2", true );
				lastAttack = LastAttack.ss_light_2;
			} else if (IsAttacking == true && IsCommittedToAttack == false && lastAttack == LastAttack.ss_light_2)
			{	
				AnimationHelper.Target.Set( "b_light_attack", true );
				lastAttack = LastAttack.ss_light_1;
			}
			else if (!IsAttacking)
			{
				AnimationHelper.Target.Set( "b_light_attack", true );
				lastAttack = LastAttack.ss_light_1;
			}
		}

		if ( IsAttacking ) {

			//Vector3 animRootPosition = SkinnedModelRenderer.RootMotion.Position;

			Vector3 bonePos = SkinnedModelRenderer.GetBoneObject( 0 ).Transform.LocalPosition;

			Log.Info(bonePos);

			//Transform.Position = animRootPosition;



			if ( !AttackTired )
			{
				Components.Get<UnitInfo>().Tire( LightAttackFatigueAmount );
				AttackTired = true;
			}
			if (IsHitboxActive)
			{

				if ( Sword.Components.TryGet<Collider>( out Collider collider ) )
				{
					foreach ( var item in collider.Touching )
					{
						//Log.Info( item );
						if ( item.Components.TryGet<UnitInfo>( out UnitInfo info ) )
						{
							if ( !AttackHit )
							{
								info.Damage( new Random().Next( 20, 42 ), Sword, GameObject);
								AttackHit = true;
							}
						}
					}
				}
			}
			return;
		};

		AttackTired = false;

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
						Components.Get<UnitInfo>().Tire( JumpFatigueAmount );
				}
				else if ( Input.AnalogMove.Normal.Length > 0 )
				{

					if (!IsRolling) 
					{
						AnimationHelper.SpecialMove = CitizenAnimationHelper.SpecialMoveStyle.Roll;
						IsRolling = true;
						Components.Get<UnitInfo>().Tire( RollFatigueAmount );
						timeSince = 0;
					}
				}
				else
				{
					Controller.Punch( BackstepForce * Transform.Rotation.Backward );
				}
			}

			//Log.Info( Input.MouseWheel );

			if ( Input.Pressed( "Heavy_Attack" ) )
			{
				//Log.Info( "Heavy Attack" );
			}	

			if ( Input.Down("Guard") && !IsRolling )
			{
				isGuarding = true;
				AnimationHelper.IkLeftHand = ShieldIKTarget;
				AnimationHelper.DuckLevel = 0.3f;
				WalkSpeed = 100f;
			} else
			{
				isGuarding = false;
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
					IsCommittedToAttack = true;	
					break;
				case "attack_end":
					IsAttacking = false;
					AttackHit = false;
					Vector3 bonePos = bone.Transform.LocalPosition;
					Vector3 forward = (Transform.Position + Transform.Rotation.Forward * bonePos.x).WithZ( Transform.Position.z );
					Transform.Position = forward;
					break;
				case "hitbox_active":
					IsHitboxActive = true;
					break;
				case "hitbox_inactive":
					IsHitboxActive = false;
					break;
				case "attack_over":
					IsCommittedToAttack = false;
					break;
			}
		};

		SkinnedModelRenderer.CreateBoneObjects = true;
		bone = SkinnedModelRenderer.GetBoneObject( 0 );
	}

	protected override void DrawGizmos()
	{
		Gizmo.Draw.LineSphere( OrbitOrigin, 10f );
	}
}
