using UnityEngine;
using System.Collections.Generic;

public class SyncTransform : Photon.MonoBehaviour, IPunObservable {

	public List<Transform> positionTransforms;
	public List<Transform> rotationTransforms;
	
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
	
	private List<LinkedList<PositionData>> positionBuffers;
	private List<LinkedList<RotationData>> rotationBuffers;

	void Awake() {
		positionBuffers = new List<LinkedList<PositionData>>();
		rotationBuffers = new List<LinkedList<RotationData>>();

		foreach (Transform positionTransform in positionTransforms) {
			positionBuffers.Add(new LinkedList<PositionData>());
		}

		foreach (Transform rotationTransform in rotationTransforms) {
			rotationBuffers.Add(new LinkedList<RotationData>());
		}
	}

	void Update() {
		if (photonView.isMine) {
			return;
		}

		int renderTimestamp = PhotonNetwork.ServerTimestamp - Utils.SYNC_DELAY;
		SyncTransformPositions(renderTimestamp);
		SyncTransformRotations(renderTimestamp);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			foreach (Transform positionTransform in positionTransforms) {
				stream.SendNext(positionTransform.position);
			}

			foreach (Transform rotationTransform in rotationTransforms) {
				stream.SendNext(rotationTransform.rotation);
			}
		} else {
			int timestamp = PhotonNetwork.ServerTimestamp;

			for (int i = 0; i < positionTransforms.Count; i++) {
				Vector3 position = (Vector3) stream.ReceiveNext();
				positionBuffers[i].AddLast(new PositionData(timestamp, position));

				if (positionBuffers[i].Count > Utils.SYNC_BUFFER_SIZE) {
					positionBuffers[i].RemoveFirst();
				}
			}

			for (int i = 0; i < rotationTransforms.Count; i++) {
				Quaternion rotation = (Quaternion) stream.ReceiveNext();
				rotationBuffers[i].AddLast(new RotationData(timestamp, rotation));

				if (rotationBuffers[i].Count > Utils.SYNC_BUFFER_SIZE) {
					rotationBuffers[i].RemoveFirst();
				}
			}
		}
	}

	private void SyncTransformPositions(int renderTimestamp) {
		for (int i = 0; i < positionTransforms.Count; i++) {
			for (LinkedListNode<PositionData> currentNode = positionBuffers[i].Last;
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
							break;
						}

						int currentTimestamp = currentNode.Value.timestamp;
						Vector3 currentPosition = currentNode.Value.position;
						int previousTimestamp = previousNode.Value.timestamp;
						Vector3 previousPosition = previousNode.Value.position;

						Vector3 dPosition = (currentPosition - previousPosition)
							/ (currentTimestamp - previousTimestamp);
						positionTransforms[i].position = previousPosition
							+ dPosition * (renderTimestamp - previousTimestamp);
					} else {
						Logger.Log("Interpolate");
						// Interpolate between pair of data
						int currentTimestamp = currentNode.Value.timestamp;
						int nextTimestamp = nextNode.Value.timestamp;
						positionTransforms[i].position = Vector3.Lerp(
							currentNode.Value.position,
							nextNode.Value.position,
							(float) (renderTimestamp - currentTimestamp) / (nextTimestamp - currentTimestamp)
						);
					}

					break;
				}
			}
		}
	}

	private void SyncTransformRotations(int renderTimestamp) {
		for (int i = 0; i < rotationTransforms.Count; i++) {
			for (LinkedListNode<RotationData> currentNode = rotationBuffers[i].Last;
				currentNode != null;
				currentNode = currentNode.Previous) {
				if (currentNode.Value.timestamp < renderTimestamp) {
					LinkedListNode<RotationData> nextNode = currentNode.Next;
					if (nextNode == null) {
						// Extrapolate from pair of data
						LinkedListNode<RotationData> previousNode = currentNode.Previous;
						if (previousNode == null) {
							// Previous node is null, not enough information to extrapolate
							break;
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
						rotationTransforms[i].rotation = Quaternion.AngleAxis(deltaAngle, deltaAxis) * previousRotation;
					} else {
						// Interpolate between pair of data
						int currentTimestamp = currentNode.Value.timestamp;
						int nextTimestamp = nextNode.Value.timestamp;
						rotationTransforms[i].rotation = Quaternion.Slerp(
							currentNode.Value.rotation,
							nextNode.Value.rotation,
							(float) (renderTimestamp - currentTimestamp) / (nextTimestamp - currentTimestamp)
						);
					}

					break;
				}
			}
		}
	}

}
