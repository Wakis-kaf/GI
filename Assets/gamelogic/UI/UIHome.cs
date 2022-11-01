using UGFramework.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace GameLogic.UI
{
    public class UIHome : BaseView
    {
        // ++
        public override string BindPath()
        {
            return "home";
        }
        private TextMeshProUGUI txtTitle;
        private Button btnRank;
        private Button btnStartGame;
        private Button btnIntroduce;
        protected override void BindVars()
        {
            this.txtTitle = this.GetVar<TextMeshProUGUI>(0);
            this.btnRank = this.GetVar<Button>(1);
            this.btnStartGame = this.GetVar<Button>(2);
            this.btnIntroduce = this.GetVar<Button>(3);
        }
        // --
    }
}
