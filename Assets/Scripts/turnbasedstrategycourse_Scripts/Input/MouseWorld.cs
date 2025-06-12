using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseWorld : MonoBehaviour
{
    public static MouseWorld Instance { get; private set; }

    public event EventHandler OnMouseDownChanged;
    public event EventHandler OnMouseUpChanged;

    [SerializeField] private LayerMask mousePlaneLayerMask;

    [Header("Selection")]
    [SerializeField] private RectTransform SelectionBox;
    private Vector2 startPosition;
    [SerializeField]
    private float DragDelay = 0.1f;
    public LayerMask m_BaseObjectLayer;

    private float MouseDownTime;

    private void Awake()
    {
        Instance = this;
        SelectionBox.gameObject.SetActive(false);

        m_BaseObjectLayer = 1 << LayerMask.NameToLayer("Building") | 1 << LayerMask.NameToLayer("Units");
    }

    public static Vector3 GetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
        Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, Instance.mousePlaneLayerMask);
        return raycastHit.point;
    }

    public static Vector3 GetPositionOnlyHitVisible()
    {
        Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
        RaycastHit[] raycastHitArray = Physics.RaycastAll(ray, float.MaxValue, Instance.mousePlaneLayerMask);
        System.Array.Sort(raycastHitArray, (RaycastHit raycastHitA, RaycastHit raycastHitB) =>
        {
            return Mathf.RoundToInt(raycastHitA.distance - raycastHitB.distance);
        });

        foreach (RaycastHit raycastHit in raycastHitArray)
        {
            if (raycastHit.transform.TryGetComponent(out Renderer renderer))
            {
                if (renderer.enabled)
                {
                    return raycastHit.point;
                }
            }
        }
        
        return Vector3.zero;
    }

    private void Update()
    {
        HandleSelectionInputs();
    }

    private void HandleSelectionInputs()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Left Mosue Button Pressed
            startPosition = Input.mousePosition;
            SelectionBox.gameObject.SetActive(true);
            SelectionBox.sizeDelta = Vector3.zero;
            MouseDownTime = Time.time;

            OnMouseDownChanged?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButton(0) && MouseDownTime + DragDelay < Time.time)
        {
            ResizeSelectionBox();
        }

        if (Input.GetMouseButtonUp(0))
        {
            SelectionBox.sizeDelta = Vector3.zero;
            SelectionBox.gameObject.SetActive(false);

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, m_BaseObjectLayer)
                && hit.collider.TryGetComponent<BaseObject>(out BaseObject unit))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if(UnitActionSystem.Instance.IsSelectedObject(unit))
                    {
                        UnitActionSystem.Instance.Deselect(unit);
                    }
                    else
                    {
                        UnitActionSystem.Instance.SetSelectedObject(unit);
                    }
                }
                else
                {
                    UnitActionSystem.Instance.DeselectAll();
                    UnitActionSystem.Instance.SetSelectedObject(unit);
                }
            }
            // Deselect all if it's a short click, not a drag
            else if (MouseDownTime + DragDelay > Time.time)
            {
                //UnitActionSystem.Instance.DeselectAll();
            }

            MouseDownTime = 0;
            OnMouseUpChanged?.Invoke(this, EventArgs.Empty);
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            UnitActionSystem.Instance.DeselectAll();

        }
    }

    private void ResizeSelectionBox()
    {
        float width = Input.mousePosition.x - startPosition.x;
        float height = Input.mousePosition.y - startPosition.y;

        SelectionBox.anchoredPosition = startPosition + new Vector2(width / 2, height / 2);
        SelectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

        Bounds bounds = new Bounds(SelectionBox.anchoredPosition, SelectionBox.sizeDelta);

        var list = UnitActionSystem.Instance.AvailableUnits.Where(x => !x.IsEnemy()).ToList();
        for (int i = 0; i < list.Count; i++)
        {
            if (UnitIsInSelectionBox(Camera.main.WorldToScreenPoint(list[i].transform.position), bounds))
            {
                UnitActionSystem.Instance.SetSelectedObject(list[i]);
            }
            else
            {
                UnitActionSystem.Instance.Deselect(list[i]);
            }
        }
    }

    private bool UnitIsInSelectionBox(Vector2 position, Bounds bounds)
    {
        return position.x > bounds.min.x && position.x < bounds.max.x
            && position.y > bounds.min.y && position.y < bounds.max.y;
    }
}