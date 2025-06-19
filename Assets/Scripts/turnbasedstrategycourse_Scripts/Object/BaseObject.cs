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

    protected GridPosition gridPosition;
    public StatSystem m_StatSystem { get; protected set; }

    [SerializeField] protected BaseAction m_CurrentAction;
    private Dictionary<Type, BaseAction> baseActionDict = new Dictionary<Type, BaseAction>();

    //public E_ObjectType ObjectType;
    [SerializeField] protected bool isEnemy;
    [SerializeField] protected bool isBusy; // Checking If Do it Yours Command
    public E_ObjectType m_ObjectType;

    // Check Timer
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
    }

    private void HandleStateMachine()
    {
        if (m_StatSystem.m_IsDead)
            return;

        // TODO
        // 플레이어의 입력이 들어오면 최우선으로 처리한다.
        // 상태이상 등으로 인해 플레이어의 입력을 막는다.

        timer -= Time.deltaTime;
        if(timer <= 0f)
        {
            timer = checkInterval;
            var nextAction = m_CurrentAction?.TakeAction();

            if (nextAction != null)
                SwitchToNextState(nextAction);
        }
    }

    public void SwitchToNextState(BaseAction state)
    {
        m_CurrentAction = state;
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

    public void Damage(int damageAmount)
    {
        m_StatSystem.Damage(damageAmount);
    }



    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }

    public float GetHealthNormalized()
    {
        return m_StatSystem.GetHealthNormalized();
    }
}
