using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("기본 속도")]
    public float moveSpeed = 4.0f;
    [Tooltip("질주 속도")]
    public float sprintSpeed = 10.67f;
    private float _SprintRatio;
    [Tooltip("앉기 속도 배수")]
    public float crouchSpeedMultiplier = 0.5f;

    [Header("Jump and Gravity")]
    [SerializeField] AnimationCurve jumpVelocity;
    [Tooltip("점프 높이")]
    public float _JumpHeight = 2.4f;
    [Tooltip("중력 계수")]
    public float _Gravity = -9.81f;

    [Header("Rotation and Camera")]
    [Range(0.01f, 1.00f)]
    [Tooltip("회전 속도")]
    public float rotationSmoothTime = 0.12f;
    [Tooltip("카메라 타겟")]
    [SerializeField] Transform _followTarget;
    [Range(15f, 75f)]
    [Tooltip("최대 위 보는 각도")]
    public float topPitchClamp = 45.0f;
    [Range(0f, -45f)]
    [Tooltip("최소 아래 보는 각도")]
    public float bottomPitchClamp = -20.0f;
    [Tooltip("카메라 위치 고정모드")]
    public bool isLockCameraPosition = false;
    private float _rotationVelocity;

    private Animator _animator;
    private CharacterController _controller;
    private PlayerInputAction _inputAction;
    private GameObject _mainCamera;

    private bool _isCrouch = false;
    private bool _isSprint = false;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private bool _isPressingMoveButton;
    private Vector2 _moveDelta;
    private float _pureSpeed = 0;
    private float _currentSpeed = 0;
    private float _currentJumpSpeed = 0;

    [SerializeField] float MoveIncreamentDuration = 0.75f;
    private float _remainIncreametalMove = 0;

    [SerializeField] float SprintChangeDuration = 0.5f;
    private float _remainSprintChangeTime = 0;

    [SerializeField] float JumpIncreamentDuration = 0.75f;
    private float _remainJumpTime = 0;

    [SerializeField] float PostMoveDuration = .75f;
    private float _remainMoveTime = 0;

    public UnityEvent OnDrink;
    public UnityEvent OnSmash;
    public UnityEvent OnInteract;
#endregion

#region UNITY_EVENTS
    private void Awake() 
    {
        if (_mainCamera == null) 
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();

        _SprintRatio = sprintSpeed / moveSpeed;
        _remainIncreametalMove = MoveIncreamentDuration;
        _remainMoveTime = PostMoveDuration;

        bottomPitchClamp = 360f + bottomPitchClamp;
        
        _inputAction = new PlayerInputAction();
        BindMoveInputAction();
        bool isTutorialEnd = true; // TODO: Tutorial End Event를 만들어야함
        if (isTutorialEnd)
        {
            BindCameraInputAction();
        }
    }

    private void OnEnable() 
    {
        _inputAction.Enable();
    }

    private void Update() 
    {
        if (_remainJumpTime > 0) 
        {
            UpdateJump();
        }

        if (_isPressingMoveButton) 
        {
            Rotate();
            AddMove();
        } 
        else if (_remainMoveTime > 0) 
        {
            AddRemainMove();
        }

        UpdateMove();
        UpdateProteinShakeTime();
    }

    private void OnDisable() 
    {
        _inputAction.Disable();
    }
#endregion

#region MOVE
    private void Rotate() 
    {
        Quaternion previousFollow = _followTarget.rotation;

        Vector3 forward = _mainCamera.transform.forward * _moveDelta.y;
        Vector3 right = _mainCamera.transform.right * _moveDelta.x;
        Vector3 direction = forward + right;

        float yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, yaw, ref _rotationVelocity, rotationSmoothTime);
        transform.rotation = Quaternion.Euler(0f, rotation, 0f);

        _followTarget.rotation = previousFollow;
    }

    private void AddMove()
    {
        if (_remainIncreametalMove > 0)
        {
            _remainIncreametalMove -= Time.deltaTime;
            _pureSpeed = Mathf.Lerp(moveSpeed, 0, _remainIncreametalMove / MoveIncreamentDuration);
        }

        _currentSpeed = MultiplyRunFactor(_pureSpeed);
    }

    private void AddRemainMove() 
    {
        _remainMoveTime -= Time.deltaTime;
        float decreasingSpeed = Mathf.Lerp(0, _pureSpeed, _remainMoveTime / PostMoveDuration);
        _currentSpeed = MultiplyRunFactor(decreasingSpeed);
    }

    private float MultiplyRunFactor(float speed)
    {
        if (_isCrouch)
        {
            return speed * crouchSpeedMultiplier;
        }

        float sprintSpeed = speed * (proteinShake.isShakeTime ? proteinShake.sprintSpeedMultiplierInCokeTime : _SprintRatio);
        float nonSprintSpeed = speed * (proteinShake.isShakeTime ? proteinShake.moveSpeedMultiplierInCokeTime : 1f);
        if (_isSprint)
        {
            if (_remainSprintChangeTime > 0)
            {
                _remainSprintChangeTime -= Time.deltaTime;
                speed = Mathf.Lerp(sprintSpeed, nonSprintSpeed, _remainSprintChangeTime / SprintChangeDuration);
            }
            else
            {
                return sprintSpeed;
            }
        }
        else
        {
            if (_remainSprintChangeTime > 0)
            {
                _remainSprintChangeTime -= Time.deltaTime;
                speed = Mathf.Lerp(nonSprintSpeed, sprintSpeed, _remainSprintChangeTime / SprintChangeDuration);
            }
            else
            {
                return nonSprintSpeed;
            }
        }

        return speed;
    }
#endregion
#region JUMP AND GRAVITY
    private void UpdateJump() 
    {
        _remainJumpTime -= Time.deltaTime;
        _currentJumpSpeed = _JumpHeight * jumpVelocity.Evaluate(_remainJumpTime) * (-_Gravity);

        if (_remainJumpTime < 0)
        {
            _currentJumpSpeed = 0;
        }
    }

    private void UpdateMove() 
    {
        Vector3 currentMovement = _currentSpeed * transform.forward;
        currentMovement.y = _currentJumpSpeed + _Gravity;
        currentMovement *= Time.deltaTime;
        _controller.Move(currentMovement);

        bool isLanding = Physics.Raycast(transform.position, Vector3.down, 1.2f);
        if (isLanding)
        {
            _animator.SetTrigger("isLanding");
        }
        _animator.SetBool("isFalling", !_controller.isGrounded);
        _animator.SetBool("isGrounded", _controller.isGrounded);
        _animator.SetFloat($"speed", _currentSpeed);
    }
#endregion

    private void UpdateProteinShakeTime() 
    {
        proteinShake.UpdateTime(Time.deltaTime);
    }

#region SUB
    private void BindMoveInputAction()
    {
        _inputAction.Camera.Look.performed += ctx => {
            var lookDelta = ctx.ReadValue<Vector2>();
        };

        _inputAction.Movement.Move.performed += ctx => { 
            Vector2 move = ctx.ReadValue<Vector2>();
            _moveDelta = move;
            _isPressingMoveButton = true; 
        };
        _inputAction.Movement.Move.canceled += ctx => {
            _isPressingMoveButton = false;
            _remainIncreametalMove = MoveIncreamentDuration;
            _remainMoveTime = PostMoveDuration;
        };

        _inputAction.Movement.Jump.performed += ctx => { 
            if (_controller.isGrounded == false) {
                return;
            }

            _currentJumpSpeed = 0;
            _remainJumpTime = JumpIncreamentDuration;

            _animator.SetTrigger("isJump");
            _animator.SetBool("isGrounded", false);
        };

        _inputAction.Movement.Crouch.performed += ctx => {
            _isCrouch = !_isCrouch;

            _animator.SetBool("isCrouch", _isCrouch);
        };

        _inputAction.Movement.Sprint.performed += ctx => {
            _isSprint = true;
            _remainSprintChangeTime = SprintChangeDuration;
        };
        _inputAction.Movement.Sprint.canceled += ctx => {
            _isSprint = false;
            _remainSprintChangeTime = SprintChangeDuration;
        };

        _inputAction.Interaction.Interact.performed += ctx => OnInteract.Invoke();
        _inputAction.Interaction.Drink.performed += ctx => OnDrink.Invoke();
        _inputAction.Interaction.Smash.performed += ctx => OnSmash.Invoke();
        _inputAction.Interaction.Inventory.performed += ctx => Debug.Log("Inventory performed");
    }

    void BindCameraInputAction()
    {
        _inputAction.Camera.Look.performed += ctx => {
            Vector2 look = ctx.ReadValue<Vector2>();

            _followTarget.rotation *= Quaternion.AngleAxis(look.x * 2f, Vector3.up);
            _followTarget.rotation *= Quaternion.AngleAxis(look.y * 2f, Vector3.right);

            Vector3 angles = _followTarget.localEulerAngles;
            angles.z = 0;
            if (angles.x > 180 && angles.x < bottomPitchClamp)
            {
                angles.x = bottomPitchClamp;
            }
            else if (angles.x < 180 && angles.x > topPitchClamp)
            {
                angles.x = topPitchClamp;
            }
            _followTarget.localEulerAngles = angles;
        };
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax) {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
#endregion
}
