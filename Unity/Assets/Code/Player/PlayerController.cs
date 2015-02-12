using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine.UI; 

public class PlayerController : MonoBehaviour {

	#region Variable initialization
	//bits to modify in the inspector
	[SerializeField]
	float _maxSpeed;
	public float MaxSpeed { get { return _maxSpeed; } }
	[SerializeField]
	float _acceleration;
	public float Acceleration { get { return _acceleration; } }
	[SerializeField]
	float _jumpPower; 
	public float JumpPower { get { return _jumpPower; } }
	[SerializeField]
	float _turnSpeed; 
	public float TurnSpeed { get { return _turnSpeed; } }

	//all the inputs from the player (sans mouse...this might go here later
	float _verticalInput;
	float _horizontalInput;
	float _jumpInput; 
	Vector3 _screenCenter; 
	Vector3 _velocity; 
	public Vector3 Velocity { get { return _velocity; } }

	//all the components
	[SerializeField]
	GroundDetection _groundD; 
	public GroundDetection GroundD { get { return _groundD; } }
	[SerializeField]
	GroundDetection _forwardD; 
	public GroundDetection ForwardD { get { return _forwardD; } }
	Rigidbody _rigid;
	public Rigidbody Rigid { get { return _rigid; } }
	Animator _anim; 
	public Animator Anim { get { return _anim; } }
	SkinnedMeshRenderer[] _meshes; 
	 

	//all the states, and hte state they go into
	Mo_Ground _moGround = new Mo_Ground(); 
	Mo_InAir _moInAir = new Mo_InAir (); 
	Motion _move; 
	string _state; 
	string _lastState;
	public string LastState { get { return _lastState; } set { _lastState = value; } }
	public string CurrentState{ get { return _state; } set { _state = value; } } 

	//Things and stuff related to things
	Thing _selectedThing; 

	#endregion

	#region Motion Stuff
	//this is the housing for all motion controls
	//they are broken into different motion classes that are switched to based on conditions.  --------------------------------

	public void NowInAir(){
		_move.ExitState (); 
		_move = _moInAir; 
		_move.EnterState ();
	}

	public void NowGrounded(){
		_move.ExitState (); 
		_move = _moGround; 
		_move.EnterState (); 
	}
	#endregion

	#region Thing Interaction
	//Thing interaction --------------------------------------------

	void Crosshair(){ //raycasts from the center of the screen to find what we are 'looking at'
		Ray _ray = Camera.main.ScreenPointToRay (_screenCenter); 
		RaycastHit _hit;
		if(Physics.Raycast(_ray,out _hit)){
			Thing _thingFromRay = _hit.collider.gameObject.GetComponent<Thing>(); 
			if(_thingFromRay != null){
				SelectTheThing(_thingFromRay); 
			}
			else {
				SelectTheThing(null); 
			}
		}
		else{
			SelectTheThing(null); 
		}
	}
	void SelectTheThing(Thing _theThing){
		if(_theThing != null){ 
			if(_selectedThing != null){
				if (_theThing.ID != _selectedThing.ID) { //they are not the same thing
					_selectedThing.Deselect(); //deselect the old thing 
					_selectedThing = _theThing;  //select the new one
					_selectedThing.Select(); 
				}
			}
			else {
				_selectedThing = _theThing;  //select the new one
				_selectedThing.Select(); 
			}
		}
		else{
			if(_selectedThing !=null){
				_selectedThing.Deselect(); 
				_selectedThing = null;
			}
		}
	}

	void Clicking(){
		if (Input.GetMouseButtonDown (0)) {
			if(_selectedThing != null){
				Debug.Log("clicked an object"); 
				Adjective.CopyAdjective(World.T.AllAdj[0],_selectedThing.gameObject,0); 
			}
		}
		if (Input.GetMouseButtonDown (1)) {
			if(_selectedThing != null){
				Debug.Log("clicked an object"); 
				Adjective.CopyAdjective(World.T.AllAdj[1],_selectedThing.gameObject,0); 
			}
		}
	}
	void GetCenterOfScreen(){
		float _x = Screen.width / 2;
		float _y = Screen.height / 2; 
		_screenCenter = new Vector3 (_x, _y, 0); 
	}
	#endregion

	#region Start Update n such
	void Awake(){
		if (GetComponent<Rigidbody> () != null) { //sanity checks and componenet starting
			_rigid = GetComponent<Rigidbody> (); 
		} else
			Debug.Log ("Player needs a rigid body component, stupid ho"); 
		if (GetComponent<Animator> () != null) {
			_anim = GetComponent<Animator> (); 	
		} else
			Debug.Log ("Player needs an animator component, stupid ho"); 
	}
	void Start () {
		_moGround.Startup (this); 
		_moInAir.Startup (this); 
		_move = _moGround; 
		_move.EnterState (); 
		GetCenterOfScreen (); 
	}
	void FixedUpdate () {
		_move.ControlsEffect (); //calls has the effects of physics stuff happen
	}
	void Update(){
		Crosshair (); 
		Clicking (); 
		_move.ControlsInput ();  //gets input
		_move.MotionState (); //checks to see if it should change states
		Debug.Log (_rigid.velocity.magnitude); 
		_velocity = _rigid.velocity; 
	}
	#endregion
}
