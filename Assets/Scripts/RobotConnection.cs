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
	Vector3 combinedForce = new Vector3();

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
		pos.Set (rawPos.Get (1), rawPos.Get (2), rawPos.Get (0), 1);
	}

	public Vector4 getPos() {
		return pos;
	}

	public void sendForce(Vector3 force) {
		combinedForce += force;
	}

	void FixedUpdate () {
		propGroupRobotRight.SendForceVector (combinedForce.z, combinedForce.y, combinedForce.x);
		combinedForce = Vector3.zero;
	}
}
