using UnityEngine;
using System.Collections;

public class Mechanical : MonoBehaviour {

	protected Rigidbody _rigid; 
	[SerializeField]
	protected float _massThreshold; 

	// Use this for initialization
	void Start () {
		_rigid = GetComponent<Rigidbody> (); 
		_rigid.constraints = RigidbodyConstraints.FreezeAll; 
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
