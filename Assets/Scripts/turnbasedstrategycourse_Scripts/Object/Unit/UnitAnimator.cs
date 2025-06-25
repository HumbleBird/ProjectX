using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

public class UnitAnimator : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private Transform bulletProjectilePrefab;
    [SerializeField] private Transform shootPointTransform;
    [SerializeField] private Transform rifleTransform;
    [SerializeField] private Transform swordTransform;

    [SerializeField] private Transform actionTransform;


    private void Awake()
    {
        if (actionTransform.TryGetComponent<CommandMoveAction>(out CommandMoveAction CommandMoveAction))
        {
            CommandMoveAction.OnStartMoving += MoveAction_OnStartMoving;
            CommandMoveAction.OnStopMoving += MoveAction_OnStopMoving;
            CommandMoveAction.OnChangedFloorsStarted += MoveAction_OnChangedFloorsStarted;
        }
        if (actionTransform.TryGetComponent<ChaseAction>(out ChaseAction chaseAction))
        {
            chaseAction.OnStartMoving += MoveAction_OnStartMoving;
            chaseAction.OnStopMoving += MoveAction_OnStopMoving;
            chaseAction.OnChangedFloorsStarted += MoveAction_OnChangedFloorsStarted;
        }

        if (actionTransform.TryGetComponent<ShootAction>(out ShootAction shootAction))
        {
            shootAction.OnShoot += ShootAction_OnShoot;
        }

        if (actionTransform.TryGetComponent<SwordAction>(out SwordAction swordAction))
        {
            swordAction.OnSwordActionStarted += SwordAction_OnSwordActionStarted;
            swordAction.OnSwordActionCompleted += SwordAction_OnSwordActionCompleted;
        }

        if (actionTransform.TryGetComponent<CombatAction>(out CombatAction combatAction))
        {
            combatAction.OnStartAttack += CombatAction_OnStartAttack;
            combatAction.OnEndAttack += CombatAction_OnEndAttack;
        }
    }

    private void MoveAction_OnChangedFloorsStarted(object sender, MoveAction.OnChangeFloorsStartedEventArgs e)
    {
        if (e.targetGridPosition.floor > e.unitGridPosition.floor)
        {
            // Jump
            animator.SetTrigger("JumpUp");
        } else
        {
            // Drop
            animator.SetTrigger("JumpDown");
        }
    }

    private void Start()
    {
        EquipRifle();
    }

    private void SwordAction_OnSwordActionCompleted(object sender, EventArgs e)
    {
        EquipRifle();
    }

    private void SwordAction_OnSwordActionStarted(object sender, EventArgs e)
    {
        EquipSword();
        animator.SetTrigger("SwordSlash");
    }

    private void MoveAction_OnStartMoving(object sender, EventArgs e)
    {
        animator.SetBool("IsWalking", true);
    }

    private void MoveAction_OnStopMoving(object sender, EventArgs e)
    {
        animator.SetBool("IsWalking", false);
    }

    private void ShootAction_OnShoot(object sender, ShootAction.OnShootEventArgs e)
    {
        animator.SetTrigger("Shoot");

        Transform bulletProjectileTransform = 
            Instantiate(bulletProjectilePrefab, shootPointTransform.position, Quaternion.identity);

        BulletProjectile bulletProjectile = bulletProjectileTransform.GetComponent<BulletProjectile>();

        Vector3 targetUnitShootAtPosition = e.targetUnit.GetWorldPosition();

        float unitShoulderHeight = 1.7f;
        targetUnitShootAtPosition.y += unitShoulderHeight;

        bulletProjectile.Setup(targetUnitShootAtPosition);
    }

    private void EquipSword()
    {
        swordTransform.gameObject.SetActive(true);
        rifleTransform.gameObject.SetActive(false);
    }

    private void EquipRifle()
    {
        swordTransform.gameObject.SetActive(false);
        rifleTransform.gameObject.SetActive(true);
    }

    private void CombatAction_OnStartAttack(object sender, CombatAction.OnAttackStartedEventArge e)
    {
        if(e.attackType == Define.E_AttackType.Melee)
        {
            animator.SetTrigger("Melee");
        }
        else if (e.attackType == Define.E_AttackType.Ranged)
        {
            animator.SetTrigger("Ranged");
        }
    }

    private void CombatAction_OnEndAttack(object sender, EventArgs e)
    {

    }

}
