using UnityEngine;
using System.Collections;

public class Mo_InAir : Motion {

	float _targetWeight; 
	Vector3 _speed; 

	void HeavyGravity(){ //increses gravity
		if(_rigid.velocity.normalized.y < -.1f || _rigid.velocity.normalized.y > .1f ){
			_rigid.AddForce (Physics.gravity*1f, ForceMode.Acceleration); 
		}
	}

	
	void MoveForward(){ //controls when they move forward. This whole script should be broken up into different states.
		_speed.z =  _verticalInput; 
	}
	void Strafe(){
		_speed.x += _horizontalInput; 
	}

	void InAirMove(){ //lets them move forward while they are going forward
		_rigid.AddRelativeForce(_speed*_player.AirSpeed ,ForceMode.Acceleration); 	
	}
	public override void ControlsInput (){
		_speed = Vector3.zero; 
		base.ControlsInput ();
		HeavyGravity (); 
		MoveForward ();
		Strafe (); 
		InAirMove (); 
	}

	public override void ControlsEffect ()
	{
		base.ControlsEffect ();
		InAirMove (); 

	}
	public override void EnterState ()
	{
		_camera.Normal (); 
	}
	public override void MotionState ()
	{
		if (_groundD.IsGrounded()) {
			_player.EnterState(_player.GroundedMo); 
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
