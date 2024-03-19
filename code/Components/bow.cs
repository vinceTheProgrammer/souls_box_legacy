using Sandbox;

public sealed class Bow : Component
{
	[Property]
	public GameObject Character {  get; set; }

	[Property]
	public int boneID { get; set; }

	private GameObject Bone { get; set; }

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		SkinnedModelRenderer CharModel = Character.Components.Get<SkinnedModelRenderer>();
		CharModel.CreateBoneObjects = true;

		Bone = CharModel.GetBoneObject( boneID );
		if (Bone != null)
		{
			GameObject.Transform.World = Bone.Transform.World;
			GameObject.Transform.Rotation = GameObject.Transform.Rotation.Angles().WithYaw( Transform.Local.RotationToWorld( Rotation.FromYaw(235) ).Yaw()).WithPitch( Transform.Local.RotationToWorld( Rotation.FromPitch( -45 ) ).Pitch() ).WithRoll( Transform.Local.RotationToWorld( Rotation.FromRoll( -110 ) ).Roll() );
			GameObject.Transform.Scale = 0.3f;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		
	}
}
