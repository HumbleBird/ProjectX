using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;



public class StatSystem : MonoBehaviour
{
    public event EventHandler OnDead;
    public event EventHandler OnDamaged;
    public event EventHandler OnMPUsed;
    public BaseStat m_Stat;

    [SerializeField] private int health { get => m_Stat.m_iCurrentHp;  set { m_Stat.m_iCurrentHp = value; } }
    private int healthMax { get => m_Stat.m_iMaxHP; set { m_Stat.m_iMaxHP = value; } }

    [SerializeField] private int mp { get => m_Stat.m_iCurrentMP;  set { m_Stat.m_iCurrentMP = value; } }
    private int mpMax { get => m_Stat.m_iMaxMP; set { m_Stat.m_iMaxMP = value; } }

    public bool m_IsDead => health == 0;

    private void Awake()
    {
        // HP
        health = healthMax;

        // MP
        mp = mpMax;

        // Set Attack Range
        int maxX = 0;
        int minX = 0;
        int maxZ = 0;
        int minZ = 0;
        int maxFloor = 0;
        int minFllor = 0;

        foreach (var attack in m_Stat.attackPatterns)
        {
            foreach (GridPosition offset in attack.m_RangeOffset)
            {
                if (offset.x > maxX) maxX = offset.x;
                if (offset.x < minX) minX = offset.x;
                if (offset.z > maxZ) maxZ = offset.z;
                if (offset.z < minZ) minZ = offset.z;
                if (offset.floor > maxFloor) maxFloor = offset.floor;
                if (offset.floor < minFllor) minFllor = offset.floor;
            }
        }

        m_Stat.m_iMaxAttackRange = new GridPosition(maxX, maxZ, maxFloor);
        m_Stat.m_iMinAttackRange = new GridPosition(minX, minZ, minFllor);
    }

    public void ReduceHP(int damageAmount)
    {
        health -= damageAmount;

        if (health < 0)
        {
            health = 0;
        }

        OnDamaged?.Invoke(this, EventArgs.Empty);

        if (health == 0)
        {
            Die();
        }
    }

    public void UseMPSkill(int count)
    {
        mp = Math.Max(0, mp - count);

        OnMPUsed?.Invoke(this, EventArgs.Empty);
    }

    private void Die()
    {
        OnDead?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return (float)health / healthMax;
    }

    public float GetManaNormalized()
    {
        return (float)mp / mpMax;
    }

    public bool IsManaCharacter()
    {
        return m_Stat.m_iMaxMP > 0;
    }
}
