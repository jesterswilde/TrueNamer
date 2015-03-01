using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GroundDetection : MonoBehaviour {

	public List<GameObject> _theGround = new List<GameObject>(); 
	[SerializeField]
	bool _isGroundD; 

	bool _canUse = true; 

	void AddSurface(Collider _collider){
		_theGround.Add (_collider.gameObject); 
		if(_canUse){
			World.PlayerCon.GetTouchedSurface(RayFromPlayer(), _isGroundD, this); //fire off a raycast to check what is going on with the new ground.
		}
	}
	void RemoveSurface(Collider _collider){
		_theGround.Remove (_collider.gameObject);
		if(_canUse){
			World.PlayerCon.GetTouchedSurface(RayFromPlayer(), _isGroundD, this); //fire off a raycast to check what is going on with the new ground.
		}
	}
	Ray RayFromPlayer(){
		return new Ray (World.PlayerCon.transform.position, (transform.position - World.PlayerCon.transform.position)); 
	}
	public void TurnOff(float _timer){
		_canUse = false; 
		Invoke ("TurnOn", _timer); 
	}
	void TurnOn(){
		_canUse = true; 
	}

	void OnTriggerEnter(Collider _collider){
		if (_collider.gameObject.layer != 8) {
			AddSurface (_collider); 

		}
	}
	void OnTriggerExit(Collider _collider){
		if (_collider.gameObject.layer != 8) {
			RemoveSurface(_collider); 
		}
	}
	public bool IsGrounded(){
		if (_theGround.Count > 0)
						return true;
				else
						return false; 
	}
}
