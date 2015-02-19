using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Rigidbody> ().velocity = Vector3.forward * Time.deltaTime; 
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
