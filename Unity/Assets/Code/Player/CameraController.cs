using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class CameraController : MonoBehaviour {

	Transform _player; 
	[SerializeField]
	Transform _theCam; 
	public Transform TheCam { get { return _theCam; } } 
	[SerializeField]
	float stationaryDistance;
	[SerializeField]
	float speedFactor; 
	float _currentCamDistance; 
	Vector3 _targetDir; 
	Vector3 _targetPos; 
	[SerializeField]
	public float orbitSpeed; 
	[SerializeField]
	LayerMask cameraCollision; 
	
	OrbitMethod _orbit; 
	NormalOrbit _normalOrbit;


	public void Normal(){
		_orbit = _normalOrbit; 
	}

	void FollowPlayer(){
		transform.position = _player.position; 
	}

	void CameraDistance(){ //figures out where walls are between the player and camera
		Ray _ray = new Ray (this.transform.position, TheCam.position - this.transform.position); 
		RaycastHit _hit; 
		if (Physics.Raycast (_ray, out _hit, stationaryDistance, cameraCollision)) {
			_currentCamDistance = Mathf.Clamp(Vector3.Distance (_hit.point,this.transform.position),3f, stationaryDistance) -.5f;  
		} 
		else{
			_currentCamDistance = (Mathf.Clamp(stationaryDistance-10,0,100) +(World.PlayerCon.Rigid.velocity.magnitude /4)) *speedFactor + stationaryDistance;
		}
	}
	void PositionCamera(){ // moves the camera into place with a lerp
		_targetDir =  _theCam.position - transform.position; 
		_theCam.position = Vector3.Lerp(TheCam.position, transform.position + _targetDir.normalized * _currentCamDistance, Time.deltaTime*3); 
	}
	void Start(){
		_player = World.PlayerTran;
		_targetPos = _theCam.position; 
		_normalOrbit = new NormalOrbit (); 
		_normalOrbit.Startup (this); 
		_orbit = _normalOrbit; 
	}

	// Update is called once per frame
	void Update () {
		_orbit.Orbit (); 
		FollowPlayer (); 
		CameraDistance (); 
		PositionCamera (); 
	}
}






public class OrbitMethod{
	protected Transform _camTrans; 
	protected float _orbitSpeed; 
	protected PlayerController _player; 

	public virtual void Startup(CameraController _controller){
		_camTrans = _controller.transform;
		_orbitSpeed = _controller.orbitSpeed; 
		_player = World.PlayerCon;
	}
	public virtual void Orbit(){
		
	}
	public virtual float ClampValue(float _value, float _max, float _min){
		if (_value >= _max+50 && _value <= 360-_min) {
			return 360-_min;		
		}
		if (_value >= _max && _value <= _max+50) {
			return _max; 	
		}
		return _value; 
	}
}


public class NormalOrbit : OrbitMethod {
	public override void Orbit ()
	{
		float _yRot = _camTrans.rotation.eulerAngles.y + Input.GetAxis ("Mouse X") * _orbitSpeed;
		float _xRot = _camTrans.rotation.eulerAngles.x + Input.GetAxis ("Mouse Y") * _orbitSpeed*-1;
		_camTrans.rotation = Quaternion.Euler (ClampValue(_xRot,70,70), _yRot, 0); 
	}
}
