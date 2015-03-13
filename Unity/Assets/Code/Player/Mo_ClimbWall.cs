using UnityEngine;
using System.Collections;

public class Mo_ClimbWall : Motion {

	Vector3 _up; 
	Vector3 _right; 
	Vector3 _out; 

	Vector3 _speed; 
	bool _holdingSpace = false; 


	void PressingSpacebar(){
		if (Input.GetKeyDown (KeyCode.Space)) {
			_holdingSpace = true; 	
		}
		if (Input.GetKeyUp (KeyCode.Space)) {
			_holdingSpace = false;
			Jump(); 
		}

	}
	void CalcSpeed(){
		_speed = Vector3.zero; 
		_speed.z = _verticalInput; 
		_speed.x = _horizontalInput; 
	}
	void ApplySpeed(){
		if(!_holdingSpace){
			Vector3 _horizontalSpeed = _speed.x * _right;
			Vector3 _verticalSpeed = _speed.z * _up; 
			_player.Rigid.velocity = (_horizontalSpeed + _verticalSpeed).normalized * _player.ClimbSpeed; 
		}
		else{
			_player.Rigid.velocity = Vector3.zero; 
		}
	}
	void Jump(){
		if(_speed == Vector3.zero){
			_player.CurrentGroundD.TurnOff(.05f); //temporarily disable the 
			_player.Rigid.velocity = _out * _player.JumpPower; 
			_player.EnterState (_player.InAirMo);
		}
		else{
			Vector3 _horizontalSpeed = _speed.x * _right;
			Vector3 _verticalSpeed = _speed.z * _up; 
			_player.CurrentGroundD.TurnOff(.5f);
			_player.Rigid.velocity = (_horizontalSpeed + _verticalSpeed).normalized * _player.JumpPower; 
			_player.EnterState (_player.InAirMo);
		}
	}


	void GetAxis(){//Calculates the directions the player will move in with up down left and right
		Vector3 _upPoint = _player.transform.position + new Vector3 (0, .1f, 0); 
		Ray _ray = new Ray (_upPoint, (_player.CurrentGroundD.transform.position - _player.transform.position)); 
		RaycastHit _hit;  //raycast from slightly above the player in the exact same direction as the prior raycast (Done in the player) to figure out 
		int _mask = ~ (1 << 8);  //if the axis the player should move in. 
		if (Physics.Raycast (_ray, out _hit, 5, _mask)) {
			Thing _otherThing = _hit.collider.gameObject.GetComponent<Thing> (); 
			if (_otherThing != null) {
				_up = (_hit.point - _player.GroundHit.point).normalized; //we get up from the direction between the first raycast, and the second (which is slightly higher
				_right = (Quaternion.AngleAxis(90,_player.GroundHit.normal) *_up).normalized; //we get right from rotating _up arond the normal of the collision. It works, the internet assures me
				_out = _player.GroundHit.normal; 
			}
		}
		else{
			_player.EnterState(_player.InAirMo); 
		}
	}
	public override void MotionState ()
	{
		base.MotionState ();
		if(_player.CurrentGroundD != null){
			if (!_player.CurrentGroundD.IsGrounded()) {
				_player.EnterState(_player.InAirMo); 	
			}
		}
		else{
			_player.EnterState(_player.InAirMo); 
		}
	}
	public override void ControlsInput ()
	{
		base.ControlsInput ();
		PressingSpacebar (); 
	}
	public override void ControlsEffect ()
	{
		base.ControlsEffect ();
		CalcSpeed (); 
		ApplySpeed (); 
	}

	public override void EnterState ()
	{
		_holdingSpace = false;
		_player.Rigid.useGravity = false;
		GetAxis (); 
		base.EnterState ();
	}
	public override void ExitState ()
	{
		base.ExitState ();
		_player.Rigid.useGravity = true; 
	}
}
