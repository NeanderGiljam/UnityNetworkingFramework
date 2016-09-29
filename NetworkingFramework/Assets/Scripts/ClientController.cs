using UnityEngine;
using System.Collections;

public class ClientController : MonoBehaviour {

	public NetworkClient client { get; private set; }

	private string ipAdress = "127.0.0.1";
	private int port = 6556;
	private string portString = "";

	private string message = "";

	private void Awake() {
		client = GetComponent<NetworkClient>();
	}

	private void OnGUI() {
		if (!client._connected) {
			GUI.Label(new Rect(10, 10, 80, 20), "IP Adress:");
			ipAdress = GUI.TextField(new Rect(90, 10, 120, 20), ipAdress);
			portString = GUI.TextField(new Rect(220, 10, 120, 20), port.ToString());
			if (!int.TryParse(portString, out port)) {
				port = 0;
			}
			if (GUI.Button(new Rect(10, 30, 120, 20), "Connect") && port != 0) {
				client.Connect(ipAdress, port);
			}
		} else {
			GUI.Label(new Rect(10, 10, 200, 20), "Connected to: " + ipAdress);
			if (GUI.Button(new Rect(210, 10, 120, 20), "Disconnect")) { client.Disconnect(); }
			GUI.Label(new Rect(10, 30, 80, 20), "Message:");
			message = GUI.TextField(new Rect(10, 60, 120, 20), message);
			if (GUI.Button(new Rect(10, 90, 120, 20), "Send Message") || Input.GetKeyDown(KeyCode.Return)) {
				client.Send(PacketHandler.Create(MessageType.Text, message));
				message = "";
			}	
		}
	}

}