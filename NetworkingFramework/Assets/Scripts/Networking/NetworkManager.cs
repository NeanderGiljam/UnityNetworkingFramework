using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {

	#region SINGLETON
	// --------------- Singleton Pattern ---------------
	private static NetworkManager instance = null;
	public static NetworkManager _Instance { get { return instance; } }
	// --------------- Singleton Pattern ---------------
	#endregion

	public ExternalUnityMethodCaller _externalMethodCall { get; private set; }

	public delegate void OnNetworkUpdate(PacketReader pr, int clientID);
	public OnNetworkUpdate onNetworkUpdate;

	private Dictionary<int, NetworkTransform> networkTransforms = new Dictionary<int, NetworkTransform>();

	private void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		_externalMethodCall = gameObject.AddComponent<ExternalUnityMethodCaller>();
	}

	// TODO: Add update interval for performance
	public void UpdateNetworkTransforms(PacketReader pr, int clientID) {
		onNetworkUpdate(pr, clientID);
	}

	public void AddPlayer(int clientID) {
		if (!ClientExists(clientID)) {
			GameObject newObj = (GameObject)Instantiate(Resources.Load("Player"), Vector3.zero, Quaternion.identity);
			newObj.name = "Player (" + clientID + ")";
			NetworkTransform nTransform = newObj.GetComponent<NetworkTransform>();
			nTransform.SetupNetworkTransform(clientID);
			networkTransforms.Add(clientID, nTransform);
		}
	}

	public void RemovePlayer(int clientID) {
		
	}

	private bool ClientExists(int clientID) {
		if (networkTransforms.ContainsKey(clientID)) {
			Debug.LogWarning("Client with id: " + clientID + " already exists");
			return true;
		}
		return false;
	}	
}