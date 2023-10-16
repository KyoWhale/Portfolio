using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialog/Dialog Conditions/Put Item")]
public class DialogItemRequireCondition : DialogCondition
{
    public CollectableData requireItem;
    public int requireAmount;

    public override bool Execute()
    {
        return requireItem.amount >= requireAmount;
    }
}
