using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class NetworkClient : MonoBehaviour {

	public delegate void OnNetworkUpdate(PacketReader pr, int clientID);
	public OnNetworkUpdate onNetworkUpdate;

	public int _networkClientID { get { return networkClientID; } private set { networkClientID = value; } }
	private int networkClientID = -1;
	public string _networkClientName { get { return networkClientName; } private set { networkClientName = value; } }
	private string networkClientName = "";

	public bool _connected { get { return socket != null ? socket.Connected : false; } }

	private Socket socket;
	private byte[] buffer;

	public void Connect(string ipAdress, int port) {
		if (socket == null) {
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}
		socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ipAdress), port), new AsyncCallback(ConnectCallback), null);	
	}

	public void Disconnect() {
		socket.Send(PacketHandler.Create(MessageType.Disconnect, _networkClientID, _networkClientName));
		socket.Shutdown(SocketShutdown.Both);
		socket = null;
	}

	public void Send(byte[] data) {
		socket.Send(data);
	}

	private void ConnectCallback(IAsyncResult result) {
		if (socket.Connected) {
			socket.EndConnect(result);
			Debug.Log("Connected to the server");
			buffer = new byte[1024];
			socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceivedCallback), null);
		} else {
			Debug.Log("Could not connect to the server!");
		}
	}

	private void ReceivedCallback(IAsyncResult result) {
		int receivedAmount = socket.EndReceive(result);
		byte[] packet = new byte[receivedAmount];
		Array.Copy(buffer, packet, packet.Length);

		// Handle received packet
		HandlePacket(packet, null);

		buffer = new byte[1024];
		socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceivedCallback), null);
	}

	private void HandlePacket(byte[] data, Socket clientSocket) {
		Debug.Log("Handle received data");

		PacketReader pr = new PacketReader(data);
		MessageType mt = (MessageType)pr.ReadUInt16();
		int clientID = pr.ReadUInt16();
		string clientName = pr.ReadString();

		switch (mt) {
			case MessageType.ConfirmConnect:
				_networkClientID = clientID;
				_networkClientName = clientName;
				Debug.Log("ClientID received with the value of " + _networkClientID + " with the name: " + _networkClientName);
				//AddPlayer(_networkClientID); // Add a player object for self
				NetworkManager.Instance().Enqueue(() => AddPlayer(_networkClientID));
				break;
			case MessageType.Connected:
				//AddPlayer(clientID); // Add a player object for the joined player
				NetworkManager.Instance().Enqueue(() => AddPlayer(clientID));
				Debug.Log("Client with id: " + clientID + " and name: " + clientName + " joined the network.");
				break;
			case MessageType.Text:
				string text = PacketHandler.Read<string>(pr, mt);
				Debug.Log("Received text: " + text);
				break;
			default:
				break;
		}

		if (mt == MessageType.Position) {
			UpdateNetworkTransforms(pr, clientID);
		}
	}

	private void UpdateNetworkTransforms(PacketReader pr, int clientID) {
		onNetworkUpdate(pr, clientID);
	}

	private List<NetworkTransform> networkTransforms = new List<NetworkTransform>();

	private void AddPlayer(int id) {
		// TODO: Add new player object
		GameObject newObj = (GameObject)Instantiate(Resources.Load("Player"), Vector3.zero, Quaternion.identity);
		NetworkTransform nTransform = newObj.GetComponent<NetworkTransform>();
		nTransform.SetupNetworkTransform(networkTransforms.Count, id);
		networkTransforms.Add(nTransform);
	}
}