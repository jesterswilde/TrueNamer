using UnityEngine;
using System.Collections;

public class Mo_Slick : Motion {

	
	Vector3 _speed; 
	bool _canJump; 
	
	float _time; 
	float _jumpTime; 
	
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
	void CalculateSpeed(){
		if (_speed == Vector3.zero && _canJump) { //if there is no input, stop them immediately
			// If the player isn't holding anything, they lower their speed by the slickness every second. 
			_rigid.velocity = Mathf.Clamp((_rigid.velocity.magnitude - _player.StandingOnMadeOf.Slick*Time.deltaTime),0,_player.MaxSpeed)*_rigid.velocity.normalized; 
		}
		else{
			LookTowardsCamera(); 
			_rigid.AddRelativeForce (_speed.normalized * _acceleration, ForceMode.Acceleration); //moves them in their selected direction
			_rigid.velocity = Mathf.Clamp(_rigid.velocity.magnitude,0,_player.MaxSpeed) * _rigid.velocity.normalized; 
		}
	}
	
	
	void StartJumpDelay(){ //these 2 are about making it so the player can't jump repeatedly by holding the button down and getting a fuckton of movement.
		_canJump = false; //I may want to consider nixing this and 'setting' the verticle component of a jump, or even movement as a whole. 
		_jumpTime = .2f;
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
		if (_player.SelectedThing != null) {
			if(Input.GetKeyDown(KeyCode.E)){ 
				//cast another ray from the player to the object
				Ray _ray = new Ray(_player.transform.position,(_player.ThingRayHit.collider.gameObject.transform.position - _player.transform.position).normalized); 
				RaycastHit _hit;
				int _mask = ~ (1 << 8); 
				float _dist = 10000; 
				if(Physics.Raycast(_ray, out _hit,10,_mask)){
					_dist = Vector3.Distance(_player.transform.position, _hit.point); 
				}
				Debug.Log(_dist); 
				if(_dist < 2.2f && _hit.collider.gameObject.GetComponent<Rigidbody>().mass < _player.MaxPull){
					_player.ThingRayHit = _hit; 
					_player.EnterState (_player.PullingMo); ; 
				}
			}
		}
	}
	public override void ControlsEffect () //called every fixed update.
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
		GrabObject (); 
	}
	public override void EnterSlickGround (bool _isSlick) //if you enter a non slick surface, walk normally. 
	{
		base.EnterSlickGround (_isSlick);
		if (!_isSlick) {
			_player.EnterState(_player.GroundedMo); 
		}
	}
	public override void EnterState ()
	{
		_camera.Normal (); 
		StartJumpDelay ();
		Debug.Log ("Now on slick ground | " + _player.Move + " | " + _player.StandingOnMadeOf); 
	}
	public override void ExitState ()
	{
		base.ExitState ();
	}
	public override void Startup ()
	{
		base.Startup ();
		_state = "slick"; 
		_lastState = "slick"; 
	}
}