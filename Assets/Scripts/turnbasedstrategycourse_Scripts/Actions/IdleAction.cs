using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class IdleAction : BaseAction
{
    int m_iDetectRange => m_StatSystem.m_Stat.m_iDetectRange;



    public override BaseAction TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        // 감지 범위 내의 적 유닛 탐색
        var (obj, pos) = LevelGrid.Instance.GetClosestTargetGridInfo(m_BaseObject.GetGridPosition(), GetValidActionGridPositionList());
        if (obj == null)
            return this;

        m_BaseObject.SetTarget(obj);

        ActionStart(onActionComplete);
        return m_BaseObject.GetAction<ChaseAction>();
    }

    public override string GetActionName()
    {
        return "Idle";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        throw new NotImplementedException();
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = m_BaseObject.GetGridPosition();

        for (int x = -m_iDetectRange; x <= m_iDetectRange; x++)
        {
            for (int z = -m_iDetectRange; z <= m_iDetectRange; z++)
            {
                for (int floor = -m_iDetectRange; floor <= m_iDetectRange; floor++)
                {
                    GridPosition offsetGridPosition = new GridPosition(x, z, floor);
                    GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                    {
                        continue;
                    }

                    if (unitGridPosition == testGridPosition)
                    {
                        // Same Grid Position where the unit is already at
                        continue;
                    }

                    // Detect Object
                    if (!LevelGrid.Instance.HasEnemyAtGridPosition(m_BaseObject.GetGridPosition(), testGridPosition))
                            continue;


                    if (!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition))
                    {
                        continue;
                    }

                    if (!Pathfinding.Instance.HasPath(unitGridPosition, testGridPosition))
                    {
                        continue;
                    }

                    // 너무 멀면 패스
                    int pathfindingDistanceMultiplier = 10;
                    if (Pathfinding.Instance.GetPathLength(unitGridPosition, testGridPosition) > 
                        m_iDetectRange * pathfindingDistanceMultiplier)
                    {
                        // Path length is too long
                        continue;
                    }

                    validGridPositionList.Add(testGridPosition);
                }
            }
        }

        return validGridPositionList;
    }

    public override void ClearAction(BaseAction TODOAction)
    {
        base.ClearAction(TODOAction);

        // 자리 변화가 생길 경우에만 예약 취소
        if (TODOAction is ChaseAction ||
           TODOAction is CommandMoveAction)
        {
            OnStartMoveGrid();
        }
    }
}
