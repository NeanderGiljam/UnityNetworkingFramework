using UnityEngine;
using System.Collections;

public interface INetworkTransform {

	void SetupNetworkTransform(int localID, int networkClientID);
	void OnNetworkUpdate(PacketReader pr, int clientID);

}