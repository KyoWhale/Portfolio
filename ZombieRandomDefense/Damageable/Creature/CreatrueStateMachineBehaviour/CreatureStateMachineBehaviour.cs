using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

/*
SMB 주의사항
1. Mono Awake에서 SMB 참조를 얻기 불안정함
    MonoBehaviour 생성 시점 : GameObject 추가 시
    SMB 생성 시점 : Animator Awake 시
2. Transition Interruption
    A -> B Transition 진행 중에 C로 상태 변경되면,
    B Exit 이벤트는 호출되지 않음
https://jinhomang.tistory.com/118
*/

public class CreatureStateMachineBehaviour : StateMachineBehaviour
{
    public event Action enter;
    public event Action exit;

    public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
    {
        enter?.Invoke();
    }

    public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
    {
        exit?.Invoke();
    }
}
