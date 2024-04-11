using UnityEngine;
using System.Collections;

public class ShakingCamera : MonoBehaviour {

	public GameObject go;
	public float shake = 0;
	private float shakeAmount = 4f;
	private float decreaseFactor = 1.0f;

	void Start() {
	}

	void Update() {
		if (shake > 0) {
			go.transform.position = Random.insideUnitSphere * shakeAmount;
			shake -= Time.deltaTime * decreaseFactor;
		} else {
			shake = 0.0f;
		}
	}

}
