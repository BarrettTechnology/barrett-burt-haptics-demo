using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticBox : HapticObject {

	private int entryIndex = -1;  // keeps track of which side the box was entered on

	void Awake () {
		// set default values for kp and kp
		kp = 400.0f;
		kd = 40.0f;
	}

	// override the definition in the base class
	void OnCollisionEnter (Collision c) {
		Debug.Log ("Collision enter: " + this.gameObject.name);

		if (c.contacts [0].otherCollider.gameObject.name == "Player") {
			// Find which side we are closest to. That is the side we entered on.
			// TODO: This is ugly. Find something cleaner?
			Vector3 centerPos = this.gameObject.transform.position;
			Vector3 contactPos = c.contacts [0].point;
			Vector3 dims = this.gameObject.transform.localScale / 2.0f;  // distances from box center to each face
			Vector3 depth = contactPos - centerPos;
			float minDepth = Mathf.Abs (dims[0] - Mathf.Abs(depth [0]));
			entryIndex = 0;
			for (int i = 1; i < 3; i++) {
				if (Mathf.Abs (depth [i]) < minDepth) {
					minDepth = Mathf.Abs (dims[i] - Mathf.Abs(depth [i]));
					entryIndex = i;
				}
			}
		}
	}

	void OnCollisionStay (Collision c) {
		if (c.contacts [0].otherCollider.gameObject.name == "Player") {
			Vector3 centerPos = this.gameObject.transform.position;
			Vector3 contactPos = c.contacts [0].point;
			Vector3 otherVelocity = c.contacts [0].otherCollider.gameObject.GetComponent<RobotController>().GetVelocity();
			Vector3 dims = this.gameObject.transform.localScale / 2.0f;  // distances from box center to each face

			float depth = dims [entryIndex] - Mathf.Abs(contactPos [entryIndex] - centerPos [entryIndex]);
			Vector3 direction = Vector3.zero;
			direction [entryIndex] = Mathf.Sign (contactPos [entryIndex] - centerPos [entryIndex]);

			force = -kp * depth * direction + // stiffness: pushes outward
				-kd * Vector3.Dot (otherVelocity, direction) * direction;  // damping: pushes against radial velocity (+ or -)
			print(force);
			force = Vector3.zero;
		}
	}

	// override the defintion in the base class
	void OnCollisionExit (Collision c) {
		Debug.Log ("Collision exit: " + this.gameObject.name);
		force = Vector3.zero;
		entryIndex = -1;
	}
}
