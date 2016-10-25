using System;
using UnityEngine;
using NetworkUDP;

public class NetworkTransform : MonoBehaviour, INetworkTransform {

	public int _networkClientID { get; protected set; }
	public bool _isMine { get { return _networkClientID == _client._clientID; } }
	public NetworkClient _client { get; protected set; }

	public void SetupNetworkTransform(int networkClientID) {
		_client = FindObjectOfType<NetworkClient>();
		_networkClientID = networkClientID;

		NetworkManager._Instance.onNetworkUpdate += OnNetworkUpdate;
	}

	public virtual void OnNetworkUpdate(PacketReader pr, int clientId) {
		Debug.Log("Network update. My id: " + _networkClientID);
	}
}