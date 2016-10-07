﻿using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace NetworkUDP {
	public class NetworkClient : MonoBehaviour {

		public bool _connected { get; private set; }
		public IPEndPoint _serverIpEndPoint { get; private set; }

		public int _clientID { get; private set; }
		public string _clientName { get; private set; }

		private string serverIp = "127.0.0.1";
		private int port = 6556;
		private UdpClient client;

		public void StartClient(string serverIp, int port, string clientName = "") {
			_serverIpEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), port);
			client = new UdpClient(0);

			_connected = true;

			SendData(PacketHandler.Create(MessageType.Connect, -1, clientName), _serverIpEndPoint);

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
			//Debug.Log("Received data from: " + remoteIpEndPoint.Address + ":" + remoteIpEndPoint.Port);

			client.BeginReceive(new AsyncCallback(ReceivedCallback), null);
			HandlePacket(receivedData);
		}

		private void HandlePacket(byte[] data) {
			PacketReader pr = new PacketReader(data);
			MessageType mt = (MessageType)pr.ReadUInt16();
			int clientID = pr.ReadUInt16();
			string clientName = pr.ReadString();

			switch (mt) {
				case MessageType.ConnectResponse:
					_clientID = clientID;
					_clientName = clientName;
					NetworkManager._Instance._externalMethodCall.Enqueue(() => NetworkManager._Instance.AddPlayer(_clientID));
					Debug.Log("Received id: " + _clientID + " and my name is: " + _clientName);
					break;
				case MessageType.DisconnectResponse:
					_connected = false;
					break;

				case MessageType.UserConnected:
					NetworkManager._Instance._externalMethodCall.Enqueue(() => NetworkManager._Instance.AddPlayer(clientID));
					Debug.Log("Client with id: " + clientID + " and name: " + clientName + " joined the network.");
					break;
				case MessageType.UserDisconnected:
					NetworkManager._Instance._externalMethodCall.Enqueue(() => NetworkManager._Instance.RemovePlayer(clientID));
					Debug.Log("Client with id: " + clientID + " and name: " + clientName + " disconnected.");
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