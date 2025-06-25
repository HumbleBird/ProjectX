using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class BaseObject : MonoBehaviour
{
    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static event EventHandler OnAnyUnitDead;

    [Header("Object Info")]
    protected GridPosition gridPosition;
    public StatSystem m_StatSystem { get; protected set; }
    [SerializeField] protected bool isEnemy;
    public E_ObjectType m_ObjectType;

    [Header("Action")]
    private Dictionary<Type, BaseAction> baseActionDict = new Dictionary<Type, BaseAction>();
    [SerializeField] private BaseAction currentAction;
    public BaseAction m_CurrentAction
    {
        get => currentAction;
        protected set => currentAction = value;
    }

    [SerializeField] protected BaseAction m_BeforeAction;

    [Header("Battle Info")]
    public BaseObject m_Target { get; protected set; }

    [Header("Player DirectCommand")]
    public bool IsPlayerControlled { get; private set; } = false;
    private Queue<Action> followUpCommands = new();

    [Header("Check Timer")]
    float checkInterval = 0.5f;
    float timer = 0f;

    protected virtual void Awake()
    {
        m_StatSystem = GetComponent<StatSystem>();
     
        foreach (var action in GetComponentsInChildren<BaseAction>())
              baseActionDict[action.GetType()] = action;
    }

    protected virtual void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.AddUnitAtGridPosition(gridPosition, this);

        //TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;

        m_StatSystem.OnDead += HealthSystem_OnDead;

        // Base Action
        SwitchToNextState(GetAction<IdleAction>());

        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void Update()
    {
        HandleStateMachine();
        UpdateGridPosition();
    }


    private  void UpdateGridPosition()
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

    private void HandleStateMachine()
    {
        if (m_StatSystem.m_IsDead)
            return;

        timer -= Time.deltaTime;
        if(timer <= 0f)
        {
            timer = checkInterval;

            ExecuteAction();
        }
    }

    private void ExecuteAction()
    {
        m_CurrentAction.ClearAction();
        var nextAction = m_CurrentAction?.TakeAction();

        if (nextAction != null)
            SwitchToNextState(nextAction);
    }

    public void SwitchToNextState(BaseAction nextAction)
    {
        m_CurrentAction = nextAction;

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

    public IEnumerable<BaseAction> GetActions()
    {
        return baseActionDict.Values;
    }

    public T GetAction<T>() where T : BaseAction
    {
        if (baseActionDict.TryGetValue(typeof(T), out var action))
            return action as T;
        return null;
    }


    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public void Hit(AttackBase attack)
    {
        // 크리티컬

        // 회피율

        // 반격율

        // 기타 등등 적용하기

        m_StatSystem.ReduceHP(attack.m_iPhysicalAttackDamage);
    }



    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }

    public float GetHealthNormalized()
    {
        return m_StatSystem.GetHealthNormalized();
    }

    public void DirectCommand<TAction>(BaseAction action, Action<BaseObject, TAction> onActionComplete) where TAction : BaseAction
    {
        m_BeforeAction = m_CurrentAction;
        m_CurrentAction = action;

        if (action is TAction typedAction)
        {
            action.SetActionComlete(() => onActionComplete?.Invoke(this, typedAction));
        }
    }

    public virtual void SetTarget(BaseObject target)
    {
        m_Target = target;
    }
}
