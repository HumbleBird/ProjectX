using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChaseAction : BaseAction
{
    public const int AVALIABLE_MOVE_Grid = 3;
    public const int ADJACENT_Grid = 3;

    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;
    public event EventHandler<OnChangeFloorsStartedEventArgs> OnChangedFloorsStarted;
    public class OnChangeFloorsStartedEventArgs : EventArgs
    {
        public GridPosition unitGridPosition;
        public GridPosition targetGridPosition;
    }

    public int isOrder = 0;

    int maxMoveDistance => m_StatSystem.m_Stat.m_iChaseRange;

    private List<Vector3> positionList;
    private int currentPositionIndex;
    private bool isChangingFloors;
    private float differentFloorsTeleportTimer;
    private float differentFloorsTeleportTimerMax = .5f;
    private int m_iReaminPathCount;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        Vector3 targetPosition = positionList[currentPositionIndex];

        if (isChangingFloors)
        {
            // Stop and Teleport Logic
            Vector3 targetSameFloorPosition = targetPosition;
            targetSameFloorPosition.y = m_BaseObject.transform.position.y;

            Vector3 rotateDirection = (targetSameFloorPosition - m_BaseObject.transform.position).normalized;

            float rotateSpeed = 10f;
            m_BaseObject.transform.forward = Vector3.Slerp(m_BaseObject.transform.forward, rotateDirection, Time.deltaTime * rotateSpeed);

            differentFloorsTeleportTimer -= Time.deltaTime;
            if (differentFloorsTeleportTimer < 0f)
            {
                isChangingFloors = false;
                m_BaseObject.transform.position = targetPosition;
            }
        }
        else
        {
            // Regular move logic
            Vector3 moveDirection = (targetPosition - m_BaseObject.transform.position).normalized;

            float rotateSpeed = 10f;
            m_BaseObject.transform.forward = Vector3.Slerp(m_BaseObject.transform.forward, moveDirection, Time.deltaTime * rotateSpeed);

            float moveSpeed = m_StatSystem.m_Stat.m_fMoveSpeed;
            m_BaseObject.transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }

        float stoppingDistance = .1f;
        if (Vector3.Distance(m_BaseObject.transform.position, targetPosition) < stoppingDistance)
        {
            // Reserver False
            LevelGrid.Instance.SetReserveGridPosition(LevelGrid.Instance.GetGridPosition(targetPosition), false);

            currentPositionIndex++;
            if (currentPositionIndex >= positionList.Count)
            {
                if(m_iReaminPathCount ==0)
                {
                    OnStopMoving?.Invoke(this, EventArgs.Empty);

                }
                ActionComplete();
            }
            else
            {
                targetPosition = positionList[currentPositionIndex];
                GridPosition targetGridPosition = LevelGrid.Instance.GetGridPosition(targetPosition);
                GridPosition unitGridPosition = LevelGrid.Instance.GetGridPosition(m_BaseObject.transform.position);

                if (targetGridPosition.floor != unitGridPosition.floor)
                {
                    // Different floors
                    isChangingFloors = true;
                    differentFloorsTeleportTimer = differentFloorsTeleportTimerMax;

                    OnChangedFloorsStarted?.Invoke(this, new OnChangeFloorsStartedEventArgs
                    {
                        unitGridPosition = unitGridPosition,
                        targetGridPosition = targetGridPosition,
                    });
                }
            }
        }
    }


    public override BaseAction TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        if (isActive)
            return this;

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

            currentPositionIndex = 0;
            positionList = new List<Vector3>();
            m_iReaminPathCount = pathGridPositionList.Count - AVALIABLE_MOVE_Grid; // Except Start Pos, Des Pos, last Pos 

            // 이동 거리가 남아 있다면
            if (pathGridPositionList.Count >= AVALIABLE_MOVE_Grid)
            {
                if (LevelGrid.Instance.GetReservedGridPosition(pathGridPositionList[1]))
                {
                    OnStopMoving?.Invoke(this, EventArgs.Empty);
                    ActionComplete();
                    return this;
                }
                else
                {
                    LevelGrid.Instance.SetReserveGridPosition(pathGridPositionList[1], true);
                    positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPositionList[1]));
                    OnStartMoving?.Invoke(this, EventArgs.Empty);
                    ActionStart(onActionComplete);
                }
            }

            return this;
        }
    }


    public override string GetActionName()
    {
        return "Move";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        throw new NotImplementedException();
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = m_BaseObject.GetGridPosition();

        for (int x = -maxMoveDistance; x <= maxMoveDistance; x++)
        {
            for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
            {
                for (int floor = -maxMoveDistance; floor <= maxMoveDistance; floor++)
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

                    int pathfindingDistanceMultiplier = 10;
                    if (Pathfinding.Instance.GetPathLength(unitGridPosition, testGridPosition) > maxMoveDistance * pathfindingDistanceMultiplier)
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
