using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A haptic sphere derived from the generic haptic object class. Make sure your
/// object has a SphereCollider that is set as a Trigger. Also make sure it is
/// tagged with the HapticObject tag.
/// </summary>
public class HapticSphere : HapticObject {

	/// <summary>
	/// Sets the default stiffness and damping for this object.
	/// </summary>
	void Awake () {
		stiffness = 500.0f;
		damping = 60.0f;
	}

	/// <summary>
	/// Calculates the force based on the collider information from the Player
	/// object.
	/// </summary>
	/// <param name="player">The collider associated with the player object.</param>
	override protected void CalcForce (Collider player) {
		Vector3 playerPos = player.gameObject.transform.position;
		Vector3 thisPos = this.gameObject.transform.position;

		/// Get the radius of each collider. Both should be spheres, but check all
		/// the dimensions just in case.
		Vector3 playerDims = player.gameObject.transform.localScale;
		Vector3 thisDims = this.gameObject.transform.localScale;

		float playerRad = player.GetComponent<SphereCollider> ().radius *
			Mathf.Max (playerDims.x, playerDims.y, playerDims.z);
		float thisRad = this.GetComponent<SphereCollider> ().radius *
			Mathf.Max (thisDims.x, thisDims.y, thisDims.z);

		/// Calculate the penetration depth and direction.
		float depth = playerRad + thisRad - (thisPos - playerPos).magnitude;  // > 0
		Vector3 direction = (thisPos - playerPos).normalized;

		/// Calculate the stiffness force (pushes outward). Allows pop-through.
		force = -stiffness * depth * direction;

		/// Get the velocity of the Player object and add the damping force, which
		/// is needed for stability. This pushes against radial velocity (+ or -).
		Vector3 playerVelocity = player.gameObject.GetComponent<RobotController>().GetVelocity();
		force += -damping * Vector3.Dot (playerVelocity, direction) * direction;
	}
}
