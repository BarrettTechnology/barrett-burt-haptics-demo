using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticBox : HapticObject {

	private Vector3 direction = Vector3.zero;  // direction of force, depends on which side player entered the box from

	void Awake () {
		// set default values for kp and kp
		kp = 700.0f;
		kd = 100.0f;
	}

	/// <summary>
	/// Raises the trigger enter event. In this case, we need to know which side
	/// the player entered the box on and save that enter the player exits the box.
	/// </summary>
	/// <param name="player">The collider associated with the player object.</param>
	void OnTriggerEnter (Collider player) {
		Vector3 depth = CalcDepth (player);

		// Find the index of the closest side.
		int index = 0;
		float minDepth = Mathf.Abs (depth [0]);
		for (int i = 1; i < 3; i++) {
			if (Mathf.Abs (depth [i]) < minDepth) {
				minDepth = Mathf.Abs (depth [i]);
				index = i;
			}
		}

		/// Define a unit vector for the direction of the force. This does not need to
		/// keep track of the sign because that information is captured in the depth.
		/// Note that this does allow pop-through.
		direction = Vector3.zero;
		direction [index] = 1.0f;
	}

	/// <summary>
	/// Calculates the force based on the collider information from the Player
	/// object.
	/// </summary>
	/// <param name="player">Player.</param>
	override protected void CalcForce (Collider player) {
		Vector3 depth = CalcDepth (player);

		// Calculate the stiffness force (pushes outward). Allows pop-through.
		force = kp * Vector3.Dot (depth, direction) * direction;

		// Get the velocity of the Player object and add the damping force, which
		// is needed for stability. This pushes against velocity in the direction
		// of the closest wall  (+ or -))
		Vector3 playerVelocity = player.gameObject.GetComponent<RobotController>().GetVelocity();
		force += -kd * Vector3.Dot (playerVelocity, direction) * direction;
	}

	/// <summary>
	/// Calculates the depth.
	/// 
	/// Assumes that the collider size is set equal to the box size, i.e., the Size
	/// parameter under the Box Collider is set to (1, 1, 1). If not, you can access
	/// the scale of the Box Collider with
	///   player.GetComponent<BoxCollider> ().size
	/// and multiply boxSize element-wise.
	/// 
	/// Also assumes that the player is a sphere.
	/// </summary>
	/// <returns>The depth.</returns>
	/// <param name="player">Player.</param>
	private Vector3 CalcDepth (Collider player) {
		/// Get the box size (distances from box center to each face).
		Vector3 boxSize = this.gameObject.transform.localScale / 2.0f;

		Vector3 playerDims = player.gameObject.transform.localScale;
		float playerRad = player.GetComponent<SphereCollider> ().radius *
			Mathf.Max (playerDims.x, playerDims.y, playerDims.z);
		
		/// Get the vector from the center of this object to the center of the player
		/// object.
		Vector3 playerPos = player.gameObject.transform.position;
		Vector3 thisPos = this.gameObject.transform.position;
		Vector3 distFromCenter = playerPos - thisPos;

		/// This is unlikely to happen, but just in case...
		if (distFromCenter.magnitude < playerRad) {
			playerRad = distFromCenter.magnitude;
		}

		/// Calculate the depth in each dimension. Depth is signed to indicate
		/// which side of center.
		Vector3 depth = Vector3.zero;
		for (int i = 0; i < 3; i++) {
			depth[i] = Mathf.Sign (distFromCenter [i]) * (boxSize [i] + playerRad) - distFromCenter [i];
		}

		return depth;
	}
}
