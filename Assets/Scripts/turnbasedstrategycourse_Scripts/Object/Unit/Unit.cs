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


    protected override void Update()
    {
        base.Update();

        GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        if (newGridPosition != gridPosition)
        {
            // Unit changed Grid Position
            GridPosition oldGridPosition = gridPosition;
            gridPosition = newGridPosition;

            LevelGrid.Instance.UnitMovedGridPosition(this, oldGridPosition, newGridPosition);
        }
    }
}