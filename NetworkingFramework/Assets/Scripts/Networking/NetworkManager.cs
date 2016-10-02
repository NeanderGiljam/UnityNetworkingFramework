using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.ComponentModel;

public class NetworkManager : MonoBehaviour {

	public Queue<Action> executionQueue = new Queue<Action>();

	private void Update() {
		lock (executionQueue) {
			while (executionQueue.Count > 0) {
				executionQueue.Dequeue().Invoke();
			}
		}
	}

	public void Enqueue(IEnumerator action) {
		lock (executionQueue) {
			executionQueue.Enqueue(() => { StartCoroutine(action); });
		}
	}

	public void Enqueue(Action action) {
		Enqueue(ActionWrapper(action));
	}

	private IEnumerator ActionWrapper(Action action) {
		action();
		yield return null;
	}

	private static NetworkManager _instance = null;

	public static bool Exists() {
		return _instance != null;
	}

	public static NetworkManager Instance() {
		if (!Exists ()) {
			
		}
		return _instance;
	}

	private void Awake() {
		if (_instance == null) {
			_instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	private void OnDestroy() {
		_instance = null;
	}
}