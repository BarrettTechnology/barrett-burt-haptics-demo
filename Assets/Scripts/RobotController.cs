using UnityEngine;

/// <summary>
/// Robot controller script. This is attached to the Player object in the burt-haptics-demo
/// Unity example. This class derives from MonoBehaviour, which defines some standard
/// functions that are called by Unity at specified times. Every function defined here is
/// one of the standard functions. Therefore, you should not change the function names.
/// However, you can add your own functions.
///
/// Useful references:
///   https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
///   https://docs.unity3d.com/Manual/ExecutionOrder.html
/// </summary>
public class RobotController : MonoBehaviour {

	// public variables can be changed in the Unity editor at any time (even during run time).
	public float kp;
	public float kd;

	private const float positionScale = 15.0f;  // scales the robot position for the Unity workspace

	// robot state
	private Vector3 tool_position;
	private Vector3 tool_velocity = Vector3.zero;
	private Vector3 tool_force = Vector3.zero;

	Barrett.UnityInterface.RobotConnection robot;

	/// <summary>
	/// Runs when the scene is entered. This is the first thing that happens, and it
	/// happens only once. Use this function for initializing objects and for setting
	/// values that don't matter to other objects. Don't use this to communicate with
	/// other objects because you don't know which objects have already had their
	/// Awake() functions called.
	/// </summary>
	void Awake () {
		robot = GameObject.Find ("RobotConnection").GetComponent<Barrett.UnityInterface.RobotConnection> ();

		// initialize kp and kp to default values
		kp = 400.0f;
		kd = 40.0f;
	}

	/// <summary>
	/// Runs when the object is enabled (and thus the scene has already been entered).
	/// This can happen multiple times, since objects can be enabled and disabled.
	/// All of the other objects in the scene have already had their Awake() functions
	/// called, so communication between objects can happen here.
	/// </summary>
	void OnEnable () {}

	/// <summary>
	/// Runs after OnEnable, but only occurs once (the first time the object is enabled).
	/// Communication between objects can also happen here.
	/// </summary>
	void Start () {}

	/// <summary>
	/// Runs during the physics loop at 250 Hz (fixed timestep of 0.004 s). Typically, robot
	/// control (receiving positions and sending forces) should happen here.
	///
	/// To change the loop rate, open the Unity editor and go to the Time Manager
	/// (menu: Edit > Project Settings > Time) and change the value of "Fixed Timestep". At
	/// this time, we recommend a maximum loop rate of 400 Hz (minimum timestep 0.0025 s).
	/// Reference: https://docs.unity3d.com/Manual/class-TimeManager.html
	/// </summary>
	void FixedUpdate ()	{
		tool_position = positionScale * robot.GetToolPosition ();
		tool_velocity = positionScale * robot.GetToolVelocity ();
		transform.position = tool_position;

		// Dividing the force by positionScale allows the gains to be independent of the value
		// of positionScale. If you add forces that are not related to haptic objects and are
		// independent of the scaling, you may need to divide only certain components of the
		// force by positionScale.
		robot.SetToolForce (tool_force / positionScale);
	}

	/// <summary>
	/// Runs every time the object is disabled. This can happen multiple times.
	/// </summary>
	void OnDisable () {}

	/// <summary>
	/// Raises the collision enter event. This happens when the object first contacts
	/// another object. This will only occur once for each collision. If the object remains
	/// in contact, OnCollisionStay() will be called instead.
	/// </summary>
	/// <param name="c">Collision object.</param>
	void OnCollisionEnter (Collision c) {
		for (int i = 0; i < c.contacts.Length; i++) {
			Debug.Log ("Collision enter: " + c.contacts [i].otherCollider.gameObject.name);
		}
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
	/// a time, and only at a single point. If it will be possible to contact multiple
	/// objects at the same time or multiple points on the same object (the latter can
	/// often happen with very thin objects), modifications must be made to handle this.
	/// </summary>
	/// <param name="c">Collision object.</param>
	void OnCollisionStay (Collision c) {
		Vector3 playerPos = this.gameObject.transform.position;

		// Because this example uses a SphereCollider for the Player, it works best if
		// the Player object is a sphere (equal scale in x, y, and z). If the Player
		// object is stretched, the diameter of the SphereCollider is equal to the
		// largest dimension. So for playerRad here, we use the max of playerDims.
		Vector3 playerDims = this.gameObject.transform.localScale;  // size of the Player object
		float playerRad = c.contacts [0].thisCollider.GetComponent<SphereCollider> ().radius *
		                  Mathf.Max (playerDims.x, playerDims.y, playerDims.z);

		Vector3 contactPos = c.contacts [0].point;
		float depth = playerRad - (playerPos - contactPos).magnitude;  // > 0
		Vector3 direction = (playerPos - contactPos).normalized;
		tool_force = kp * depth * direction + // stiffness: pushes outward
			-kd * Vector3.Dot (tool_velocity, direction) * direction;  // damping: pushes against radial velocity (+ or -)
		print (c.contacts [0].otherCollider.gameObject.name + ", player pos = " + playerPos + ", contact pos = " + contactPos + ", depth = " + depth + ", force = " + tool_force);
	}

	/// <summary>
	/// Raises the collision exit event. This happens at the timestep when the player object
	/// loses contact with the other object.
	///
	/// Sets the force back to zero. Again, this assumes that the player was only in contact
	/// with one object.
	/// </summary>
	/// <param name="c">Collision object.</param>
	void OnCollisionExit (Collision c) {
		tool_force = Vector3.zero;
	}

	/// <summary>
	/// This function demonstrates how to capture key presses and write messages to the Unity
	/// console.
	/// </summary>
	void OnGUI () {
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
