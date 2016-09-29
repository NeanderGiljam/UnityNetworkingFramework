using UnityEngine;
using System.Collections;

public class CubeController : MonoBehaviour {

	public ClientController clientController;

	private float speed = 5f;

	private bool network = true;

	void Update () {
		transform.Translate(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Time.deltaTime * speed, Space.World);

		if (network && clientController != null && clientController.client._connected) {
			if (Input.GetKeyDown(KeyCode.G)) {
				clientController.client.Send(PacketHandler.Create(MessageType.Position, transform.position));
			}
		}
	}

	private float timer = 0;
	private void SendNetworkUpdate(Vector3 pos) {
		timer += Time.deltaTime;

		if (timer > 4f) {
			clientController.client.Send(PacketHandler.Create(MessageType.Position, pos));
			timer = 0;
		}
	}

	[NetworkRemoteMethod]
	public void DoDamage(float dmg = 0) {
		Debug.Log("Got " + dmg + " damage.");
	}

}