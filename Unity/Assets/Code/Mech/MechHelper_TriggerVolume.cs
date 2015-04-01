using UnityEngine;
using System.Collections;

public class MechHelper_TriggerVolume : MonoBehaviour {

	Mechanical _mech; 

	public void MechToCall(Mechanical _theMech){
		_mech = _theMech; 
	}

	void OnTriggerEnter(Collider _coll){
		if (_coll.gameObject.layer == 8) {
			_mech.PlayerEnteredVolume(); 	
		}
	}
	void OnTriggerExit(Collider _coll){
		if (_coll.gameObject.layer == 8) {
			_mech.PlayerLeftVolume(); 	
		}
	}
}
