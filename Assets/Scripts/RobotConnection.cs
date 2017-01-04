using UnityEngine;
using System.Collections;
using EXILANT.Labs.CoAP.Channels;
using EXILANT.Labs.CoAP.Message;
using EXILANT.Labs.CoAP.Helpers;
using EXILANT.Labs.CoAP.Exceptions;

public class RobotConnection : MonoBehaviour {

	public bt.Comm.CommCoapClient comm1;
	public bt.Comm.RobotApp propGroupRobotRight;
	public bt.KeyboardManager keyboardManager;

	bt.Types.Vec rawPos;
	Vector4 pos = new Vector4();
	Vector4 force = new Vector4();
	Vector3 combinedForce = new Vector3();
	Matrix4x4 transCoordsRobotToUnity = Matrix4x4.identity;
	Matrix4x4 transCoordsUnityToRobot = Matrix4x4.identity;

	/// <summary>
	/// This is the first function to be called. It sets up the coordinate
	/// transformations between the robot frame (a right-handed coordinate system) and
	/// the Unity frame (a left-handed coordinate system). Edit the first three lines
	/// to change the transformation.
	/// </summary>
	void Awake () {
		transCoordsRobotToUnity.SetRow(0, new Vector4(0, -1, 0, 0)); // Unity x is robot -y, no translation
		transCoordsRobotToUnity.SetRow(1, new Vector4(0,  0, 1, 0)); // Unity y is robot z, no translation
		transCoordsRobotToUnity.SetRow(2, new Vector4(1,  0, 0, 0)); // Unity z is robot x, no translation
		transCoordsUnityToRobot = transCoordsRobotToUnity.inverse;
	}

	void OnEnable () {
		///<summary>create new CoAP Client</summary>
		comm1 = new bt.Comm.CommCoapClient (bt.connection.IP_1, bt.connection.CLIENT_PORT);

		///<summary>create Robot Property Group, attach CoAP Client</summary>
		propGroupRobotRight = new bt.Comm.RobotApp (comm1._client, bt.Comm.Robot.COAP_PREFIX_ROBOT_RIGHT);

		///<summary>All incoming Req/Responses go into robot.Parse{Request|Response} methods</summary>
		comm1._client.CoAPRequestReceived += new CoAPRequestReceivedHandler (propGroupRobotRight.ParseRequest);
		comm1._client.CoAPResponseReceived += new CoAPResponseReceivedHandler (propGroupRobotRight.ParseResponse);

		propGroupRobotRight.Enable ();

		comm1.Subscribe ("r/ee_pos", OnReceiveRobotStatus);	
	}

	void OnDisable() {
		propGroupRobotRight.Close ();
	}

	public void OnReceiveRobotStatus (CoAPRequest req)
	{
		rawPos = propGroupRobotRight.GetPos ();
		pos.Set (rawPos.Get (0), rawPos.Get (1), rawPos.Get (2), 1);
		pos = transCoordsRobotToUnity * pos;
	}

	public Vector4 getPos() {
		return pos;
	}

	public void SendForce(Vector3 force) {
		combinedForce += force;
	}

	void FixedUpdate () {
		force = transCoordsUnityToRobot * combinedForce;
		propGroupRobotRight.SendForceVector (force.x, force.y, force.z);
		combinedForce = Vector3.zero;
	}
}
