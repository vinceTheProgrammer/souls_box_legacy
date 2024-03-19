using Sandbox;

public sealed class LockOnAble : Component
{

	[Property]
	public GameObject Player { get; set; }

	[Property]
	public GameObject LockOnNode { get; set; }


	private int MyIndex;

	protected override void OnUpdate()
	{

	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		if ( Player.Components.Get<Player>().LockOnAbles.Count != 0 && Player.Components.Get<Player>().LockedOnIndex <= Player.Components.Get<Player>().LockOnAbles.Count - 1)
			Player.Components.Get<Player>().LockOnTarget = Player.Components.Get<Player>().LockOnAbles[Player.Components.Get<Player>().LockedOnIndex].LockOnNode.Transform.Position;
		//Log.Info( GameObject.Transform.Position.Distance( Player.Transform.Position ) );
		if (GameObject.Transform.Position.Distance(Player.Transform.Position) < 500f && GameObject.Components.Get<UnitInfo>().IsAlive)
		{
			//Log.Info( "Yes" );
			//Log.Info(Player.Components.Get<Player>().LockOnAbles.Contains( this ));
			if ( Player.Components.Get<Player>().LockOnAbles.Contains( this ) ) return;
			int length = Player.Components.Get<Player>().LockOnAbles.Count;
			//Log.Info( length );
			Player.Components.Get<Player>().LockOnAbles.Add( this );
			MyIndex = length;
			if ( Player.Components.Get<Player>().LockOnAbles.Count != 0 && Player.Components.Get<Player>().LockedOnIndex <= Player.Components.Get<Player>().LockOnAbles.Count - 1 )
				Player.Components.Get<Player>().LockOnTarget = Player.Components.Get<Player>().LockOnAbles[Player.Components.Get<Player>().LockedOnIndex].LockOnNode.Transform.Position;
		}
		else
		{
			if ( !Player.Components.Get<Player>().LockOnAbles.Contains( this ) ) return;
			bool staleLock = false;
			if ( Player.Components.Get<Player>().LockedOnIndex == Player.Components.Get<Player>().LockOnAbles.IndexOf(this)) staleLock = true;
			Player.Components.Get<Player>().LockOnAbles.Remove( this );
			if (staleLock)
			{
				if ( Player.Components.Get<Player>().LockOnAbles.Count == 0 )
				{
					Player.Components.Get<Player>().LockedOn = false;
					return;
				}
				LockOnAble lockOnAble = Player.Components.Get<Player>().LockOnAbles.FirstOrDefault<LockOnAble>();
				Player.Components.Get<Player>().LockedOnIndex = Player.Components.Get<Player>().LockOnAbles.IndexOf( lockOnAble );
				Player.Components.Get<Player>().LockOnTarget = Player.Components.Get<Player>().LockOnAbles[Player.Components.Get<Player>().LockedOnIndex].LockOnNode.Transform.Position;
			}
			MyIndex = -1;
		}
	}
}
