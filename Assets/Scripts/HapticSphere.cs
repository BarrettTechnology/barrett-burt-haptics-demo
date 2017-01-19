using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticSphere : HapticObject {

	void Awake () {
		// set default values for kp and kp
		kp = 700.0f;
		kd = 100.0f;
	}

	/// <summary>
	/// Calculates the force based on the collider information from the Player
	/// object.
	/// </summary>
	/// <param name="player">The collider associated with the player object.</param>
	override protected void CalcForce (Collider player) {
		Vector3 playerPos = player.gameObject.transform.position;
		Vector3 thisPos = this.gameObject.transform.position;

		// Both should be spheres, but check all the dimensions just in case.
		Vector3 playerDims = player.gameObject.transform.localScale;
		Vector3 thisDims = this.gameObject.transform.localScale;

		float playerRad = player.GetComponent<SphereCollider> ().radius *
			Mathf.Max (playerDims.x, playerDims.y, playerDims.z);
		float thisRad = this.GetComponent<SphereCollider> ().radius *
			Mathf.Max (thisDims.x, thisDims.y, thisDims.z);
		
		float depth = playerRad + thisRad - (thisPos - playerPos).magnitude;  // > 0

		Vector3 otherVelocity = player.gameObject.GetComponent<RobotController>().GetVelocity();

		Vector3 direction = (thisPos - playerPos).normalized;
		force = -kp * depth * direction +  // stiffness: pushes outward
			-kd * Vector3.Dot (otherVelocity, direction) * direction;  // damping: pushes against radial velocity (+ or -)
	}
}
