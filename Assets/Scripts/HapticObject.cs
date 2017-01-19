using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic haptic object. Create custom haptic objects that derive from this class.
/// </summary>
abstract public class HapticObject : MonoBehaviour {

	/// Every object has a stiffness (kp) and damping (kd). They are defined as
	/// public variables, so they can be changed in the Unity editor at any time
	/// (even during run time). It is recommended to set default values for kp and
	/// kd in your custom haptic objects.
	public float kp;
	public float kd;

	/// This variable is protected so it can be accessed by derived classes (e.g.,
	/// HapticSphere and HapticBox).
	protected Vector3 force = Vector3.zero;

	/// <summary>
	/// Raises the collision stay event. If the colliding object is tagged as a
	/// PlayerObject, calculate the resulting forces. An object can be tagged
	/// in the Unity editor through the Tags drop-down menu.
	/// </summary>
	/// <param name="c">C.</param>
	void OnCollisionStay (Collision c) {
		if (c.contacts [0].otherCollider.gameObject.CompareTag ("PlayerObject")) {
			CalcForce (c);
		}
	}

	/// <summary>
	/// Raises the collision exit event.
	///
	/// Sets the force back to zero.
	/// </summary>
	/// <param name="c">C.</param>
	void OnCollisionExit (Collision c) {
		force = Vector3.zero;
	}

	/// <summary>
	/// Calculates the force based on the collision information. Override this in your
	/// derived class.
	/// </summary>
	/// <param name="c">C.</param>
	abstract protected void CalcForce (Collision c);

	public Vector3 GetForce () {
		return force;
	}
}
