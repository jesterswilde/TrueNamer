using UnityEngine;
using System.Collections;

public class MechHelper_TargetMarker : MonoBehaviour {

	Vector3 _pos; 
	Vector3 _prevPos; 

	public float GetSpeed(){
		return Vector3.Distance (_prevPos, transform.position) / Time.deltaTime; 
	}

	void Update () {
		_prevPos = _pos; 
		_pos = transform.position; 
	}
}
