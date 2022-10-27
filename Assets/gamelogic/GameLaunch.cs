using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitFramework.Runtime;
using GFramework.UI;
using GameLogic.UI;

public class GameLaunch : Procedure
{
    public override void OnEnter(ProcedureComponent procedureComponent)
    {
        base.OnEnter(procedureComponent);
        Log.Info("Load");
        UICanvas.Setup();
        UIMgr.ShowUI<UIEntry>();
    }
}
