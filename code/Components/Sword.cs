using Sandbox;

public sealed class Sword : Component
{
	[Property]
	public GameObject Character {  get; set; }

	private GameObject Bone { get; set; }

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		if (Bone != null)
		{
			GameObject.Transform.World = Bone.Transform.World;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		SkinnedModelRenderer CharModel = Character.Components.Get<SkinnedModelRenderer>();
		CharModel.CreateBoneObjects = true;

		//GameObject.SetParent( Character, false );
		Bone = CharModel.GetBoneObject( 15 );
	}
}
