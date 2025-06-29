using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;
using static UnityEditor.Experimental.GraphView.GraphView;

public class UnitActionSystem : MonoBehaviour
{
    public static UnitActionSystem Instance { get; private set; }

    public event EventHandler OnSelectedUnitChanged;
    public event EventHandler OnSelectedActionChanged;
    public event EventHandler<bool> OnBusyChanged;
    public event EventHandler OnActionStarted;

    //[SerializeField] private Unit selectedUnit;
    public HashSet<BaseObject> m_SelectedObjects = new HashSet<BaseObject>();
    public List<BaseObject> AvailableUnits = new List<BaseObject>();
    [SerializeField] private LayerMask unitLayerMask;

    private BaseAction m_SelectedAction;
    private BaseAction m_AfterAction;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one UnitActionSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }

    private void Start()
    {
        MouseWorld.Instance.OnMouseUpChanged += HandleCommand;

    }

    private void Update()
    {
        if (m_SelectedObjects.Count <= 0)
            return;

        HandleSelectedAction();
    }

    private void HandleSelectedAction()
    {
        if (InputManager.Instance.IsMouseButtonDownThisFrame())
        {
            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPositionOnlyHitVisible());

            // Select Object
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, unitLayerMask)
                && hit.collider.TryGetComponent<BaseObject>(out BaseObject result))
            {
                if(result is Unit)
                {

                }
                // Enemy Attack
                // etc...
            }
            // Select Grid
            else
            {
                if (m_SelectedAction == null)
                {
                    ExecuteActionForUnits<CommandMoveAction>(
                        mouseGridPosition,
                        (u,a)=>
                        {
                            u.SwitchToNextState(u.GetAction<IdleAction>());
                        });
                }
            }
        }
    }

    private void ExecuteActionForUnits<TAction>(GridPosition gridPosition, Action<BaseObject, TAction> onActionComplete = null) where TAction : BaseAction
    {
        var units = m_SelectedObjects.Where(x => x.m_ObjectType == E_ObjectType.Unit).ToList();
        var filterObjects = FilterUnitsWithAction<TAction>(units);

        foreach (var (unit, action) in filterObjects)
        {
            if (!action.IsValidActionGridPosition(gridPosition))
                return;

            action.DestGirdPosition = gridPosition;
            unit.DirectCommand(action, onActionComplete);
            OnActionStarted?.Invoke(this, EventArgs.Empty);
        }
    }



    #region Select Object

    public void SetSelectedObject(BaseObject unit)
    {
        m_SelectedObjects.Add(unit);
        unit.OnSelected();

        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Deselect(BaseObject unit)
    {
        unit.OnDeselected();
        m_SelectedObjects.Remove(unit);
        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
    }

    public void DeselectAll()
    {
        foreach (BaseObject unit in m_SelectedObjects)
            unit.OnDeselected();
        m_SelectedObjects.Clear();
        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsSelectedObject(BaseObject unit)
    {
        return m_SelectedObjects.Contains(unit);
    }


    #endregion

    public void SetSelectedAction(BaseAction baseAction)
    {
        m_SelectedAction = baseAction;

        OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);

        Debug.Log($"({m_SelectedAction.GetActionName()}) Action 이 선택됨");
    }


    #region Command

    public void HandleCommand(object sender, EventArgs e)
    {
        if (m_SelectedObjects.Count == 0)
            return;
    }

    public List<BaseAction> GetCommonActionTypes(List<BaseObject> selectedUnits)
    {
        // 선택된 유닛 리스트가 비어있거나 null이면 빈 HashSet 반환
        if (selectedUnits == null || selectedUnits.Count == 0)
            return new List<BaseAction>();

        // 1. 유닛별 액션 타입 집합 만들기
        var commonTypes = selectedUnits
            .Select(unit =>
                System.Linq.Enumerable.ToHashSet(
                    unit.GetActions().Select(action => action.GetType())))
            .Aggregate((set1, set2) =>
            {
                set1.IntersectWith(set2); // 타입의 교집합
                return set1;
            });

        // 2. 첫 번째 유닛에서 공통 타입에 해당하는 액션 인스턴스 가져오기
        var firstUnitActions = selectedUnits[0].GetActions();

        return firstUnitActions
            .Where(action => commonTypes.Contains(action.GetType()))
            .ToList();
    }

    public List<(BaseObject unit, TAction action)> FilterUnitsWithAction<TAction>(List<BaseObject> selectedUnits) where TAction : BaseAction
    {
        return selectedUnits
            .Select(unit => (unit, action: unit.GetActions().OfType<TAction>().FirstOrDefault()))
            .Where(pair => pair.action != null)
            .ToList();
    }
    #endregion
}