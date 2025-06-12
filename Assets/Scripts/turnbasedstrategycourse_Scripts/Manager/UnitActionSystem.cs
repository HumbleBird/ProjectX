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

    private BaseAction selectedAction;

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
            // Select Object
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, unitLayerMask)
                && hit.collider.TryGetComponent<BaseObject>(out BaseObject result))
            {
                // Enemy Attack
                // etc...
            }
            // Select Grid
            else
            {
                if(selectedAction is MoveAction)
                {
                    GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPositionOnlyHitVisible());

                    if (!selectedAction.IsValidActionGridPosition(mouseGridPosition))
                        return;

                    selectedAction.TakeAction(mouseGridPosition, null);

                    OnActionStarted?.Invoke(this, EventArgs.Empty);
                }
            }
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
        selectedAction = baseAction;

        OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);

        Debug.Log($"({selectedAction.GetActionName()}) Action �� ���õ�");
    }


    #region Command

    public void HandleCommand(object sender, EventArgs e)
    {
        if (m_SelectedObjects.Count == 0)
            return;

        var units = m_SelectedObjects.Where(x => x.m_ObjectType == E_ObjectType.Unit).ToList() ;

        List<BaseAction> commonActionTypes = GetCommonActionTypes(units);

        // �⺻ �̵� ����
        // ���� + �ǹ� ���� ���� ���׿��� �̵� ������ ������Ʈ�� �̵� ��Ű��
        MoveAction commonMoveAction = commonActionTypes
            .FirstOrDefault(x => x is MoveAction) as MoveAction;


        if (commonMoveAction != null)
        {
            SetSelectedAction(commonMoveAction);
            selectedAction = commonMoveAction;
        }
    }

    public List<BaseAction> GetCommonActionTypes(List<BaseObject> selectedUnits)
    {
        // ���õ� ���� ����Ʈ�� ����ְų� null�̸� �� HashSet ��ȯ
        if (selectedUnits == null || selectedUnits.Count == 0)
            return new List<BaseAction>();

        // 1. ���ֺ� �׼� Ÿ�� ���� �����
        var commonTypes = selectedUnits
            .Select(unit =>
                System.Linq.Enumerable.ToHashSet(
                    unit.GetActions().Select(action => action.GetType())))
            .Aggregate((set1, set2) =>
            {
                set1.IntersectWith(set2); // Ÿ���� ������
                return set1;
            });

        // 2. ù ��° ���ֿ��� ���� Ÿ�Կ� �ش��ϴ� �׼� �ν��Ͻ� ��������
        var firstUnitActions = selectedUnits[0].GetActions();

        return firstUnitActions
            .Where(action => commonTypes.Contains(action.GetType()))
            .ToList();
    }

    public  List<BaseObject> FilterUnitsWithAction<TAction>(List<BaseObject> selectedUnits) where TAction : BaseAction
    {
        return selectedUnits
            .Where(unit => unit.GetActions().Any(action => action is TAction))
            .ToList();
    }



    #endregion
}