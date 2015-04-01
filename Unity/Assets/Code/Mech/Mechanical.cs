using UnityEngine;
using System.Collections;

public class Mechanical : MonoBehaviour {

	protected Rigidbody _rigid; 
	[SerializeField]
	protected float _massThreshold; 
	public virtual void Activate(){
		
	}
	public virtual void PlayerEnteredVolume(){
		
	}
	public virtual void PlayerLeftVolume(){
		
	}

	// Use this for initialization
	void Start () {
		_rigid = GetComponent<Rigidbody> (); 
		_rigid.constraints = RigidbodyConstraints.FreezeAll; 
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
