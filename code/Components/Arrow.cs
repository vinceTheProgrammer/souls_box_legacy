using Sandbox;
using System;

public sealed class Arrow : Component
{
	[Property]
	float TimeToLive { get; set; }

	public GameObject owner {  get; set; }

	public Vector3 initialVelocity { get; set; } 

	TimeSince aliveTime;

	protected override void OnFixedUpdate()	
	{
		//Log.Info( aliveTime );
		if ( aliveTime > TimeToLive ) GameObject.Destroy();

		if (Components.Get<Rigidbody>().Velocity.Length < initialVelocity.Length) GameObject.Destroy();

		if ( Components.TryGet( out Collider collider ) )
		{
			//Log.Info( "Got collider" );
			foreach ( var item in collider.Touching )
			{
				Log.Info( "Touching" );
				Log.Info( item );
				if ( item.Components.TryGet<UnitInfo>( out UnitInfo info ) )
				{

					info.Damage( new Random().Next( 100, 250 ), GameObject, owner );
				}
				GameObject.Destroy();
			}
		}
	}

	protected override void OnStart()
	{
		aliveTime = 0;
		if ( Components.TryGet( out Collider collider ) )
		{
			collider.OnTriggerEnter = ( collider ) =>
			{
				Log.Info( "Hi :)" );
			};
		}
	}
}
