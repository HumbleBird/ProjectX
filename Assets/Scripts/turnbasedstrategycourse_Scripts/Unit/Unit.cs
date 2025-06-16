using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : BaseObject
{
    protected override void Awake()
    {
        base.Awake();

        m_ObjectType = Define.E_ObjectType.Unit;
    }


    private void Update()
    {
        GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        if (newGridPosition != gridPosition)
        {
            // Unit changed Grid Position
            GridPosition oldGridPosition = gridPosition;
            gridPosition = newGridPosition;

            LevelGrid.Instance.UnitMovedGridPosition(this, oldGridPosition, newGridPosition);
        }
    }

    public T GetAction<T>() where T : BaseAction
    {
        foreach (BaseAction baseAction in baseActionArray)
        {
            if (baseAction is T)
            {
                return (T)baseAction;
            }
        }
        return null;
    }


    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }

    public BaseAction[] GetBaseActionArray()
    {
        return baseActionArray;
    }

    public void Damage(int damageAmount)
    {
        m_StatSystem.Damage(damageAmount);
    }

    public float GetHealthNormalized()
    {
        return m_StatSystem.GetHealthNormalized();
    }
}