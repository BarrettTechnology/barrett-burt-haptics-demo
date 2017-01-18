using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticObject : MonoBehaviour {

	public float kp;
	public float kd;

	protected Vector3 force = Vector3.zero;

	public Vector3 GetForce () {
		return force;
	}
}
