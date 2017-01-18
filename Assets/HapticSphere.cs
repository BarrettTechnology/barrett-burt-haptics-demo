using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticSphere : HapticObject {

	void Awake () {
		// initialize kp and kp to default values
		kp = 400.0f;
		kd = 40.0f;
	}

	void OnCollisionStay (Collision c) {
		if (c.contacts [0].otherCollider.gameObject.name == "Player") {
			Vector3 position = this.gameObject.transform.position;

			// This should be a sphere, but just in case it's not, check all the dimensions.
			Vector3 dims = this.gameObject.transform.localScale;
			float radius = c.contacts [0].thisCollider.GetComponent<SphereCollider> ().radius *
				Mathf.Max (dims.x, dims.y, dims.z);

			Vector3 contactPos = c.contacts [0].point;
			float depth = radius - (position - contactPos).magnitude;  // > 0

			Vector3 other_velocity = c.contacts [0].otherCollider.gameObject.GetComponent<RobotController>().GetVelocity();

			Vector3 direction = (position - contactPos).normalized;
			force = -kp * depth * direction +  // stiffness: pushes outward
				-kd * Vector3.Dot (other_velocity, direction) * direction;  // damping: pushes against radial velocity (+ or -)
		}
	}
}
