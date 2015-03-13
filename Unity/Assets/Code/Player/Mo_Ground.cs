using UnityEngine;
using System.Collections;

public class Mo_Ground : Motion {

	//This is the primary movement script. it deals with unburdened walking / moving. 

	Vector3 _speed; 
	bool _canJump; 

	float _time; 
	float _jumpTime; 
	Vector3 _up; 
	Vector3 _right; 

	void MoveForward(){ //controls when they move forward. This whole script should be broken up into different states.
		if (_verticalInput > 0 &&  !_forwardD.IsGrounded()){
			_speed.z += 5; 
		}
	}
	void MoveBackwards(){ //simlar to forward, this is different because I may want to clamp and have different anims and such
		if(_verticalInput < 0){
			_speed.z -= 1; 
		}
	}
	void Strafe(){
		_speed.x += 3 * _horizontalInput; 
	}

	void Jump(){ //can only jump when on the ground. When space is pushed they go up
		if (_jumpInput > 0.5f && _verticalInput > 0 &&  _canJump && !_forwardD.IsGrounded()) {
			//they are moving forward and jumping
			//_rigid.AddRelativeForce(new Vector3(0,0,1) *_jumpPower/16,ForceMode.Impulse); 
			_rigid.AddForce(new Vector3(0,1,0)*_jumpPower,ForceMode.Impulse);
			StartJumpDelay(); 
			return;
		}
		if (_jumpInput > 0.5f && _canJump) {
			_rigid.AddForce(new Vector3(0,1,0)*_jumpPower,ForceMode.Impulse);
			StartJumpDelay(); 
		}
	} 
	void CalculateAxis(){
		_up = Vector3.ProjectOnPlane(_player.transform.forward,_player.GroundHit.normal); //we are rotating up based on the the camera
		_right = (Quaternion.AngleAxis(-90,_player.GroundHit.normal) *_up).normalized;
	}
	void CalculateSpeed(){
		float _modMaxSpeed = _maxSpeed;
		float _modAcceleration = _acceleration; 
		if(_player.StandingOnMadeOf != null){ //this is getting whether the player is standing on someting rough
			_modMaxSpeed = _maxSpeed - _player.StandingOnMadeOf.Rough; 
			_modAcceleration = Mathf.Clamp(_acceleration - (_player.StandingOnMadeOf.Rough/2),3,100);
		}
		if (_speed == Vector3.zero && _canJump) { //if there is no input, stop them immediately
			_rigid.velocity = Vector3.zero;  	
		}
		else{ 
			if(_canJump){
				LookTowardsCamera(); //this is the player is holding a direction and not jumping
				Vector3 _unprojectedForward = (_player.transform.forward * _speed.z + _player.transform.right * _speed.x).normalized; 
				Vector3 _projectedForward = Vector3.ProjectOnPlane(_unprojectedForward, _player.GroundHit.normal).normalized; 
				if(_projectedForward.y <= 0 || Vector3.Angle(_unprojectedForward, _projectedForward)*2 < _player.SlopeAngle){
					float _mag = Mathf.Clamp(_rigid.velocity.magnitude,0,_modMaxSpeed); 
					_rigid.velocity = _projectedForward * (Mathf.Max (_mag,3) + _modAcceleration * Time.fixedDeltaTime );
				}
				else{
					_rigid.velocity = Vector3.zero; 
				}
				Debug.Log(_player.GroundHit.collider.name + " | " + _projectedForward); 
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

	void GrabObject(){
		if (_player.SelectedThing != null && _canJump) {
			if(Input.GetKeyDown(KeyCode.E)){ 
				//cast another ray from the player to the object
				Ray _ray = new Ray(_player.transform.position,(_player.ThingRayHit.collider.gameObject.transform.position - _player.transform.position).normalized); 
				RaycastHit _hit;
				int _mask = ~ (1 << 8); 
				float _dist = 10000; 
				if(Physics.Raycast(_ray, out _hit,10,_mask)){
					_dist = Vector3.Distance(_player.transform.position, _hit.point); 
				}
				if(_dist < 2.2f && _hit.collider.gameObject.GetComponent<Rigidbody>().mass < _player.MaxPull){
					_player.ThingRayHit = _hit; 
					_player.EnterState (_player.PullingMo); ; 
				}
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
		Jump ();
		JumpTimer (); 
	}
	public override void ControlsInput ()
	{
		base.ControlsInput ();

	}
	public override void MotionState ()
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
		//_rigid.useGravity = false;
	}
	public override void ExitState ()
	{
		base.ExitState ();
		//_rigid.useGravity = true;
	}
	public override void Startup ()
	{
		base.Startup ();
		_state = "ground"; 
		_lastState = "ground"; 
	}
}
