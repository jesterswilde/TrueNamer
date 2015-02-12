using UnityEngine;
using System.Collections;

public class Mo_InAir : Motion {

	float _targetWeight; 

	void HeavyGravity(){ //increses gravity
		if(_rigid.velocity.normalized.y < -.1f || _rigid.velocity.normalized.y > .1f ){
			_rigid.AddForce (Physics.gravity*1f, ForceMode.Acceleration); 
		}
	}
	void Forward(){ //lets them move forward while they are going forward
		Vector3 xyVelocity = new Vector3 (_rigid.velocity.x, 0, _rigid.velocity.z); 
		if (_verticalInput > 0 && _forwardD.IsGrounded() == false && _player.LastState == "ground") {
			_rigid.AddRelativeForce(new Vector3(0,0,1)*10,ForceMode.Acceleration); 	
		}
	}
	public override void ControlsInput (){
		base.ControlsInput ();
		HeavyGravity (); 
		Forward (); 
	}

	public override void ControlsEffect ()
	{
		base.ControlsEffect ();
		Forward (); 

	}
	public override void EnterState ()
	{
		_camera.Normal (); 
	}
	public override void MotionState ()
	{
		if (_groundD.IsGrounded()) {
			_player.NowGrounded(); 
		}
	}
	protected override void SetAnim ()
	{
		base.SetAnim ();
	}
	public override void Startup (PlayerController _thePlayer)
	{
		base.Startup (_thePlayer);

	}
	public override void ExitState ()
	{
		base.ExitState ();
	}
}
