using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Motion {

	protected PlayerController _player;
	protected Rigidbody _rigid; 
	protected Animator _anim; 
	protected CameraController _camera; 


	protected float _verticalInput; 
	protected float _horizontalInput; 
	protected float _jumpInput; 


	protected float _maxSpeed; 
	protected float _acceleration;
	protected float _jumpPower;
	protected float _turnSpeed; 
	protected float _previousSpeed = 0; 
	

	protected Transform _cameraTrans; 
	protected GroundDetection _groundD; 
	protected GroundDetection _forwardD; 
	

	protected string _state; 
	protected string _lastState; 
	public string LastState { get { return _lastState; } }
	public string StateName { get { return _state; } } 

	//this bit is just to get all the variables that I need taken from the player controller script

	public virtual void Startup(){ //sets all the variables
		_player = World.PlayerCon;
		_rigid = _player.GetComponent<Rigidbody> (); 
		_maxSpeed = _player.MaxSpeed;
		_acceleration = _player.Acceleration; 
		_jumpPower = _player.JumpPower; 
		_turnSpeed = _player.TurnSpeed; 
		_cameraTrans = World.CamTrans; 
		_camera = World.CamCon;
		_anim = _player.Anim; 
		_forwardD = _player.ForwardD;
		_groundD = _player.GroundD; 
	}

	public virtual void ControlsEffect(){ // Called every physics update

	}
	public virtual void ControlsInput(){ //called every update
		SetAxis ();
		SetAnim ();
	}
	protected virtual void SetAnim(){ //gives the animator information every update


	}
	void SetAxis(){ //gets all inputs
		_verticalInput = Input.GetAxisRaw ("Vertical");
		_horizontalInput = Input.GetAxisRaw ("Horizontal"); 
		_jumpInput = Input.GetAxisRaw ("Jump"); 
	}
	public virtual void MotionState(){ //the states that can be entered from this place

	}
	public virtual void EnterSlickGround(bool _isSlick){//called whenever the player enters  a new surface

	}
	//mostly book keeping
	public virtual void EnterState(){

	}
	public virtual void ExitState(){
		_player.LastState = this; 
	}
	public virtual void LookTowardsCamera(){ //called when you move forwards or backwards
		Quaternion _targetRotation =  Quaternion.Euler(_player.transform.rotation.eulerAngles.x, _cameraTrans.rotation.eulerAngles.y, _player.transform.rotation.eulerAngles.z);
		//I use player rotations for X and Z because I am only trying to rotate in the Y axis
		_player.transform.rotation = Quaternion.Slerp (_player.transform.rotation, _targetRotation, _turnSpeed*Time.deltaTime); 
	}
	public virtual void LookTowardsCamera(float _turning){
		Quaternion _targetRotation =  Quaternion.Euler(_player.transform.rotation.eulerAngles.x, _cameraTrans.rotation.eulerAngles.y, _player.transform.rotation.eulerAngles.z);
		_player.transform.rotation = Quaternion.Lerp(_player.transform.rotation, _targetRotation, _turning); 
	}
	protected virtual void LookTowardsVelocity(){ //called when the player jumps
		Vector3 _noYVel = new Vector3 (_player.Rigid.velocity.x, 0, _player.Rigid.velocity.z); 
		if(_noYVel.magnitude > 6){ //This prevents the player from spinning wildly in the air
			Vector3 _back = _player.transform.position + _noYVel; //It's called back because it's mostly for wall jumps
			_player.transform.LookAt (_back); //you'll note no lerping in this version. That's beacuse Snapping is important, I may put the lerping back in later
		}
	}
}
