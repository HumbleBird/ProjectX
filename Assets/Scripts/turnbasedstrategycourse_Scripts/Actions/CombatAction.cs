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
    float timer = 0;
    [SerializeField] float m_rotateTick = 0.1f;
    [SerializeField] float rotateSpeed = 70;

    BaseAction m_TODOChangeAction;

    protected override void Update()
    {
        base.Update();
        
        if (m_BaseObject.m_CurrentAction != this)
            return;

        if (RotateTowardTarget() == false)
            return;

        // 1. 유닛이 특수 상태일 경우 페이즈 전환 (예: 2페이즈 보스)
        HandlePhaseTransition();

        if (isChaningPase)
            return;

        if (!CanAttackAtCoolTime())
            return;

        // 3. 타겟 거리 확인 및 공격 방식 결정
        var target = m_BaseObject.m_Target;

        // 3.1 Live or Dead?
        if (target == null || target.m_StatSystem.m_IsDead)
        {
            target = null;
            return;
        }


        // Set Attack Pattern
        AttackPattern attackPattern = SelectAttackPattern();

        if (attackPattern == null)
        {
            // 현재 위치에서 공격할 수 있는 공격이 없음.
            m_TODOChangeAction = m_BaseObject.GetAction<ChaseAction>();
            Debug.Log($"m_ThisAttackPattern is Nul!!!!");
            return;
        }

        // Event 실행 (Animation)
        OnStartAttack?.Invoke(this, new OnAttackStartedEventArge
        {
            attackType = attackPattern.attackType
        });

        // 5. 데미지 적용
        Damage(attackPattern);

        // 6. 쿨타임 갱신
        UpdateAttackCooldown(attackPattern);
    }

    private bool RotateTowardTarget()
    {
        var target = m_BaseObject.m_Target;
        if (target == null)
            return false;

        // 타겟 방향 계산
        Vector3 moveDirection = (target.transform.position - m_BaseObject.transform.position).normalized;

        // 회전 완료 여부 판단
        float angleThreshold = 5f; // 허용 오차 각도 (예: 5도)
        float angle = Vector3.Angle(m_BaseObject.transform.forward, moveDirection);

        if (angle < angleThreshold)
        {
            return true;
        }
        else
        {
            timer -= Time.deltaTime;
            if(timer <= 0)
            {
                timer = m_rotateTick;

                // 회전
                m_BaseObject.transform.forward = Vector3.Slerp(
                    m_BaseObject.transform.forward,
                    moveDirection,
                    Time.deltaTime * rotateSpeed
                );
            }

            return false;
        }
    }


    public override BaseAction TakeAction(GridPosition gridPosition = default, Action onActionComplete = null)
    {
        if (m_BaseObject.m_Target == null || m_BaseObject.m_Target.m_StatSystem.m_IsDead)
            return m_BaseObject.GetAction<IdleAction>();

        if(m_TODOChangeAction != null)
        {
            BaseAction ac = m_TODOChangeAction;
            m_TODOChangeAction = null;
            return ac;
        }
        else
            return this;
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
        // 적의 위치와 내 위치와의 거리 차를 구하고.
        // 해당 적 위치에 공격할 수 있는 공격들을 필터링 후 리스트로 뽑고
        // 그 리스트 중 랜덤으로 한 개를 뽑는다.

        List<AttackPattern> patterns = m_BaseObject.m_StatSystem.m_Stat.attackPatterns;

        GridPosition selfPos = m_BaseObject.GetGridPosition();
        GridPosition targetPos = m_BaseObject.m_Target.GetGridPosition();
        E_Dir dir = LevelGrid.Instance.GetDirGridPosition(selfPos, targetPos);

        // 유효한 패턴 필터링
        List<AttackPattern> validPatterns = patterns
            .Where(pattern =>
                pattern.m_RangeOffset.Any(offset =>
                    LevelGrid.Instance.ToGridPosition(offset, selfPos, dir) == targetPos
                )
            ).ToList();

        if (validPatterns.Count == 0)
            return null; // 공격 가능한 패턴이 없다면 null


        Console.WriteLine("가능한 공격들 : " + string.Join(" ", validPatterns));

        // 무작위로 하나 선택
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

        // attack 범위 내에 있는 그리드의 모든 적 오브젝트 데미지 주기
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
