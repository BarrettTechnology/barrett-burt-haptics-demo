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
	/// Raises the collision enter event. In this case, we need to know which side
	/// the player entered the box on and save that enter the player exits the box.
	/// </summary>
	/// <param name="c">C.</param>
	void OnCollisionEnter (Collision c) {
		Vector3 depth = CalcDepth (c);

		// Find the index of the closest side.
		int index = 0;
		float minDepth = Mathf.Abs (depth [0]);
		for (int i = 1; i < 3; i++) {
			if (Mathf.Abs (depth [i]) < minDepth) {
				minDepth = Mathf.Abs (depth [i]);
				index = i;
			}
		}

		// Define a unit vector for the direction of the force. This does not need to
		// keep track of the sign because that information is captured in the depth.
		direction = Vector3.zero;
		direction [index] = 1.0f;
	}

	override protected void CalcForce (Collision c) {
		Vector3 depth = CalcDepth (c);

		// Calculate the stiffness force (pushes outward). Allows pop-through.
		force = kp * Vector3.Dot (depth, direction) * direction;

		// Get the velocity of the Player object and add the damping force, which
		// is needed for stability. This pushes against velocity in the direction
		// of the closest wall  (+ or -))
		Vector3 playerVelocity = c.contacts [0].otherCollider.gameObject.GetComponent<RobotController>().GetVelocity();
		force += -kd * Vector3.Dot (playerVelocity, direction) * direction;
	}

	private Vector3 CalcDepth (Collision c) {
		// Get the box size (distances from box center to each face). Assumes that
		// the collider size is set equal to the box size, i.e., the Size parameter
		// under the Box Collider is set to (1, 1, 1). If not, you can access the
		// scale of the Box Collider with
		//   c.contacts [0].thisCollider.GetComponent<BoxCollider> ().size
		// and multiply component-wise.
		Vector3 boxSize = this.gameObject.transform.localScale / 2.0f;

		// Get the distance of the contact point from the center of this object.
		Vector3 centerPos = this.gameObject.transform.position;
		Vector3 contactPos = c.contacts [0].point;
		Vector3 distFromCenter = contactPos - centerPos;

		// Calculate the depth in each dimension. Depth is signed to indicate
		// which side of center.
		Vector3 depth = Vector3.zero;
		for (int i = 0; i < 3; i++) {
			depth[i] = Mathf.Sign (distFromCenter [i]) * boxSize [i] - distFromCenter [i];
		}

		return depth;
	}
}
