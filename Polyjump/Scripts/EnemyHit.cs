using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityStandardAssets.ImageEffects;
using Slider = UnityEngine.UI.Slider;

public class EnemyHit : MonoBehaviour {

	public ParticleSystem fire;
	public ParticleSystem smoke;
	AudioSource src;
	public float radius = 5.0F;
	public float power = 100.0F;
	public Camera go;

	[SerializeField]
	private Slider _twirlSlider;
	[SerializeField]
	private float _sliderDuration = 3f;

	void Start() {
		src = GetComponent<AudioSource> ();
		go = Camera.main;

      _twirlSlider = GetSliderReference();

   }

   private Slider GetSliderReference()
   {
      return GameObject.FindGameObjectWithTag("MainCanvas").gameObject.transform.GetChild(17).gameObject.GetComponent<Slider>();
   }

   void OnTriggerEnter (Collider other)
	{
		Vector3 explosionPos = transform.position;
		if (other.gameObject.tag == "Player") {
			Collider[] colliders = Physics.OverlapSphere (explosionPos, radius);
			foreach (Collider hit in colliders) {
				if (!hit)
					continue;

				if (hit.GetComponent<Rigidbody> ())
					hit.GetComponent<Rigidbody> ().AddExplosionForce (power, explosionPos, radius, 3.0F);
				fire.Play ();
				smoke.Play ();
				src.Play ();
				go.GetComponent<Twirl> ().enabled = true;

				this.gameObject.GetComponent<MeshRenderer>().enabled = false;
				StartCoroutine (DecreaseSlider());
				
			}
		}
	}

   private IEnumerator DecreaseSlider()
   {
		_twirlSlider.gameObject.SetActive(true);
      float startTime = Time.time;
      float elapsedTime = 0f;
      float startValue = _twirlSlider.value;
      float endValue = 0f;

      while (elapsedTime < _sliderDuration)
      {
         elapsedTime = Time.time - startTime;
         float t = elapsedTime / _sliderDuration;
         _twirlSlider.value = Mathf.Lerp(startValue, endValue, t);
         yield return null;
      }

      _twirlSlider.value = endValue;
      go.GetComponent<Twirl>().enabled = false;
      _twirlSlider.gameObject.SetActive(false);
      Destroy(this, .1f);
   }
}
