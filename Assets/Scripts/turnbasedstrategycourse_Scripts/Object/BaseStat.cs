using UnityEditor;
using UnityEngine;
using static Define;

#region Skill

// Passive
interface Attribute
{

}

interface ISkill
{
    string Id { get; set; }
    float cooldown { get; set; }
    bool IsReady { get; set; }

    void Initialize(BaseObject owner, SkillData data);
    void Activate(BaseObject target); // 혹은 Vector3 position
    void Update(float deltaTime);
}

class SkillData
{

}

#endregion
interface IUnitAbility
{
    void OnAttack(BaseObject target);
    void OnTakeDamage(ref float damage, E_DamageType type);
    void OnMove(Vector3 destination); 
}

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

    public int m_iMaxAttackRange; // 최대 공격 범위
    public int m_iMinAttackRange; // 최소 공격 범위 (레인저의 경우 근접 타일을 공격 못 한다)
    public int m_iDefaultMoveRange; // 기본 이동 거리
    public int m_iDetectRange; // 감지 거리
    public int m_iChaseRange; // 추격 거리

    [Header("Damage")]
    public int m_iPhysicalAttackDamage ;     // 물리 공격 데미지
    public int m_iMagicAttackDamage ;        // 미밥 공격 데미지
    public int m_iPhysicalFixedDamage ;      // 물리 고정 데미지
    public int m_iMagicFixedDamage ;         // 마법 고정 데미지
    public float m_fPhysicalArmorPenetraion;    // 물리 방어구 관통력
    public float m_fMagicalArmorPenetraion;     // 마법 방어구 관통력

    [Header("Defence")]
    public int m_iPhysicalDefence; // 물리 방어력
    public int m_iMagicalDefence ; // 마법 방어력

    [Header("Battle Attack Chance")]
    public float m_fCriticalChance ;     // 치명타율
    public float m_fCriticalDamageUp ;   // 치명타 데미지 증가율
    public float m_fEvasion ;            // 회피율
    public float m_fCounterAttack ;      // 반격율
    public float m_fAccuracy ;           // 명중률
    public float m_fAttackSpeed ;        // 공격 속도
    public float m_fKnockbackChance ;    // 넉백 확률
    public float m_fKnockbackRegist ;    // 넉백 저확률

    [Header("Cost")]
    public int m_iSpawnCost; // 소환 비용 (아군 유닛일 때만)
    public int m_iRewardCost ; // 처치 보상 (적군 유닛일 때만)


}




