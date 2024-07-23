using Sandbox;
using Sandbox.Citizen;
using System;

public enum UnitType
{
	None,
	Player,
	Enemy,
	Boss
}

public sealed class UnitInfo : Component
{

	[Property]
	public int MaxHealth { get; set; }

	[Property]
	public int Health { get; private set; }

	[Property]
	public int MaxStamina { get; set; }

	[Property]
	public int Stamina { get; private set; }

	[Property]
	public UnitType Type { get; set; } = UnitType.None;

	[Property]
	public int recoveryAmount { get; set; }

	[Property]
	public float recoveryPeriod { get; set; }

	TimeSince lastRecovered;

	public int Souls { get; private set; }

	public bool IsAlive { get; private set; } = true;

	private float lastYaw;

	public TimeSince timeSinceHealthLag;
	public TimeSince timeSinceStaminaLag;

	protected override void OnUpdate()
	{
		if ( !IsAlive ) Transform.LocalRotation = Transform.LocalRotation.Angles().WithRoll( 90 ).WithYaw( lastYaw );
	}

	protected override void OnFixedUpdate()
	{
		if (Type == UnitType.Player)
		{
			if (Components.Get<Player>() != null)
			{

				//Log.Info( "Running: " + Components.Get<Player>().IsRunning );
				//Log.Info( "Rolling: " + Components.Get<Player>().IsRolling );
				//Log.Info( "Attacking: " + Components.Get<Player>().IsAttacking );

				if ( Components.Get<Player>().IsRunning || Components.Get<Player>().IsRolling || Components.Get<Player>().IsAttacking ) return;
			}
		}


		if (lastRecovered > recoveryPeriod )
		{
			Stamina = Math.Clamp( Stamina + recoveryAmount, 0, MaxStamina );
			lastRecovered = 0;
		}
		
	}

	protected override void OnStart()
	{
		Health = MaxHealth;
		Stamina = MaxStamina;
		Souls = 0;
	}

	public void Damage(int damage, GameObject weapon, GameObject attacker)
	{
		if ( !IsAlive ) return;

		if (Type == UnitType.Player)
		{
			if (Components.Get<Player>().isGuarding)
			{
				return;
			}
		}


		Health = Math.Clamp(Health - damage, 0, MaxHealth);

		timeSinceHealthLag = 0;

		if (GameObject.Components.TryGet<CitizenAnimationHelper>( out CitizenAnimationHelper helper ))
		{
			var damageTrace = Scene.Trace.Ray( weapon.Transform.Position, Transform.Position.WithZ(Transform.Position.z + 30) ).Size( 1f ).WithTag( "character" ).UseHitboxes().Run();
			DamageInfo info = new DamageInfo( (float)damage, attacker, weapon, damageTrace.Hitbox);
			helper.ProceduralHitReaction( info, 5f, -damageTrace.Direction * 100f);
			//Log.Info(helper);
			
		}

		if (Type == UnitType.Enemy )
		{
			Vector3 directionToAttacker = (attacker.Transform.Position - Transform.Position) / Transform.Position.Length;
			Components.Get<SoundPointComponent>().StartSound();
			Components.Get<CharacterController>().Punch(-directionToAttacker * 1000f);
		}

		if ( Health <= 0 ) Kill();
	}

	public void Tire(int fatigue)
	{
		if ( !IsAlive ) return;


		Stamina = Math.Clamp( Stamina - fatigue, 0, MaxStamina );

		timeSinceStaminaLag = 0;
	}

	public void Kill()
	{
		IsAlive = false;
		lastYaw = Transform.Rotation.Angles().yaw;
	}
}
