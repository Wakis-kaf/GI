using UGFramework.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace GameLogic.UI
{
    public class UILogin : BaseView
    {

        protected override void BindEvents()
        {
            base.BindEvents();
            this.btnLogin.onClick.AddListener(() => UIMgr.ShowUI<UIHome>());
        }
        // ++
        public override string BindPath()
        {
            return "login";
        }
        private TextMeshProUGUI txtTitle;
        private Button btnLogin;
        protected override void BindVars()
        {
            this.txtTitle = this.GetVar<TextMeshProUGUI>(0);
            this.btnLogin = this.GetVar<Button>(1);
        }
        // --
    }
}
