using UnityEngine;
using System.Collections.Generic;

public class SyncTransform : Photon.MonoBehaviour, IPunObservable {

	public int renderDelay;
	public int bufferSize;
	
	private struct PositionData {
		public int timestamp;
		public Vector3 position;

		public PositionData(Vector3 position) {
			timestamp = PhotonNetwork.ServerTimestamp;
			this.position = position;
		}

		public PositionData(int timestamp, Vector3 position) {
			this.timestamp = timestamp;
			this.position = position;
		}
	}
	
	private struct RotationData {
		public int timestamp;
		public Quaternion rotation;

		public RotationData(Quaternion rotation) {
			timestamp = PhotonNetwork.ServerTimestamp;
			this.rotation = rotation;
		}

		public RotationData(int timestamp, Quaternion rotation) {
			this.timestamp = timestamp;
			this.rotation = rotation;
		}
	}

	private LinkedList<PositionData> positionBuffer;
	private LinkedList<RotationData> rotationBuffer;

	void Awake() {
		positionBuffer = new LinkedList<PositionData>();
		rotationBuffer = new LinkedList<RotationData>();
	}

	void Update() {
		if (photonView.isMine) {
			return;
		}
		
		int renderTimestamp = PhotonNetwork.ServerTimestamp - renderDelay;
		SyncPosition(renderTimestamp);
		SyncRotation(renderTimestamp);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			PositionData positionData = new PositionData(transform.position);
			stream.SendNext(positionData.timestamp);
			stream.SendNext(positionData.position);

			RotationData rotationData = new RotationData(transform.rotation);
			stream.SendNext(rotationData.timestamp);
			stream.SendNext(rotationData.rotation);
		} else {
			positionBuffer.AddLast(new PositionData(
				(int) stream.ReceiveNext(), 
				(Vector3) stream.ReceiveNext()
			));

			rotationBuffer.AddLast(new RotationData(
				(int) stream.ReceiveNext(), 
				(Quaternion) stream.ReceiveNext()
			));

			if (positionBuffer.Count > bufferSize) {
				positionBuffer.RemoveFirst();
			}

			if (rotationBuffer.Count > bufferSize) {
				rotationBuffer.RemoveFirst();
			}
		}
	}

	private void SyncPosition(int renderTimestamp) {
		for (LinkedListNode<PositionData> currentNode = positionBuffer.Last;
			currentNode != null;
			currentNode = currentNode.Previous) {
			if (currentNode.Value.timestamp < renderTimestamp) {
				LinkedListNode<PositionData> nextNode = currentNode.Next;
				if (nextNode == null) {
					continue;
				}

				int currentTimestamp = currentNode.Value.timestamp;
				int nextTimestamp = nextNode.Value.timestamp;
				transform.position = Vector3.Lerp(
					currentNode.Value.position,
					nextNode.Value.position,
					(float) (renderTimestamp - currentTimestamp) / (nextTimestamp - currentTimestamp)
				);
				return;
			}
		}
	}

	private void SyncRotation(int renderTimestamp) {
		for (LinkedListNode<RotationData> currentNode = rotationBuffer.Last;
			currentNode != null;
			currentNode = currentNode.Previous) {
			if (currentNode.Value.timestamp < renderTimestamp) {
				LinkedListNode<RotationData> nextNode = currentNode.Next;
				if (nextNode == null) {
					continue;
				}

				int currentTimestamp = currentNode.Value.timestamp;
				int nextTimestamp = nextNode.Value.timestamp;
				transform.rotation = Quaternion.Slerp(
					currentNode.Value.rotation,
					nextNode.Value.rotation,
					(float) (renderTimestamp - currentTimestamp) / (nextTimestamp - currentTimestamp)
				);
				return;
			}
		}
	}

}
