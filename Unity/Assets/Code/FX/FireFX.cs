using UnityEngine;
using System.Collections;

public class FireFX : MonoBehaviour {
	

	public void ModifySettings(GameObject _theGO){
		ParticleSystem _pSys = GetComponent<ParticleSystem> (); 
		MeshRenderer _mesh = _theGO.GetComponent<MeshRenderer> (); 
		float _avgSize = (_mesh.bounds.size.x * _mesh.bounds.size.y * _mesh.bounds.size.z) / 3; 
		_pSys.emissionRate = 20 * _avgSize;
		_pSys.startSize = 1.5f + _avgSize / 10;
	}
}
