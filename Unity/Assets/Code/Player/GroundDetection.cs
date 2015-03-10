using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GroundDetection : MonoBehaviour {

	[SerializeField]
	public List<GameObject> _theGround = new List<GameObject>(); 
	[SerializeField]
	bool _isGroundD; 
	public bool IsTheGround { get { return _isGroundD; } }
	[SerializeField]
	bool _canUse = true; 
	bool _shouldGetStayColliders = false;

	void AddSurface(Collider _collider){
		if(!_theGround.Contains(_collider.gameObject)){
			_theGround.Add (_collider.gameObject); 
			World.PlayerCon.GetTouchedSurface(RayFromPlayer(), _isGroundD, this); //fire off a raycast to check what is going on with the new ground.
		}
	}
	void RemoveSurface(Collider _collider){
		if(_theGround.Contains(_collider.gameObject)){
			_theGround.Remove (_collider.gameObject);
			World.PlayerCon.GetTouchedSurface(RayFromPlayer(), _isGroundD, this); //fire off a raycast to check what is going on with the new ground.
		}
	}
	Ray RayFromPlayer(){ 
		return new Ray (World.PlayerCon.transform.position, (transform.position - World.PlayerCon.transform.position)); 
	}
	public void TurnOff(float _timer){ //temporarily disables this collider sending info
		_canUse = false; 
		Invoke ("TurnOn", _timer); 
		_theGround.Clear (); 
		Debug.Log ("Turning off"); 
	}
	void TurnOn(){ //starting seidng info again
		_canUse = true; 
		StartGettingStayColliders (); 
	}
	void StartGettingStayColliders(){ //when you start sending info, collect the colliders you are already in
		_shouldGetStayColliders = true; 
		Invoke ("StopGettingStayColliders", .1f); 
	}
	void StopGettingStayColliders(){
		_shouldGetStayColliders = false; 
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
	void OnTriggerStay(Collider _collider){
		if(_shouldGetStayColliders){
			if (_collider.gameObject.layer != 8) {
				AddSurface (_collider); 
				Debug.Log("Grabbed surface " + _collider.name); 
			}
		}
	}
	public bool IsGrounded(){
		if (_theGround.Count > 0)
						return true;
				else
						return false; 
	}
}
