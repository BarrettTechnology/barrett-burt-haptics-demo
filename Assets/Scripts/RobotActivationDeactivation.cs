using UnityEngine;
using System.Collections;
//
public class RobotActivationDeactivation : MonoBehaviour {

	public bt.Comm.CommCoapClient comm1;
	public bt.Comm.RobotApp pgRobotRight;
	public bt.KeyboardManager keyboardManager;

	void Awake () {
		bool notLaunchedFromUI = true;
		foreach (string arg in System.Environment.GetCommandLineArgs ()) {
			if (arg.Equals ("-launched")) {
				notLaunchedFromUI = false;
				break;
			}
		}

		if (notLaunchedFromUI) {
			DontDestroyOnLoad (gameObject);

			///<summary>create new CoAP Client</summary>
			comm1 = new bt.Comm.CommCoapClient (bt.connection.IP_1, bt.connection.CLIENT_PORT);

			///<summary>create Robot Property Group, attach CoAP Client</summary>
			pgRobotRight = new bt.Comm.RobotApp (comm1._client, bt.Comm.Robot.COAP_PREFIX_ROBOT_RIGHT);

			pgRobotRight.Enable ();
			pgRobotRight.Close ();
		}
	}

	void OnApplicationQuit () {
		///<summary>create new CoAP Client</summary>
		comm1 = new bt.Comm.CommCoapClient (bt.connection.IP_1, bt.connection.CLIENT_PORT);

		///<summary>create Robot Property Group, attach CoAP Client</summary>
		pgRobotRight = new bt.Comm.RobotApp (comm1._client, bt.Comm.Robot.COAP_PREFIX_ROBOT_RIGHT);

		pgRobotRight.Disable ();
		pgRobotRight.Close ();
	}
}
