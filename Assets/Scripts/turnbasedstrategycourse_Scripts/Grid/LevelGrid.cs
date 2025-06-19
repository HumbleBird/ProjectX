using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{

    public static LevelGrid Instance { get; private set; }

    public const float FLOOR_HEIGHT = 3f;

    public event EventHandler<OnAnyUnitMovedGridPositionEventArgs> OnAnyUnitMovedGridPosition;
    public class OnAnyUnitMovedGridPositionEventArgs : EventArgs
    {
        public Unit unit;
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


    private GridSystem<GridObject> GetGridSystem(int floor)
    {
        return gridSystemList[floor];
    }

    public void AddUnitAtGridPosition(GridPosition gridPosition, BaseObject unit)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        gridObject.AddUnit(unit);
    }

    public List<BaseObject> GetUnitListAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.GetUnitList();
    }

    public void RemoveUnitAtGridPosition(GridPosition gridPosition, BaseObject baseObject)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        gridObject.RemoveUnit(baseObject);
    }

    public void UnitMovedGridPosition(Unit unit, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        RemoveUnitAtGridPosition(fromGridPosition, unit);

        AddUnitAtGridPosition(toGridPosition, unit);

        OnAnyUnitMovedGridPosition?.Invoke(this, new OnAnyUnitMovedGridPositionEventArgs {
            unit = unit,
            fromGridPosition = fromGridPosition,
            toGridPosition = toGridPosition,
        });
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

    public bool IsValidGridPosition(GridPosition gridPosition)
    {
        if (gridPosition.floor < 0 || gridPosition.floor >= floorAmount)
        {
            return false;
        } else
        {
            return GetGridSystem(gridPosition.floor).IsValidGridPosition(gridPosition);
        }
    }

    public int GetWidth() => GetGridSystem(0).GetWidth();
    
    public int GetHeight() => GetGridSystem(0).GetHeight();
    
    public int GetFloorAmount() => floorAmount;

    public bool HasAnyUnitOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.HasAnyUnit();
    }

    public BaseObject GetUnitAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.GetUnit();
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

    public IInteractable GetInteractableAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.GetInteractable();
    }

    public void SetInteractableAtGridPosition(GridPosition gridPosition, IInteractable interactable)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        gridObject.SetInteractable(interactable);
    }

    public void ClearInteractableAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        gridObject.ClearInteractable();
    }

    public GridPosition DistanceGridPosition(GridPosition gridPosition, GridPosition targetPosition)
    {
        return gridPosition - targetPosition;
    }

    public GridPosition GetClosestTargetGridPosition(GridPosition gridPosition, List<GridPosition> positions)
    {
        GridPosition selfPos = gridPosition;
        GridPosition closest = positions[0];

        float minDistanceSqr = GridPosition.GetGridDistanceSquared(selfPos, closest);

        foreach (GridPosition pos in positions)
        {
            float distSqr = GridPosition.GetGridDistanceSquared(selfPos, pos);
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                closest = pos;
            }
        }

        return closest;
    }

    public bool IsTargetInAttackRange(GridPosition gridPosition, GridPosition targetPosition)
    {
        BaseObject attacker = GetUnitAtGridPosition(gridPosition);

        int pos = gridPosition.x - targetPosition.x + gridPosition.z - targetPosition.z;
        if (attacker.m_StatSystem.m_Stat.m_iMaxAttackRange >= pos &&
           attacker.m_StatSystem.m_Stat.m_iMinAttackRange <= pos)
            return true;

        return false;
    }

    public void SetReserveGridPosition(GridPosition gridPosition, bool isReserve)
    {
        if (reserveGirdPosition.TryGetValue(gridPosition, out bool result) == false)
            reserveGirdPosition.Add(gridPosition, isReserve);
        else
            reserveGirdPosition[gridPosition] = isReserve;
    }

    public bool GetReservedGridPosition(GridPosition gridPosition)
    {
        reserveGirdPosition.TryGetValue(gridPosition, out bool result);

        return result;
    }
}