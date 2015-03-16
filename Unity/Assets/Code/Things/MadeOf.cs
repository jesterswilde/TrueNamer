using UnityEngine;
using System.Collections;

public class MadeOf : MonoBehaviour {

	[SerializeField]
	Material _material; 
	public Material Mat { get { return _material; } }
	//What defines the material
	[SerializeField]
	float _density = 1; 
	[SerializeField]
	float _stability = 1; 
	[SerializeField]
	float _friction = 1;
	[SerializeField]
	float _life = 1; 


	//Properties --------------------------
	[SerializeField]
	float _massMod = 1;
	public float MassMod { get { return _massMod; } }
	[SerializeField]
	bool _flammable = false;
	public bool IsFlammable {get { return _flammable; } set{ _flammable =value;}}
	[SerializeField]
	float _onFireThreshold = 1; 
	public float OnFireThreshold { get { return _onFireThreshold; } }
	[SerializeField]
	float _fuelAmount = 1; 
	public float FuelAmount { get { return _fuelAmount; } }
	[SerializeField]
	bool _bouncy = false;
	public bool IsBouncy {get{return _bouncy;} set{ _bouncy = value;}}
	[SerializeField]
	float _bounceAmount = .7f; 
	public float BounceAmount { get { return _bounceAmount; } }
	[SerializeField] 
	float _fragility = 1; 
	public float Fragility { get { return _fragility; } set { _fragility = value; } }
	[SerializeField]
	float _nourshiment = 1 ; 
	public float Nourishment { get { return _nourshiment; } set { _nourshiment = value; } }
	[SerializeField]
	bool _isSlick = false;
	public bool IsSlick { get { return _isSlick; } }
	[SerializeField]
	float _slick = 0; 
	public float Slick { get { return _slick; } set { _slick = value; } }
	[SerializeField]
	float _rough = 0; 
	public float Rough { get { return _rough; } }
	public bool isRough { get { if(_rough >0) return true; else return false;}}
	[SerializeField]
	bool _climbable; 
	public bool IsClimbable { get { return _climbable; } }
	[SerializeField]
	float _climbSpeedMod; 
	public float ClimbSpeedMod { get { return _climbSpeedMod; } }

	public float MaterialDistance(Thing _theThing){
		float _distance = 0; 
		_distance += Mathf.Abs(_density - _theThing.Density);
		_distance += Mathf.Abs (_stability - _theThing.Stability); 
		_distance += Mathf.Abs(_friction - _theThing.Friction); 
		_distance += Mathf.Abs (_life - _theThing.Life); 
		return _distance; 
	}



	void Start(){
		
	}



}
