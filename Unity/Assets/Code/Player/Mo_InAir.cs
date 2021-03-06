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
		LookTowardsVelocity (); 
		base.ControlsInput ();
		//GroundTimer (); 
	}

	public override void ControlsEffect ()
	{
		base.ControlsEffect ();
		_speed = Vector3.zero;
		//HeavyGravity (); 
		MoveForward ();
		Strafe (); 
		InAirMove (); 

	}
	public override void EnterState ()
	{
		_player.ClearStandingOn (); 
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
	public override void ExitState ()
	{
		base.ExitState ();
	}
}
