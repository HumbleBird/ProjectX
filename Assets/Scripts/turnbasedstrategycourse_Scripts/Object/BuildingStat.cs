using UnityEngine;

[CreateAssetMenu(menuName = "Stat/BuildingStat")]
public class BuildingStat : BaseStat
{
    public int m_iReconstructionCost; // 재건 비용
    public int m_iDestructionCost; // 파괴 비용
}