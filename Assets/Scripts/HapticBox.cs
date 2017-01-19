using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticBox : HapticObject {
	
	void Awake () {
		// set default values for kp and kp
		kp = 400.0f;
		kd = 40.0f;
	}

	override protected void CalcForce (Collision c) {
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

		// Calculate the depth in each dimension.
		Vector3 depth = Vector3.zero;
		for (int i = 0; i < 3; i++) {
			depth[i] = boxSize [i] - Mathf.Abs (distFromCenter [i]);
		}

		// Find the index of the closest side.
		int index = 0;
		float minDepth = depth [0];
		for (int i = 1; i < 3; i++) {
			if (depth [i] < minDepth) {
				minDepth = depth [i];
				index = i;
			}
		}

		// Define a unit vector for the direction of the force. The direction is
		// determined by the sign of distFromCenter, which points outward from
		// the center of the box.
		Vector3 direction = Vector3.zero;
		direction [index] = Mathf.Sign (distFromCenter [index]);

		// Calculate the stiffness force (pushes outward).
		force = kp * depth [index] * direction;

		// Get the velocity of the Player object and add the damping force, which
		// is needed for stability. This pushes against velocity in the direction
		// of the closest wall  (+ or -))
		Vector3 playerVelocity = c.contacts [0].otherCollider.gameObject.GetComponent<RobotController>().GetVelocity();
		force += -kd * Vector3.Dot (playerVelocity, direction) * direction;
	}
}
