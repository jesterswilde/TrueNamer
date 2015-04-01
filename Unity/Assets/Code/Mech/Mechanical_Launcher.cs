using UnityEngine;
using System.Collections;

public class Mechanical_Launcher : Mechanical {

	[SerializeField]
	Transform _launchPoint; 
	[SerializeField]
	bool _loadable =false; 
	[SerializeField]
	Object _projectile; 
	[SerializeField]
	bool _limitedAmmo = false; 
	[SerializeField]
	float _totalAmmo = 10; 
	[SerializeField]
	float _rateOfFire; 
	[SerializeField]
	bool _autoFire = false; 
	[SerializeField]
	Collider _loadingBay; 
	[SerializeField]
	float _initialSpeed; 
	[SerializeField]
	float _forceAppliedToThing;
	[SerializeField]
	Collider _triggerVolume; 
	bool _playerInVolume = false; 

	float _fireTimer = 0; 
	bool _canFire = true; 
	GameObject _loadedProjectile; 
	bool _hasAmmo = false; 

	void UpdateTimer(){
		if(_canFire == false){
			_fireTimer += Time.deltaTime;
			if (_fireTimer > _rateOfFire) {
				_canFire = true; 
				_fireTimer = 0; 
			}
		}
	}
	void CommenceFiring(){
		_canFire = false; 
		GameObject _theProjectile = null; 
		if (_loadable) { //if the cannon uses a loaded object
			if(_loadedProjectile != null){
				_theProjectile = _loadedProjectile;
				_hasAmmo = false; 
			}
		}
		else{ //if it auto fires a cannon
			_theProjectile = Instantiate(_projectile) as GameObject; 
		}
		if (_limitedAmmo) {
			_totalAmmo -=1 ;
			if(_totalAmmo <= 0 ){
				_hasAmmo = false; 
			}
		}
		Thing _theThing = _theProjectile.GetComponent<Thing> (); 
		if(_theThing != null){
			if(_theThing.BreakThreshold() > _forceAppliedToThing){ //if the thing doesn't shatter from the force of the explosion
				_theProjectile.SetActive (true); 
				_theProjectile.transform.position = _launchPoint.position; 
				_theThing.SetLaunchVelocity(_initialSpeed * _launchPoint.forward);
				//Debug.Log(_theProjectile.GetComponent<Rigidbody>().velocity); 
			}
		}
		else{
			_theProjectile.transform.position = _launchPoint.position; 
			_theProjectile.GetComponent<Rigidbody> ().AddForce(_launchPoint.forward * _initialSpeed, ForceMode.Impulse);
		}
	}
	void TimerFiring(){
		if (_autoFire && _hasAmmo && _canFire) {
			CommenceFiring(); 
		}
	}

	public override void PlayerEnteredVolume ()
	{
		_playerInVolume = true; 
	}
	public override void PlayerLeftVolume ()
	{
		_playerInVolume = false;
	}

	public override void Activate ()
	{
		if (_triggerVolume != null && _playerInVolume && _hasAmmo && _canFire) {
			CommenceFiring(); 
		}
		if (_triggerVolume == null &&_hasAmmo && _canFire) {
			CommenceFiring(); 
		}
	}

	void Update(){
		UpdateTimer ();
		TimerFiring (); 
	}
	void Start(){
		if (_autoFire && !_limitedAmmo || _autoFire && _totalAmmo > 0) {
			_hasAmmo = true; 
		}
		if (_triggerVolume != null) {
			MechHelper_TriggerVolume _trigger = _triggerVolume.gameObject.AddComponent<MechHelper_TriggerVolume>(); 
			_trigger.MechToCall(this); 
		}
	}

}
