using UnityEngine;
using System.Collections.Generic;

public class SyncTransform : Photon.MonoBehaviour, IPunObservable {
	
	public int bufferSize;
	
	private struct PositionData {
		public int timestamp;
		public Vector3 position;

		public PositionData(int timestamp, Vector3 position) {
			this.timestamp = timestamp;
			this.position = position;
		}
	}
	
	private struct RotationData {
		public int timestamp;
		public Quaternion rotation;

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

		int renderTimestamp = PhotonNetwork.ServerTimestamp - Utils.SERIALIZE_SYNC_DELAY;
		SyncPosition(renderTimestamp);
		SyncRotation(renderTimestamp);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			stream.SendNext(PhotonNetwork.ServerTimestamp);
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
		} else {
			int timestamp = (int) stream.ReceiveNext();
			Vector3 position = (Vector3) stream.ReceiveNext();
			Quaternion rotation = (Quaternion) stream.ReceiveNext();

			positionBuffer.AddLast(new PositionData(timestamp, position));
			rotationBuffer.AddLast(new RotationData(timestamp, rotation));

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
					Logger.Log("Extrapolate");
					// Extrapolate from pair of data
					LinkedListNode<PositionData> previousNode = currentNode.Previous;
					if (previousNode == null) {
						// Previous node is null, not enough information to extrapolate
						return;
					}

					int currentTimestamp = currentNode.Value.timestamp;
					Vector3 currentPosition = currentNode.Value.position;
					int previousTimestamp = previousNode.Value.timestamp;
					Vector3 previousPosition = previousNode.Value.position;

					Vector3 dPosition = (currentPosition - previousPosition)
						/ (currentTimestamp - previousTimestamp);
					transform.position = previousPosition
						+ dPosition * (renderTimestamp - previousTimestamp);
				} else {
					Logger.Log("Interpolate");
					// Interpolate between pair of data
					int currentTimestamp = currentNode.Value.timestamp;
					int nextTimestamp = nextNode.Value.timestamp;
					transform.position = Vector3.Lerp(
						currentNode.Value.position,
						nextNode.Value.position,
						(float) (renderTimestamp - currentTimestamp) / (nextTimestamp - currentTimestamp)
					);
				}

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
					Logger.Log("Extrapolate");
					// Extrapolate from pair of data
					LinkedListNode<RotationData> previousNode = currentNode.Previous;
					if (previousNode == null) {
						// Previous node is null, not enough information to extrapolate
						return;
					}

					int currentTimestamp = currentNode.Value.timestamp;
					Quaternion currentRotation = currentNode.Value.rotation;
					int previousTimestamp = previousNode.Value.timestamp;
					Quaternion previousRotation = previousNode.Value.rotation;

					Quaternion deltaRotation = currentRotation * Quaternion.Inverse(previousRotation);
					float deltaTime = (float) (renderTimestamp - previousTimestamp)
						/ (currentTimestamp - previousTimestamp);

					float deltaAngle;
					Vector3 deltaAxis;
					deltaRotation.ToAngleAxis(out deltaAngle, out deltaAxis);

					if (deltaAngle > 180.0f) {
						deltaAngle = deltaAngle - 360.0f;
					}
					deltaAngle = deltaAngle * deltaTime % 360.0f;
					transform.rotation = Quaternion.AngleAxis(deltaAngle, deltaAxis) * previousRotation;
				} else {
					Logger.Log("Interpolate");
					// Interpolate between pair of data
					int currentTimestamp = currentNode.Value.timestamp;
					int nextTimestamp = nextNode.Value.timestamp;
					transform.rotation = Quaternion.Slerp(
						currentNode.Value.rotation,
						nextNode.Value.rotation,
						(float) (renderTimestamp - currentTimestamp) / (nextTimestamp - currentTimestamp)
					);
				}

				return;
			}
		}
	}

}
