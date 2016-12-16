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

	// Converts comm coordinates to unity coordinates. 
	// Note that the robot uses a right-handed coordinate system and Unity uses a
	// left-handed coordinate system. Some conversion is done in the comm layer,
	// but the output of the comm layer is still a right-handed coordinate system,
	// so it needs to be switched to left.
	// Unity axes (+):				Robot axes (+):					Comm axes (+):
	//   x   right					  x   into screen0				  x   left
	//   y   up						  y   left						  y   up
	//   z   into screen			  z   up						  z   into screen
	// Unity x is comm -x (robot -y)
	// Unity y is comm y (robot z)
	// Unity z is comm z (robot x)
	// TODO(ab): Update the comm layer to output either robot position or position
	// correctly converted to Unity position.
	Vector3 robotToUnity(Vector3 input) {
		return new Vector3 (-input.x, input.y, input.z);
	}

	// Converts unity coordinates to comm coordinates. 
	// Note that the robot uses a right-handed coordinate system and Unity uses a
	// left-handed coordinate system. Some conversion is done in the comm layer,
	// but the output of the comm layer is still a right-handed coordinate system,
	// so it needs to be switched to left.
	// Unity axes (+):				Robot axes (+):					Comm axes (+):
	//   x   right					  x   into screen				  x   left
	//   y   up						  y   left						  y   up
	//   z   into screen			  z   up						  z   into screen
	// comm x (robot y) is Unity -x
	// comm y (robot z) is Unity y
	// comm z (robot x) is Unity z
	// TODO(ab): Update the comm layer to output either robot position or position
	// correctly converted to Unity position.
	Vector3 unityToRobot(Vector3 input) {
		return new Vector3 (-input.x, input.y, input.z);
	}

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
		Vector3 robotPos = conn.getPos();
		Vector3 positionNew = positionScale * robotToUnity (robotPos);
		Vector3 velocityNew = (positionNew - position) / Time.fixedDeltaTime;  // raw unfiltered
		const float filterFreq = 20;
		float dt = Time.fixedDeltaTime;
		velocity = (velocity + filterFreq * dt * velocityNew) / (1 + filterFreq * dt);  // filtered
		position = positionNew;
		transform.position = position;

		conn.sendForce (unityToRobot(force));
	}

	void OnDisable() {
	}

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
	/// </summary>
	/// <param name="other">Other.</param>
	void OnTriggerStay(Collider other){
		//other.GetComponent<SphereCollider> ().radius;
		// TODO: switch statement on object name
		Vector3 playerPos = this.gameObject.transform.position;
		float playerRad = this.gameObject.transform.localScale.x / 2;  // TODO: get this from the collider instead because it might be different
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
		//print ("Trigger exit " + other.gameObject.name + ", force = " + force);
	}

	// EVENTS
	void OnCollisionEnter(Collision c ) {
	}

	void OnCollisionStay(Collision c ) {
		Vector3 playerPos = this.gameObject.transform.position;
		Vector3 playerDims = this.gameObject.transform.localScale;
		float playerRad = c.contacts [0].thisCollider.GetComponent<SphereCollider> ().radius *
		                  Mathf.Max (playerDims.x, playerDims.y, playerDims.z);
		Vector3 contactPos = c.contacts [0].point;
		float depth = playerRad - (playerPos - contactPos).magnitude;  // > 0
		Vector3 direction = (playerPos - contactPos).normalized;
		force = kp * depth * direction +	                         // stiffness: pushes outward
			   -kd * Vector3.Dot(velocity, direction) * direction;  // damping: pushes against radial velocity (+ or -)
		print (c.contacts [0].otherCollider.gameObject.name + ", player pos = " + playerPos + ", contact pos = " + contactPos + ", depth = " + depth);
	}

	void OnCollisionExit(Collision c ) {
		force = Vector3.zero;
	}

	void OnGUI() {
		Event e = Event.current;
		string keyPressed = e.keyCode.ToString();
		if(e.type == EventType.KeyUp) 
		{
			Debug.Log ("Key pressed: " + keyPressed);
			keyPressed = keyPressed.ToLower ();
			//keyboard_manager.handleKeyPress( keyPressed );
		}
	}

	// close    
	void OnApplicationQuit() {
		//comm.Close();
	}
}
