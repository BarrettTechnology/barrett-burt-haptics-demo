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

	RobotConnection robot;

	/// <summary>
	/// Runs when the scene is entered. This is the first thing that happens, and it
	/// happens only once. Use this function for initializing objects and for setting
	/// values that don't matter to other objects. Don't use this to communicate with
	/// other objects because you don't know which objects have already had their
	/// Awake() functions called.
	/// </summary>
	void Awake () {
		robot = GameObject.Find ("RobotConnection").GetComponent<RobotConnection>();
	}

	/// <summary>
	/// Runs when the object is enabled (and thus the scene has already been entered).
	/// This can happen multiple times, since objects can be enabled and disabled.
	/// All of the other objects in the scene have already had their Awake() functions
	/// called, so communication between objects can happen here.
	/// </summary>
	void OnEnable() {}

	/// <summary>
	/// Runs after OnEnable, but only occurs once (the first time the object is enabled).
	/// Communication between objects can also happen here.
	/// </summary>
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

	/// <summary>
	/// Runs during the physics loop at 500 Hz. Typically, robot control (receiving positions
	/// and sending forces) should happen here.
	/// </summary>
	void FixedUpdate ()	{
		position = positionScale * robot.GetToolPosition ();
		velocity = positionScale * robot.GetToolVelocity ();
		transform.position = position;

		robot.SetToolForce (force);
	}

	/// <summary>
	/// Runs every time the object is disabled.
	/// </summary>
	void OnDisable() {}

	/// <summary>
	/// Raises the trigger enter event. This happens when the trigger object first contacts
	/// another object. This will only occur once for each collision. If the object remains
	/// in contact, OnTriggerStay() will be called instead.
	/// 
	/// Initializes the force to zero and prints a debug message.
	/// </summary>
	/// <param name="other">Other.</param>
	void OnTriggerEnter(Collider other) {
		force = Vector3.zero;
		print ("Trigger enter " + other.gameObject.name);
	}

	/// <summary>
	/// Raises the trigger stay event. This happens for every timestep during with
	/// the player object remains in contact with the other object.
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
	/// Raises the trigger exit event. This happens at the timestep when the player object
	/// loses contact with the other object.
	/// 
	/// Sets the force back to zero.
	/// </summary>
	/// <param name="other">Other.</param>
	void OnTriggerExit(Collider other) {
		force = Vector3.zero;
	}

	/// <summary>
	/// Raises the collision enter event. This happens when the object first contacts
	/// another object. This will only occur once for each collision. If the object remains
	/// in contact, OnCollisionStay() will be called instead.
	/// </summary>
	/// <param name="c">Collision object.</param>
	void OnCollisionEnter(Collision c ) {
	}

	/// <summary>
	/// Raises the collision stay event. This happens for every timestep during with
	/// the player object remains in contact with the other object.
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
	/// Raises the collision exit event. This happens at the timestep when the player object
	/// loses contact with the other object.
	/// 
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

	/// <summary>
	/// Raises the application quit event. This is called when you quit the game.
	/// </summary>
	void OnApplicationQuit() {}
}
