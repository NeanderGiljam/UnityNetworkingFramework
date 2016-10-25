using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace NetworkTCP {
	public class NetworkServer : MonoBehaviour {

		protected byte[] buffer;
		protected Socket serverSocket;

		protected Dictionary<Socket, ClientSocket> clients = new Dictionary<Socket, ClientSocket>();
		protected Dictionary<Socket, ClientSocket> disconnectedClients = new Dictionary<Socket, ClientSocket>();

		private void Awake() {
			serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			ConfigureSocket(serverSocket);
			Bind(6556);
			Listen();
			Accept();
		}

		public void Bind(int port) {
			serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
			Debug.Log("Assigned port: " + ((IPEndPoint)serverSocket.LocalEndPoint).Port);
		}

		private void ConfigureSocket(Socket socket) {
			socket.ExclusiveAddressUse = true;
			socket.NoDelay = true;
			socket.ReceiveBufferSize = 512;
			socket.ReceiveTimeout = 1000;
			socket.SendBufferSize = 512; // 8192
			socket.SendTimeout = 1000;
		}

		public void Listen(int backlog = 500) {
			serverSocket.Listen(backlog);
		}

		public void Accept() {
			serverSocket.BeginAccept(new AsyncCallback(AcceptedCallback), null);
		}

		protected virtual void AcceptedCallback(IAsyncResult result) {
			Socket clientSocket = serverSocket.EndAccept(result);
			buffer = new byte[1024];
			clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceivedCallback), clientSocket);
			Accept();

			ConfirmAccepted(clientSocket);
		}

		protected virtual void ConfirmAccepted(Socket clientSocket) {
			int clientID = clients.Count;
			clients.Add(clientSocket, new ClientSocket(clientID, "", clientSocket));

			clientSocket.Send(PacketHandler.Create(MessageType.ConnectResponse, clients[clientSocket].clientID, clients[clientSocket].clientName));
			BroadcastMessage(PacketHandler.Create(MessageType.UserConnected, clients[clientSocket].clientID, clients[clientSocket].clientName), clientSocket);
		}

		protected virtual void ReceivedCallback(IAsyncResult result) {
			Socket clientSocket = (Socket)result.AsyncState;
			int receivedAmount = clientSocket.EndReceive(result);
			byte[] packet = new byte[receivedAmount];
			Array.Copy(buffer, packet, packet.Length);

			// Handle received packet
			HandlePacket(packet, clientSocket);

			buffer = new byte[1024];
			clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceivedCallback), clientSocket);
		}

		private void HandlePacket(byte[] data, Socket clientSocket) {
			PacketReader pr = new PacketReader(data);
			MessageType mt = (MessageType)pr.ReadUInt16();
			//int clientID = pr.ReadUInt16();
			//string clientName = pr.ReadString();

			switch (mt) {
				case MessageType.Disconnect:
					Debug.Log("Client with ip disconnected: " + clientSocket.RemoteEndPoint.ToString());
					ClientSocket cS = clients[clientSocket];
					clients.Remove(clientSocket);
					disconnectedClients.Add(clientSocket, cS);
					clientSocket.Shutdown(SocketShutdown.Both);
					// TODO: Broadcast to other clients that client has left
					BroadcastMessage(data, clientSocket);
					break;
				case MessageType.Text:
					BroadcastMessage(data, clientSocket);
					break;
				case MessageType.Position:
					BroadcastMessage(data, clientSocket);
					break;
			}
		}

		private void BroadcastMessage(byte[] data, Socket clientSocket) {
			// Adds data from the source socket
			foreach (KeyValuePair<Socket, ClientSocket> pair in clients) {
				if (pair.Key != clientSocket) {
					pair.Key.Send(data);
				}
			}
		}
	}

	public class ClientSocket {

		public int clientID; // Assigned by server
		public string clientName; // Assigned by user
		public Socket clientSocket;

		public ClientSocket(int clientID, string clientName, Socket clientSocket) {
			this.clientID = clientID;
			this.clientName = clientName;
			this.clientSocket = clientSocket;
		}

	}
}