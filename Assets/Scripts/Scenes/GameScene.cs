using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static Define;


public class GameScene : BaseScene
{
    public bool m_isNewGame = true;
    public bool m_isDeveling = true;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;


    }

    public void Start()
    {
    }

    public override void Clear()
    {
        
    }
}
