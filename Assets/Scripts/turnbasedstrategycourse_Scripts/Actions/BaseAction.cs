using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour
{
    public static event EventHandler OnAnyActionStarted;
    public static event EventHandler OnAnyActionCompleted;

    protected Unit m_BaseObject;
    protected StatSystem m_StatSystem;
    protected bool isActive;
    protected Action onActionComplete;

    protected LayerMask detectionLayer;
    protected LayerMask layerThatBlockLineOfSight;


    public GridPosition DestGirdPosition;

    protected virtual void Awake()
    {
        m_BaseObject = GetComponentInParent<Unit>();
        m_StatSystem = GetComponentInParent<StatSystem>();

        detectionLayer = 1 << LayerMask.NameToLayer("Units") | 1 << LayerMask.NameToLayer("Building");
        layerThatBlockLineOfSight = 1 << LayerMask.NameToLayer("Obstacles");
    }

    protected virtual void Update()
    {

    }

    public abstract string GetActionName();

    public abstract BaseAction TakeAction(GridPosition gridPosition = default, Action onActionComplete = null);

    public virtual bool IsValidActionGridPosition(GridPosition gridPosition)
    {
        List<GridPosition> validGridPositionList = GetValidActionGridPositionList();
        return validGridPositionList.Contains(gridPosition);
    }

    public abstract List<GridPosition> GetValidActionGridPositionList();


    public void ActionStart(Action onActionComplete)
    {
        isActive = true;
        //this.onActionComplete = onActionComplete;

        OnAnyActionStarted?.Invoke(this, EventArgs.Empty);
    }

    protected void ActionComplete()
    {
        isActive = false;
        onActionComplete?.Invoke();

        OnAnyActionCompleted?.Invoke(this, EventArgs.Empty);
    }

    public void SetActionComlete(Action onActionComplete)
    {
        this.onActionComplete = onActionComplete;

    }

    public BaseObject GetObject()
    {
        return m_BaseObject;
    }

    public EnemyAIAction GetBestEnemyAIAction()
    {
        List<EnemyAIAction> enemyAIActionList = new List<EnemyAIAction>();

        List<GridPosition> validActionGridPositionList = GetValidActionGridPositionList();

        foreach (GridPosition gridPosition in validActionGridPositionList)
        {
            EnemyAIAction enemyAIAction = GetEnemyAIAction(gridPosition);
            enemyAIActionList.Add(enemyAIAction);
        }

        if (enemyAIActionList.Count > 0)
        {
            enemyAIActionList.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue - a.actionValue);
            return enemyAIActionList[0];
        } else
        {
            // No possible Enemy AI Actions
            return null;
        }

    }

    public abstract EnemyAIAction GetEnemyAIAction(GridPosition gridPosition);




    public void CancelReverveGridPosition()
    {
        LevelGrid.Instance.SetReserveGridPosition(DestGirdPosition, false);
    }

    public virtual void ClearAction()
    {

    }

}