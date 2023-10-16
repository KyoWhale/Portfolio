using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialog/Dialog Actions/Teleport")]
public class DialogTeleportAction : DialogAction
{
    public Vector3 targetPosition;
    public override void Execute(DialogSpeaker speaker)
    {
        speaker.transform.position = targetPosition;
    }
}
