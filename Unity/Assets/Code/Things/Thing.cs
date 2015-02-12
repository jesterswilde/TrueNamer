using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class Thing : MonoBehaviour {


	#region Property Delceration
	[SerializeField]
	float _mass = 1;
	float _modMass; 
	public float Mass{get{return _modMass;}set{ _modMass = value;}}
	[SerializeField]
	float _scale = 1;
	float _modScale;
	public float Scale {get{return _modScale;} set{ _modScale = value;}}
	[SerializeField]
	bool _flammable = false;
	bool _modFlammable; 
	public bool Flammable {get { return _modFlammable; } set{ _modFlammable =value;}}
	[SerializeField]
	bool _bouncy = false;
	bool _modBouncy; 
	public bool Bouncy {get{return _modBouncy;} set{ _modBouncy = value;}}

	Rigidbody _rigid; 

	[SerializeField]
	List<Adjective> _localAdj = new List<Adjective>(); 
	#endregion 

	#region Non Designer Modified Properties
	int _id; 
	public int ID { get { return _id; } }

	float _sleepAmount = .1f;
	float _sleepTimer;
	bool _isKinematic  = true; 
	public bool IsKinematic { get { return _isKinematic; } }

	bool _isSelected = false; 
	bool _isColorLerping = false; 
	MeshRenderer _renderer; 
	Color _startingColor; 
	Material _startingMaterial; 

	#endregion

	#region Update Thing Routine
	public void UpdateThing(){  //Called every time you change the adjectives of a thing
		ResetThingToBase ();  //first we set all the variables to their starting state
		foreach (Adjective _adj in _localAdj) { //then we go through all variables and have them do their modification
			_adj.ModifyThing(); 
		}
		ApplyAdjectives (); //we then change the thing to reflect it's new state
	}
	void ResetThingToBase(){
		_modMass = _mass; 
		_modScale = _scale; 
		_modFlammable = _flammable; 
		_modBouncy = _bouncy; 
	}
	void ApplyAdjectives(){ //some Adjectives are more than a boolean. In that case, this will call the list of functions
		ApplyScaleMod (); 
		_rigid.mass = _modMass; 
	}
	void ApplyScaleMod(){
		if (_modScale != _scale || transform.localScale.x != _modScale) {
			Debug.Log("modify scale"); 
			transform.localScale = new Vector3(_modScale,_modScale,_modScale); 
			_rigid.WakeUp(); 
			PhysicsSleep(); 
		}
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
	void SelectMaterialLerp(){
		if (_isColorLerping) {
			if(_isSelected == true){
				_startingMaterial.color = Color.Lerp(_startingMaterial.color,World.SelectedColor, Time.deltaTime *3);
				if(_startingMaterial.color == World.SelectedColor){
					_isColorLerping = false; 
				}
			}
			else{
				_startingMaterial.color = Color.Lerp(_startingMaterial.color, _startingColor, Time.deltaTime*3); 
				if(_startingMaterial.color == _startingColor){
					_isColorLerping = false; 
				}
			}
		}
	}

	#endregion

	#region Physics Stuff
	void PhysicsSleep(){ //When we don't want objects to be flying around, we sleep 'em
		_rigid.freezeRotation = true; 
		_rigid.drag = 10; 
		_isKinematic = true; 
		_sleepTimer = 0; 
	}
	void PhysicsWake(){ //Reverts this state
		_rigid.freezeRotation = false;
		_rigid.drag = 0; 
		_rigid.isKinematic = false; 
	}
	void UpdateSleepTimer(){
		if (_isKinematic) {
			_sleepTimer += Time.deltaTime;
			if(_sleepTimer >= _sleepAmount){
				_isKinematic = false;
				PhysicsWake(); 
			}
		}
	}

	void OnCollisionEnter(Collision _other){ //when we hit objects, do shit. 
		Thing _colThing = _other.gameObject.GetComponent<Thing> (); 
		if (_colThing != null) {
			if(!_colThing.IsKinematic){
				
			}
		}
	}

	#endregion

	#region Adjective Modification
	public void ReplaceAdjective(Adjective _newAdj, int _slot){ //replaces one adjective with another
		Destroy (_localAdj [_slot]); 
		_localAdj [_slot] = _newAdj; 
		UpdateThing (); 
	}
	public void AddAdjective(Adjective _newAdj){ //this is mostly used at the start ofa game.
		_localAdj.Add (_newAdj); 
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


	void Start(){
		_rigid = GetComponent<Rigidbody> (); 
		UpdateThing (); 
		_renderer =  GetComponent<MeshRenderer> ();
		_startingMaterial = _renderer.material; 
		_startingColor = _startingMaterial.color; 
		_id = World.GetID (); 
	}
	void Update(){
		UpdateSleepTimer (); 
		SelectMaterialLerp (); 
	}

}
