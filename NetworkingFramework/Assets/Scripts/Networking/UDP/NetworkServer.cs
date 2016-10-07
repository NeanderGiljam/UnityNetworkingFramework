using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace NetworkUDP {
	public class NetworkServer : MonoBehaviour {

		private int port = 6556;
		private UdpClient server;

		private Dictionary<IPEndPoint, ClientData> clients = new Dictionary<IPEndPoint, ClientData>();

		private void Awake() {
			StartServer(port);
		}

		public void StartServer(int port) {
			server = new UdpClient(port);

			Debug.Log("Started server on port: " + port);

			try {
				server.BeginReceive(new AsyncCallback(ReceivedCallback), null);
			} catch (Exception e) {
				Debug.Log("Could not start receiving data. Message: " + e.Message);
			}
		}

		private void ReceivedCallback(IAsyncResult result) {
			IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);
			byte[] receivedData = server.EndReceive(result, ref remoteIpEndPoint); // Outputs the ipEndpointData of the client who send it
			
			server.BeginReceive(new AsyncCallback(ReceivedCallback), null);

			HandlePacket(receivedData, remoteIpEndPoint);
		}

		private void HandlePacket(byte[] data, IPEndPoint remoteIpEndPoint) {
			PacketReader pr = new PacketReader(data);
			MessageType mt = (MessageType)pr.ReadUInt16();
			int clientID = pr.ReadUInt16();
			string clientName = pr.ReadString();

			switch (mt) {
				case MessageType.Connect:
					ClientData clientData = ConnectNewClient(remoteIpEndPoint, clientName);
					if (clientData != null) {
						clients.Add(remoteIpEndPoint, clientData);
						SendData(PacketHandler.Create(MessageType.ConnectResponse, clientData.clientID, clientData.clientName), remoteIpEndPoint);
						Debug.Log("New client connected. Client count: " + clients.Count);
						// TODO: Broadcast that a new client connected
						BroadcastMessage(PacketHandler.Create(MessageType.UserConnected, clientData.clientID, clientData.clientName), remoteIpEndPoint);
					}
					break;
				case MessageType.Disconnect:
					bool disconnected = DisconnectClient(remoteIpEndPoint);
					if (disconnected) {
						SendData(PacketHandler.Create(MessageType.DisconnectResponse, clientID, clientName), remoteIpEndPoint);
						BroadcastMessage(PacketHandler.Create(MessageType.UserDisconnected, clientID, clientName), remoteIpEndPoint);
					}
					break;
				default:
					// Broadcast message to all clients
					if (mt == MessageType.Text) { // TODO: Remove this is test
						string message = pr.ReadString();
						Debug.Log("Message received on server: " + message + ", from client: " + clientID + " aka: " + clientName);
					}
					break;
			}
		}

		private void BroadcastMessage(byte[] message, IPEndPoint sender) {
			
		}

		private bool SendData(byte[] packetData, IPEndPoint toIpEndPoint) {
			try {
				server.Send(packetData, packetData.Length, toIpEndPoint);
				return true;
			} catch (Exception e) {
				Debug.Log("Error. Message: " + e.Message);
				return false;
			}
		}

		private void OnApplicationQuit() {
			server.Close();
		}

		private ClientData ConnectNewClient(IPEndPoint clientIpEndpoint, string clientName) {
			if (!clients.ContainsKey(clientIpEndpoint)) {
				return new ClientData(clients.Count, clientName, clientIpEndpoint);
			}
			return null;
		}

		private bool DisconnectClient(IPEndPoint clientIpEndpoint) {
			if (clients.ContainsKey(clientIpEndpoint)) {
				clients.Remove(clientIpEndpoint);
				return true;
			}
			return false;
		}
	}

	public class ClientData {

		public int clientID; // Assigned by server
		public string clientName; // Assigned by user
		public IPEndPoint clientIpEndpoint;

		public ClientData(int clientID, string clientName, IPEndPoint clientIpEndpoint) {
			this.clientID = clientID;
			this.clientName = clientName;
			this.clientIpEndpoint = clientIpEndpoint;
		}

	}
}