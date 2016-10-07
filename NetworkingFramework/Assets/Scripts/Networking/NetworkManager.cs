using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {

	#region SINGLETON
	// --------------- Singleton Pattern ---------------
	private static NetworkManager instance;
	public static NetworkManager _Instance
	{
		get
		{
			instance = FindObjectOfType<NetworkManager>();
			if (instance == null) {
				instance = new GameObject("NetworkManager").AddComponent<NetworkManager>();
			}
			return instance;
		}
	}
	// --------------- Singleton Pattern ---------------
	#endregion

	public ExternalUnityMethodCaller _externalMethodCall { get; private set; }

	public delegate void OnNetworkUpdate(PacketReader pr, int clientID);
	public OnNetworkUpdate onNetworkUpdate;

	private List<NetworkTransform> networkTransforms = new List<NetworkTransform>();

	private void Awake() {
		_externalMethodCall = gameObject.AddComponent<ExternalUnityMethodCaller>();
	}

	// TODO: Add update interval for performance
	public void UpdateNetworkTransforms(PacketReader pr, int clientID) {
		onNetworkUpdate(pr, clientID);
	}

	public void AddPlayer(int clientID) {
		GameObject newObj = (GameObject)Instantiate(Resources.Load("Player"), Vector3.zero, Quaternion.identity);
		NetworkTransform nTransform = newObj.GetComponent<NetworkTransform>();
		nTransform.SetupNetworkTransform(networkTransforms.Count, clientID);
		networkTransforms.Add(nTransform);
	}

	public void RemovePlayer(int clientID) {
		
	}
}