using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class NetworkClient : MonoBehaviour {

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
		socket.Send(PacketHandler.Create(MessageType.Disconnect));
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
		PacketReader pr = new PacketReader(data);
		MessageType mt = (MessageType)pr.ReadUInt16();
		switch (mt) {
			case MessageType.Text:
				string text = PacketHandler.Read<string>(pr, mt, data);
				Debug.Log("Received text: " + text);
				break;
			case MessageType.Position:
				Vector3 pos = PacketHandler.Read<Vector3>(pr, mt, data);
				Debug.Log("Received position: " + pos);
				break;
			case MessageType.Rotation:
				Vector3 rot = PacketHandler.Read<Vector3>(pr, mt, data);
				Debug.Log("Received rotation: " + rot);
				break;
			default:
				break;
		}
	}
}