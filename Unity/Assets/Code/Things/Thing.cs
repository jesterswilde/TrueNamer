using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class Thing : MonoBehaviour {


	#region Property Delceration

	//These first set of properties help define what an object is. They are read by the world and then the thing is told what it is made of
	[SerializeField]
	float _mass = 1;
	float _modMass; 
	public float Mass{get{return _modMass;}set{ _modMass = value;}}
	float _modScale;
	[SerializeField, HideInInspector]
	Vector3 _startScale = new Vector3 (1, 1, 1); 
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
	[SerializeField]
	float _durability = 30; 
	[SerializeField]
	bool _breakable = true; 
	[SerializeField]
	float _onFireThreshold;
	[SerializeField]
	float _fuelAmount; 
	
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
	float _force; 
	public float Force { get { return _force; } }
	Quaternion _rotation;
	bool _isFreeRotating = false; 
	public Quaternion Rotation { get { return _rotation; } } 
	bool _isShunting = false; 
	public bool IsShunting { get { return _isShunting; } }
	bool _isGrowing = false;
	Vector3 _targetScale;
	float _unscaledTime = 2;  


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
		if(World.IsPaused){ //when the object is updated, you need to move physics forward a frame to allow for shunting. 
			//World.StepForwardOneFrame (); 
		}
	} 
	public void ClearAdjList(){
		_localAdj.Clear (); 
	}
	void ResetThingToBase(){
		_modMass = _mass; 
		_rigid.mass = _modMass;
		_modScale = 1; 
		_modDensity = _density;
		_modStability = _stability;
		_modFriction = _friction; 
		_modLife = _life;
	}
	void ApplyAdjectives(){ //some Adjectives are more than a boolean. In that case, this will call the list of functions
		ApplyScaleMod (); 
	}
	void ApplyMadeOf(){ //setting the materials, other things may happen here later. 
		_renderer.material = _madeOf.Mat; 
		foreach (Renderer _childRend in _meshes) {
			_childRend.material = _madeOf.Mat; 
		}
	}
	void SlowMoGrow(){
		if (World.IsPaused && _isGrowing) {
			Debug.Log(_unscaledTime); 
			_unscaledTime += Time.unscaledDeltaTime; 
			transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.fixedDeltaTime*20); 
			if(transform.localScale == _targetScale){
				_isGrowing = false; 
				World.UnSlowMo(); 
			}
			if(_isGrowing && _unscaledTime > .15f){ //we can only spend so long doing this. 
				Debug.Log("overridden"); 
				transform.localScale = _targetScale; 
				_isGrowing = false; 
				World.UnSlowMo(); 
			}
		}
	}
	void ApplyScaleMod(){ //applies the scale modfications, and does the physics for it. 
		_isShunting = true; 
		PhysicsSleep(); 
		ShuntObjects(); 
		_targetScale  = _modScale*_startScale; 
		_unscaledTime = 0;
		if(World.IsPaused){
			_isGrowing = true; 
			World.SlowMo (); 
		}
		else transform.localScale = _modScale*_startScale; 
		_rigid.mass = _mass *_modScale *_modScale *_modScale; 
		_rigid.WakeUp(); 
	}

	public void RemoveAdjectiveInfluence(){ //this is used as a button for the designer, so they can modify the base scale. 
		_shouldSetStartScale = true; 
		ClearAdjList (); 
		_madeOf = null; 
		transform.localScale = _startScale; 
		if (_rigid == null) {
			_rigid = GetComponent<Rigidbody>(); 
		}
		_rigid.mass = _mass; 
	}
	public void SetAsModifiedScale(){
		Debug.Log (_startScale); 
		float _tempMass = _rigid.mass; 
		Vector3 _theScale = transform.localScale; 
		foreach (Adjective _adj in _localAdj) {
			_theScale /= _adj.scale; 
			_tempMass /= (_adj.scale*_adj.scale*_adj.scale); 
		}
		_startScale = _theScale; 
		_mass =_tempMass; 
		_shouldSetStartScale = false; 
	}
	public void ApplyLocalAdjectives(){ //If you want to just update this one thing. 
		Adjective[] _adjs = GetComponents<Adjective> (); 
		ClearAdjList (); 
		foreach (Adjective _adj in _adjs) {
			if(!_localAdj.Contains(_adj)){
				_localAdj.Add(_adj); 
			}
		}
		UpdateThing (); 
	}
	public void StartGame(){
		RemoveAdjectiveInfluence (); 
		ApplyLocalAdjectives (); 
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
				_renderer.material.SetFloat("_Blend", Mathf.Lerp(_renderer.material.GetFloat("_Blend") ,1, Time.unscaledDeltaTime *3));
				if(_renderer.material.GetFloat("_Blend") == 1){
					_isColorLerping = false; 
				}
			}
			else{
				_renderer.material.SetFloat("_Blend", Mathf.Lerp(_renderer.material.GetFloat("_Blend"), 0, Time.unscaledDeltaTime*3)); 
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

	#region Fire

	bool _isOnFire = false; 
	float _heatDampening = 10; 
	float _heatValue = 0; 

	public void HeatingHandling(){
		CheckIfOnFire (); 
		if (_isOnFire) {
			World.T.RemoveFromHeatingThings(this); 
			World.T.AddToFireList(this); 
		}
		else{ //if it is not on fire, lower the heat amount
			_heatValue = Mathf.Max (0, _heatValue - _heatDampening); 
			if(_heatValue == 0){
				World.T.RemoveFromHeatingThings(this); 
			}
		}
	}
	public void OnFireHandling(){ //handles all the fire related eventes

	}
	float TotalFireThreshold(){ 
		return _onFireThreshold * _madeOf.OnFireThreshold; 
	}
	void CheckIfOnFire(){ //currently there is no way to put an object out. We should address this
		if (_heatValue > TotalFireThreshold()) {
			_isOnFire = true; 
		}
	}
	public void RecieveHeat(float _temperature){
		_heatValue += _temperature; 
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
	float BounceFactor(Thing _otherThing){
		float _otherBounce = 0; 
		float _thisBounce = 0; 
		if(_otherThing != null){
			if(_otherThing.MadeFrom.IsBouncy ) 
						_otherBounce = _otherThing.MadeFrom.BounceAmount;
		}
		if (_madeOf.IsBouncy)
						_thisBounce = _madeOf.BounceAmount;
		if (_otherBounce > _thisBounce)
						return _otherBounce;
				else
						return _thisBounce;
	}
	public void GetBounced(Collision _theCollision, Thing _otherThing){ //when 2 things collide with eachother, and at least one is bouncy
		if(!_isKinematic){
			float _bounceAmount = BounceFactor(_otherThing); 
			if(_velocity.magnitude > 7 || _otherThing.Velocity.magnitude > 7 && _bounceAmount > 0){ //Only bounce at above certain speeds
				_isKinematic = true; 
				_kinamticTimer = 0; 
				RotationSleep(); 
				_rigid.velocity = Vector3.Reflect(_velocity,_theCollision.contacts[0].normal) * _bounceAmount; 
				EliminateLandingChaos(); 
			}
		}
	}
	public void GetBounced(Collision _theCollision, PlayerController _player){ //when the player collides with a bouncy object. 
		if (_madeOf.IsBouncy && _player.Move.StateName != "ground" && _player.Velocity.magnitude > 8) {
			_isKinematic = true; 
			_kinamticTimer = 0; 
			_player.Rigid.velocity = Vector3.Reflect(_player.Velocity ,_theCollision.contacts[0].normal)* _madeOf.BounceAmount; 
			EliminateLandingChaos(); 
		}
	}
	float BreakThreshold(){ //returns how much it takes to break this object
		return _rigid.mass *_durability * _madeOf.Fragility;
	}
	void Breaktest(Thing _otherThing){
		float _totalForce = _force; 
		if (_otherThing != null) {
			_totalForce += _otherThing.Force; 
		}
		if(_breakable){
			if (_totalForce > BreakThreshold ()) {
				BreakObject(); 
			}
		}
		Debug.Log (_totalForce + " | " + BreakThreshold ());
	}
	void Breaktest(float _otherForce){
		float _totalForce = _force + _otherForce; 
		if(_breakable){
			if (_totalForce > BreakThreshold ()) {
				BreakObject(); 
			}
		}
		Debug.Log (_totalForce + " | " + BreakThreshold ());
	}
	void BreakObject(){
		Destroy (this.gameObject); 
	}
	public void HitByMechanical(bool _isAffected, float _force){
		Debug.Log ("Hit by mechanical"); 
		if (_isAffected) {
			Breaktest(_force); 	
		}
		else{
			_rigid.velocity = _velocity;
			transform.position = _position; 
			transform.rotation = _rotation; 
		}
	}

	public void StopMoving(){ //minimizes small movements
		if(World.Chaos < 4){
			if(!_isKinematic && !_rigid.isKinematic){
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
			if(!_rigid.isKinematic){
				_rigid.velocity = new Vector3(_x,_rigid.velocity.y,_z); 
			}
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
		float _sphereRad = Vector3.Distance (this.GetComponent<Collider>().bounds.max, this.GetComponent<Collider>().bounds.center) *2; 
		Collider[] _shuntedColliders = Physics.OverlapSphere (transform.position, _sphereRad); 
		foreach (Collider _col in _shuntedColliders) {
			Thing _colThing =  _col.gameObject.GetComponent<Thing>(); 
			if(_colThing != null){
				_colThing.PhysicsSleep(); 
			}
		}
	}

	void FreeRotate(){//free rotation is about objects rotating as they move through space
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
				if(!_isFreeRotating && _rigid.velocity.magnitude > .01f){
					transform.rotation = _rotation; 
				}
			}
		}
		_force = _rigid.mass * _velocity.magnitude; 
	}

	void OnCollisionEnter(Collision _other){ //when we hit objects, do shit. 
		if(_other.collider.GetComponent<Mechanical>() == null){ //mechanical objects have their own collision setup
			Thing _colThing = _other.gameObject.GetComponent<Thing> (); 
			Breaktest (_colThing); 
			if (_colThing != null) { //if a thing collides with another thing
				GetBounced(_other, _colThing); 
				//StopMoving(); 
			}
			PlayerController _player = _other.gameObject.GetComponent<PlayerController> (); //if it's colliding with a plaer
			if (_player != null) {
				GetBounced(_other, _player); 
			}
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
	void Start(){
		if(_rigid == null){
			_rigid = GetComponent<Rigidbody> (); 
		}
		if(_meshes == null){
			_meshes = GetComponentsInChildren<Renderer> (); 
		}
		if(_renderer == null){
			_renderer =  GetComponent<MeshRenderer> ();
		}
		_position = transform.position; 
		_rotation = transform.rotation; 

		_id = World.GetID (); 


	}
	void Update(){
		OnFireHandling (); 
		SelectMaterialLerp (); 
		AwakePhysicsUpdate (); 
	}
	void FixedUpdate(){
		AwakePhysicsFixedUpdate (); 
		UpdateSleepTimer (); 
		SlowMoGrow (); 
	}
	#endregion
}
