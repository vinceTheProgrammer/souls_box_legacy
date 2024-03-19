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

	public int Souls { get; private set; }

	public bool IsAlive { get; private set; } = true;

	private float lastYaw;

	protected override void OnUpdate()
	{
		if ( !IsAlive ) Transform.LocalRotation = Transform.LocalRotation.Angles().WithRoll( 90 ).WithYaw( lastYaw );
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


		Health = Math.Clamp(Health - damage, 0, MaxHealth);

		if (GameObject.Components.TryGet<CitizenAnimationHelper>( out CitizenAnimationHelper helper ))
		{
			var damageTrace = Scene.Trace.Ray( weapon.Transform.Position, Transform.Position.WithZ(Transform.Position.z + 30) ).Size( 1f ).WithTag( "character" ).UseHitboxes().Run();
			DamageInfo info = new DamageInfo( (float)damage, attacker, weapon, damageTrace.Hitbox);
			helper.ProceduralHitReaction( info, 5f );
			Log.Info(helper);
			
		}

		if ( Health <= 0 ) Kill();
	}

	public void Kill()
	{
		IsAlive = false;
		lastYaw = Transform.Rotation.Angles().yaw;
	}
}
