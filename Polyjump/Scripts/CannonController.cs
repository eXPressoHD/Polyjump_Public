using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class CannonController : MonoBehaviour
{
    [SerializeField]
    private int speed;

    private float friction = 50;
    [SerializeField]
    private float lerpSpeed;

    private float xDegrees;
    private float yDegrees;

    Quaternion fromRotation;
    Quaternion toRotation;

    //Cannon variablen
    [SerializeField]
    private GameObject _playerBall;

    [SerializeField]
    public Transform shotPos;

    public float firePower;

    private GameObject smokeGameObject;

    [SerializeField]
    private float _minDegreeX;

    [SerializeField]
    private float _maxDegreeX;

    [SerializeField]
    private float _minDegreeY;

    [SerializeField]
    private float _maxDegreeY;

    [SerializeField]
    private GameObject _visualAimAssist;


    public Camera CannonCam
    {
        get { return camera; }
        set { camera = value; }
    }

    private bool controlsUnlocked;

    [SerializeField]
    private Camera camera;


    private GameObject mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Camera");
        smokeGameObject = gameObject.transform.GetChild(0).gameObject;
        controlsUnlocked = false;
        _visualAimAssist = gameObject.transform.GetChild(2).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerBall)
        {
         float moveHorizontal = 0;
         float moveVertical = 0;

         if (controlsUnlocked)
         {
            _visualAimAssist.SetActive(true);
            moveHorizontal = -Input.GetAxisRaw("Horizontal");
            moveVertical = -Input.GetAxisRaw("Vertical");
         }

         // Adjust these variables as needed
         float rotationSpeed = 90.0f;  // Controls the speed of rotation
         float friction = 5.0f;        // Adjust as needed
         float lerpSpeed = 5.0f;      // Adjust as needed

         xDegrees -= moveVertical * speed * friction * Time.deltaTime;
         yDegrees -= moveHorizontal * speed * friction * Time.deltaTime;

         if (yDegrees <= _maxDegreeY)
         {
            yDegrees = _maxDegreeY;
         }
         else if (yDegrees >= _minDegreeY)
         {
            yDegrees = _minDegreeY;
         }

         if (xDegrees >= _minDegreeX)
         {
            xDegrees = _minDegreeX;
         }
         else if (xDegrees <= _maxDegreeX)
         {
            xDegrees = _maxDegreeX;
         }

         // Rotate the object based on xDegrees and yDegrees
         Quaternion toRotation = Quaternion.Euler(xDegrees, yDegrees, 90);
         Quaternion fromRotation = transform.rotation;

         // Use Quaternion.RotateTowards to control the rotation direction
         transform.rotation = Quaternion.RotateTowards(fromRotation, toRotation, rotationSpeed * Time.deltaTime);
      }

        if (Input.GetButton("Jump"))
        {
            if (_playerBall != null)
            {
                FireCannon();
                Debug.Log("Fire");
            }
        }
    }

    public void FireCannon()
    {
        if (smokeGameObject != null)
        {
            smokeGameObject.GetComponent<ParticleSystem>().Play();
            StartCoroutine(FireDelay());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            camera.gameObject.SetActive(true);
            mainCamera.SetActive(false);
            other.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            other.gameObject.SetActive(false);
            _playerBall = other.gameObject;
            controlsUnlocked = true;
        }
    }

    IEnumerator FireDelay()
    {
        yield return new WaitForSeconds(.1f);

        firePower = 2000;
        try
        {
            _playerBall.transform.position = shotPos.position;
            _playerBall.GetComponent<Rigidbody>().isKinematic = false;
            _playerBall.gameObject.SetActive(true);
            mainCamera.gameObject.SetActive(true);
            camera.gameObject.SetActive(false);
            _playerBall.GetComponent<Rigidbody>().AddForce(shotPos.forward * firePower);
            _playerBall = null;
            controlsUnlocked = false;
            _visualAimAssist.SetActive(false);
        }
        catch (Exception ex)
        {

        }
    }
}
