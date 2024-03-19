using Sandbox;

public sealed class StupidLadder : Component
{

	[Property]
	public GameObject Player { get; set; }

	protected override void OnFixedUpdate()
	{
		if (Player != null)
		{
			//Log.Info( Transform.Position.Distance( Player.Transform.Position ) );
			if (Transform.Position.Distance(Player.Transform.Position) < 50f)
			{
				//Log.Info( "Yes" );
				Player.Components.Get<Player>().Controller.Punch( Vector3.Up * 180f );
				Player.Components.Get<Player>().Controller.Punch( Vector3.Forward * 10f );
			}
		}
	}
}
