using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialog/Dialog Actions/Animation")]
public class DialogAnimationAction : DialogAction
{
    public string triggerName;

    public override void Execute(DialogSpeaker speaker)
    {
        Animator animator = speaker.GetComponent<Animator>();
        if (animator == null)
        {
            return;
        }

        animator.SetTrigger(triggerName);
    }
}
