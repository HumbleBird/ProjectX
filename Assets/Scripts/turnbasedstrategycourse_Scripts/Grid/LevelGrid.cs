using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class LevelGrid : MonoBehaviour
{

    public static LevelGrid Instance { get; private set; }

    public const float FLOOR_HEIGHT = 3f;

    public event EventHandler<OnAnyUnitMovedGridPositionEventArgs> OnAnyUnitMovedGridPosition;
    public class OnAnyUnitMovedGridPositionEventArgs : EventArgs
    {
        public BaseObject unit;
        public GridPosition fromGridPosition;
        public GridPosition toGridPosition;
    }


    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    [SerializeField] private int floorAmount;
    
    private List<GridSystem<GridObject>> gridSystemList;
    private Dictionary<GridPosition, bool> reserveGirdPosition = new Dictionary<GridPosition, bool>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one LevelGrid! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gridSystemList = new List<GridSystem<GridObject>>();

        for (int floor = 0; floor < floorAmount; floor++)
        {
            GridSystem<GridObject> gridSystem = new GridSystem<GridObject>(width, height, cellSize, floor, FLOOR_HEIGHT,
                    (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition));
            //gridSystem.CreateDebugObjects(gridDebugObjectPrefab);

            gridSystemList.Add(gridSystem);
        }
    }

    private void Start()
    {
        Pathfinding.Instance.Setup(width, height, cellSize, floorAmount);
    }

    #region Base System

    public void AddUnitAtGridPosition(GridPosition gridPosition, BaseObject unit)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        gridObject.AddUnit(unit);
    }

    public void RemoveUnitAtGridPosition(GridPosition gridPosition, BaseObject baseObject)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        gridObject.RemoveUnit(baseObject);
    }

    public void UnitMovedGridPosition(BaseObject unit, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        RemoveUnitAtGridPosition(fromGridPosition, unit);

        AddUnitAtGridPosition(toGridPosition, unit);

        OnAnyUnitMovedGridPosition?.Invoke(this, new OnAnyUnitMovedGridPositionEventArgs {
            unit = unit,
            fromGridPosition = fromGridPosition,
            toGridPosition = toGridPosition,
        });
    }

    #endregion

    #region GetInfo

    #region GetObjectInfo

    public List<BaseObject> GetUnitListAtGridPosition(GridPosition gridPosition)
    {
        // ?? 어디에 쓰는 물건이고
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.GetUnitList();
    }

    public BaseObject GetUnitAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.GetUnit();
    }

    public List<BaseObject> GetObjectsAtGridPositions(List<GridPosition> gridPositions)
    {
        return gridPositions.Select(pos => GetUnitAtGridPosition(pos)).ToList();
    }

    #endregion

    #region GetSystemInfo

    private GridSystem<GridObject> GetGridSystem(int floor)
    {
        return gridSystemList[floor];
    }







    public int GetFloor(Vector3 worldPosition)
    {
        return Mathf.RoundToInt(worldPosition.y / FLOOR_HEIGHT);
    }

    public GridPosition GetGridPosition(Vector3 worldPosition)
    {
        int floor = GetFloor(worldPosition);
        return GetGridSystem(floor).GetGridPosition(worldPosition);
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition) => GetGridSystem(gridPosition.floor).GetWorldPosition(gridPosition);
    
    public int GetWidth() => GetGridSystem(0).GetWidth();
    
    public int GetHeight() => GetGridSystem(0).GetHeight();

    public int GetFloorAmount() => floorAmount;



    public IInteractable GetInteractableAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.GetInteractable();
    }

    public bool GetReservedGridPosition(GridPosition gridPosition)
    {
        reserveGirdPosition.TryGetValue(gridPosition, out bool result);

        return result;
    }

    public E_Dir GetDirGridPosition(GridPosition origin, GridPosition target)
    {
        int dx = target.x - origin.x;
        int dz = target.z - origin.z;

        if (dx == 0 && dz == 0)
            return E_Dir.North; // 자기 자신 → 기본값 반환

        float angle = Mathf.Atan2(dz, dx) * Mathf.Rad2Deg;
        angle = (angle + 360f) % 360f; // 0~360도 정규화

        if (angle >= 337.5f || angle < 22.5f)
            return E_Dir.East;
        else if (angle >= 22.5f && angle < 67.5f)
            return E_Dir.NorthEast;
        else if (angle >= 67.5f && angle < 112.5f)
            return E_Dir.North;
        else if (angle >= 112.5f && angle < 157.5f)
            return E_Dir.NorthWest;
        else if (angle >= 157.5f && angle < 202.5f)
            return E_Dir.West;
        else if (angle >= 202.5f && angle < 247.5f)
            return E_Dir.SouthWest;
        else if (angle >= 247.5f && angle < 292.5f)
            return E_Dir.South;
        else // angle >= 292.5f && angle < 337.5f
            return E_Dir.SouthEast;
    }

    public float GetGridDistanceSquared_float(GridPosition a, GridPosition b)
    {
        int dx = a.x - b.x;
        int dz = a.z - b.z;
        int df = a.floor - b.floor;
        return dx * dx + dz * dz + df * df; // 3D 거리의 제곱 (정수 기반)
        // return dx * dx + dz * dz + (df * floorWeight) * (df * floorWeight);
    }

    public GridPosition GetGridDistanceSquared_GridPosition(GridPosition a, GridPosition b)
    {
        int dx = a.x - b.x;
        int dz = a.z - b.z;
        int df = a.floor - b.floor;
        return new GridPosition(Math.Abs(dx), Math.Abs(dz), Math.Abs(df));
    }

    public GridPosition GetClosestTargetGridPosition(GridPosition gridPosition, List<GridPosition> positions)
    {
        GridPosition selfPos = gridPosition;
        GridPosition closest = positions[0];

        float minDistanceSqr = GetGridDistanceSquared_float(selfPos, closest);

        foreach (GridPosition pos in positions)
        {
            float distSqr = GetGridDistanceSquared_float(selfPos, pos);
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                closest = pos;
            }
        }

        return closest;
    }

    #endregion


    #endregion

    #region SetInfo

    public void SetReserveGridPosition(GridPosition gridPosition, bool isReserve)
    {
        if (reserveGirdPosition.TryGetValue(gridPosition, out bool result) == false)
            reserveGirdPosition.Add(gridPosition, isReserve);
        else
            reserveGirdPosition[gridPosition] = isReserve;

        PathNode node = Pathfinding.Instance.GetNode(gridPosition.x, gridPosition.z, gridPosition.floor);
        node.SetIsWalkable(!isReserve);
    }

    public void SetInteractableAtGridPosition(GridPosition gridPosition, IInteractable interactable)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        gridObject.SetInteractable(interactable);
    }


    #endregion

    #region Condition

    public bool IsValidGridPosition(GridPosition gridPosition)
    {
        if (gridPosition.floor < 0 || gridPosition.floor >= floorAmount)
        {
            return false;
        }
        else
        {
            return GetGridSystem(gridPosition.floor).IsValidGridPosition(gridPosition);
        }
    }


    public bool HasAnyUnitOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.HasAnyUnit();
    }

    public bool HasEnemyAtGridPosition(GridPosition gridPosition, BaseObject searcher)
    {
        BaseObject o = GetUnitAtGridPosition(gridPosition);
        if (o == null)
            return false;
        else if (o.IsEnemy() == searcher.IsEnemy())
            return false;

        return true;
    }


    public E_Distance IsTargetInAttackRange(GridPosition gridPosition, GridPosition targetPosition)
    {
        BaseObject attacker = GetUnitAtGridPosition(gridPosition);

        int distance = Mathf.Abs(gridPosition.x - targetPosition.x) + Mathf.Abs(gridPosition.z - targetPosition.z);

        int minRange = attacker.m_StatSystem.m_Stat.m_iMinAttackRange;
        int maxRange = attacker.m_StatSystem.m_Stat.m_iMaxAttackRange;

        if (distance > maxRange)
            return E_Distance.Far;
        else if (distance < minRange)
            return E_Distance.Close;
        else
            return E_Distance.Proper;
    }

    public bool IsTargeSoFarAtChase(GridPosition gridPosition, GridPosition targetPosition)
    {
        BaseObject attacker = GetUnitAtGridPosition(gridPosition);

        int pos = gridPosition.x - targetPosition.x + gridPosition.z - targetPosition.z;
        if (attacker.m_StatSystem.m_Stat.m_iChaseRange <= pos)
            return true;

        return false;
    }

    #endregion

    #region Caculate

    public GridPosition ToGridPosition(GridPosition offset, GridPosition origin, E_Dir dir)
    {
        int x = offset.x;
        int z = offset.z;
        int rotatedX = 0;
        int rotatedZ = 0;

        switch (dir)
        {
            case E_Dir.North:
                rotatedX = x;
                rotatedZ = z;
                break;
            case E_Dir.East:
                rotatedX = z;
                rotatedZ = -x;
                break;
            case E_Dir.South:
                rotatedX = -x;
                rotatedZ = -z;
                break;
            case E_Dir.West:
                rotatedX = -z;
                rotatedZ = x;
                break;
                //case E_Dir.NorthEast:
                //    rotatedX = Mathf.RoundToInt(x * 0.7071f - z * 0.7071f);
                //    rotatedZ = Mathf.RoundToInt(x * 0.7071f + z * 0.7071f);
                //    break;
                //case E_Dir.SouthEast:
                //    rotatedX = Mathf.RoundToInt(-x * 0.7071f - z * 0.7071f);
                //    rotatedZ = Mathf.RoundToInt(x * 0.7071f - z * 0.7071f);
                //    break;
                //case E_Dir.SouthWest:
                //    rotatedX = Mathf.RoundToInt(-x * 0.7071f + z * 0.7071f);
                //    rotatedZ = Mathf.RoundToInt(-x * 0.7071f - z * 0.7071f);
                //    break;
                //case E_Dir.NorthWest:
                //    rotatedX = Mathf.RoundToInt(x * 0.7071f + z * 0.7071f);
                //    rotatedZ = Mathf.RoundToInt(-x * 0.7071f + z * 0.7071f);
                //    break;
        }

        return origin + new GridPosition(rotatedX, rotatedZ, offset.floor);
    }

    public GridPosition DistanceGridPosition(GridPosition gridPosition, GridPosition targetPosition)
    {
        return gridPosition - targetPosition;
    }


    #endregion


    #region Order

    public void ClearInteractableAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        gridObject.ClearInteractable();
    }
    
    #endregion










}