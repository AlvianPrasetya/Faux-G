using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Detonator))]
[AddComponentMenu("Detonator/Sound")]
public class DetonatorSound : DetonatorComponent {

	public AudioClip[] nearSounds;
	public AudioClip[] farSounds;

	public float delay;
	public float nearBaseVolume;
	public float farBaseVolume;
	public float distanceThreshold; //threshold in m between playing nearSound and farSound
	public float minDistance;
	public float maxDistance;
	public AudioRolloffMode rolloffMode;

	private AudioSource soundComponent;

	override public void Init() {
		soundComponent = gameObject.AddComponent<AudioSource>();

		soundComponent.pitch = Time.timeScale;
		soundComponent.minDistance = minDistance;
		soundComponent.maxDistance = maxDistance;
		soundComponent.rolloffMode = rolloffMode;
		soundComponent.spatialBlend = 1.0f;
		soundComponent.spatialize = true;
	}
	
	override public void Explode() {
		if (detailThreshold > detail)
			return;

		StartCoroutine(WaitForExplosion());
	}

	public void Reset() {
	}

	private IEnumerator WaitForExplosion() {
		yield return new WaitForSeconds(delay);

		PlayExplosionSound();
	}

	private void PlayExplosionSound() {
		if (Vector3.SqrMagnitude(Camera.main.transform.position - transform.position)
			< distanceThreshold * distanceThreshold) {
			if (nearSounds.Length > 0) {
				soundComponent.volume = nearBaseVolume;
				soundComponent.PlayOneShot(nearSounds[Random.Range(0, nearSounds.Length)]);
			}
		} else {
			if (farSounds.Length > 0) {
				soundComponent.volume = farBaseVolume;
				soundComponent.PlayOneShot(farSounds[Random.Range(0, farSounds.Length)]);
			}
		}
	}

}