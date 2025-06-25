using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static Define;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using Random = System.Random;

public class CombatAction : BaseAction
{
    public event EventHandler<OnAttackStartedEventArge> OnStartAttack;
    public event EventHandler OnEndAttack;

    public class OnAttackStartedEventArge : EventArgs
    {
        public E_AttackType attackType;
    }

    Func<bool> conditionPase;
    bool isChaningPase;
    float coolTick;

    protected override void Update()
    {
        base.Update();
        
        if (m_BaseObject.m_CurrentAction != this)
            return;

        // 1. ������ Ư�� ������ ��� ������ ��ȯ (��: 2������ ����)
        HandlePhaseTransition();

        if (isChaningPase)
            return;

        if (!CanAttackAtCoolTime())
            return;

        // 3. Ÿ�� �Ÿ� Ȯ�� �� ���� ��� ����
        var target = m_BaseObject.m_Target;

        // 3.1 Live or Dead?
        if (target == null || target.m_StatSystem.m_IsDead)
        {
            target = null;
            return;
        }

        // 3.2 Distance
        // TODO Change
        var distance = LevelGrid.Instance.IsTargetInAttackRange(target.GetGridPosition(), m_BaseObject.GetGridPosition());

        if (distance == E_Distance.Far)
            return;

        // Set Attack Pattern
        AttackPattern attackPattern = SelectAttackPattern();

        if (attackPattern == null)
        {
            Debug.Log($"m_ThisAttackPattern is Nul!!!!");
            return;
        }

        // Event ���� (Animation)
        OnStartAttack?.Invoke(this, new OnAttackStartedEventArge
        {
            attackType = attackPattern.attackType
        });

        // 5. ������ ����
        Damage(attackPattern);

        // 6. ��Ÿ�� ����
        UpdateAttackCooldown(attackPattern);
    }


    public override BaseAction TakeAction(GridPosition gridPosition = default, Action onActionComplete = null)
    {
        var target = m_BaseObject.m_Target;

        if (target == null || target.m_StatSystem.m_IsDead)
        {
            return m_BaseObject.GetAction<IdleAction>();
        }

        // UpdateRotationDirToTarget
        
        // Regular move logic
        Vector3 moveDirection = (target.transform.position - m_BaseObject.transform.position).normalized;

        float rotateSpeed = 10f;
        m_BaseObject.transform.forward = Vector3.Slerp(m_BaseObject.transform.forward, moveDirection, Time.deltaTime * rotateSpeed);

        // ���� ���� �ȿ� �ִٸ� ����
        if (LevelGrid.Instance.IsTargetInAttackRange(m_BaseObject.GetGridPosition(), target.GetGridPosition()) == E_Distance.Proper)
        {
            // Todo Check
            // �ּ� ���� �Ÿ� ���ʺ��� ������ �Ÿ� �����ֱ�
            return this;
        }
        else if (LevelGrid.Instance.IsTargetInAttackRange(m_BaseObject.GetGridPosition(), target.GetGridPosition()) == E_Distance.Proper)
        {
            return m_BaseObject.GetAction<ChaseAction>();
        }
        else
            return null;
    }

    public override string GetActionName()
    {
        return "Combat";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        throw new NotImplementedException();
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        throw new NotImplementedException();
    }

    public void HandlePhaseTransition()
    {
        if(conditionPase != null && conditionPase.Invoke())
        {

        }
    }

    private bool CanAttackAtCoolTime()
    {
        if(coolTick <= 0)
        {
            coolTick = 0;
            return true;
        }
        else
        {
            coolTick -= Time.deltaTime;
            return false;
        }
    }

    private AttackPattern SelectAttackPattern()
    {
        // ���� ��ġ�� �� ��ġ���� �Ÿ� ���� ���ϰ�.
        // �ش� �� ��ġ�� ������ �� �ִ� ���ݵ��� ���͸� �� ����Ʈ�� �̰�
        // �� ����Ʈ �� �������� �� ���� �̴´�.

        List<AttackPattern> patterns = m_BaseObject.m_StatSystem.m_Stat.attackPatterns;

        GridPosition selfPos = m_BaseObject.GetGridPosition();
        GridPosition targetPos = m_BaseObject.m_Target.GetGridPosition();
        E_Dir dir = LevelGrid.Instance.GetDirGridPosition(selfPos, targetPos);

        // ��ȿ�� ���� ���͸�
        List<AttackPattern> validPatterns = patterns
            .Where(pattern =>
                pattern.m_RangeOffset.Any(offset =>
                    LevelGrid.Instance.ToGridPosition(offset, selfPos, dir) == targetPos
                )
            ).ToList();

        if (validPatterns.Count == 0)
            return null; // ���� ������ ������ ���ٸ� null


        Console.WriteLine("������ ���ݵ� : " + string.Join(" ", validPatterns));

        // �������� �ϳ� ����
        int index = UnityEngine.Random.Range(0, validPatterns.Count);
        return validPatterns[index];
    }

    private void Damage(AttackBase attack)
    {
        GridPosition selfPos = m_BaseObject.GetGridPosition();
        GridPosition targetPos = m_BaseObject.m_Target.GetGridPosition();
        E_Dir dir = LevelGrid.Instance.GetDirGridPosition(selfPos, targetPos);

        List<GridPosition> attackGridPostison = 
            attack.m_RangeOffset
            .Select(x => LevelGrid.Instance.ToGridPosition(x, selfPos, dir)).ToList();

        // attack ���� ���� �ִ� �׸����� ��� �� ������Ʈ ������ �ֱ�
        var targets = attackGridPostison
            .Select(p => LevelGrid.Instance.GetUnitAtGridPosition(p))
            .Where(unit => unit != null && unit.IsEnemy() != m_BaseObject.IsEnemy())
            .ToList();

        foreach (var target in targets)
        {
            target.Hit(attack);
            Debug.Log($"Attack : {target.name}");
        }
    }

    private void UpdateAttackCooldown(AttackBase attack)
    {
        coolTick = attack.m_iCoolTime;
    }
}
