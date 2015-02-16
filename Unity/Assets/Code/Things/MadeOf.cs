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
	[SerializeField]
	float _slick = 1; 
	public float Slick { get { return _slick; } set { _slick = value; } }

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
