using System;
using UnitFramework.Runtime;
using GFramework.UI;
using UnityEngine;
using UnityEngine.UI;
namespace GameLogic.UI
{
    public class UIEntry : BaseView
    {

        protected override void BindEvents()
        {
            base.BindEvents();
            this.btnRank.onClick.AddListener(OnClickRank);
        }

        private void OnClickRank()
        {
            Log.DebugInfo("zhangjian sb");
        }

        // ++
    public override string BindPath()
    {
        return "Home/EntryView [首页]";
    }
    private Button btnRank;
    private Button btnLevel;
    private Button btnChallenge;
    private Button btnIntroduce ;
    protected override void BindVars() {
        this.btnRank = this.GetVar<Button>(0);
        this.btnLevel = this.GetVar<Button>(1);
        this.btnChallenge = this.GetVar<Button>(2);
        this.btnIntroduce  = this.GetVar<Button>(3);
    }
        // --
    }
}
