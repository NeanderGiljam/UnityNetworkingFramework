using UnityEngine;
using System.Collections;
using NetworkUDP;

public class ClientController : MonoBehaviour {

	public NetworkClient client { get; private set; }

	private string ipAdress = "127.0.0.1";
	private int port = 6556;
	private string portString = "";
	private string clientName = "";

	private string message = "";

	private void Awake() {
		client = GetComponent<NetworkClient>();
	}

	private void OnGUI() {
		if (!client._connected) {
			GUI.Label(new Rect(10, 10, 80, 20), "Server IP Adress:");
			ipAdress = GUI.TextField(new Rect(90, 10, 120, 20), ipAdress);
			portString = GUI.TextField(new Rect(220, 10, 120, 20), port.ToString());
			clientName = GUI.TextField(new Rect(350, 10, 120, 20), clientName);
			if (!int.TryParse(portString, out port)) {
				port = 0;
			}
			if (!client._connectionPending) {
				if (GUI.Button(new Rect(10, 30, 120, 20), "Setup Client")) {
					client.StartClient(ipAdress, port, clientName);
				}
			}
		} else {
			if (GUI.Button(new Rect(10, 10, 120, 20), "Disconnect")) {
				client.SendData(PacketHandler.Create(MessageType.Disconnect, client._clientID), client._serverIpEndPoint);
			}
			GUI.Label(new Rect(10, 40, 80, 20), "Message:");
			message = GUI.TextField(new Rect(10, 70, 120, 20), message);
			if (GUI.Button(new Rect(10, 100, 120, 20), "Send Message")) {
				client.SendData(PacketHandler.Create(MessageType.Text, client._clientID, message), client._serverIpEndPoint);
				message = "";
			}
		}
	}
}