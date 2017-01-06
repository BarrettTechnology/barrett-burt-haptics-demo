using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using EXILANT.Labs.CoAP.Channels;
using EXILANT.Labs.CoAP.Message;
using EXILANT.Labs.CoAP.Helpers;
using EXILANT.Labs.CoAP.Exceptions;

public class RobotController : MonoBehaviour {

	public float kp = 0;
	public float kd = 0;
	private const float positionScale = 15;
	private Vector3 position;
	private Vector3 velocity = Vector3.zero;
	private Vector3 force = Vector3.zero;

	RobotConnection conn;

	void Awake () {
		conn = GameObject.Find ("RobotConnection").GetComponent<RobotConnection>();
	}

	void OnEnable() {
	}

	void Start () {
		// for collision method
		kp = 20;
		kd = 1.5f;

		/*
		// for trigger method
		kp = 10;
		kd = 1;
		*/
	}

	void FixedUpdate ()	{
		Vector3 positionNew = positionScale * conn.GetToolPosition();
		Vector3 velocityNew = (positionNew - position) / Time.fixedDeltaTime;  // raw unfiltered
		const float filterFreq = 20;
		float dt = Time.fixedDeltaTime;
		velocity = (velocity + filterFreq * dt * velocityNew) / (1 + filterFreq * dt);  // filtered
		position = positionNew;
		transform.position = position;

		conn.SendToolForce (force);
	}

	void OnDisable() {
	}

	/// <summary>
	/// Raises the trigger enter event.
	/// Initializes the force to zero and prints a debug message.
	/// </summary>
	/// <param name="other">Other.</param>
	void OnTriggerEnter(Collider other) {
		force = Vector3.zero;
		print ("Trigger enter " + other.gameObject.name);
	}

	/// <summary>
	/// Raises the trigger stay event.
	///
	/// This means that the player object is in contact with another object. The force
	/// is proportional to the penetration depth, which depends on the sizes, shapes,
	/// and relative positions of the objects.
	///
	/// Note that this code works when the player object can only contact one object at
	/// a time. If it will be possible to contact multiple objects, modifications must
	/// be made to handle this.
	///
	/// TODO: This is currently not in use. To use this version, change the player object
	/// to a trigger. The behavior should be the same as with the collision version. Plan
	/// to move one version to a separate demo.
	/// </summary>
	/// <param name="other">Other.</param>
	void OnTriggerStay(Collider other) {
		// TODO: switch statement on object name or object tag so only haptic objects result
		// in forces.
		// TODO: get player radius from the collider instead because it might be different. Use
		// other.GetComponent<SphereCollider> ().radius;
		Vector3 playerPos = this.gameObject.transform.position;
		float playerRad = this.gameObject.transform.localScale.x / 2;
		Vector3 otherPos = other.gameObject.transform.position;
		float otherRad = other.gameObject.transform.localScale.x / 2;
		float depth = playerRad + otherRad - (playerPos - otherPos).magnitude;  // > 0
		Vector3 direction = (playerPos - otherPos).normalized;
		//when k is positive repulsion
		//when k negative attraction
		force = kp * depth * direction +	                         // stiffness: pushes outward
				-kd * Vector3.Dot(velocity, direction) * direction;  // damping: pushes against radial velocity (+ or -)
		print ("In contact with object " + other.gameObject.name + ", force = " + force);
	}

	/// <summary>
	/// Raises the trigger exit event.
	/// Sets the force back to zero.
	/// </summary>
	/// <param name="other">Other.</param>
	void OnTriggerExit(Collider other) {
		force = Vector3.zero;
	}

	/// <summary>
	/// Raises the collision enter event.
	/// </summary>
	/// <param name="c">Collision object.</param>
	void OnCollisionEnter(Collision c ) {
	}

	/// <summary>
	/// Raises the collision stay event.
	///
	/// This means that the player object is in contact with another object. The force
	/// is proportional to the penetration depth, which depends on the sizes, shapes,
	/// and relative positions of the objects.
	///
	/// Note that this code works when the player object can only contact one object at
	/// a time. If it will be possible to contact multiple objects, modifications must
	/// be made to handle this.
	///
	/// TODO: This *is* currently in use. To use the trigger version, change the player
	/// object to a trigger. The behavior should be the same as with the this version.
	/// Plan to move one version to a separate demo.
	/// </summary>
	/// <param name="c">Collision object.</param>
	void OnCollisionStay(Collision c ) {
		Vector3 playerPos = this.gameObject.transform.position;
		Vector3 playerDims = this.gameObject.transform.localScale;
		float playerRad = c.contacts [0].thisCollider.GetComponent<SphereCollider> ().radius *
		                  Mathf.Max (playerDims.x, playerDims.y, playerDims.z);
		Vector3 contactPos = c.contacts [0].point;
		float depth = playerRad - (playerPos - contactPos).magnitude;  // > 0
		Vector3 direction = (playerPos - contactPos).normalized;
		force = kp * depth * direction +	                           // stiffness: pushes outward
			   -kd * Vector3.Dot(velocity, direction) * direction;     // damping: pushes against radial velocity (+ or -)
		print (c.contacts [0].otherCollider.gameObject.name + ", player pos = " + playerPos + ", contact pos = " + contactPos + ", depth = " + depth);
	}

	/// <summary>
	/// Raises the collision exit event.
	/// Sets the force back to zero.
	/// </summary>
	/// <param name="c">Collision object.</param>
	void OnCollisionExit(Collision c ) {
		force = Vector3.zero;
	}

	void OnGUI() {
		Event e = Event.current;
		string keyPressed = e.keyCode.ToString();
		if (e.type == EventType.KeyUp) {
			Debug.Log ("Key pressed: " + keyPressed);
			keyPressed = keyPressed.ToLower ();
		}
	}

	// close    
	void OnApplicationQuit() {
	}
}
