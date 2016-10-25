using UnityEngine;
using System.Collections;

public interface INetworkTransform {

	void SetupNetworkTransform(int networkClientID);
	void OnNetworkUpdate(PacketReader pr, int clientID);

}