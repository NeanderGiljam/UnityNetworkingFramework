using UnityEngine;
using System.Collections;

public class CubeController : NetworkTransform {

	private float speed = 5f;

	private Vector3 pos;

	private void Update() {
		if (_client != null && _client._connected) {
			if (_isMine) {
				transform.Translate(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Time.deltaTime * speed, Space.World);
				_client.Send(PacketHandler.Create(MessageType.Position, _client._networkClientID, _client._networkClientName, transform.position));
			} else {
				transform.position = Vector3.Lerp(transform.position, pos, 0.5f);
			}
		}
	}

	public override void OnNetworkUpdate(PacketReader pr, int clientId) {
		base.OnNetworkUpdate(pr, clientId);

		if (!_isMine) {
			if (clientId == _networkClientID) {
				pos = PacketHandler.Read<Vector3>(pr, MessageType.Position);
				Debug.Log("New Pos: " + pos);
			}
		}
	}

}