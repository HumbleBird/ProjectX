using System;
using UnityEngine;
using static Define;

public class BaseObject : MonoBehaviour
{
    protected const int ACTION_POINTS_MAX = 9;

    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static event EventHandler OnAnyUnitDead;

    protected GridPosition gridPosition;
    protected StatSystem healthSystem;
    protected BaseAction[] baseActionArray;

    //public E_ObjectType ObjectType;
    [SerializeField] protected bool isEnemy;
    public E_ObjectType m_ObjectType;

    protected virtual void Awake()
    {
        healthSystem = GetComponent<StatSystem>();
        baseActionArray = GetComponents<BaseAction>();
    }

    protected virtual void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.AddUnitAtGridPosition(gridPosition, this);

        //TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;

        healthSystem.OnDead += HealthSystem_OnDead;

        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
    }

    public virtual void OnDeselected()
    {
        //Debug.Log($"{name} DeSelectMe");
    }

    public virtual void OnSelected()
    {
        //Debug.Log($"{name} SelectMe");
    }

    public bool IsEnemy()
    {
        return isEnemy;
    }

    protected void HealthSystem_OnDead(object sender, EventArgs e)
    {
        LevelGrid.Instance.RemoveUnitAtGridPosition(gridPosition, this);

        Destroy(gameObject);

        OnAnyUnitDead?.Invoke(this, EventArgs.Empty);
    }


    public BaseAction[] GetActions()
    {
        return baseActionArray;
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }
}
