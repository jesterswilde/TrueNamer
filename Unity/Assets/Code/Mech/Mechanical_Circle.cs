using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mechanical_Circle : Mechanical {
	
	[SerializeField]
	Vector3 _axisPerSecond = Vector3.right; 
	[SerializeField]
	float _stutterDuration = .5f;
	[SerializeField]
	List<Transform> _targets = new List<Transform>(); 


	float _stutterTimer = 0; 
	bool _forward; 

	void Spin(){
		if(_forward){
			transform.Rotate (_axisPerSecond * Time.deltaTime); 
		}
		else{
			transform.Rotate (_axisPerSecond * Time.deltaTime * -1);
		}
	}
	void StutterTimer(){
		if(!_forward){
			_stutterTimer += Time.deltaTime;
			if(_stutterTimer > _stutterDuration){
				_forward = true; 
			}
		}
	}
	int ClosestTarget(Vector3 _point){  //returns the closest target marker to the collision point. 
		int _index = 0; 
		float _distance = 100000;
		if(_targets.Count > 1){
			for(int i = 0; i < _targets.Count; i++){
				if(Vector3.Distance(_point, _targets[i].position) < _distance){
					_distance = Vector3.Distance(_point, _targets[i].position); 
					_index = i; 
				}
			}	
		}
		return _index; 
	}
	void ObjectTooHeavy(){
		_forward = false; 
		_stutterTimer = 0; 
	}

	void AddMarkers(){
		foreach (Transform _trans in _targets) {
			_trans.gameObject.AddComponent<MechHelper_TargetMarker>(); 
		}
	}

	void OnCollisionEnter(Collision _col){
		Thing _theThing = _col.collider.GetComponent<Thing> ();
		if (_theThing != null) {
			Debug.Log("Mass Threshold " + _theThing.Rigid.mass +  " | " + _massThreshold); 
			if(_theThing.Rigid.mass < _massThreshold){ //object is not too small
				Debug.Log(_targets[ClosestTarget(_col.contacts[0].point)].GetComponent<MechHelper_TargetMarker>().GetSpeed() + " | Speed " ) ;
				_theThing.HitByMechanical(true, _targets[ClosestTarget(_col.contacts[0].point)].GetComponent<MechHelper_TargetMarker>().GetSpeed()*_rigid.mass); 
			}
			else{
				ObjectTooHeavy(); 
				_theThing.HitByMechanical(false,0); 
			}
		}
	}


	void Update () {
		Spin (); 
		StutterTimer (); 
	}
	void Start(){
		AddMarkers (); 
		_rigid = GetComponent<Rigidbody> (); 
		_rigid.constraints = RigidbodyConstraints.FreezeAll; 
	}

}
