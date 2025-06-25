using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class AttackBase : ScriptableObject
{
    [Header("Base Info")]
    public int ID;
    public string AttackName;                    // 예: "전방3칸", "부채꼴" 등
    public List<GridPosition> m_RangeOffset = new();   // 공격 범위 오프셋 (유닛 기준)
    public float m_iCoolTime;                    // 쿨타임 (초 단위)
    public E_AttackType attackType;              // 근접/원거리/하이브리드
    public bool m_bIsCanCombo;
    public int m_iNextAttackPatternID;

    [Header("Damage Info")]
    public int m_iPhysicalAttackDamage;     // 물리 공격 데미지
    public int m_iMagicAttackDamage;        // 미밥 공격 데미지
    public int m_iPhysicalFixedDamage;      // 물리 고정 데미지
    public int m_iMagicFixedDamage;         // 마법 고정 데미지
    public float m_fPhysicalArmorPenetraion;    // 물리 방어구 관통력
    public float m_fMagicalArmorPenetraion;     // 마법 방어구 관통력

    [Header("Battle Attack Chance")]
    public float m_fCriticalChance;     // 치명타율
    public float m_fCriticalDamageUp;   // 치명타 데미지 증가율
    public float m_fEvasion;            // 회피율
    public float m_fCounterAttack;      // 반격율
    public float m_fAccuracy;           // 명중률
    public float m_fAttackSpeed;        // 공격 속도
    public float m_fKnockbackChance;    // 넉백 확률
    public float m_fKnockbackRegist;    // 넉백 저확률    
}

