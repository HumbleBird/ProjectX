using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ChaseAction : MoveAction
{
    protected override void Awake()
    {
        base.Awake();

        m_iMaxMoveDistance = m_StatSystem.m_Stat.m_iChaseRange;
        m_fMoveSpeed = m_StatSystem.m_Stat.m_fChaseSpeed;
    }

    public override BaseAction TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        // <예외사항>
        // 타겟 사망
        // 타겟 없음
        // 타겟 거리가 멈
        if (m_Target == null ||
            m_Target.m_StatSystem.m_IsDead ||
            LevelGrid.Instance.IsTargeSoFarAtChase(m_BaseObject.GetGridPosition(), m_Target.GetGridPosition()))
        {
            m_Target = null;

            List<GridPosition> NewdetectedPositions = GetValidActionGridPositionList();

            if (NewdetectedPositions.Count > 0)
            {
                // 가장 가까운 적의 GridPosition을 가져옴
                GridPosition targetGridPosition =
                    LevelGrid.Instance.GetClosestTargetGridPosition(m_BaseObject.GetGridPosition(), NewdetectedPositions);
                BaseObject target = LevelGrid.Instance.GetUnitAtGridPosition(targetGridPosition);
                SetTarget(target);
            }
            else
                return m_BaseObject.GetAction<IdleAction>();
        }

        if (m_Target == null)
            return m_BaseObject.GetAction<IdleAction>();

        #region Find Close New Enemy
        GridPosition baseObjectGirdPosition = m_BaseObject.GetGridPosition();
        GridPosition targetPosition = m_Target.GetGridPosition();

        // 현재 그리드 내에서 새롭게 적을 탐새함.
        List<GridPosition> detectedPositions = GetValidActionGridPositionList();

        // 새로 발견한 가장 가까운 적이 현재 타겟보다 가깝다면 변경
        if (detectedPositions.Count > 0)
        {
            GridPosition newtargetGridPosition = LevelGrid.Instance.GetClosestTargetGridPosition(m_BaseObject.GetGridPosition(), detectedPositions);

            if (GridPosition.GetGridDistanceSquared(newtargetGridPosition, baseObjectGirdPosition)
                < GridPosition.GetGridDistanceSquared(targetPosition, baseObjectGirdPosition))
                m_Target = LevelGrid.Instance.GetUnitAtGridPosition(newtargetGridPosition);
        }

        targetPosition = m_Target.GetGridPosition();

        #endregion

        // 공격 범위 안에 있다면 공격
        if (LevelGrid.Instance.IsTargetInAttackRange(baseObjectGirdPosition, targetPosition))
        {
            return m_BaseObject.GetAction<CombatAction>();
        }
        // 공격 범위 밖에 있다면 이동
        else
        {
            // Find Path
            List<GridPosition> pathGridPositionList = Pathfinding.Instance.FindPath(m_BaseObject.GetGridPosition(), targetPosition, out int pathLength);

            // Remove Eenemy Grid Position
            pathGridPositionList.RemoveAt(pathGridPositionList.Count - 1);
            DestGirdPosition = pathGridPositionList[pathGridPositionList.Count - 1]; // 적 바로 앞에서 멈춤

            currentPositionIndex = 0;
            positionList = new List<Vector3>();

            // 이동 거리가 남아 있다면
            if (pathGridPositionList.Count >= AVALIABLE_MOVE_GRID)
            {
                LevelGrid.Instance.SetReserveGridPosition(pathGridPositionList[1], true);
                positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPositionList[1]));
                InvokeOnStartMoving();
                ActionStart(onActionComplete);
            }

            return this;
        }
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
                    if (!LevelGrid.Instance.HasEnemyAtGridPosition(testGridPosition, m_BaseObject))
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

                    // 이미 등록한 타겟의 위치 제외
                    if (m_Target != null && m_Target.GetGridPosition() == testGridPosition)
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
