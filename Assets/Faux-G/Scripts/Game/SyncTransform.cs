using UnityEngine;
using System.Collections.Generic;

/**
 * This method defines the routines that synchronize transforms (position and rotation 
 * information) across the network. Interpolation and extrapolation logics are also 
 * defined within this class. Entities using SyncTransform must have a PhotonView that 
 * observes this script for the synchronization to work.
 */
public class SyncTransform : Photon.MonoBehaviour, IPunObservable {

	private struct PositionData {
		public int packetNum;
		public int timestampMs;
		public Vector3 position;

		public PositionData(int packetNum, int timestampMs, Vector3 position) {
			this.packetNum = packetNum;
			this.timestampMs = timestampMs;
			this.position = position;
		}
	}

	private struct RotationData {
		public int packetNum;
		public int timestampMs;
		public Quaternion rotation;

		public RotationData(int packetNum, int timestampMs, Quaternion rotation) {
			this.packetNum = packetNum;
			this.timestampMs = timestampMs;
			this.rotation = rotation;
		}
	}

	public List<Transform> positionTransforms;
	public List<Transform> rotationTransforms;
	public bool extrapolatePosition;
	public bool extrapolateRotation;

	// Sender parameters
	private int senderPacketNum;

	// Receiver parameters
	private List<LinkedList<PositionData>> positionBuffers;
	private List<LinkedList<RotationData>> rotationBuffers;
	private int receiverPacketNum;

	void Awake() {
		positionBuffers = new List<LinkedList<PositionData>>();
		rotationBuffers = new List<LinkedList<RotationData>>();

		foreach (Transform positionTransform in positionTransforms) {
			positionBuffers.Add(new LinkedList<PositionData>());
		}

		foreach (Transform rotationTransform in rotationTransforms) {
			rotationBuffers.Add(new LinkedList<RotationData>());
		}

		senderPacketNum = 0;
		receiverPacketNum = 0;
	}

	void Update() {
		if (photonView.isMine) {
			return;
		}

		// Calculate render timestamp based on base sync delay and full trip latency from remote to local
		int remoteToServerLatency = GameManagerBase.Instance.networkManager.GetPlayerLatency(photonView.owner);
		int serverToLocalLatency = GameManagerBase.Instance.networkManager.GetPlayerLatency(PhotonNetwork.player);
		int syncDelay = Utils.Network.BASE_SYNC_DELAY + remoteToServerLatency + serverToLocalLatency;
		int renderTimestamp = PhotonNetwork.ServerTimestamp - syncDelay;

		SyncTransformPositions(renderTimestamp);
		SyncTransformRotations(renderTimestamp);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			WritePhotonSerializeView(stream, info);
		} else {
			ReadPhotonSerializeView(stream, info);
		}
	}

	/**
	 * This method writes into the photon serialized stream.
	 * This method will be called PhotonNetwork.sendRateOnSerialize times per second on 
	 * the local instance of this photonView.
	 */
	private void WritePhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		stream.SendNext(senderPacketNum);
		stream.SendNext(PhotonNetwork.ServerTimestamp);

		foreach (Transform positionTransform in positionTransforms) {
			stream.SendNext(positionTransform.position);
		}

		foreach (Transform rotationTransform in rotationTransforms) {
			stream.SendNext(rotationTransform.rotation);
		}

		senderPacketNum++;
	}

	/**
	 * This method reads from the photon serialized stream.
	 * This method will be called PhotonNetwork.sendRateOnSerialize times per second on 
	 * the remote instances of this photonView.
	 */
	private void ReadPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		int packetNum = (int) stream.ReceiveNext();
		int timestampMs = (int) stream.ReceiveNext();

		if (packetNum < receiverPacketNum) {
			// Data are stale, discard them
			for (int i = 0; i < positionTransforms.Count; i++) {
				stream.ReceiveNext();
			}

			for (int i = 0; i < rotationTransforms.Count; i++) {
				stream.ReceiveNext();
			}
		} else {
			// Data are fresh, queue them
			for (int i = 0; i < positionTransforms.Count; i++) {
				Vector3 position = (Vector3) stream.ReceiveNext();

				positionBuffers[i].AddLast(new PositionData(packetNum, timestampMs, position));

				if (positionBuffers[i].Count > Utils.Network.SYNC_BUFFER_SIZE) {
					positionBuffers[i].RemoveFirst();
				}
			}

			for (int i = 0; i < rotationTransforms.Count; i++) {
				Quaternion rotation = (Quaternion) stream.ReceiveNext();

				rotationBuffers[i].AddLast(new RotationData(packetNum, timestampMs, rotation));

				if (rotationBuffers[i].Count > Utils.Network.SYNC_BUFFER_SIZE) {
					rotationBuffers[i].RemoveFirst();
				}
			}

			receiverPacketNum = packetNum;
		}
	}

	private void SyncTransformPositions(int renderTimestamp) {
		for (int i = 0; i < positionTransforms.Count; i++) {
			if (positionBuffers[i].Count == 0) {
				continue;
			}

			// Seek pivot node - the node which timestamp is right before the render timestamp
			LinkedListNode<PositionData> pivotNode = positionBuffers[i].Last;
			while (pivotNode.Previous != null && pivotNode.Value.timestampMs >= renderTimestamp) {
				pivotNode = pivotNode.Previous;
			}

			LinkedListNode<PositionData> interpolationNode = pivotNode.Next;
			LinkedListNode<PositionData> extrapolationNode = pivotNode.Previous;

			if (interpolationNode != null) {
				// Interpolate between pivotNode and interpolationNode
				positionTransforms[i].position = InterpolatePosition(
					new PositionData[] { pivotNode.Value, interpolationNode.Value }, renderTimestamp);
			} else if (extrapolationNode != null && extrapolatePosition) {
				// Extrapolate between extrapolationNode and pivotNode
				positionTransforms[i].position = ExtrapolatePosition(
					new PositionData[] { extrapolationNode.Value, pivotNode.Value }, renderTimestamp);
			}
		}
	}

	private void SyncTransformRotations(int renderTimestamp) {
		for (int i = 0; i < rotationTransforms.Count; i++) {
			if (rotationBuffers[i].Count == 0) {
				continue;
			}

			// Seek pivot node - the node which timestamp is right before the render timestamp
			LinkedListNode<RotationData> pivotNode = rotationBuffers[i].Last;
			while (pivotNode.Previous != null && pivotNode.Value.timestampMs >= renderTimestamp) {
				pivotNode = pivotNode.Previous;
			}

			LinkedListNode<RotationData> interpolationNode = pivotNode.Next;
			LinkedListNode<RotationData> extrapolationNode = pivotNode.Previous;

			if (interpolationNode != null) {
				// Interpolate between pivotNode and interpolationNode
				rotationTransforms[i].rotation = InterpolateRotation(
					new RotationData[] { pivotNode.Value, interpolationNode.Value }, renderTimestamp);
			} else if (extrapolationNode != null && extrapolateRotation) {
				// Extrapolate between extrapolationNode and pivotNode
				rotationTransforms[i].rotation = ExtrapolateRotation(
					new RotationData[] { extrapolationNode.Value, pivotNode.Value }, renderTimestamp);
			}
		}
	}

	private Vector3 InterpolatePosition(PositionData[] positionData, int targetTimestamp) {
		float interpolationFactor = (float) (targetTimestamp - positionData[0].timestampMs)
			/ (positionData[1].timestampMs - positionData[0].timestampMs);

		return Vector3.Lerp(positionData[0].position, positionData[1].position, interpolationFactor);
	}

	private Vector3 ExtrapolatePosition(PositionData[] positionData, int targetTimestamp) {
		float extrapolationFactor = (float) (targetTimestamp - positionData[0].timestampMs)
			/ (positionData[1].timestampMs - positionData[0].timestampMs);

		return Vector3.LerpUnclamped(positionData[0].position, positionData[1].position, extrapolationFactor);
	}

	private Quaternion InterpolateRotation(RotationData[] rotationData, int targetTimestamp) {
		float interpolationFactor = (float) (targetTimestamp - rotationData[0].timestampMs)
			/ (rotationData[1].timestampMs - rotationData[0].timestampMs);

		return Quaternion.Slerp(rotationData[0].rotation, rotationData[1].rotation, interpolationFactor);
	}

	private Quaternion ExtrapolateRotation(RotationData[] rotationData, int targetTimestamp) {
		float extrapolationFactor = (float) (targetTimestamp - rotationData[0].timestampMs)
			/ (rotationData[1].timestampMs - rotationData[0].timestampMs);

		return Quaternion.SlerpUnclamped(rotationData[0].rotation, rotationData[1].rotation, extrapolationFactor);
	}

}
