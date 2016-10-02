using UnityEngine;
using System;
using System.IO;

public static class PacketHandler {

	public static byte[] Create(MessageType messageType, int sourceClientID, string sourceClientName) {
		PacketWriter pw = new PacketWriter();
		pw.Write((ushort)messageType);
		pw.Write((ushort)sourceClientID);
		pw.Write(sourceClientName);

		return pw.GetBytes();
	}
	public static byte[] Create(MessageType messageType, int sourceClientID, string sourceClientName, byte[] data) {
		PacketWriter pw = new PacketWriter();
		pw.Write((ushort)messageType);
		pw.Write((ushort)sourceClientID);
		pw.Write(sourceClientName);
		pw.Write(data);

		return pw.GetBytes();
	}
	public static byte[] Create(MessageType messageType, int sourceClientID, string sourceClientName, string text) {
		PacketWriter pw = new PacketWriter();
		pw.Write((ushort)messageType);
		pw.Write((ushort)sourceClientID);
		pw.Write(sourceClientName);
		pw.Write(text);

		return pw.GetBytes();
	}
	public static byte[] Create(MessageType messageType, int sourceClientID, string sourceClientName, Vector2 vector) {
		PacketWriter pw = new PacketWriter();
		pw.Write((ushort)messageType);
		pw.Write((ushort)sourceClientID);
		pw.Write(sourceClientName);
		pw.Write(vector);

		return pw.GetBytes();
	}
	public static byte[] Create(MessageType messageType, int sourceClientID, string sourceClientName, Vector3 vector) {
		PacketWriter pw = new PacketWriter();
		pw.Write((ushort)messageType);
		pw.Write((ushort)sourceClientID);
		pw.Write(sourceClientName);
		pw.Write(vector);

		return pw.GetBytes();
	}

	public static T Read<T>(PacketReader pr, MessageType messageType) {
		object o = null;
		switch (messageType) {
			case MessageType.Text:
				o = pr.ReadString();
				break;
			case MessageType.Position:
				o = pr.ReadVector3();
				break;
			case MessageType.Rotation:
				o = pr.ReadVector3();
				break;
			default:
				throw new NotImplementedException();
		}
		return (T)Convert.ChangeType(o, typeof(T));
	}

}

class PacketWriter : BinaryWriter {

	private MemoryStream ms;

	public PacketWriter() : base() {
		ms = new MemoryStream();
		OutStream = ms;
	}

	public void Write(Vector3 vector) {
		Write(vector.x);
		Write(vector.y);
		Write(vector.z);
	}

	public byte[] GetBytes() {
		Close();
		byte[] data = ms.ToArray();
		return data;
	}
}

public class PacketReader : BinaryReader {

	public PacketReader(byte[] data) : base (new MemoryStream(data)) {
		
	}

	public Vector3 ReadVector3() {
		float x = ReadSingle();
		float y = ReadSingle();
		float z = ReadSingle();

		return new Vector3(x, y, z);
	}

}