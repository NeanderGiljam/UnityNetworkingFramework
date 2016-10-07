using System;
using UnityEngine;
using NetworkTCP;

public class NetworkTransform : MonoBehaviour, INetworkTransform {

	public int _localID { get; protected set; }
	public int _networkClientID { get; protected set; }
	public bool _isMine { get { return _networkClientID == _client._networkClientID; } }
	public NetworkClient _client { get; protected set; }

	public void SetupNetworkTransform(int localID, int networkClientID) {
		_client = FindObjectOfType<NetworkClient>();
		_localID = localID;
		_networkClientID = networkClientID;

		_client.onNetworkUpdate += OnNetworkUpdate;
	}

	public virtual void OnNetworkUpdate(PacketReader pr, int clientId) {
		Debug.Log("Network update. My id: " + _localID);
	}
}