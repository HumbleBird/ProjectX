using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Define
{
    public enum E_MoveType
    {
        Basic,
        Patrol,
        Chase
    }

    public enum E_DamageType
    {
        Normal,                 // 일반 공격, 물리 방어력 만큼 데미지가 감소한다.
        Physical_Piercing,      // 유닛&건물의 물리 방어력을 무시한다.
        Magical_Piercing,       // 유닛&건물의 마법 방어력을 무시한다.
        Sige,                   //건물이나 구조물 오브젝트에 50%의 추가 피해를 입힌다.
                                //유닛 오브젝트에 50%의 감소 피해를 입힌다.                   
                                
    }

    public enum E_ObjectType
    {
        None = 0,
        Unit = 1,
        Building = 2,
        Interact = 3,

    }

    #region Base

    public enum E_TeamId
    {
        Player = 0,
        Monster = 1,
        NPC = 2,
    }

    public enum E_RandomSoundType
    {
        Damage,
        Block,
        WeaponWhoose
    }

    public enum Scene
    {
        Unknown = 0,
        Start = 1,
        Lobby = 2,
        Game = 3,
    }

    public enum Sound
    {
        Bgm = 0,
        Effect = 1,
        MaxCount,
    }

    public enum UIEvent
    {
        Click,
        Pressed,
        PointerDown,
        PointerUp,
        
    }

    public enum CursorType
    {
        None,
        Arrow,
        Hand,
        Look,
    }
    #endregion
}
