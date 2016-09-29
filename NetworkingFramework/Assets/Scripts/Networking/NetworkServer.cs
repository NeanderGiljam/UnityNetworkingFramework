using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class NetworkServer : MonoBehaviour {

	protected byte[] buffer;
	protected Socket serverSocket;
	
	private void Awake() {
		serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		Bind(6556);
		Listen();
		Accept();
	}

	public void Bind(int port) {
		serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
		Debug.Log("Assigned port: " + ((IPEndPoint)serverSocket.LocalEndPoint).Port);
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
		switch (mt) {
			case MessageType.Disconnect:
				Debug.Log("Client with ip disconnected: " + clientSocket.RemoteEndPoint.ToString());
				// TODO: Remove client socket from any list
				clientSocket.Shutdown(SocketShutdown.Both);
				// TODO: Broadcast to other clients that client has left
				break;
			case MessageType.Text:
				string text = PacketHandler.Read<string>(pr, mt, data);
				Debug.Log("Received message type: " + mt + ", text: " + text);
				break;
			case MessageType.Position:
				Vector3 pos = PacketHandler.Read<Vector3>(pr, mt, data);
				Debug.Log("Received message type: " + mt + ", position: " + pos);
				break;
			case MessageType.Rotation:
				Vector3 rot = PacketHandler.Read<Vector3>(pr, mt, data);
				Debug.Log("Received message type: " + mt + ", rotation: " + rot);
				break;
			default:
				break;
		}
	}
}