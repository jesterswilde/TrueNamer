using UnityEngine;
using System.Collections;

public class Mo_Ground : Motion {

	//This is the primary movement script. it deals with unburdened walking / moving. 

	Vector3 _speed; 
	bool _canJump; 

	float _time; 
	float _jumpTime; 

	void MoveForward(){ //controls when they move forward. 
		if (!_forwardD.IsGrounded()){ //no forward movement when they are in front of walls, it messes with jumping 
			_speed.z = _verticalInput; 
		}
	}
	void Strafe(){
		_speed.x = _horizontalInput; 
	}

	void Jump(){ //can only jump when on the ground. When space is pushed they go up
		if (_jumpInput > 0.5f && _verticalInput > 0 &&  _canJump && !_forwardD.IsGrounded()) {
			//they are moving forward and jumping
			_rigid.AddForce(new Vector3(0,1,0)*_jumpPower,ForceMode.Impulse);
			StartJumpDelay(); 
			return;
		}
		if (_jumpInput > 0.5f && _canJump) {
			_rigid.AddForce(new Vector3(0,1,0)*_jumpPower,ForceMode.Impulse);
			StartJumpDelay(); 
		}
	} 
	void CalculateSpeed(){
		float _modMaxSpeed = _maxSpeed;
		float _modAcceleration = _acceleration; 
		if(_player.StandingOnMadeOf != null){ //this is getting whether the player is standing on someting rough
			_modMaxSpeed = _maxSpeed - _player.StandingOnMadeOf.Rough; 
			_modAcceleration = Mathf.Max(_acceleration - (_player.StandingOnMadeOf.Rough),3);
		}
		if (_speed == Vector3.zero && _canJump) { //if there is no input, stop them immediately
			_rigid.velocity = Vector3.zero;  	
		}
		else{ 
			if(_canJump){
				LookTowardsCamera(); //this is the player is holding a direction and not jumping
				Vector3 _unprojectedForward = (_player.transform.forward * _speed.z + _player.transform.right * _speed.x).normalized; 
				Vector3 _projectedForward = Vector3.ProjectOnPlane(_unprojectedForward, _player.GroundHit.normal).normalized; 
				if(_projectedForward.y <= 0 || Vector3.Angle(_unprojectedForward, _projectedForward)*2 < _player.SlopeAngle){ //if the surface they are standing on is not sloped upward above teh slope angle
					float _mag = Mathf.Clamp(_rigid.velocity.magnitude,0,_modMaxSpeed); 
					//_rigid.velocity = _projectedForward * (Mathf.Max (_mag,3) + _modAcceleration * Time.fixedDeltaTime );
					_rigid.velocity = _projectedForward * (Mathf.Max (_mag,3) + _modAcceleration * Time.fixedDeltaTime );
				}
				else{
					_rigid.velocity = Vector3.zero; 
				}
			}
		}
	}
	

	void StartJumpDelay(){ //these 2 are about making it so the player can't jump repeatedly by holding the button down and getting a fuckton of movement.
		_canJump = false; //I may want to consider nixing this and 'setting' the verticle component of a jump, or even movement as a whole. 
		_jumpTime = .1f;
		_time = 0;
	}
	void JumpTimer(){
		if (!_canJump) {
			_time += Time.fixedDeltaTime;
			if(_time >= _jumpTime){
				_canJump = true; 
			}
		}
	}
	void FloorCheck(){ //in case we are in this state and there is no ground where there should be. 
		if (_player.StandingOnMadeOf == null) {
			_player.GetTouchedSurface(); 
		}
	}

	void GrabObject(){ //The player can only grab objects when grounded, Though may need to move this section to Motion when slick is fully implemented
		if(_player.HeldThing == null){
			if (_player.SelectedThing != null && _canJump) {
				if(Input.GetKeyDown(KeyCode.E)){ 
					//cast another ray from the player to the object
					Ray _ray = new Ray(_player.transform.position,(_player.ThingRayHit.collider.gameObject.transform.position - _player.transform.position).normalized); 
					RaycastHit _hit;
					int _mask = ~ (1 << 8); //ignore the player layer
					float _dist = 10000; 
					if(Physics.Raycast(_ray, out _hit,10,_mask)){
						_dist = Vector3.Distance(_player.transform.position, _hit.point); 
					}
					if(_dist < 2.2f && _hit.collider.gameObject.GetComponent<Rigidbody>().mass < _player.MaxHold){ //if the player is close enough to pick up the thing
						_player.ThingRayHit = _hit; 
						_player.PickUpThing(); 
						return; 
					}
					if(_dist < 2.2f && _hit.collider.gameObject.GetComponent<Rigidbody>().mass < _player.MaxPull){
						_player.ThingRayHit = _hit; 
						_player.EnterState (_player.PullingMo); ; 
						return; 
					}
				}
			}
		}
		else{
			if(Input.GetKeyDown(KeyCode.E)){
				Debug.Log("Holding " + _player.HeldThing); 
				_player.DropThing(); 
			}
		}
	}
	public override void ControlsEffect () //called on fixed update
	{
		base.ControlsEffect ();
		_speed = Vector3.zero; 
		MoveForward (); 
		Strafe (); 
		CalculateSpeed (); 
		Jump ();
		JumpTimer (); 
		FloorCheck ();
	}
	public override void ControlsInput () //called on Update
	{
		base.ControlsInput ();

	}
	public override void MotionState ()//Checks for conditions to leave this movement state
	{
		if (_groundD.IsGrounded() == false) {
			_player.EnterState(_player.InAirMo); 
		}
		if(_player.StandingOnMadeOf != null){
			if (_player.StandingOnMadeOf.IsSlick) {
				_player.EnterState(_player.SlickMo); 
			}
		}
		GrabObject (); 
	}
	public override void EnterState ()
	{
		StartJumpDelay (); 
		_camera.Normal (); 
		MotionState (); 
	}
	public override void ExitState ()
	{
		base.ExitState ();
	}
	public override void Startup ()
	{
		base.Startup ();
	}
}
