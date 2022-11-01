using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitFramework.Runtime;
using UGFramework.UI;
using GameLogic.UI;

public class GameLaunch : Procedure
{
    public override void OnEnter(ProcedureComponent procedureComponent)
    {
        base.OnEnter(procedureComponent);
        UICanvas.Setup();
        UIMgr.ShowUI<UILogin>();
    }
}
