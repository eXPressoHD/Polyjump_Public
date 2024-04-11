using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetpack : MonoBehaviour
{
   private GameObject _player;
   private Rigidbody _playerRb;
   public int w;
   public ParticleSystem fireL, fireR;

   private void Start()
   {
      _player = GameObject.Find("Player");
      _playerRb = _player.GetComponent<Rigidbody>();

      fireL = this.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.GetComponent<ParticleSystem>();
      fireR = this.transform.GetChild(1).gameObject.transform.GetChild(2).gameObject.GetComponent<ParticleSystem>();
   }

   private void Update()
   {
      Quaternion targetRotation = Quaternion.identity;/*Quaternion.LookRotation(_player.GetComponent<PlayerMovement>().Movement);*/

      targetRotation = Quaternion.RotateTowards(
          _player.transform.rotation,
          targetRotation,
          180 * Time.fixedDeltaTime);

      gameObject.transform.rotation = new Quaternion(targetRotation.x, targetRotation.y, targetRotation.z, w);
   }

   public void JetpackOn()
   {
      fireL.Play();
      fireR.Play();
   }

   public void JetpackOff()
   {
      fireL.Stop();
      fireR.Stop();
   }
}
