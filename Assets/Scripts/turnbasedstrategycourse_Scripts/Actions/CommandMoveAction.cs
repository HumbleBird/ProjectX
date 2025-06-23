using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CommandMoveAction : MoveAction
{
    protected override void Awake()
    {
        base.Awake();

        m_iMaxMoveDistance = m_StatSystem.m_Stat.m_iDefaultMoveRange;
        m_fMoveSpeed = m_StatSystem.m_Stat.m_fMoveSpeed;
    }

    public override BaseAction TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        // Find Path
        List<GridPosition> pathGridPositionList = Pathfinding.Instance.FindPath(m_BaseObject.GetGridPosition(), DestGirdPosition, out int pathLength);

        if (pathGridPositionList != null && pathGridPositionList.Count >= AVALIABLE_MOVE_GRID)
        {
            currentPositionIndex = 0;
            positionList = new List<Vector3>();

            if(PrevReservePosition != default)
                LevelGrid.Instance.SetReserveGridPosition(PrevReservePosition, false);

            PrevReservePosition = pathGridPositionList[1];

            LevelGrid.Instance.SetReserveGridPosition(pathGridPositionList[1], true);
            positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPositionList[1]));
            InvokeOnStartMoving();
            ActionStart(onActionComplete);
        }

        return this;
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = m_BaseObject.GetGridPosition();

        for (int x = -m_iMaxMoveDistance; x <= m_iMaxMoveDistance; x++)
        {
            for (int z = -m_iMaxMoveDistance; z <= m_iMaxMoveDistance; z++)
            {
                for (int floor = -m_iMaxMoveDistance; floor <= m_iMaxMoveDistance; floor++)
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
                    if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                        continue;

                    // Check Reverse Pos
                    if (LevelGrid.Instance.GetReservedGridPosition(testGridPosition))
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
                    if (Pathfinding.Instance.GetPathLength(unitGridPosition, testGridPosition) > m_iMaxMoveDistance * pathfindingDistanceMultiplier)
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
