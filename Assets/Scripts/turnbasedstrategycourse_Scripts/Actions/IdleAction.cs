using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class IdleAction : BaseAction
{
    // TODO 
    // 시야 시스템, 나중에 앞에 있는지 여부 

    int m_iDetectRange => m_StatSystem.m_Stat.m_iDetectRange;

    public override BaseAction TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        // 감지 범위 내의 적 유닛 탐색
        List<GridPosition> detectedPositions = GetValidActionGridPositionList();

        ActionStart(onActionComplete);

        if (detectedPositions.Count > 0)
        {
            // 가장 가까운 적의 GridPosition을 가져옴
            GridPosition targetGridPosition = LevelGrid.Instance.GetClosestTargetGridPosition(m_BaseObject.GetGridPosition(), detectedPositions);

            // 타겟 객체 캐싱 (추후 PurseAction에서 사용)
            BaseObject target = LevelGrid.Instance.GetUnitAtGridPosition(targetGridPosition);
            m_BaseObject.SetTarget(target);

            //Debug.Log($"Detected Closet Enemy : {target.name}, Pos : {targetGridPosition}");

            ActionComplete();
            return m_BaseObject.GetAction<ChaseAction>();
        }

        ActionComplete();
        return this;
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
                    if (!LevelGrid.Instance.HasEnemyAtGridPosition(testGridPosition, m_BaseObject))
                        continue;

                    if (!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition))
                    {
                        continue;
                    }

                    if (!Pathfinding.Instance.HasPath(unitGridPosition, testGridPosition))
                    {
                        continue;
                    }

                    int pathfindingDistanceMultiplier = 10;
                    if (Pathfinding.Instance.GetPathLength(unitGridPosition, testGridPosition) > m_iDetectRange * pathfindingDistanceMultiplier)
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

}
