using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSkill : ActiveSkill
{
    protected override void SetLauncherTransform(DamagerLauncher launcher, int index, float angle)
    {
        Vector3 newPosition = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
        launcher.transform.forward = newPosition;
        launcher.transform.localPosition = newPosition;
    }

    protected override void SetLauncherDetail(DamagerLauncher launcher)
    {
        
    }
}
