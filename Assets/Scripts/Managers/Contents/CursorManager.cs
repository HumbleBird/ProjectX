using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager 
{
    // Start is called before the first frame update
    public void Init()
    {
        Texture2D m_CursorImg = Managers.Resource.Load<Texture2D>("Art/Textures/UI/Cursors/G_Cursor_Basic2");
        Cursor.SetCursor(m_CursorImg, Vector2.zero, CursorMode.ForceSoftware);
    }

    public void PowerOn() 
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PowerOff()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
