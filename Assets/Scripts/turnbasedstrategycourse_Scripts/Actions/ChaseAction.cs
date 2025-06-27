using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Define;

public class ChaseAction : MoveAction
{
    protected override void Awake()
    {
        base.Awake();

        m_iMaxMoveDistance = m_StatSystem.m_Stat.m_iChaseRange;
        m_fMoveSpeed = m_StatSystem.m_Stat.m_fChaseSpeed;
        SetActionComlete(() => { m_BaseObject.m_CommandAction = m_BaseObject.GetAction<CombatAction>() ; });
    }

    public override BaseAction TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        GridPosition selfPos = m_BaseObject.GetGridPosition();

        // 감지 범위 내의 적 유닛 탐색
        var (obj, pos) = LevelGrid.Instance.GetClosestTargetGridInfo(selfPos, GetValidActionGridPositionList());
        if (obj == null)
         {
            return FailSerachTarget();
        }
        else
            m_BaseObject.SetTarget(obj);

        GridPosition targetPos = obj.GetGridPosition(); // 정보 갱신

        // 현재 공격하기에 가장 좋은 위치를 탐색함.
        List<GridPosition> attackPosisions = GetAttackGridPosition(selfPos, targetPos);
        
        if (attackPosisions == default)
        {
            return FailSerachTarget();
        }
        else
            DestGirdPosition = attackPosisions[attackPosisions.Count - 1];


        // Change Reserve
        LevelGrid.Instance.SetReserveGridPosition(attackPosisions[0], true);

        forwardPosition = LevelGrid.Instance.GetWorldPosition(attackPosisions[0]);

        // Event
        OnStartMoveGrid();
        InvokeOnStartMoving();
        ActionStart(onActionComplete);

        return this;
    }

    private List<GridPosition> GetAttackGridPosition(GridPosition gridPosition, GridPosition targetPosition)
    {
        // 1. 공격 패턴 분석
        var attackPatterns = m_BaseObject.m_StatSystem.m_Stat.attackPatterns;
        List<GridPosition> bestPosition = new();

        // 2. 모든 공격 위치 오프셋 가져오기
        HashSet<GridPosition> allAttackOffsets = new();
        foreach (var attackPattern in attackPatterns)
            foreach (var offset in attackPattern.m_RangeOffset)
                allAttackOffsets.Add(offset);

        // 3. 공격 오프셋과 방향, 타겟 위치를 이용해 공격자 위치 후보 도출
        HashSet<GridPosition> attackFromPositions = new();
        foreach (var dir in Enum.GetValues(typeof(E_Dir)).Cast<E_Dir>())
        {
            foreach (var offset in allAttackOffsets)
            {
                GridPosition attackerPos = LevelGrid.Instance.ToGridPosition(offset, targetPosition, dir);
                if (LevelGrid.Instance.IsValidGridPosition(attackerPos)) // 유효한 위치만 추가
                {
                    // 현재 내 위치가 공격 포인트라면 바로 반환
                    if (gridPosition == attackerPos)
                    {
                        var lists = Pathfinding.Instance.FindPath(gridPosition, attackerPos, out int len);
                        if(Pathfinding.Instance.IsWalkableGridPosition( lists[lists.Count-1]))
                            return LastFilter(lists);
                    }
                    attackFromPositions.Add(attackerPos);
                }

            }
        }

        // 4. 실제 이동 가능한 위치만 필터링
        var walkablePositions = attackFromPositions
            .Where(pos => 
                  Pathfinding.Instance.IsWalkableGridPosition(pos) && 
                  !LevelGrid.Instance.IsReservedGridPosition(pos) &&
                  !LevelGrid.Instance.HasAnyUnitOnGridPosition(pos))
            .ToList();

        // 5. 가장 가까운 이동 가능 위치 탐색

        // 이동 가능한 거리가 전부 막혔다면  계산한 후보들 중에서 현재 나의 위치를 계산 후.
        // 가장 가까운 거리, 그리고 이동 가능한 거리를 도출한다.
        if (walkablePositions.Count == 0)
        {
            // 후보들 선출하기.
            // 언제까지? 후보의 위치와 내 위치가 겹칠때까지
            HashSet<(List<GridPosition>, int)> candidatePositions = new(); //위치와 거리길이

            // 모든 공격 포지션의 -1 만큼의 이동 경로를 전부 가져온다.
            foreach (var pos in attackFromPositions)
            {
                List<GridPosition> lists = Pathfinding.Instance.FindPath(gridPosition, pos, out int pathLength);
                int count = lists.Count;

                while (count-- > 0)
                {
                    if (lists.Count >= Remove_MOVE_GRID)
                    {
                        lists.RemoveAt(lists.Count - 1); // 현재 이동 불가능한 공격 오프셋 자리를 제외한다.
                        if (Pathfinding.Instance.IsWalkableGridPosition(lists[lists.Count - 1]))
                        {
                            // TODO
                            // 만약 최종 후보지의 이동 거리가 생각보다 엄청 길다면?
                            // 현재 내 위치가 적과 가깝다면 이동할지 말지를 계산해야 한다.
                            candidatePositions.Add((lists, pathLength));
                            break;
                        }
                    }
                    else
                        break;
                }
            }

            // 후보들 중에서 가장 가까운 거리를 set
            int minPathCost = int.MaxValue;

            foreach (var (pos, len) in candidatePositions)
            {
                if (len < minPathCost)
                {
                    minPathCost = len;
                    bestPosition = pos;
                }
            }
        }

        // 이동 가능한 거리가 있다면 가장 가까운 거리를 Set
        else
        {
            int bestLength = int.MaxValue;

            foreach (var pos in walkablePositions)
            {
                var lists = Pathfinding.Instance.FindPath(gridPosition, pos, out int length);
                if (Pathfinding.Instance.IsWalkableGridPosition(lists[lists.Count - 1]))
                {
                    if (lists != null && length < bestLength)
                    {
                        bestLength = length;
                        bestPosition = lists;
                    }
                }
            }
        }

        // 마지막 필터링
        return LastFilter(bestPosition);
    }

    private List<GridPosition> LastFilter(List<GridPosition> bestPosition)
    {
        if (bestPosition.Count >= Remove_MOVE_GRID)
            bestPosition.RemoveAt(0); // 현재 유닛 위치 제거.

        return bestPosition;
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
