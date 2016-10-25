using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;

namespace NetworkUDP {
	public class NetworkServer : MonoBehaviour {

		// TODO: Create max connections amount

		private int port = 6556;
		private UdpClient server;

		//private List<IPEndPoint> clients = new List<IPEndPoint>();
		private Dictionary<IPEndPoint, int> clients = new Dictionary<IPEndPoint, int>();

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

			switch (mt) {
				case MessageType.Connect:
					if (!ClientAlreadyConnected(remoteIpEndPoint)) {
						int newClientID = clients.Count;
						clients.Add(remoteIpEndPoint, newClientID);
						SendData(PacketHandler.Create(MessageType.ConnectResponse, newClientID), remoteIpEndPoint);
						// TODO: Broadcast that a new client connected
						BroadcastMessage(PacketHandler.Create(MessageType.UserConnected, newClientID), remoteIpEndPoint);
						Debug.Log("Player connected: " + newClientID + ", broadcasting to all connected clients: " + (clients.Count - 1));
					}
					break;
				case MessageType.Disconnect:
					bool disconnected = DisconnectClient(remoteIpEndPoint);
					if (disconnected) {
						SendData(PacketHandler.Create(MessageType.DisconnectResponse, clientID), remoteIpEndPoint);
						BroadcastMessage(PacketHandler.Create(MessageType.UserDisconnected, clientID), remoteIpEndPoint);
					}

					Debug.Log("Player disconnected: " + clientID);
					break;

				case MessageType.GetPlayers:
					byte[] playerIDs = new byte[clients.Values.Count * sizeof(int)];
					Buffer.BlockCopy(clients.Values.ToArray(), 0, playerIDs, 0, playerIDs.Length);

					SendData(PacketHandler.Create(MessageType.GetPlayersResponse, clientID, playerIDs.Length, playerIDs), remoteIpEndPoint);

					Debug.Log("Send get players response.");
					break;

				case MessageType.Position:
					BroadcastMessage(data, remoteIpEndPoint);
					break;
				default:
					// Broadcast message to all clients
					if (mt == MessageType.Text) { // TODO: Remove this is test
						string message = pr.ReadString();
						Debug.Log("Message received on server: " + message + ", from client: " + clientID);
					}
					break;
			}
		}

		private void BroadcastMessage(byte[] message, IPEndPoint sender) {
			foreach (KeyValuePair<IPEndPoint, int> p in clients) {
				if (p.Key != sender) {
					SendData(message, p.Key);
				}
			}
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

		private bool ClientAlreadyConnected(IPEndPoint clientIpEndpoint) {
			foreach (KeyValuePair<IPEndPoint, int> p in clients) {
				if (p.Key.Port == clientIpEndpoint.Port) {
					if (p.Key.Address == clientIpEndpoint.Address) {
						Debug.Log("Client already connected");
						return true;
					}
				}
			}
			return false;
		}

		private bool DisconnectClient(IPEndPoint clientIpEndpoint) {
			int i = clients.Count;

			while (i-- >= 0) {
				if (clients.ContainsKey(clientIpEndpoint)) {
					clients.Remove(clientIpEndpoint);
					return true;
				}
			}

			//if (clients.ContainsValue(clientIpEndpoint)) {
			//	clients.Remove(clientIpEndpoint);
			//	return true;
			//}
			return false;
		}
	}
}