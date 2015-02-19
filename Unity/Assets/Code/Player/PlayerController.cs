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
	float _airSpeed;
	public float AirSpeed { get { return _airSpeed; } }
	[SerializeField]
	float _turnSpeed; 
	public float TurnSpeed { get { return _turnSpeed; } }
	[SerializeField]
	float _maxPull;
	public float MaxPull{ get { return _maxPull; } }

	//all the inputs from the player (sans mouse...this might go here later)
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
	MadeOf _standingOnMadeOf;
	public MadeOf StandingOnMadeOf { get { return _standingOnMadeOf; } }
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
	public Mo_Ground GroundedMo { get { return _moGround; } }
	Mo_InAir _moInAir = new Mo_InAir ();
	public Mo_InAir InAirMo { get { return _moInAir; } }
	Mo_Pulling _moPulling = new Mo_Pulling(); 
	public Mo_Pulling PullingMo { get { return _moPulling; } }
	Mo_Stasis _moStasis = new Mo_Stasis (); 
	public Mo_Stasis StasisMo { get { return _moStasis; } }
	Mo_Slick _moSlick = new Mo_Slick(); 
	public Mo_Slick SlickMo { get { return _moSlick; } }
	[SerializeField]
	Motion _move; 
	public Motion Move { get { return _move; } }
	Motion _lastState; 
	public Motion LastState { get { return _lastState; } set { _lastState = value; } }

	//Things and stuff related to things
	Thing _selectedThing; 
	public Thing SelectedThing { get { return _selectedThing; } set { _selectedThing = value; } }
	Thing _pulledThing; 
	RaycastHit _thingRayHit;
	public RaycastHit ThingRayHit { get { return _thingRayHit; } set { _thingRayHit = value; } }
	public Thing PulledThing { get { return _pulledThing; } set { _pulledThing = value; } }

	//Adjective stuff 
	[SerializeField]
	GameObject _adjectiveHolder;
	public GameObject AdjHolder { get { return _adjectiveHolder; } }
	[SerializeField]
	List<Adjective> _localAdj = new List<Adjective>();
	public List<Adjective> LocalAdj { get { return _localAdj; } }

	#endregion

	#region Motion Stuff


	public void EnterState(Motion _theMove){ //This is how the player moves through different motion states.
		_move.ExitState ();
		_move = _theMove;
		_move.EnterState (); 
	}
	public void Pause(){
		
	}
	public void UnPause(){
		
	}
	void OnCollisionEnter(Collision _other){
		if(_velocity.y > 0){
			_rigid.velocity = new Vector3(_rigid.velocity.x, _velocity.y,_rigid.velocity.z);  //THis preserves up and down momentum on collision
		}
	}

	public void IsGroundSlick(){ //fired whenever you enter a new surface, or whenever the player swaps adjectives
		Debug.Log ("Checking for sllick");
		Ray _ray = new Ray (transform.position, Vector3.down); 
		RaycastHit _hit;  //it raycasts directly down and figures out if it is on a slick surface or not. 
		int _mask = ~ (1 << 8); 
		if(Physics.Raycast(_ray,out _hit, 10,_mask)){
			Thing _otherThing = _hit.collider.gameObject.GetComponent<Thing>(); 
			if(_otherThing != null){
				Debug.Log(_otherThing.MadeFrom.IsSlick); 
				_move.EnterSlickGround(_otherThing.MadeFrom.IsSlick); 
				_standingOnMadeOf = _otherThing.MadeFrom; 
			}
		}
	}

	#endregion

	#region Thing Interaction
	//Thing interaction --------------------------------------------

	void Crosshair(){ //raycasts from the center of the screen to find what we are 'looking at'
		if(!World.IsPaused){
			Ray _ray = Camera.main.ScreenPointToRay (_screenCenter); 
			RaycastHit _hit;
			if(Physics.Raycast(_ray,out _hit)){
				Thing _thingFromRay = _hit.collider.gameObject.GetComponent<Thing>(); 
				SelectTheThing(_thingFromRay, _hit); 
			}
			else{
				SelectTheThing(null,_hit); 
			}
		}
	}
	void SelectTheThing(Thing _theThing, RaycastHit _theHit){
		if(_theThing != null){ 
			if(_selectedThing != null){ //you are selecting something that isn't null
				if (_theThing.ID != _selectedThing.ID) { //they are not the same thing
					_selectedThing.Deselect(); //deselect the old thing 
					_selectedThing = _theThing;  //select the new one
					_selectedThing.Select(); 
					_thingRayHit = _theHit; 
				}
			}
			else {
				_selectedThing = _theThing;  //It was null before, but now you have a selection
				_selectedThing.Select(); 
				_thingRayHit = _theHit;
			}
		}
		else{
			if(_selectedThing !=null){ //If you are now selecting nothing
				_selectedThing.Deselect();  //and there was something to select before
				_selectedThing = null;
			}
		}
	}

	void Clicking(){
		if (Input.GetMouseButtonDown (0)) {
			if(!World.IsPaused){
				if(_selectedThing!= null){ //select a thing, and pause the game
					World.PauseTime(); 
					WorldUI.ShowThingUI(); 
					_selectedThing.TurnFullSelected(); 
				}
			}
			else if(_selectedThing != null){ //if the game is paused and you have a thing selected, do the swap
				World.SwapAdj(); 
			}
		}
		if(Input.GetMouseButtonDown (1)){
			if(World.IsPaused){ //right clicking unpauses the game
				WorldUI.HideThingUI(); 
				World.UnPauseTime();
			}
			else{
				WorldUI.ShowInventoryOnly(); //right click also shows your inventory when not paused
			}
		}
		if (Input.GetMouseButtonUp (1)) {
			if(!World.IsPaused){
				WorldUI.HideInventoryOnly(); 
			}
		}
	}
	


	void GetCenterOfScreen(){
		float _x = Screen.width / 2;
		float _y = Screen.height / 2; 
		_screenCenter = new Vector3 (_x, _y, 0); 
	}
	#endregion

	#region Adjectives

	void GetStartingAdj(){ //compiles the players starting inventory
		Adjective[] _theAdjs = _adjectiveHolder.GetComponents < Adjective> (); 
		for(int i = 0; i < _theAdjs.Length ; i++){
			_localAdj[i] = _theAdjs[i]; 
		}
	}

	public Adjective GetAdj(int _slot){ //these 2 are called when replacing an adj on the player. This first bit gives the selected adj
		return _localAdj [_slot]; 
	}
	public void ReplaceAdjOnPlayer(int _slot, Adjective _theAdj){ //used when the player swaps adjectives
		if(_localAdj[_slot] != null){
			Destroy (_localAdj [_slot]); 
		}
		_localAdj [_slot] = _theAdj; 
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
		_moGround.Startup (); 
		_moInAir.Startup (); 
		_moPulling.Startup (); 
		_moSlick.Startup (); 
		_move = _moGround; 
		_move.EnterState (); 
		GetCenterOfScreen (); 
		GetStartingAdj (); 
	}
	void FixedUpdate () {
		_move.ControlsEffect (); //calls has the effects of physics stuff happen
	}
	void Update(){
		Crosshair (); 
		Clicking (); 
		_move.ControlsInput ();  //gets input
		_move.MotionState (); //checks to see if it should change states
		_velocity = _rigid.velocity; 
	}
	#endregion
}
