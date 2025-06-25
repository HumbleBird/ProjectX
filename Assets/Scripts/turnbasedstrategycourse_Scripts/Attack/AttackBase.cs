using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class AttackBase : ScriptableObject
{
    [Header("Base Info")]
    public int ID;
    public string AttackName;                    // ��: "����3ĭ", "��ä��" ��
    public List<GridPosition> m_RangeOffset = new();   // ���� ���� ������ (���� ����)
    public float m_iCoolTime;                    // ��Ÿ�� (�� ����)
    public E_AttackType attackType;              // ����/���Ÿ�/���̺긮��
    public bool m_bIsCanCombo;
    public int m_iNextAttackPatternID;

    [Header("Damage Info")]
    public int m_iPhysicalAttackDamage;     // ���� ���� ������
    public int m_iMagicAttackDamage;        // �̹� ���� ������
    public int m_iPhysicalFixedDamage;      // ���� ���� ������
    public int m_iMagicFixedDamage;         // ���� ���� ������
    public float m_fPhysicalArmorPenetraion;    // ���� �� �����
    public float m_fMagicalArmorPenetraion;     // ���� �� �����

    [Header("Battle Attack Chance")]
    public float m_fCriticalChance;     // ġ��Ÿ��
    public float m_fCriticalDamageUp;   // ġ��Ÿ ������ ������
    public float m_fEvasion;            // ȸ����
    public float m_fCounterAttack;      // �ݰ���
    public float m_fAccuracy;           // ���߷�
    public float m_fAttackSpeed;        // ���� �ӵ�
    public float m_fKnockbackChance;    // �˹� Ȯ��
    public float m_fKnockbackRegist;    // �˹� ��Ȯ��    
}

