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
        if (m_BaseObject.m_Target == null ||
            m_BaseObject.m_Target.m_StatSystem.m_IsDead ||
            LevelGrid.Instance.IsTargeSoFarAtChase(m_BaseObject.GetGridPosition(), m_BaseObject.m_Target.GetGridPosition()))
        {
            m_BaseObject.SetTarget(null);

            List<GridPosition> NewdetectedPositions = GetValidActionGridPositionList();

            if (NewdetectedPositions.Count > 0)
            {
                // ���� ����� ���� GridPosition�� ������
                GridPosition targetGridPosition =
                    LevelGrid.Instance.GetClosestTargetGridPosition(m_BaseObject.GetGridPosition(), NewdetectedPositions);
                BaseObject target = LevelGrid.Instance.GetUnitAtGridPosition(targetGridPosition);
                m_BaseObject.SetTarget(target);
            }
            else
                return m_BaseObject.GetAction<IdleAction>();
        }

        if (m_BaseObject.m_Target == null)
            return m_BaseObject.GetAction<IdleAction>();

        #region Find Close New Enemy
        GridPosition baseObjectGirdPosition = m_BaseObject.GetGridPosition();
        GridPosition targetPosition = m_BaseObject.m_Target.GetGridPosition();

        // ���� �׸��� ������ ���Ӱ� ���� Ž����.
        List<GridPosition> detectedPositions = GetValidActionGridPositionList();

        // ���� �߰��� ���� ����� ���� ���� Ÿ�ٺ��� �����ٸ� ����
        if (detectedPositions.Count > 0)
        {
            GridPosition newtargetGridPosition = LevelGrid.Instance.GetClosestTargetGridPosition(m_BaseObject.GetGridPosition(), detectedPositions);

            if (LevelGrid.Instance.GetGridDistanceSquared_float(newtargetGridPosition, baseObjectGirdPosition)
                < LevelGrid.Instance.GetGridDistanceSquared_float(targetPosition, baseObjectGirdPosition))
                m_BaseObject.SetTarget(LevelGrid.Instance.GetUnitAtGridPosition(newtargetGridPosition));
        }

        targetPosition = m_BaseObject.m_Target.GetGridPosition();

        #endregion

        // ���� ���� �ȿ� �ִٸ� ����
        if (LevelGrid.Instance.IsTargetInAttackRange(baseObjectGirdPosition, targetPosition) == E_Distance.Proper)
        {
            return m_BaseObject.GetAction<CombatAction>();
        }
        // ���� ���� �ۿ� �ִٸ� �̵�
        else if (LevelGrid.Instance.IsTargetInAttackRange(baseObjectGirdPosition, targetPosition) == E_Distance.Far)
        {
            // Find Path
            List<GridPosition> pathGridPositionList = Pathfinding.Instance.FindPath(m_BaseObject.GetGridPosition(), targetPosition, out int pathLength);

            // �̵� �Ÿ��� ���� �ִٸ�
            // ����
            if (pathGridPositionList.Count >= AVALIABLE_MOVE_GRID)
            {
                pathGridPositionList.RemoveAt(pathGridPositionList.Count - 1); // �� ��ġ ����
                pathGridPositionList.RemoveAt(0); // �ڽ� ��ġ ����

                if (pathGridPositionList.Count >= 1)
                {
                    DestGirdPosition = pathGridPositionList[pathGridPositionList.Count - 1]; // �� �ٷ� �տ��� ����
                    currentPositionIndex = 0;
                    positionList = new List<Vector3>();

                    // Change Reserve
                    if (PrevReservePosition != default)
                        LevelGrid.Instance.SetReserveGridPosition(PrevReservePosition, false);

                    PrevReservePosition = pathGridPositionList[0];

                    LevelGrid.Instance.SetReserveGridPosition(pathGridPositionList[0], true);
                    positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPositionList[0]));
                    InvokeOnStartMoving();
                    ActionStart(onActionComplete);
                }
            }


            return this;
        }
        else
        {
            // TODO
            // ���� �ʹ� �����ٸ� �Ÿ��� ������.
            return null;
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
                    if (m_BaseObject.m_Target != null && m_BaseObject.m_Target.GetGridPosition() == testGridPosition)
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
