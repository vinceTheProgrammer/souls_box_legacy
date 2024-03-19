using Sandbox;

public sealed class Shield : Component
{
	[Property]
	public GameObject Character { get; set; }

	[Property]
	public Vector3 PositionOffset { get; set; }

	[Property]
	public Rotation RotationOffset { get; set; }

	[Property]
	public int BoneId { get; set; }

	private GameObject Bone { get; set; }

	private SkinnedModelRenderer CharModel;

	protected override void OnUpdate()
	{
		base.OnFixedUpdate();

		if (CharModel != null )
		{
			Bone = CharModel.GetBoneObject( BoneId );
		}
		

		if ( Bone != null )
		{
			GameObject.Transform.Position = Bone.Transform.World.Position;
			GameObject.Transform.Rotation = Bone.Transform.World.Rotation;
			Vector3 Pos = GameObject.Transform.LocalPosition;
			Rotation Rot = GameObject.Transform.LocalRotation; 
			GameObject.Transform.LocalPosition = Pos.WithX(Pos.x + PositionOffset.x).WithY(Pos.y + PositionOffset.y).WithZ(Pos.y + PositionOffset.z);
			GameObject.Transform.LocalRotation = Rot.Angles().WithPitch(RotationOffset.Pitch()).WithYaw(RotationOffset.Yaw()).WithRoll(RotationOffset.Roll());
		}
	}

	protected override void OnStart()
	{
		base.OnStart();

		CharModel = Character.Components.Get<SkinnedModelRenderer>();
		CharModel.CreateBoneObjects = true;

		Bone = CharModel.GetBoneObject( BoneId );

		GameObject.Transform.Position = Bone.Transform.World.Position;
		GameObject.Transform.Rotation = Bone.Transform.World.Rotation;

		GameObject.SetParent( Bone, false );
	}
}
