using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{

    private GridSystem<GridObject> gridSystem;
    private GridPosition gridPosition;
    private List<BaseObject> unitList;
    private IInteractable interactable;

    public GridObject(GridSystem<GridObject> gridSystem, GridPosition gridPosition)
    {
        this.gridSystem = gridSystem;
        this.gridPosition = gridPosition;
        unitList = new List<BaseObject>();
    }

    public override string ToString()
    {
        string unitString = "";
        foreach (Unit unit in unitList)
        {
            unitString += unit + "\n";
        }

        return gridPosition.ToString() + "\n" + unitString;
    }

    public void AddUnit(BaseObject unit)
    {
        unitList.Add(unit);
    }

    public void RemoveUnit(BaseObject unit)
    {
        unitList.Remove(unit);
    }

    public List<BaseObject> GetUnitList()
    {
        return unitList;
    }

    public bool HasAnyUnit()
    {
        return unitList.Count > 0;
    }

    public BaseObject GetUnit()
    {
        if (HasAnyUnit())
        {
            return unitList[0];
        } else
        {
            return null;
        }
    }

    public IInteractable GetInteractable()
    {
        return interactable;
    }

    public void SetInteractable(IInteractable interactable)
    {
        this.interactable = interactable;
    }

    public void ClearInteractable()
    {
        this.interactable = null;
    }

}