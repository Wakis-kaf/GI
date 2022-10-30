using UGFramework.UI;

using UnityEngine;

using UnityEngine.UI;

namespace GameLogic.UI

{

    public class UILogin : BaseView

    {

        // ++

        public override string BindPath()
        {
            return "login";
        }
        private Button btnLogin;
        protected override void BindVars()
        {
            this.btnLogin = this.GetVar<Button>(0);
        }
        // --

    }

}
