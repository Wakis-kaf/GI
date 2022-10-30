using UGFramework.UI;

using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.UI
{

    public class UIEntryView : BaseView

    {

        // ++

        public override string BindPath()
        {
            return "Home/EntryView";
        }
        private RectTransform Btn_Rank;
        private RectTransform Btn_Level;
        private RectTransform Btn_Challenge;
        private RectTransform Btn_Introduce;
        private RectTransform Setting;
        protected override void BindVars()
        {
            this.Btn_Rank = this.GetVar<RectTransform>(0);
            this.Btn_Level = this.GetVar<RectTransform>(1);
            this.Btn_Challenge = this.GetVar<RectTransform>(2);
            this.Btn_Introduce = this.GetVar<RectTransform>(3);
            this.Setting = this.GetVar<RectTransform>(4);
        }
        // --

    }

}
