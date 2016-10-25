using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace NetworkTCP {
	public class NetworkClient : MonoBehaviour {

		public delegate void OnNetworkUpdate(PacketReader pr, int clientID);
		public OnNetworkUpdate onNetworkUpdate;

		public int _networkClientID { get { return networkClientID; } private set { networkClientID = value; } }
		private int networkClientID = -1;
		public string _networkClientName { get { return networkClientName; } private set { networkClientName = value; } }
		private string networkClientName = "";

		public bool _connected { get { return clientSocket != null ? clientSocket.Connected : false; } }

		private Socket clientSocket;
		private byte[] buffer;

		public void Connect(string ipAdress, int port) {
			if (clientSocket == null) {
				clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				ConfigureSocket(clientSocket);
			}
			clientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(ipAdress), port), new AsyncCallback(ConnectCallback), null);
		}

		private void ConfigureSocket(Socket socket) {
			socket.ExclusiveAddressUse = true;
			socket.NoDelay = true;
			socket.ReceiveBufferSize = 512;
			socket.ReceiveTimeout = 1000;
			socket.SendBufferSize = 512; // 8192
			socket.SendTimeout = 1000;
		}

		public void Disconnect() {
			clientSocket.Send(PacketHandler.Create(MessageType.Disconnect, _networkClientID, _networkClientName));
			clientSocket.Shutdown(SocketShutdown.Both);
			clientSocket = null;
		}

		public void Send(byte[] data) {
			clientSocket.Send(data);
		}

		private void ConnectCallback(IAsyncResult result) {
			if (clientSocket.Connected) {
				clientSocket.EndConnect(result);
				Debug.Log("Connected to the server");
				buffer = new byte[1024];
				clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceivedCallback), null);
			} else {
				Debug.Log("Could not connect to the server!");
			}
		}

		private void ReceivedCallback(IAsyncResult result) {
			int receivedAmount = clientSocket.EndReceive(result);
			byte[] packet = new byte[receivedAmount];
			Array.Copy(buffer, packet, packet.Length);

			// Handle received packet
			HandlePacket(packet, null);

			buffer = new byte[1024];
			clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceivedCallback), null);
		}

		private void HandlePacket(byte[] data, Socket clientSocket) {
			Debug.Log("Handle received data");

			PacketReader pr = new PacketReader(data);
			MessageType mt = (MessageType)pr.ReadUInt16();
			int clientID = pr.ReadUInt16();
			string clientName = pr.ReadString();

			switch (mt) {
				case MessageType.ConnectResponse:
					_networkClientID = clientID;
					_networkClientName = clientName;
					Debug.Log("ClientID received with the value of " + _networkClientID + " with the name: " + _networkClientName);
					//AddPlayer(_networkClientID); // Add a player object for self
					NetworkManager._Instance._externalMethodCall.Enqueue(() => AddPlayer(_networkClientID));
					break;
				case MessageType.UserConnected:
					//AddPlayer(clientID); // Add a player object for the joined player
					NetworkManager._Instance._externalMethodCall.Enqueue(() => AddPlayer(clientID));
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
			nTransform.SetupNetworkTransform(id);
			networkTransforms.Add(nTransform);
		}
	}
}