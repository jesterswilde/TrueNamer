using UnityEngine;
using System.Collections;

public class Mo_Pulling : Motion {


	//This is for when the player is dragging an object. It's mainly a slow version of Mo_Ground, with an object to worry about. 
	
	float _startinMass; 



	void GrabObject(){
		_player.PulledThing = _player.SelectedThing; 
		_startinMass = _rigid.mass; 
		_rigid.mass = _player.PulledThing.Mass; 
	}
	public void MoveToObject(){
		Ray _ray = new Ray (_player.ThingRayHit.point, (_player.transform.position - _player.ThingRayHit.point)); 
		RaycastHit _hit;
		int _mask = (1 << 9); 
		Vector3 _frontOfPlayer = new Vector3(); 
		if(Physics.Raycast(_ray,out _hit, _mask)){
			_frontOfPlayer = _hit.point; 
		}
		else _player.EnterState(_player.GroundedMo); //if the above errors out, leave the pulling stae. 

		Vector3 _moveDir = _player.ThingRayHit.point - _frontOfPlayer; 
		Vector3 _endPos = _player.transform.position + _moveDir; 
		_player.transform.position = new Vector3 (_endPos.x, _player.transform.position.y, _endPos.z); 
		_player.Rigid.WakeUp (); 
	}
	void ReleaseObject(){
		_player.PulledThing = null;
		_rigid.mass = _startinMass; 
	}





	Vector3 _speed; 
	
	void MoveForward(){ //controls when they move forward. This whole script should be broken up into different states.
		if (_verticalInput > 0){
			_speed.z += 5; 
			//_rigid.AddRelativeForce (new Vector3(0,0,1) * _verticalInput * _acceleration,ForceMode.Acceleration); 
		}
	}
	void MoveBackwards(){ //simlar to forward, this is different because I may want to clamp and have different anims and such
		if(_verticalInput < 0){
			_speed.z -= 1; 
			//_rigid.AddRelativeForce (new Vector3(0,0,1) * _verticalInput * _acceleration,ForceMode.Acceleration); 
		}
	}
	void Strafe(){
		_speed.x += 3 * _horizontalInput; 
	}
	

	void CalculateSpeed(){
		if (_speed == Vector3.zero) { //if there is no input, stop them immediately
			_rigid.velocity = Vector3.zero; 
			_player.PulledThing.Rigid.velocity = Vector3.zero; 
		}
		else{
			if(_rigid.velocity.magnitude <= _player.MaxSpeed) { //speed cap
				Vector3 _pulledVelocity =  ((_player.transform.forward *_speed.z) + (_player.transform.right *_speed.x)).normalized  * 4; 
				_rigid.velocity = _player.PulledThing.Rigid.velocity = _pulledVelocity;
				
				//_rigid.AddRelativeForce (_speed.normalized * _acceleration, ForceMode.Acceleration); //moves them in their selected direction

			}
		}
	}
	
	public override void ControlsEffect ()
	{
		base.ControlsEffect ();
		_speed = Vector3.zero; 
		MoveForward (); 
		MoveBackwards ();
		Strafe (); 
		CalculateSpeed (); 
	}
	public override void ControlsInput ()
	{
		base.ControlsInput ();
	}

	public override void MotionState ()
	{
		base.MotionState ();
		if (_groundD.IsGrounded() == false) {
			_player.EnterState(_player.InAirMo); 
		}
		if (Input.GetKeyDown (KeyCode.E)) {
			_player.EnterState(_player.GroundedMo); 
		}
	}
	public override void EnterState ()
	{
		base.EnterState ();
		MoveToObject ();
		GrabObject (); 
	}
	public override void ExitState ()
	{
		base.ExitState ();
		ReleaseObject (); 
	}

}
