using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Define;

public class BaseStat : ScriptableObject
{
    [Header("Base")]
    public int ID;
    public string Name ;
    public int m_iMaxHP ;        // 최대 체력
    public int m_iCurrentHp ;    // 현재 체력

    public int m_iMaxMP ;     // 최대 마나
    public int m_iCurrentMP ; // 현재 마나

    public float m_fMoveSpeed ; //   기본 이동 속도
    public float m_fChaseSpeed ; //  정찰 이동 속도
    public float m_fPatrolSpeed ; // 추격 이동 속도

    public GridPosition m_iMaxAttackRange; // 최대 공격 범위
    public GridPosition m_iMinAttackRange; // 최소 공격 범위 (레인저의 경우 근접 타일을 공격 못 한다)
    public int m_iDefaultMoveRange; // 기본 이동 거리
    public int m_iDetectRange; // 감지 거리
    public int m_iChaseRange; // 추격 거리

    [Header("Defence")]
    public int m_iPhysicalDefence; // 물리 방어력
    public int m_iMagicalDefence; // 마법 방어력

    [Header("Cost")]
    public int m_iSpawnCost; // 소환 비용 (아군 유닛일 때만)
    public int m_iRewardCost ; // 처치 보상 (적군 유닛일 때만)

    [Header("Attack Pattern")]
    public List<AttackPattern> attackPatterns = new List<AttackPattern>();
    public List<Skill> Skills;                   // 연결된 스킬 (다단히트 or 이펙트 등)
}




