using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 컨디션 목록
// 다른 퀘스트를 완수했는가?
    // 물체들을 다 부쉈는가? (하나의 서브 퀘스트 완료로 볼 수 있음)
// 해당 물품을 일정 개수 갖고 있는가?

public abstract class DialogCondition : ScriptableObject
{
    public abstract bool Execute();
}
