﻿using UnityEngine;
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
	float _climbSpeed = 5; 
	public float ClimbSpeed { get { return _climbSpeed; } }
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
	[SerializeField]
	float _maxHold;
	public float MaxHold { get { return _maxPull; } }
	[SerializeField]
	float _adjMaxDistance = 7; 
	public float AdjMaxDistance { get { return _adjMaxDistance; } }
	[SerializeField]
	float _slopeAngle = 30; 
	public float SlopeAngle { get { return _slopeAngle; } }

	//all the inputs from the player (sans mouse...this might go here later)
	float _verticalInput;
	float _horizontalInput;
	float _jumpInput; 
	Vector3 _screenCenter; 
	Vector3 _velocity; 
	public Vector3 Velocity { get { return _velocity; } }

	//all the components and relevant game objects
	[SerializeField]
	Transform _heldThingParent; 
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
	Mo_ClimbWall _moClimbwall = new Mo_ClimbWall (); 
	public Mo_ClimbWall ClimbWallMO { get { return _moClimbwall; } }
	Mo_ClimbCeiling _moClimbCeiling = new Mo_ClimbCeiling(); 
	public Mo_ClimbCeiling ClimbCeilingMo { get { return _moClimbCeiling; } }
	[SerializeField]
	Motion _move; 
	public Motion Move { get { return _move; } }
	Motion _lastState; 
	public Motion LastState { get { return _lastState; } set { _lastState = value; } }
	RaycastHit _groundHit; 
	GroundDetection _curGrounD; 
	GameObject _standingOnGO; 
	public GroundDetection CurrentGroundD { get { return _curGrounD; } }
	public RaycastHit GroundHit { get { return _groundHit; } }
	Vector3 _pauseVelocity = new Vector3();
	bool _isPauseMotion = false; 
	float _pauseDrag = 0; 
	float _pauseTimer = 0; 
	float _pauseMotionDelay = .1f; 

	//Things and stuff related to things
	[SerializeField]
	Thing _selectedThing; 
	public Thing SelectedThing { get { return _selectedThing; } set { _selectedThing = value; } }
	Thing _pulledThing; 
	Thing _heldThing = null; 
	public Thing HeldThing { get { return _heldThing; } }
	RaycastHit _thingRayHit;
	public RaycastHit ThingRayHit { get { return _thingRayHit; } set { _thingRayHit = value; } }
	public Thing PulledThing { get { return _pulledThing; } set { _pulledThing = value; } }
	Mechanical _selectedMech = null;
	public Mechanical SelectedMech { get { return _selectedMech; } }

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
	void GroundedCheck(){ //puts the player into the air as a neutral state
		if(_curGrounD != null){
			if (!_curGrounD.IsGrounded ()) {
				EnterState(_moInAir); 
			}
		}
	}
	public void PickUpThing(){ //These are called by the motition states, currenly only mo_ground
		if(_selectedThing.PickUp (_heldThingParent)){ //check to make sure you can pick up object
			_heldThing = _selectedThing; 
			World.RemoveHeldObjectFromGroundD() ;
		}
	}
	public void DropThing(){
		_heldThing.Drop (); 
		_heldThing = null; 
	}

	public void Pause(){//when the game is paused we increase drag in case the player gets shunted by growing objects
		_isPauseMotion = true; 
		_pauseTimer = 0;
		_pauseVelocity = _rigid.velocity;
		_pauseDrag = _rigid.drag; 
		_rigid.velocity = Vector3.zero; 
		_rigid.drag = 1000; 
	}
	void UnpauseMotionDelay(){
		if (!World.IsPaused && _isPauseMotion) {
			_pauseTimer += Time.fixedDeltaTime; 
			if(_pauseTimer > _pauseMotionDelay){
				UnPause(); 
			}
		}
	}
	public void UnPause(){
		_rigid.velocity = _pauseVelocity;
		_rigid.drag = _pauseDrag;
		_isPauseMotion = false;
	}
	void OnCollisionEnter(Collision _other){
		if(_velocity.y > 0){
			_rigid.velocity = new Vector3(_rigid.velocity.x, _velocity.y,_rigid.velocity.z);  //THis preserves up and down momentum on collision
		}
	}
	bool IsThisACeiling(RaycastHit _theHit){
		if (Vector3.Dot (_theHit.normal, Vector3.down) > .8f) {
			return true;
		}
		return false; 
	}
	void TouchedSurfaceSafetyCheck(){ //just make sure we are moving on the correct plane. 
		if (_curGrounD != null) {
			Ray _ray = new Ray(transform.position, _curGrounD.transform.position - transform.position); 
			GetTouchedSurface(_ray,_curGrounD.IsTheGround,_curGrounD); 
		}
	}
	public void GetTouchedSurface(Ray _ray, bool _isGround, GroundDetection _theD){ //fired whenever you enter a new surface, or whenever the player swaps adjectives
		Debug.Log ("Shooting rays"); 
		if (_isGround) {
			CastToGround(_ray, _theD); 
		}
		else{
			CastToClimbable(_ray, _theD); 
		}
		Debug.Log (" ==== " + _curGrounD + " | " + _standingOnGO  + " | " + _move); 
	}
	public void GetTouchedSurface(){ //this version gets called when an adjective swaps
		Ray _ray = new Ray (transform.position, Vector3.down); //will need to modify this for when the player is climbing
		CastToGround (_ray, GroundD); 
	}
	public void ClearStandingOn(){
		_curGrounD = null;
		_standingOnGO = null;
		_standingOnMadeOf = null; 
	}
	void CastToGround(Ray _ray, GroundDetection _theD){//this is called whenever a ground is entered or exited
		Collider _coll = _theD.GetComponent<Collider> (); 
		RaycastHit _hit;  //it raycasts directly down and figures out if it is on a slick surface or not. 
		if(Physics.SphereCast(_ray, _coll.bounds.extents.magnitude ,out _hit, 10, World.NoPlayerMask)){
			Thing _otherThing = _hit.collider.gameObject.GetComponent<Thing>(); 
			if(_otherThing != null){
				_standingOnMadeOf = _otherThing.MadeFrom; 
				_move.EnterSlickGround(_otherThing.MadeFrom.IsSlick); 
			}
			else{
				_standingOnMadeOf = null; 
			}
			_standingOnGO = _hit.collider.gameObject; 
			_groundHit = _hit; 
			_curGrounD = _theD; 
		}
	}
	
	void CastToClimbable(Ray _ray, GroundDetection _theD){//this is used for surfaces you would climb on, such as walls or ceilings
		Collider _coll = _theD.GetComponent<Collider> (); 
		RaycastHit _hit;  //it raycasts directly down and figures out if it is on a slick surface or not. 
		if(Physics.Raycast(_ray,out _hit, 10,World.NoPlayerMask)){
			Debug.Log("found a wall"); 
			Thing _otherThing = _hit.collider.gameObject.GetComponent<Thing>(); 
			if(_otherThing != null){
				if(_otherThing.MadeFrom.IsClimbable && _otherThing.gameObject != _standingOnGO){ //if you ran into a thing, and that thing is climbable
					_standingOnMadeOf = _otherThing.MadeFrom; 
					_groundHit = _hit; 
					_curGrounD = _theD; 
					_standingOnGO = _hit.collider.gameObject; 
					if(IsThisACeiling(_hit)){ //we now have to figure out if you are on a wall or a ceiling
						EnterState(_moClimbCeiling);
					}
					else{
						EnterState(_moClimbwall); 
					}
				}
			}
		}
		else{
			if(_curGrounD == _theD){
				World.RecalculateGroundD(); 
				Debug.Log(_theD); 
				Debug.Log("Recalculating"); 
				EnterState(_moInAir);
			}
		}
	}
	#endregion

	#region Thing Interaction
	//Thing interaction --------------------------------------------

	void Crosshair(){ //raycasts from the center of the screen to find what we are 'looking at'
		int _mask = ~ (1 << 8); 
		Ray _ray = Camera.main.ScreenPointToRay (_screenCenter); 
		RaycastHit _hit;
		if(Physics.Raycast(_ray,out _hit,100,_mask)){
			Thing _thingFromRay = _hit.collider.gameObject.GetComponent<Thing>(); 
			if(_thingFromRay != null){
				float _distance = Vector3.Distance(_hit.point, transform.position); 
				SelectTheThing(_thingFromRay, _hit, _distance); 
			}
			else{
				Mechanical _mechFromRay = _hit.collider.gameObject.GetComponent<Mechanical>(); 
				if(_mechFromRay !=null){
					SelectTheMech(_mechFromRay); 
				}
			}
		}
		else{
			SelectTheMech(null); 
		}
	}
	void SelectTheMech(Mechanical _theMech){ //we are looking at something mechanical
		if(_selectedThing != null){
			_selectedThing.Deselect(); 
			_selectedThing = null; 
		}
		_thingRayHit = new RaycastHit (); 
		_selectedMech = _theMech; 
	}
	void SelectTheThing(Thing _theThing, RaycastHit _theHit, float _distance){ //we are looking at a thing
		if(_theThing != null && _distance < _adjMaxDistance){ 
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
	void ActivateMech(){
		if (Input.GetKeyDown (KeyCode.E)) {
			if(_selectedMech != null){
				_selectedMech.Activate(); 
			}
		}
	}
	void Clicking(){
		if (Input.GetMouseButtonDown (0)) {
			if(!World.IsPaused){ //if the game is not pause
				World.PauseTime(); 
			}
			else if(_selectedThing != null){ //if the game is paused and you have a thing selected, do the swap
				World.SwapAdj(); 
			}
		}
		if(Input.GetMouseButtonDown (1)){
			Debug.Log("right click"); 
			if(World.IsPaused && World.CanUnpause){ //right clicking unpauses the game
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
		ActivateMech ();
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
		_moClimbwall.Startup (); 
		_moClimbCeiling.Startup (); 
		_move = _moGround; 
		_move.EnterState (); 
		GetCenterOfScreen (); 
		GetStartingAdj (); 
		InvokeRepeating ("TouchedSurfaceSafetyCheck", 2, 1f); 
	}
	void FixedUpdate () {
		_move.ControlsEffect (); //calls has the effects of physics stuff happen
		GroundedCheck (); 
		UnpauseMotionDelay ();
	}
	void Update(){
		Crosshair (); 
		Clicking (); 
		_move.ControlsInput ();  //gets input
		_move.MotionState (); //checks to see if it should change states
		_velocity = _rigid.velocity; 
		//Debug.Log (_curGrounD + " | " + _standingOnGO + " | " + _move); 
		//Debug.Log (_standingOnGO); 
	}
	#endregion
}
