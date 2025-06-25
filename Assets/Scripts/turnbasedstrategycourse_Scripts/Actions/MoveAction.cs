using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static Define;

public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;
    public event EventHandler<OnChangeFloorsStartedEventArgs> OnChangedFloorsStarted;
    public class OnChangeFloorsStartedEventArgs : EventArgs
    {
        public GridPosition unitGridPosition;
        public GridPosition targetGridPosition;
    }

    protected GridPosition PrevReservePosition;

    protected int    m_iMaxMoveDistance;
    protected  float m_fMoveSpeed;

    protected List<Vector3> positionList;
    protected int currentPositionIndex;
    protected bool isChangingFloors;
    protected float differentFloorsTeleportTimer;
    protected float differentFloorsTeleportTimerMax = .5f;



    protected override void Update()
    {
        base.Update();

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

            float moveSpeed = m_fMoveSpeed;
            m_BaseObject.transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }

        float stoppingDistance = .1f;
        if (Vector3.Distance(m_BaseObject.transform.position, targetPosition) < stoppingDistance)
        {
            //currentPositionIndex++;
            //if (currentPositionIndex >= positionList.Count)
            {
                // 최종 목적지에 도착했는지 여부 따지기
                if(DestGirdPosition == m_BaseObject.GetGridPosition())
                {
                    DestGirdPosition = default;
                    OnStopMoving?.Invoke(this, EventArgs.Empty);
                    ActionComplete();
                }
            }
            //else
            {
                // TODO Change Floor

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
        return this;
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
                    if(m_BaseObject.m_Target != null && m_BaseObject.m_Target.GetGridPosition() == testGridPosition)
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


    protected void InvokeOnStartMoving()
    {
        OnStartMoving?.Invoke(this, EventArgs.Empty);
    }


    protected void InvokeOnStopMoving()
    {
        OnStopMoving?.Invoke(this, EventArgs.Empty);

    }


    public override void ClearAction()
    {
        base.ClearAction();

        if (PrevReservePosition != default)
        {
            LevelGrid.Instance.SetReserveGridPosition(PrevReservePosition, false);
            PrevReservePosition = default;
        }
    }
}
