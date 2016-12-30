using UnityEngine;
using System.Collections;
using EXILANT.Labs.CoAP.Channels;
using EXILANT.Labs.CoAP.Message;
using EXILANT.Labs.CoAP.Helpers;
using EXILANT.Labs.CoAP.Exceptions;

public class RobotConnection : MonoBehaviour {

	public bt.Comm.CommCoapClient comm1;
	public bt.Comm.RobotApp pgRobotRight;
	public bt.KeyboardManager keyboardManager;

	bt.Types.Vec rawPos;
	Vector4 pos = new Vector4();
	Vector3 combinedForce = new Vector3();

	void OnEnable () {
		///<summary>create new CoAP Client</summary>
		comm1 = new bt.Comm.CommCoapClient (bt.connection.IP_1, bt.connection.CLIENT_PORT);

		///<summary>create Robot Property Group, attach CoAP Client</summary>
		pgRobotRight = new bt.Comm.RobotApp (comm1._client, bt.Comm.Robot.COAP_PREFIX_ROBOT_RIGHT);

		///<summary>All incoming Req/Responses go into robot.Parse{Request|Response} methods</summary>
		comm1._client.CoAPRequestReceived += new CoAPRequestReceivedHandler (pgRobotRight.ParseRequest);
		comm1._client.CoAPResponseReceived += new CoAPResponseReceivedHandler (pgRobotRight.ParseResponse);

		pgRobotRight.Enable ();

		comm1.Subscribe ("r/ee_pos", OnReceiveRobotStatus);	
	}

	void OnDisable() {
		pgRobotRight.Close ();
	}

	public void OnReceiveRobotStatus (CoAPRequest req)
	{
		rawPos = pgRobotRight.GetPos ();
		pos.Set (rawPos.Get (1), rawPos.Get (2), rawPos.Get (0), 1);
//		bt.Logger.Debug (bt.Logger.DEBUG_TX_REQ, "got robot status [" + pos.ToString ("F4") + "]");
	}

	public Vector4 getPos() {
		return pos;
	}

public void sendForce(Vector3 force) {
		//Game x is -Robot y, Game y is Robot z, Game z is Robot x
		//Robot: Vec x is Robot x, Vec y is Robot z, Vec z is Robot y
		//Game: Game z is Vec x, Game y is Vec y, Game x is Vec z
		//x is backwards
		combinedForce += force;
		//Debug.Log (force.ToString("F4"));
	}

	void FixedUpdate () {
		pgRobotRight.SendForceVector (combinedForce.z, combinedForce.y, combinedForce.x);
		combinedForce = Vector3.zero;
	}
}
