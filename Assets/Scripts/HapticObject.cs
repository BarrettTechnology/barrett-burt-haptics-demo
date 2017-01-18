using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticObject : MonoBehaviour {

	public float kp;
	public float kd;

	protected Vector3 force = Vector3.zero;

	void OnCollisionEnter (Collision c) {
		Debug.Log ("Collision enter: " + this.gameObject.name);
	}

	void OnCollisionExit (Collision c) {
		Debug.Log ("Collision exit: " + this.gameObject.name);
		force = Vector3.zero;
	}

	public Vector3 GetForce () {
		return force;
	}
}
