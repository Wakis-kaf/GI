using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitFramework.Runtime;

namespace UGFramework.UI
{
    public class BaseControl : Unit, IModelController
    {
        public string ControllerName => "BaseControl";

        protected CVBlackBoard cvBlackBoard;
        protected IViewer viewer;

        public virtual void ControllerUpdate() { }

        public void OnCtrViewModelConnected(CVBlackBoard cvBlackBoard)
        {
            this.cvBlackBoard = cvBlackBoard;
        }

        public void OnCtrViewModelDisConnected(CVBlackBoard cvBlackBoard)
        {
            this.cvBlackBoard = null;
        }

        public void OnViewModelConnected(IViewer viewer)
        {
            this.viewer = viewer;
        }

        public void OnViewModelDisConnected(IViewer viewer)
        {
            this.viewer = null;
        }
    }
}