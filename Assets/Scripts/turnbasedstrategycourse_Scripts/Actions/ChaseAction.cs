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
        // <���ܻ���>
        // Ÿ�� ���
        // Ÿ�� ����
        // Ÿ�� �Ÿ��� ��
        if (m_Target == null ||
            m_Target.m_StatSystem.m_IsDead ||
            LevelGrid.Instance.IsTargeSoFarAtChase(m_BaseObject.GetGridPosition(), m_Target.GetGridPosition()))
        {
            m_Target = null;

            List<GridPosition> NewdetectedPositions = GetValidActionGridPositionList();

            if (NewdetectedPositions.Count > 0)
            {
                // ���� ����� ���� GridPosition�� ������
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

        // ���� �׸��� ������ ���Ӱ� ���� Ž����.
        List<GridPosition> detectedPositions = GetValidActionGridPositionList();

        // ���� �߰��� ���� ����� ���� ���� Ÿ�ٺ��� �����ٸ� ����
        if (detectedPositions.Count > 0)
        {
            GridPosition newtargetGridPosition = LevelGrid.Instance.GetClosestTargetGridPosition(m_BaseObject.GetGridPosition(), detectedPositions);

            if (GridPosition.GetGridDistanceSquared(newtargetGridPosition, baseObjectGirdPosition)
                < GridPosition.GetGridDistanceSquared(targetPosition, baseObjectGirdPosition))
                m_Target = LevelGrid.Instance.GetUnitAtGridPosition(newtargetGridPosition);
        }

        targetPosition = m_Target.GetGridPosition();

        #endregion

        // ���� ���� �ȿ� �ִٸ� ����
        if (LevelGrid.Instance.IsTargetInAttackRange(baseObjectGirdPosition, targetPosition))
        {
            return m_BaseObject.GetAction<CombatAction>();
        }
        // ���� ���� �ۿ� �ִٸ� �̵�
        else
        {
            // Find Path
            List<GridPosition> pathGridPositionList = Pathfinding.Instance.FindPath(m_BaseObject.GetGridPosition(), targetPosition, out int pathLength);

            // Remove Eenemy Grid Position
            pathGridPositionList.RemoveAt(pathGridPositionList.Count - 1);
            DestGirdPosition = pathGridPositionList[pathGridPositionList.Count - 1]; // �� �ٷ� �տ��� ����

            currentPositionIndex = 0;
            positionList = new List<Vector3>();

            // �̵� �Ÿ��� ���� �ִٸ�
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

                    // �̹� ����� Ÿ���� ��ġ ����
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
