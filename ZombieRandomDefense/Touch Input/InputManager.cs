using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;

public class InputManager : Singleton<InputManager>
{
    // Press
    private bool[] m_press = new bool[2];
    private double m_startTime0;
    private double m_startTime1;

    // Tap
    private double[] m_tapTime = new double[2];

    // Hold
    private bool m_isHoldTriggered = false;
    private bool m_isDragging = false;   // 시작지점에서 멀어졌다가 시작지점으로 왔을 때를 처리
    private bool m_isDragCanceled = false;
    private bool m_isTwoFingerDrag = false;
    private bool m_isPinch = false;

    // Position
    private bool m_shouldInitStartPosition0 = true;
    private bool m_shouldInitStartPosition1 = true;
    private InputAction position0Action;
    private InputAction position1Action;
    private Vector2 m_startPosition0;
    private Vector2 m_startPosition1;
    private Vector2 m_position0;
    private Vector2 m_position1;
    private float m_previousPositionMagnitude = 0;

    // Delta
    private Vector2 m_delta0;
    private Vector2 m_delta1;
    private Vector2 m_totalDelta0;
    private Vector2 m_totalDelta1;

    // One Finger
    public event Action<Vector2> tap;               // world position
    public event Action<Vector2> hold;              // world position
    public event Action<Vector2, Vector2, Vector2> drag;    // start position, position, delta position
    public event Action<Vector2, Vector2> cancelDrag;       // start position, position
    public event Action<Vector2, Vector2> endDrag;          // start position, position

    // Two Fingers
    public event Action tapWithTwoFinger; // 두 손가락을 동시에 누르는 것에 초점, 위치는 중요하지 않음
    public event Action<Vector2> dragWithTwoFinger; // 두 손가락이 서로 같은 방향으로 이동해야하고, 매개변수는 delta
    public event Action endDragWithTwoFinger;
    public event Action<float> pinch; // 두 손가락을 동시에 오므리는 것에 초점, 양수는 확대, 음수는 축소
    public event Action endPinch;

    public const float MaxTapDuration = 0.2f;       // unity default 0.2
    public const float MaxTapSpacing = 0.35f;       // unity default 0.75
    public const float MaxTwoFingerSpacing = 0.1f;  // 두 손가락 뗄 때의 최대 시간 차이
    public const float HoldTime = 0.4f;             // unity default 0.4
    public const float MaxSpotRangeMag = 50f;
    public const float MinDotTwoFingerDrag = 0.8f; // 두 손가락 방향벡터의 내적 최소값
    public const float MaxDotTwoFingerPinch = -0.6f; // 두 손가락 오므리고 벌릴 때 최대값
    public const float MinDragSqrMag = 70f * 70f;
    public const float MinPinchSqrMag = MinDragSqrMag;

    private void Start()
    {
        // Press : for Tap action process and Position action process
        AddTouch0PressAction();
        AddTouch1PressAction();

        // Delta : for Position action process
        AddTouch0DeltaAction();
        AddTouch1DeltaAction();

        // Tap : for Tap, Double Tap, Two Finger Tap and Two Finger Double Tap
        AddTouch0TapAction();
        AddTouch1TapAction();

        // Hold
        AddTouch0HoldAction();

        // Position : for Drag and Pinch
        AddTouch0PositionAction();
        AddTouch1PositionAction();

        ClearTapData();
    }

    private void AddTouch0PressAction()
    {
        var press0Action = new InputAction(
            type: InputActionType.Button,
            binding: "<Touchscreen>/touch0/press"
        );
        press0Action.Enable();
        press0Action.performed += press =>
        {
            m_press[0] = true;
            m_shouldInitStartPosition0 = true;
            m_startTime0 = press.startTime;
            m_totalDelta0 = Vector2.zero;
            m_isHoldTriggered = false;
            m_isDragging = false;
            m_isDragCanceled = false;
        };
        press0Action.canceled += _ =>
        {
            m_press[0] = false;
            if (m_isDragging)
            {
                m_isDragging = false;
                m_isDragCanceled = true;
                EndDrag();
            }
        };
    }

    private void AddTouch1PressAction()
    {
        var press1Action = new InputAction(
            type: InputActionType.Button,
            binding: "<Touchscreen>/touch1/press"
        );
        press1Action.Enable();
        press1Action.performed += press =>
        {
            m_press[1] = true;
            m_shouldInitStartPosition1 = true;
            m_startTime1 = press.startTime;
            m_totalDelta1 = Vector2.zero;
            m_isTwoFingerDrag = false;
            m_isPinch = false;
        };
        press1Action.canceled += _ =>
        {
            m_press[1] = false;
            if (m_isTwoFingerDrag)
            {
                m_isTwoFingerDrag = false;
                EndDragWithTwoFinger();
            }
            else if (m_isPinch)
            {
                m_isPinch = false;
                m_previousPositionMagnitude = 0;
                EndPinch();
            }
        };
    }

    private void AddTouch0DeltaAction()
    {
        var delta0Action = new InputAction(
            type: InputActionType.Value,
            binding: "<Touchscreen>/touch0/delta"
        );
        delta0Action.Enable();
        delta0Action.performed += delta =>
        {
            m_delta0 = delta.ReadValue<Vector2>();
            m_totalDelta0 += m_delta0;
        };
    }

    private void AddTouch1DeltaAction()
    {
        var delta1Action = new InputAction(
            type: InputActionType.Value,
            binding: "<Touchscreen>/touch1/delta"
        );
        delta1Action.Enable();
        delta1Action.performed += delta =>
        {
            m_delta1 = delta.ReadValue<Vector2>();
            m_totalDelta1 += m_delta1;
        };
    }

    // touch0Tap 처리
    // 1. touch1이 탭 아님 touch0이 이후 탭 -> None
    // 2. 한 손가락 탭
    // 3. touch1이 먼저 탭 touch0이 이후 탭 -> Two Finger Double Tap || Two Tap || Double Tap || Tap and Tap
    // 4. touch0이 먼저 탭 touch1이 이후 탭 -> touch1Tap에서 처리하는 조건 (TFDT, TT), touch0 탭 시간 저장
    private void AddTouch0TapAction()
    {
        var tap0Action = new InputAction(
            type: InputActionType.Button,
            binding: "<Touchscreen>/touch0/tap"
        );
        tap0Action.Enable();
        tap0Action.performed += tap =>
        {
            m_tapTime[0] = tap.startTime;
            m_position0 = position0Action.ReadValue<Vector2>();

            if (m_press[1]) // m_press[1]이 tap이 될 수도 있지만 그 조건은 touch1Tap에서 처리함
            {
                return;
            }

            if (tap.startTime - m_tapTime[1] <= MaxTwoFingerSpacing) // 두 손가락 다 탭인지
            {
                TapWithTwoFinger();
                ClearTapData();
            }
            else
            {
                Tap();
                ClearTapData();
            }
        };
    }

    // touch1Tap 처리
    // X: 한 손 탭은 발동 안 됨(처음 손가락의 탭은 touch0Tap이 처리하기 때문)
    // 5. touch0이 탭 아님 touch1이 이후 탭 -> None
    // 6. touch0이 먼저 탭 touch1이 이후 탭 -> Two Finger Double Tap || Two Tap
    // 7. touch1이 먼저 탭 touch0이 이후 탭 -> touch0Tap에서 처리하는 조건 (TFDT, TT), touch1 탭 시간 저장
    private void AddTouch1TapAction()
    {
        var tap1Action = new InputAction(
            type: InputActionType.Button,
            binding: "<Touchscreen>/touch1/tap"
        );
        tap1Action.Enable();
        tap1Action.performed += tap =>
        {
            m_tapTime[1] = tap.startTime;

            if (m_press[0]) // m_press[0]이 tap이 될 수도 있지만 그 조건은 touch0Tap에서 처리함
            {
                return;
            }

            // touch1Tap Input Action이 performed됐다는 것은 다른 손가락도 이미 눌려있었다는 뜻이기에 한 손가락 제스쳐는 안 됨
            if (tap.startTime - m_tapTime[0] <= MaxTwoFingerSpacing) // 두 손가락 다 탭인지
            {
                TapWithTwoFinger();
                ClearTapData();
            }
        };
    }

    private void AddTouch0HoldAction()
    {
        var hold0Action = new InputAction(
            type: InputActionType.Button,
            binding: "<Touchscreen>/touch0/press",
            interactions: $"hold(duration={HoldTime})"
        );
        hold0Action.Enable();
        hold0Action.performed += hold =>
        {
            if (m_totalDelta0.magnitude <= MaxSpotRangeMag &&
                    m_press[1] == false &&
                    m_isDragging == false)
            {
                m_isHoldTriggered = true;
                Hold();
            }
        };
    }

    private void AddTouch0PositionAction()
    {
        position0Action = new InputAction(
            type: InputActionType.Value,
            binding: "<Touchscreen>/touch0/position"
        );
        position0Action.Enable();
        position0Action.performed += position =>
        {
            m_position0 = position.ReadValue<Vector2>();

            if (m_press[1])
            {
                if (m_isDragging)
                {
                    m_isDragging = false;
                    m_isDragCanceled = true;
                    CancelDrag();
                }
                return;
            }

            if (m_isDragging)
            {
                Drag();
                return;
            }

            if (m_shouldInitStartPosition0)
            {
                m_startPosition0 = position0Action.ReadValue<Vector2>();
                m_shouldInitStartPosition0 = false;
                return;
            }

            if (m_isHoldTriggered == false && m_isDragCanceled == false &&
                    (m_isDragging || m_totalDelta0.sqrMagnitude >= MinDragSqrMag))
            {
                m_startPosition0 = m_position0;
                m_isDragging = true; // 시작지점에서 멀어졌다가 시작지점으로 왔을 때를 처리
                return;
            }
        };
    }

    private void AddTouch1PositionAction()
    {
        position1Action = new InputAction(
            type: InputActionType.Value,
            binding: "<Touchscreen>/touch1/position"
        );
        position1Action.Enable();
        position1Action.performed += position =>
        {
            m_position1 = position.ReadValue<Vector2>();

            if (m_press[0] == false || m_press[1] == false) // press -> position event 순으로 호출되므로 press[1]이 false가 되는 프레임에도 position1이 호출됨
            {
                return;
            }

            if (m_isTwoFingerDrag)
            {
                DragWithTwoFinger();
                return;
            }
            else if (m_isPinch)
            {
                Pinch();
                return;
            }

            if (m_shouldInitStartPosition1)
            {
                m_startPosition1 = position1Action.ReadValue<Vector2>();
                m_shouldInitStartPosition1 = false;
                return;
            }

            if (m_totalDelta0.sqrMagnitude < MinPinchSqrMag ||
                m_totalDelta1.sqrMagnitude < MinPinchSqrMag)
            {
                return;
            }

            var direction0 = m_position0 - m_startPosition0;
            var direction1 = m_position1 - m_startPosition1;
            var dot = Vector2.Dot(direction0.normalized, direction1.normalized);
            if (dot >= MinDotTwoFingerDrag)
            {
                m_startPosition0 = m_position0;
                m_startPosition1 = m_position1;
                m_isDragCanceled = true;

                m_isTwoFingerDrag = true;
                DragWithTwoFinger();
            }
            else if (dot <= MaxDotTwoFingerPinch)
            {
                m_startPosition0 = m_position0;
                m_startPosition1 = m_position1;
                m_isDragCanceled = true;

                m_isPinch = true;
                m_previousPositionMagnitude = (m_position0 - m_position1).magnitude;
                Pinch();
            }
        };
    }

    private void ClearTapData()
    {
        m_tapTime[0] = 0;
        m_tapTime[1] = 0;
    }

#region Polling System

    // // private ReadOnlyArray<TouchControl> touches;
    // // private bool m_shouldProcessReleaseTouches = true;

    // // private float m_totalSqrMag = 0;
    // // private bool m_isDrag = false;
    // // private bool m_isStayInSpot = true;


    // // 고려해야할 사항이 많은 것부터 살펴보아야함
    // // Two Double Tap > Two Drag = Pinch > Two Tap
    // private void Update()
    // {
    //     if (Touchscreen.current == null)
    //     {
    //         return;
    //     }

    //     touches = Touchscreen.current.touches;
    //     if (touches.Count > 2 || touches.Count == 0) // 하나 혹은 두 개의 터치만 고려함
    //     {
    //         ReleaseTouches();
    //         return;
    //     }
        
    //     if (touches.Count == 1)
    //     {
    //         ProcessOneTouch();
    //     }
    //     else
    //     {
    //         ProcessTwoTouches();
    //     }

    //     m_shouldProcessReleaseTouches = true;
    // }

    // // Primary Process : Two Double Tap, Two Tap, Double Tap, Tap
    // // Secondary Process : Save previous data, Reset
    // private void ReleaseTouches()
    // {
    //     if (m_shouldProcessReleaseTouches == false)
    //     {
    //         return;
    //     }
    //     m_shouldProcessReleaseTouches = false;
        
        
    //     if (touches[0].tapCount)
    //     {
    //         Tap();
    //     }
    //     if (m_previousTouchCount == 2 && m_currentTouchCount == 1) // 둘 하나 터치
    //     {
    //         //    
    //     }
    //     else if (m_previousTouchCount == 2 && m_currentTouchCount == 2)
    //     {

    //     }
    //     else if (m_previousTouchCount == 1 && m_currentTouchCount == 2)
    //     {
    //         //
    //     }

    //     m_totalSqrMag = 0;
    //     m_isDrag = false;
    //     m_isStayInSpot = true;
    // }

    // private void ProcessOneTouch() // Process : Drag, Hold
    // {
    //     Vector2 startPosition = touches[0].startPosition.ReadValue();
    //     Vector2 currentPosition = touches[0].position.ReadValue();
    //     m_totalSqrMag = (startPosition - currentPosition).sqrMagnitude;
    //     double totalTimeSpent = Time.realtimeSinceStartupAsDouble - touches[0].startTime.ReadValue();

    //     if (totalTimeSpent < MaxStayOnDragStart && m_totalSqrMag >= DragMinimumSqrMag) // 제자리에서 빨리 벗어났다면 drag
    //     {
    //         m_isDrag = true;
    //     }

    //     if (m_isDrag) // 제자리에서 이미 빨리 벗어난 상태
    //     {
    //         Drag();
    //     }
    //     else if (m_totalSqrMag > MaxSpotTouchSqrMag) // 제자리를 벗어났다면 hold 아님
    //     {
    //         m_isStayInSpot = false;
    //     }
    //     else if (m_isStayInSpot && totalTimeSpent > HoldTime) // 제자리를 지키고 홀드 최소 시간을 넘기면
    //     {
    //         Hold();
    //     }
    // }

    // private void ProcessTwoTouches() // Process : Two Drag, Pinch
    // {
    //     if (m_isDrag)
    //     {
    //         CancelDrag();
    //         return;
    //     }

    //     Vector2 startPosition = touches[0].startPosition.ReadValue();
    //     Vector2 currentPosition = touches[0].position.ReadValue();
    //     m_totalSqrMag = (startPosition - currentPosition).sqrMagnitude;
    //     double totalTimeSpent = Time.realtimeSinceStartupAsDouble - touches[0].startTime.ReadValue();

    // }

    // private void Clear()
    // {
    //     m_totalSqrMag = 0;
    //     m_isDrag = false;
    //     m_isStayInSpot = true;
    // }
#endregion
#region Events
    private void Tap()
    {
        if (tap == null)
        {
            WorldCamera.instance.Click(m_position0);
        }
        else
        {
            tap.Invoke(m_position0);
        }
    }

    private void Drag()
    {
        if (drag == null)
        {
            WorldCamera.instance.UpdateMoveInput(m_startPosition0, m_position0, m_delta0);
        }
        else
        {
            drag.Invoke(m_startPosition0, m_position0, m_delta0);
        }
    }

    private void CancelDrag()
    {
        if (cancelDrag == null)
        {
            WorldCamera.instance.EndInput();
        }
        else
        {
            cancelDrag.Invoke(m_startPosition0, m_position0);
        }
    }

    private void EndDrag()
    {
        if (endDrag == null)
        {
            WorldCamera.instance.EndInput();
        }
        else
        {
            endDrag.Invoke(m_startPosition0, m_position0);
        }
    }

    private void Hold()
    {
        if (hold == null)
        {
            WorldCamera.instance.Hold(m_position0);
        }
        else
        {
            hold.Invoke(m_position0);
        }
    }

    private void TapWithTwoFinger()
    {
        if (tapWithTwoFinger == null)
        {
            WorldCamera.instance.StopCameraReact();
        }
        else
        {
            tapWithTwoFinger.Invoke();
        }
    }

    private void DragWithTwoFinger()
    {
        if (dragWithTwoFinger == null)
        {
            WorldCamera.instance.UpdateRotateInput(m_startPosition0, m_position0, m_delta0);
        }
        else
        {
            dragWithTwoFinger.Invoke(m_delta0);
        }
    }

    private void EndDragWithTwoFinger()
    {
        if (endDragWithTwoFinger == null)
        {
            WorldCamera.instance.EndInput();
        }
        else
        {
            endDragWithTwoFinger.Invoke();
        }
    }

    private void Pinch()
    {
        float totalMagnitude = (m_position0 - m_position1).magnitude;
        float difference = m_previousPositionMagnitude - totalMagnitude;
        m_previousPositionMagnitude = totalMagnitude;

        if (pinch == null)
        {
            WorldCamera.instance.UpdateZoomInput(difference);
        }
        else
        {
            pinch.Invoke(difference);
        }
    }

    private void EndPinch()
    {
        if (endPinch == null)
        {
            WorldCamera.instance.EndInput();
        }
        else
        {
            endPinch.Invoke();
        }
    }
#endregion
}
