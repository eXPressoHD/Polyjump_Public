using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class SteerZoneTrigger : MonoBehaviour
{
   private PlayerMovement _playerRef;
   private Camera _cameraRef;

   private void Start()
   {
      _playerRef = GameObject.Find("Player").GetComponent<PlayerMovement>();
      _cameraRef = Camera.main;

      if (_playerRef == null)
         Debug.LogError("PlayerMovement component not found on Player object");
   }

   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         _playerRef.IsInSteerZone = !_playerRef.IsInSteerZone;
         _cameraRef.GetComponent<SmoothFollow>().Height = _playerRef.IsInSteerZone ? 8.0f : 1.0f;
      }
   }
}
