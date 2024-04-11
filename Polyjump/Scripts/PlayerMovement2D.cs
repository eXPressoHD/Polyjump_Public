using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityStandardAssets.ImageEffects;

public class PlayerMovement2D : MonoBehaviour
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
    private Slider _boostSlider;
    [SerializeField]
    private const float _boostConstant = 1.1f;
    [SerializeField]
    private bool _boostLock = false;
    [SerializeField]
    private bool _isAtWall;

    private string _relevantDashPercentValue;

    private float _distanceToGround;
    private float _distanceToWallXPositive;
    private float _distanceToWallXNegative;
    private float _distanceToWallZ;
    private float _maxSpeed = 35f;
    private bool _canJump;
    private bool _jump; //Status while jumping
    private bool _canDash;
    private Rigidbody _rb;
    private GameObject _player;

    private const float DASH_WITHDRAW = 0.015f;
    private const float DASH_BEGIN_VALUE = 0.16f;

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

    /// <summary>
    /// Setup of all necessary fields
    /// </summary>
    void Start()
    {
        SetNormalTime();
        _isAtWall = IsAtWall();
        _boostLock = false;
        _speed = 15f;
        _relevantDashPercentValue = $"100";
        _jumpSpeed = 150f;
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
        _boostSlider = GameObject.FindGameObjectWithTag("BoostSlider").GetComponent<Slider>();
        _boostSlider.value = _dashProgress;

        _boostText = _boostSlider.GetComponentInChildren<Text>();
        SetCountText();
    }

    void SetNormalTime()
    {
        if (Time.timeScale != 1f)
        {
            Time.timeScale = 1f;
        }
    }


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

    private bool IsAtWall()
    {
        return Physics.Raycast(transform.position, Vector3.forward, _distanceToWallXPositive + 0.02f) && Physics.Raycast(transform.position, Vector3.forward, _distanceToWallXNegative + 0.02f);
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
    }

    /// <summary>
    /// Check each Frame if the player can jump 
    /// </summary>
	void Update()
    {

        if (Input.GetButton("Jump") && _canJump == true && IsGrounded() == true && !IsAtWall())
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
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(0.0f, 0.0f, moveHorizontal);
        _rb.AddForce(movement * _speed);


        if (_rb.velocity.magnitude > _maxSpeed)
        {
            _rb.velocity = _rb.velocity.normalized * _maxSpeed;
        }

        if (_jump == true)
        {
            _rb.AddForce(Vector3.up * _jumpSpeed);
        }

        if (Input.GetKey(KeyCode.LeftShift) && IsGrounded() == false && _dashProgress > 0 && !BoostLock)
        {
            _canDash = true;
            _dashProgress -= DASH_WITHDRAW;
            UpdateDashText();
            _boostSlider.value = _dashProgress;
            Debug.Log(_boostSlider.value);
        }
        else
        {
            _canDash = false;
        }

        UpdateBoostLockText();

        if (_canDash == true)
        {
            _rb.AddForce(Vector3.up + Vector3.forward * _boostConstant * Time.deltaTime, ForceMode.Impulse);
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