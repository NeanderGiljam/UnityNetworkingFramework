using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace NetworkUDP {
	public class NetworkClient : MonoBehaviour {

		public bool _connectionPending { get; private set; }
		public bool _connected { get; private set; }
		public IPEndPoint _serverIpEndPoint { get; private set; }

		public int _clientID { get; private set; }
		public string _clientName { get; private set; }

		//private string serverIp = "127.0.0.1";
		private int port = 6556;
		private UdpClient client;

		public void StartClient(string serverIp, int port, string clientName = "") {
			_connectionPending = true;
			// TODO: Start connection timeout timer timer

			_clientID = 0;
			_clientName = clientName;

			_serverIpEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), port);
			client = new UdpClient(0);

			SendData(PacketHandler.Create(MessageType.Connect, _clientID), _serverIpEndPoint);

			try {
				client.BeginReceive(new AsyncCallback(ReceivedCallback), null);
			} catch (Exception e) {
				Debug.Log("Could not start receiving data. Message: " + e.Message);
			}
		}

		public bool SendData(byte[] packetData, IPEndPoint toIpEndPoint) {
			try {
				client.Send(packetData, packetData.Length, toIpEndPoint);
				return true;
			} catch (Exception e) {
				Debug.Log("Error. Message: " + e.Message);
				return false;
			}
		}

		private void ReceivedCallback(IAsyncResult result) {
			IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);
			byte[] receivedData = client.EndReceive(result, ref remoteIpEndPoint);

			client.BeginReceive(new AsyncCallback(ReceivedCallback), null);
			HandlePacket(receivedData);
		}

		private void HandlePacket(byte[] data) {
			PacketReader pr = new PacketReader(data);
			MessageType mt = (MessageType)pr.ReadUInt16();
			int clientID = pr.ReadUInt16();

			switch (mt) {
				case MessageType.ConnectResponse:
					_connected = true;
					_connectionPending = false;
					_clientID = clientID;

					NetworkManager._Instance._externalMethodCall.Enqueue(() => NetworkManager._Instance.AddPlayer(_clientID));
					Debug.Log("Received id: " + _clientID + " and my name is: " + _clientName);
					SendData(PacketHandler.Create(MessageType.GetPlayers, _clientID), _serverIpEndPoint);
					break;
				case MessageType.DisconnectResponse:
					_connected = false;
					break;

				case MessageType.UserConnected:
					if (clientID != _clientID) {
						NetworkManager._Instance._externalMethodCall.Enqueue(() => NetworkManager._Instance.AddPlayer(clientID));
						Debug.Log("Client with id: " + clientID + " joined the network.");
					}
					break;
				case MessageType.UserDisconnected:
					NetworkManager._Instance._externalMethodCall.Enqueue(() => NetworkManager._Instance.RemovePlayer(clientID));
					Debug.Log("Client with id: " + clientID + " disconnected.");
					break;

				case MessageType.GetPlayersResponse:
					Debug.Log("Got players response");

					int dataAmount = pr.ReadUInt16();
					byte[] idData = pr.ReadBytes(dataAmount);
					int[] playerIDs = idData.ToIntArray();

					for (int i = 0; i < playerIDs.Length; i++) {
						if (playerIDs[i] != _clientID) {
							int id = playerIDs[i];
							NetworkManager._Instance._externalMethodCall.Enqueue(() => NetworkManager._Instance.AddPlayer(id));
						}
					}
					break;

				case MessageType.Position:
					NetworkManager._Instance.UpdateNetworkTransforms(pr, clientID);
					break;
				case MessageType.Rotation:
					NetworkManager._Instance.UpdateNetworkTransforms(pr, clientID);
					break;
				default:
					break;
			}
		}

		private void OnApplicationQuit() {
			client.Close();
		}
	}
}

public static class Extensions {
	public static int[] ToIntArray(this byte[] intArrayData) {
		int[] intArray = new int[intArrayData.Length / 4];
		for (int i = 0; i < intArrayData.Length; i += 4) {
			intArray[i / 4] = BitConverter.ToInt32(intArrayData, i);
		}
		return intArray;
	}
}