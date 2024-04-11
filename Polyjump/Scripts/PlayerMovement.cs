using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

using UnityStandardAssets.ImageEffects;
using UnityStandardAssets.Utility;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{

   [SerializeField]
   private float _speed;
   [SerializeField]
   private float _jumpSpeed;
   [SerializeField]
   private float _dashProgress;
   [SerializeField]
   private int _count;
   [SerializeField]
   private int _maxNumber = 5;
   [SerializeField]
   private Text _countText;
   [SerializeField]
   private Text _boostText;
   [SerializeField]
   private UnityEngine.UI.Slider _boostSlider;
   [SerializeField]
   private const float _boostConstant = 1.1f;
   [SerializeField]
   private bool _boostLock = false;
   [SerializeField]
   private bool _isInCannon = false;
   [SerializeField]
   private float smoothMoveSpeed = 5f;

   public LayerMask _whatIsWall;

   private string _relevantDashPercentValue;

   private float _distanceToGround;
   private float _distanceToWallXPositive;
   private float _distanceToWallXNegative;
   private float _distanceToWallZ;
   private float _maxSpeed = 35f;
   public bool _canJump;
   [SerializeField]
   private bool _jump; //Status while jumping
   private bool _canDash;
   private Rigidbody _rb;
   private GameObject _player;
   private GameObject _windZone;
   [SerializeField]
   private bool _inSteerZone;

   private GameObject _mainCanvas;
   private GameObject _devPanel;

   public GameObject JetPack;

   private const float DASH_WITHDRAW = 0.015f;
   private const float DASH_BEGIN_VALUE = 0.16f;

   public bool IsAtWallProp;

   public bool IsAtWallBackwardsProp;

   public Vector3 Movement { get; set; }
   public float Vertical { get; set; }
   public float Horizontal { get; set; }

   public bool IsInWindZone { get; set; }


   private SmoothFollow smoothFollow;

   private UnityEngine.UI.Slider _devSpeedSlider;
   private Text _devSpeedTextValue;
   private UnityEngine.UI.Button _devSkipLevelButton;

   Camera cannonCam = null;
   private Camera cam;

   /// <summary>
   /// Actual Count of Gems
   /// </summary>
   public int Count
   {
      get { return _count; }
      set { _count = value; }
   }

   public bool BoostLock
   {
      get { return _boostLock; }
      set { _boostLock = value; }
   }



   public bool IsInSteerZone
   {
      get { return _inSteerZone; }
      set { _inSteerZone = value; }
   }

   [SerializeField]
   private float _currentSpeed;


   /// <summary>
   /// Setup of all necessary fields
   /// </summary>
   void Start()
   {
      SetNormalTime();
      _mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
      _boostLock = false;
      _speed = 115f; //100f
      _relevantDashPercentValue = $"100";
      _jumpSpeed = 250f;
      _player = GameObject.Find("Player");
      _player.GetComponent<Renderer>().material.color = PlayerPrefsX.GetColor("Color");
      _rb = GetComponent<Rigidbody>();
      _count = 0;
      _canJump = true;
      _jump = false;
      _distanceToWallXPositive = _player.GetComponent<Collider>().bounds.extents.x;
      _distanceToWallXNegative = _player.GetComponent<Collider>().bounds.extents.x * (-1);
      _distanceToGround = _player.GetComponent<Collider>().bounds.extents.y;
      _distanceToWallZ = _player.GetComponent<Collider>().bounds.extents.z;
      _canDash = false;
      _dashProgress = DASH_BEGIN_VALUE;
      _boostSlider = GameObject.FindGameObjectWithTag("BoostSlider").GetComponent<UnityEngine.UI.Slider>();
      _boostSlider.value = _dashProgress;
      cam = Camera.main;
      smoothFollow = cam.GetComponent<SmoothFollow>();
      JetPack = GameObject.Find("Jetpack");
      _devPanel = _mainCanvas.transform.Find("DevPanel").gameObject;
      _devSpeedTextValue = _devPanel.transform.Find("PlayerSpeedSlider").transform.GetChild(4).gameObject.GetComponent<Text>();
      _devSpeedSlider = _devPanel.transform.Find("PlayerSpeedSlider").GetComponent<UnityEngine.UI.Slider>();
      _devSkipLevelButton = _devPanel.transform.Find("ButtonSkipLevel").GetComponent<UnityEngine.UI.Button>();
      _inSteerZone = false;

      if (_devSpeedSlider != null)
      {
         _devSpeedSlider.onValueChanged.AddListener(ChangePlayerSpeed);
      }

      if (_devSkipLevelButton != null)
      {
         _devSkipLevelButton.onClick.AddListener(() =>
         {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
         });
      }

      try
      {
         cannonCam = GameObject.Find("CannonCamera").GetComponent<Camera>();
      }
      catch (Exception ex)
      {

      }
      _boostText = _boostSlider.GetComponentInChildren<Text>();
      SetCountText();
   }

   private void ChangePlayerSpeed(float speed)
   {
      _speed = speed;
      _devSpeedTextValue.text = $"{speed.ToString()}f";
   }

   void SetNormalTime()
   {
      if (Time.timeScale != 1f)
      {
         Time.timeScale = 1f;
      }
   }

   #region CollisionChecks

   /// <summary>
   /// If player doesn't collide it's in midair
   /// </summary>
   /// <param name="collisionInfo"></param>
   void OnCollisionExit(Collision collisionInfo)
   {
      _canJump = false;
      _jump = false;
   }

   /// <summary>
   /// If player detect a collision it can jump again
   /// </summary>
   /// <param name="collisionInfo"></param>
   void OnCollisionStay(Collision collisionInfo)
   {
      _canJump = true;
   }

   /// <summary>
   /// Raycast on Y-Axis which checks if player stays on a floor
   /// </summary>
   /// <returns></returns>
   private bool IsGrounded()
   {
      return Physics.Raycast(transform.position, -Vector3.up, _distanceToGround + 0.05f);
   }

   public float hitDistance;

   private bool IsAtWall()
   {
      RaycastHit hit;
      if (Physics.Raycast(transform.position, Vector3.forward, out hit, hitDistance, _whatIsWall))
      {
         Debug.DrawLine(transform.position, hit.transform.position);
         return true;
      }

      return false;
   }

   private bool IsAtWallBackwards()
   {
      RaycastHit hit;
      if (Physics.Raycast(transform.position, Vector3.back, out hit, hitDistance, _whatIsWall))
      {
         Debug.DrawLine(transform.position, hit.transform.position);
         return true;
      }

      return false;
   }

   private bool IsAtWallLeft()
   {
      RaycastHit hit;
      if (Physics.Raycast(transform.position, Vector3.left, out hit, hitDistance, _whatIsWall))
      {
         Debug.DrawLine(transform.position, hit.transform.position);
         return true;
      }

      return false;
   }

   private bool ÍsAtWallFrontLeft()
   {
      RaycastHit hit;
      if (Physics.Raycast(transform.position, (Vector3.forward + Vector3.left).normalized, out hit, hitDistance, _whatIsWall))
      {
         Debug.DrawLine(transform.position, hit.transform.position);
         return true;
      }

      return false;
   }

   private bool IsAtWallBackLeft()
   {
      RaycastHit hit;
      if (Physics.Raycast(transform.position, (Vector3.back + Vector3.left).normalized, out hit, hitDistance, _whatIsWall))
      {
         Debug.DrawLine(transform.position, hit.transform.position);
         return true;
      }

      return false;
   }

   private bool IsAtWallBackRight()
   {
      RaycastHit hit;
      if (Physics.Raycast(transform.position, (Vector3.back + Vector3.right).normalized, out hit, hitDistance, _whatIsWall))
      {
         Debug.DrawLine(transform.position, hit.transform.position);
         return true;
      }

      return false;
   }

   private bool IsAtWallFrontRight()
   {
      RaycastHit hit;
      if (Physics.Raycast(transform.position, (Vector3.forward + Vector3.right).normalized, out hit, hitDistance, _whatIsWall))
      {
         Debug.DrawLine(transform.position, hit.transform.position);
         return true;
      }

      return false;
   }

   private bool IsAtWallRight()
   {
      RaycastHit hit;
      if (Physics.Raycast(transform.position, Vector3.right, out hit, hitDistance, _whatIsWall))
      {
         Debug.DrawLine(transform.position, hit.transform.position);
         return true;
      }

      return false;
   }

   /// <summary>
   /// If player collides with TypeOf Gem this trigger will be activated and Gemcount goes up
   /// </summary>
   /// <param name="other"></param>
   void OnTriggerEnter(Collider other)
   {
      if (other.gameObject.tag == "Collectible")
      {
         //W = p * g / 100
         float percentBack = (20 * DASH_BEGIN_VALUE) / 100;

         _dashProgress += percentBack;
         if (_dashProgress > DASH_BEGIN_VALUE)
         {
            _dashProgress = DASH_BEGIN_VALUE;
         }
         _boostSlider.value = _dashProgress;
         UpdateDashText();
         other.gameObject.SetActive(false);
         _count += 1;
         SetCountText();
      }
      else if (other.gameObject.tag == "WindZone")
      {
         _windZone = other.gameObject;
         IsInWindZone = true;
      }
   }

   private void OnTriggerExit(Collider other)
   {
      if (other.gameObject.tag == "WindZone")
      {
         _windZone = other.gameObject;
         IsInWindZone = false;
      }
   }


   #endregion

   /// <summary>
   /// Check each Frame if the player can jump 
   /// </summary>
   void Update()
   {
      //TODO Remove devconsole
      if (Input.GetKeyDown(KeyCode.F1))
      {

         bool state = _devPanel.activeSelf ? true : false;

         UnityEngine.Cursor.visible = !state;

         _devPanel.SetActive(!state);
      }

      IsAtWallBackwardsProp = IsAtWallBackwards();
      IsAtWallProp = IsAtWall();



      if (Input.GetButton("Jump") && _canJump == true && IsGrounded() == true && !IsAtWall() && !IsAtWallBackwards() && !IsAtWallLeft() && !IsAtWallRight()
          && !IsAtWallBackLeft() && !IsAtWallBackRight() && !ÍsAtWallFrontLeft() && !IsAtWallFrontRight())
      {
         _jump = true;
      }
   }

   private void UpdateDashText()
   {
      if (_dashProgress >= DASH_BEGIN_VALUE)
      {
         _boostText.text = "Boost 100%";
         _relevantDashPercentValue = "100";
      }


      float newPercentValue = _dashProgress;
      float calculatedValue = newPercentValue / DASH_BEGIN_VALUE;

      string newValueText = calculatedValue.ToString();

      string[] parts = newValueText.Split(',');

      string relevantVal = String.Empty;
      if (parts.Length >= 2)
      {
         relevantVal = parts[1];

         _relevantDashPercentValue = relevantVal;
      }

      Debug.Log(relevantVal);

      if (relevantVal.Length > 1)
      {
         relevantVal = relevantVal.Substring(0, 2);
      }

      if (relevantVal == "")
      {
         relevantVal = "100";
      }

      _relevantDashPercentValue = relevantVal;
   }


   /// <summary>
   /// Player Movement
   /// </summary>
   void FixedUpdate()
   {
      if (!_inSteerZone)
      {
         _currentSpeed = _rb.velocity.magnitude;
         float moveHorizontal = Input.GetAxis("Horizontal");
         float moveVertical = Input.GetAxis("Vertical");

         // Smooth the input values
         float smoothMoveHorizontal = Mathf.Lerp(0, moveHorizontal, Time.deltaTime * smoothMoveSpeed);
         float smoothMoveVertical = Mathf.Lerp(0, moveVertical, Time.deltaTime * smoothMoveSpeed);

         Vector3 movement = new Vector3(smoothMoveHorizontal, 0.0f, smoothMoveVertical);

         Vertical = smoothMoveVertical;
         Horizontal = smoothMoveHorizontal;
         Movement = movement;

         // Apply velocity instead of adding force
         _rb.AddForce(movement * _speed);
      }
      else
      {
         // Horizontal control inside the steer zone, similar to the if block
         float moveHorizontal = Input.GetAxis("Horizontal");
         float moveVertical = Input.GetAxis("Vertical");

         // Smooth the input values
         float smoothMoveHorizontal = Mathf.Lerp(0, moveHorizontal, Time.deltaTime * smoothMoveSpeed);
         float smoothMoveVertical = Mathf.Lerp(0, moveVertical, Time.deltaTime * smoothMoveSpeed);

         Vector3 movement = new Vector3(smoothMoveHorizontal, 0.0f, smoothMoveVertical);

         Vertical = smoothMoveVertical;
         Horizontal = smoothMoveHorizontal;
         Movement = movement;

         // Apply velocity instead of adding force
         _rb.velocity = new Vector3(movement.x * _speed, _rb.velocity.y, _currentSpeed);
      }


      if (_rb.velocity.magnitude > _maxSpeed)
      {
         _rb.velocity = _rb.velocity.normalized * _maxSpeed;
      }

      if (_jump == true)
      {
         _rb.AddForce(Vector3.up * _jumpSpeed);
      }

      if (Input.GetButton("Boost") && IsGrounded() == false && _dashProgress > 0 && !BoostLock) //Controller: || Input.GetAxisRaw("Boost") != 0
      {
         if (JetPack != null)
         {
            JetPack.GetComponent<Jetpack>().JetpackOn();
         }

         _canDash = true;
         _dashProgress -= DASH_WITHDRAW;
         UpdateDashText();
         _boostSlider.value = _dashProgress;
         Debug.Log(_boostSlider.value);
      }
      else
      {
         _canDash = false;
         //JetPack.GetComponent<Jetpack>().JetpackOff();
      }

      UpdateBoostLockText();

      if (_canDash == true)
      {
         _rb.AddForce(Vector3.up + Vector3.forward * _boostConstant * Time.deltaTime, ForceMode.Impulse);
      }

      if (IsInWindZone)
      {
         _rb.AddForce(_windZone.GetComponent<WindArea>().Direction * _windZone.GetComponent<WindArea>().Strength);
      }
   }

   private void UpdateBoostLockText()
   {
      if (BoostLock)
      {
         //_boostText.text = $"Boost: {_relevantDashPercentValue}% <color=#ff0000>LOCKED</color>";
         _boostText.text = _dashProgress >= 0 ? $"Boost {_relevantDashPercentValue}% <color=#ff0000>LOCKED</color>" : $"Boost 0% <color=#ff0000>LOCKED</color>";
         Camera.main.GetComponent<VignetteAndChromaticAberration>().enabled = true;
      }
      else
      {
         //_boostText.text = $"Boost: {_relevantDashPercentValue}%";
         _boostText.text = _dashProgress >= 0 ? $"Boost {_relevantDashPercentValue}%" : $"Boost 0%";
         Camera.main.GetComponent<VignetteAndChromaticAberration>().enabled = false;
      }
   }

   /// <summary>
   /// Set/Reset Gemcount
   /// </summary>
   void SetCountText()
   {
      GameObject textGameObject = GameObject.FindGameObjectWithTag("CountText");
      _countText = textGameObject.GetComponent<Text>();
      _countText.text = $"Gems: {_count.ToString()} / {_maxNumber.ToString()}";
   }

}