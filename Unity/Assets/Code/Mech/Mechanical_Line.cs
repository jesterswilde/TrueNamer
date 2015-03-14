using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class Mechanical_Line : Mechanical {

	[SerializeField]
	float _speed; 
	[SerializeField]
	List<Transform>_path = new List<Transform>(); 
	int _nextPoint = 1; 
	int _tally = 1; 
	Vector3 _lastPos; 
	Vector3 _pos; 
	float _force; 
	bool _forward = true; 
	float _stutterTimer =0; 
	[SerializeField]
	float _stutterThreshold = .1f; 

	void MoveForward(){ //every frame move towards your next target
		float _remainingDistance = Vector3.Distance (transform.position, _path [_nextPoint].position); 
		float _distanceThisFrame = Mathf.Min (_remainingDistance, _speed * Time.deltaTime); //don't move furtherthan the target
		Vector3 _dir = (_path [_nextPoint].position - transform.position).normalized;
		if (!_forward)
						_dir *= -1; 
		transform.position = transform.position + _dir * _distanceThisFrame; 
		if (transform.position == _path [_nextPoint].position) { //if you are at the target
			_tally +=1; 
			_nextPoint = _tally % _path.Count; 

		}
	}
	void MoveBackwards(){
		_forward = false; 
		_stutterTimer = 0; 
	}

	void Stutter(){
		if (!_forward) {
			_stutterTimer += Time.deltaTime; 
			if(_stutterTimer > _stutterThreshold){
				_forward = true; 
			}
		}
	}
	void UpdateSpeed(){
		_lastPos = _pos; 
		_pos = transform.position; 
		_force = Vector3.Distance (_lastPos, _pos) / Time.deltaTime * _rigid.mass; 
	}

	void OnCollisionEnter(Collision _col){
		Thing _theThing = _col.collider.GetComponent<Thing> ();
		if (_theThing != null) {
			if(_theThing.Rigid.mass < _massThreshold){ //object is not too small
				_theThing.HitByMechanical (true,_force); 
			}
			else{
				MoveBackwards(); 
				_theThing.HitByMechanical(false,0); 
			}
		}
	}
	
	
	void Update () {
		MoveForward (); 
		Stutter (); 
		UpdateSpeed (); 
	}
	void Start(){
		transform.position = _path [0].position;
		_rigid = GetComponent<Rigidbody> (); 
		_rigid.constraints = RigidbodyConstraints.FreezeAll; 
	}
}
