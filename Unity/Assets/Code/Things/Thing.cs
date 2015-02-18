using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class Thing : MonoBehaviour {


	#region Property Delceration
	[SerializeField]
	float _mass = 1;
	float _modMass; 
	public float Mass{get{return _modMass;}set{ _modMass = value;}}
	float _modScale;
	[SerializeField, HideInInspector]
	Vector3 _startScale; 
	bool _shouldSetStartScale = true; 
	public float Scale {get{return _modScale;} set{ _modScale = value;}}
	[SerializeField]
	float _density = 1; 
	float _modDensity; 
	public float Density { get { return _modDensity; } set { _modDensity = value; } }
	[SerializeField]
	float _stability = 1; 
	float _modStability; 
	public float Stability { get { return _modStability; } set { _modStability = value; } }
	[SerializeField]
	float _friction = 1;
	float _modFriction; 
	public float Friction { get { return _modFriction; } set { _modFriction = value; } }
	[SerializeField]
	float _life = 1; 
	float _modLife;
	public float Life { get { return _modLife; } set { _modLife = value; } }



	/* Old Variables
	[SerializeField]
	bool _flammable = false;
	public bool IsFlammable {get { return _flammable; } set{ _flammable =value;}}
	[SerializeField]
	bool _bouncy = false;
	public bool IsBouncy {get{return _bouncy;} set{ _bouncy = value;}}
	[SerializeField] 
	float _fragility = 1; 
	public float Fragility { get { return _fragility; } set { _fragility = value; } }
	[SerializeField]
	float _nourshiment = 1 ; 
	public float Nourishment { get { return _nourshiment; } set { _nourshiment = value; } }
	float _slick = 1; 
	public float Slick { get { return _slick; } set { _slick = value; } }
	*/
	[SerializeField]
	MadeOf _madeOf; 
	public MadeOf MadeFrom { get { return _madeOf; } }

	#endregion 

	#region Non Designer Modified Properties

	Rigidbody _rigid; 
	public Rigidbody Rigid {get{return _rigid;}}
	[SerializeField]
	List<Adjective> _localAdj = new List<Adjective>(); 
	public List<Adjective> LocalAdj { get { return _localAdj; } }
	Renderer[] _meshes; 

	int _id; 
	public int ID { get { return _id; } }

	float _sleepAmount = .2f;
	float _kinamticTimer;
	bool _isKinematic  = true; 
	public bool IsKinematic { get { return _isKinematic; } }
	Vector3 _velocity; 
	public Vector3 Velocity { get { return _velocity; } }
	Vector3 _position; 
	Quaternion _rotation;
	bool _isFreeRotating = false; 
	public Quaternion Rotation { get { return _rotation; } } 
	bool _isShunting = false; 
	public bool IsShunting { get { return _isShunting; } }

	bool _isSelected = false; 
	bool _isColorLerping = false; 
	Renderer _renderer; 
	Color _startingColor; 
	Material _startinMaterial; 

	#endregion

	#region Update Thing Routine
	public void UpdateThing(){  //Called every time you change the adjectives of a thing
		if(_rigid == null){
			_rigid = GetComponent<Rigidbody> (); 
		}
		if(_meshes == null){
			_meshes = GetComponentsInChildren<Renderer> (); 
		}
		if(_renderer == null){
			_renderer =  GetComponent<MeshRenderer> ();
		}
		if(_shouldSetStartScale){
			_startScale = transform.localScale; 
			_shouldSetStartScale = false; 
		}
		ResetThingToBase ();  //first we set all the variables to their starting state
		foreach (Adjective _adj in _localAdj) { //then we go through all variables and have them do their modification
			_adj.ModifyThing();
		}
		ApplyAdjectives (); //we then change the thing to reflect it's new state
		if(World.T != null){
			_madeOf = World.WhatAmIMadeOf (this); 
			ApplyMadeOf (); 
		}
		if(World.IsPaused){
			World.StepForwardOneFrame (); 
		}
	}
	public void ClearAdjList(){
		_localAdj.Clear (); 
	}
	void ResetThingToBase(){
		_modMass = _mass; 
		_modScale = 1; 
		_modDensity = _density;
		_modStability = _stability;
		_modFriction = _friction; 
		_modLife = _life;
	}
	void ApplyAdjectives(){ //some Adjectives are more than a boolean. In that case, this will call the list of functions
		ApplyScaleMod (); 
		_rigid.mass = _modMass; 
	}
	void ApplyMadeOf(){ //setting the materials, other things may happen here later. 
		_renderer.material = _madeOf.Mat; 
		foreach (Renderer _childRend in _meshes) {
			_childRend.material = _madeOf.Mat; 
		}
	}
	void ApplyScaleMod(){ //applies the scale modfications, and does the physics for it. 
		_isShunting = true; 
		PhysicsSleep(); 
		ShuntObjects(); 
		transform.localScale = _modScale*_startScale; 
		_rigid.WakeUp(); 
	}
	public void RemoveAdjectiveInfluence(){ //this is used as a button for the designer, so they can modify the base scale. 
		Debug.Log (_startScale + " | " + _shouldSetStartScale); 
		_shouldSetStartScale = true; 
		ClearAdjList (); 
		_madeOf = null; 
		transform.localScale = _startScale; 
	}
	public void SetAsModifiedScale(){
		Debug.Log (_startScale); 
		Vector3 _theScale = transform.localScale; 
		foreach (Adjective _adj in _localAdj) {
			_theScale /= _adj.scale; 
		}
		_startScale = _theScale; 
		_shouldSetStartScale = false; 
		Debug.Log (_startScale); 
	}
	public void ApplyLocalAdjectives(){ //If you want to just update this one thing. 
		Adjective[] _adjs = GetComponents<Adjective> (); 
		ClearAdjList (); 
		foreach (Adjective _adj in _adjs) {
			_adj.GameStart(); 
		}
		UpdateThing (); 
	}

	#endregion

	#region Visual Indicators of stuff happening

	public void Select(){ //when the player selects the thing
		_isSelected = true; 
		_isColorLerping = true; 
	}

	public void Deselect(){ //when it gets deselected
		_isSelected = false; 
		_isColorLerping = true; 
	}
	public void TurnFullSelected(){
		_isColorLerping = false; 
		_renderer.material.SetFloat ("_Blend", 1); 
	}
	void SelectMaterialLerp(){
		if (_isColorLerping) {
			if(_isSelected == true){
				_renderer.material.SetFloat("_Blend", Mathf.Lerp(_renderer.material.GetFloat("_Blend") ,1, Time.deltaTime *3));
				if(_renderer.material.GetFloat("_Blend") == 1){
					_isColorLerping = false; 
				}
			}
			else{
				_renderer.material.SetFloat("_Blend", Mathf.Lerp(_renderer.material.GetFloat("_Blend"), 0, Time.deltaTime*3)); 
				if(_renderer.material.GetFloat("_Blend")== 0){
					_isColorLerping = false; 
				}
			}
		}/*
		if (_isColorLerping) {
			if(_isSelected == true){
				_renderer.material.color = Color.Lerp(_renderer.material.color,World.SelectedColor, Time.deltaTime *4);
			}
			else{
				_renderer.material.color = Color.Lerp(_renderer.material.color, _startingColor, Time.deltaTime*4); 
			}
		}
		*/
	}

	#endregion

	#region Physics Stuff
	void PhysicsSleep(){ //When we don't want objects to be flying around, we sleep 'em
		if(_rigid != null){
			if(World.Chaos < 5){
				_rigid.freezeRotation = true; 
				_rigid.drag = 1000; 
				_isKinematic = true; 
				_kinamticTimer = 0;
			}
			else{
				if(World.Chaos < 8){
					_rigid.drag = 5;
					_rigid.freezeRotation = true; 
					_isKinematic = true; 
					_kinamticTimer = 0;
				}
			}
		}
	}
	void RotationSleep(){
		if(World.Chaos < 3){
			_rigid.freezeRotation = true; 
			_kinamticTimer = 0; 
		}
	}
	void PhysicsWake(){ //Reverts this state
		_rigid.freezeRotation = false;
		_rigid.drag = 0; 
		_isKinematic= false; 
		_isShunting = false; 
	}
	void UpdateSleepTimer(){
		if (_isKinematic) {
			_kinamticTimer += Time.fixedDeltaTime;
			if(_kinamticTimer >= _sleepAmount){
				_isKinematic = false;
				PhysicsWake(); 
			}
		}
	}
	public void MakePhysicsKinematic(){
		_rigid.isKinematic = true; 
	}
	public void MakePhysicsNotKinematic(){
		_rigid.isKinematic = false; 
	}
	public void GetBounced(Collision _theCollision, Thing _otherThing){ //when 2 things collide with eachother, and at least one is bouncy
		if(!_isKinematic){
			if (_madeOf.IsBouncy || _otherThing.MadeFrom.IsBouncy) {
				if(_velocity.magnitude > 7 || _otherThing.Velocity.magnitude > 7){ //Only bounce at above certain speeds
					_isKinematic = true; 
					_kinamticTimer = 0; 
					RotationSleep(); 
					_rigid.velocity = Vector3.Reflect(_velocity,_theCollision.contacts[0].normal) *.8f; 
					EliminateLandingChaos(); 
				}
			}
		}
	}
	public void GetBounced(Collision _theCollision, PlayerController _player){ //when the player collides with a bouncy object. 
		if (_madeOf.IsBouncy && _player.Move.StateName != "ground" && _player.Velocity.magnitude > 8) {
			_isKinematic = true; 
			_kinamticTimer = 0; 
			_player.Rigid.velocity = Vector3.Reflect(_player.Velocity ,_theCollision.contacts[0].normal) *.7f; 
			EliminateLandingChaos(); 
		}
	}
	public void StopMoving(){
		if(World.Chaos < 4){
			if(!_isKinematic){
				_rigid.velocity = Vector3.zero;
			}
		}
	}
	void EliminateHorizontalChaos(){
		if(World.Chaos <1 ){ //Eliminates minute changes in inertia on the XZ plane
			float _x = 0;
			float _z = 0; 
			float _posX; 
			float _posZ; 
			if(_rigid.velocity.x >= -.2f && _rigid.velocity.x <= .2f){
				_x = 0; 
				_posX = _position.x; 
			}
			else{
				_x = _rigid.velocity.x; 
				_posX = transform.position.x; 
			}
			if(_rigid.velocity.z >= -.2f && _rigid.velocity.z <= .2f){
				_z = 0; 
				_posZ = _position.z; 
			}
			else{
				_z = _rigid.velocity.z; 
				_posZ = transform.position.z; 
			}
			_rigid.velocity = new Vector3(_x,_rigid.velocity.y,_z); 
			transform.position = new Vector3(_posX, transform.position.y,_posZ); 
		}
	}
	void EliminateLandingChaos(){ //when something lands, change it's position to be right before it landed
		if(World.Chaos < 1){
			transform.position = _position; 
		}
	}
	void ShuntObjects(){ //When an object needs to displace objects around it
		PhysicsSleep (); 
		float _sphereRad = Vector3.Distance (this.collider.bounds.max, this.collider.bounds.center) *2; 
		Collider[] _shuntedColliders = Physics.OverlapSphere (transform.position, _sphereRad); 
		foreach (Collider _col in _shuntedColliders) {
			Thing _colThing =  _col.gameObject.GetComponent<Thing>(); 
			if(_colThing != null){
				_colThing.PhysicsSleep(); 
			}
		}
	}

	void FreeRotate(){
		_isFreeRotating = true; 
	}
	void StopFreeRotation(){
		_rotation = transform.rotation; 
		_isFreeRotating = false; 
	}
	void AwakePhysicsUpdate(){ //Things that should be done every frame if the object is being calculated for physics
		if(!_rigid.IsSleeping ()){
			_velocity = _rigid.velocity; 
			_position = transform.position; 
		}
	}
	void AwakePhysicsFixedUpdate(){
		if(World.Chaos < 2){
			if (!_rigid.IsSleeping ()) {
				EliminateHorizontalChaos(); 
				if(!_isFreeRotating){
					transform.rotation = _rotation; 
				}
			}
		}
	}

	void OnCollisionEnter(Collision _other){ //when we hit objects, do shit. 
		Thing _colThing = _other.gameObject.GetComponent<Thing> (); 
		if (_colThing != null) { //if a thing collides with another thing
			GetBounced(_other, _colThing); 
			StopMoving(); 
		}
		PlayerController _player = _other.gameObject.GetComponent<PlayerController> (); //if it's colliding with a plaer
		if (_player != null) {
			GetBounced(_other, _player); 
		}
	}

	#endregion

	#region Adjective Modification

	public Adjective GetAdj(int _slot){
		if (_slot < 0 || _slot >= _localAdj.Count)
						return null;
		return _localAdj [_slot]; 
	}

	public void SwapAdjectives(Adjective _newAdj, int _slot){
		if (_slot < 0 || _slot  >= _localAdj.Count) {
			AddAdjective(_newAdj);
		}
		else{
			ReplaceAdjective(_newAdj,_slot); 
		}
	}

	public void ReplaceAdjective(Adjective _newAdj, int _slot){ //replaces one adjective with another
		Destroy (_localAdj [_slot]); 
		if(_newAdj != null){
			_localAdj [_slot] = _newAdj;
		}
		else _localAdj.RemoveAt(_slot);
		UpdateThing (); 
	}
	public void AddAdjective(Adjective _newAdj){ //this is mostly used at the start ofa game.
		if(_newAdj != null){
			_localAdj.Add (_newAdj);
		}
		SortAdjectives (); 
		UpdateThing (); 
	}
	void SortAdjectives(){ //this puts them in the order that the designer wants to see them in (at least in the localAdj list)
		List<Adjective> _tempList = _localAdj; 
		List<Adjective> _sortedList = new List<Adjective> (); 
		while(_tempList.Count > 0 ){
			int _priority = 0; 
			int _index = 0; 
			for(int i = 0; i < _tempList.Count;i++){
				if(_localAdj[i].SortPriority > _priority){
					_priority = _localAdj[i].SortPriority; 
					_index = i; 
				}
			}
			_sortedList.Add (_tempList [_index]); 
			_tempList.Remove (_tempList [_index]); 
		}
		_localAdj = _sortedList; 
	}
	#endregion

	#region Start and Update
	void Awake(){
		if(_rigid == null){
			_rigid = GetComponent<Rigidbody> (); 
		}
		if(_meshes == null){
			_meshes = GetComponentsInChildren<Renderer> (); 
		}
		if(_renderer == null){
			_renderer =  GetComponent<MeshRenderer> ();
		}
		if(_shouldSetStartScale){
			_startScale = transform.localScale; 
			_shouldSetStartScale = false; 
		}
		ClearAdjList (); 
		ResetThingToBase (); 
	}
	void Start(){
		_position = transform.position; 
		_rotation = transform.rotation; 

		UpdateThing (); 
		_id = World.GetID (); 
		_startingColor = Color.white; 
		_startinMaterial = _renderer.material; 

	}
	void Update(){
		UpdateSleepTimer (); 
		SelectMaterialLerp (); 
		AwakePhysicsUpdate (); 
	}
	void FixedUpdate(){
		AwakePhysicsFixedUpdate (); 
	}
	#endregion
}
