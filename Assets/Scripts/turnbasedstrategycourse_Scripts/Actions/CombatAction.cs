using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CombatAction : BaseAction
{
    public override string GetActionName()
    {
        throw new NotImplementedException();
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        throw new NotImplementedException();
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        throw new NotImplementedException();
    }

    public override BaseAction TakeAction(GridPosition gridPosition = default, Action onActionComplete = null)
    {
        throw new NotImplementedException();
    }
}
