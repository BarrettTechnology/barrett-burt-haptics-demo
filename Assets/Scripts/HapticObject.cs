using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic haptic object. Create custom haptic objects that derive from this class.
/// 
/// Any haptic object you create in the Unity editor should have an associated
/// trigger collider and should be tagged with the HapticObject tag, which is a
/// custom tag created for this purpose.
/// 
/// Likewise, your cursor should be tagged with the HapticCursor tag, another
/// custom tag.
///
/// An object can be tagged in the Unity editor through the Tags drop-down menu.
/// New custom tags can also be created in this menu.
/// 
/// Useful references:
///   https://unity3d.com/learn/tutorials/topics/physics/colliders-triggers
///   https://docs.unity3d.com/Manual/Tags.html
/// </summary>
abstract public class HapticObject : MonoBehaviour {

	/// Every haptic object has a stiffness and damping. These are public
	/// variables, so they can be changed in the Unity editor at any time (even
	/// during run time). It is recommended to set default values for stiffness
	/// and damping in your custom haptic objects.
	public float stiffness;
	public float damping;

	/// This variable is protected so it can be accessed by derived classes (e.g.,
	/// HapticSphere and HapticBox).
	protected Vector3 force = Vector3.zero;

	/// <summary>
	/// Raises the trigger stay event.
	/// 
	/// If the colliding object is tagged as a HapticCursor, calls the function
	/// to calculate the force.
	/// </summary>
	void OnTriggerStay (Collider other) {
		if (other.gameObject.CompareTag ("HapticCursor")) {
			CalcForce (other);
		}
	}

	/// <summary>
	/// Raises the trigger exit event.
	///
	/// Sets the force back to zero.
	/// </summary>
	void OnTriggerExit () {
		force = Vector3.zero;
	}

	/// <summary>
	/// Calculates the force based on the collider information. Override this in your
	/// derived class.
	/// </summary>
	/// <param name="other">The Collider associated with the player object.</param>
	abstract protected void CalcForce (Collider other);

	/// <summary>
	/// Returns the most recently calculated force.
	/// </summary>
	public Vector3 GetForce () {
		return force;
	}
}
