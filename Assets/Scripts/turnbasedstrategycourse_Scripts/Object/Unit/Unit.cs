using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : BaseObject
{
    protected override void Awake()
    {
        base.Awake();

        m_ObjectType = Define.E_ObjectType.Unit;
    }


}