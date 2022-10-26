using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnitFramework.Runtime
{
    public class CVBlackBoard : BlackBoard
    {
        private IViewer m_Viewer;
        private string m_BlackBoardName;
        private IModelController m_Controller;

        public IViewer Viewer => m_Viewer;
        public string BlackBoardName => m_BlackBoardName;
        public IModelController Controller => m_Controller;

        public CVBlackBoard(IModelController controller, IViewer viewer, string blackBoardName) : base(blackBoardName)
        {
            m_Viewer = viewer;
            m_Controller = controller;
            m_BlackBoardName = blackBoardName;
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();

            if (!ReferenceEquals(m_Viewer, null)) m_Controller?.OnViewModelDisConnected(m_Viewer);
            if (!ReferenceEquals(m_Controller, null)) m_Viewer?.OnCtrModelDisConnected(m_Controller);
            m_Controller?.OnCtrViewModelDisConnected(this);
            m_Viewer?.OnCtrViewModelDisConnected(this);
        }

        public bool IsEqual(IModelController controller, IViewer viewer)
        {
            return controller == m_Controller && viewer == m_Viewer;
        }

        public bool IsEqual(IModelController controller, IViewer viewer, string connectionName)
        {
            return controller == m_Controller &&
                   viewer == m_Viewer &&
                   (string.IsNullOrEmpty(connectionName) || connectionName == m_BlackBoardName);
        }
    }

    public static class UnitModelExtend
    {
        private static List<CVBlackBoard> mBBsConnectedCtrAndModel = new List<CVBlackBoard>();

        // 绑定视图
        public static CVBlackBoard BindViewer(this ModelModule modelModule, IModelController controller, IViewer viewer)
        {
            //modelModule.AddBlackBoard()
            if (modelModule.TryGetCVBlackBoard(controller, viewer, out var res))
            {
                return res;
            }

            return modelModule.GetAndCreateCVBlackBoard(controller, viewer, "");
        }

        public static CVBlackBoard BindViewer(this ModelModule modelModule, IModelController controller, IViewer viewer,
            string bbName)
        {
            if (modelModule.TryGetCVBlackBoard(controller, viewer, out var res, bbName))
            {
                return res;
            }

            return modelModule.GetAndCreateCVBlackBoard(controller, viewer, bbName);
        }

        public static CVBlackBoard GetAndCreateCVBlackBoard(this ModelModule modelModule, IModelController controller,
            IViewer viewer, string bbName)
        {
            // 判断是否该连接已经存在
            if (modelModule.TryGetCVBlackBoard(controller, viewer, out var res, bbName))
            {
                return res;
            }

            CVBlackBoard cvBlackBoard = new CVBlackBoard(controller, viewer, bbName);
            modelModule.AddBlackBoard(cvBlackBoard);
            mBBsConnectedCtrAndModel.Add(cvBlackBoard);

            // 触发事件
            controller?.OnCtrViewModelConnected(cvBlackBoard);
            viewer?.OnCtrViewModelConnected(cvBlackBoard);
            if (!ReferenceEquals(viewer, null))
            {
                controller?.OnViewModelConnected(viewer);
            }
            else
            {
                Log.Warning($"Viewer null warning{viewer.OwnerUnit.UnitName}");
            }


            if (!ReferenceEquals(controller, null))
            {
                viewer?.OnCtrModelConnected(controller);
            }
            else
            {
                Log.Warning($"controller null warning{controller.OwnerUnit.UnitName}");
            }

            return cvBlackBoard;
        }

        public static bool TryGetCVBlackBoard(this ModelModule modelModule, IModelController controller, IViewer viewer,
            out CVBlackBoard cvBlackBoard, string name = "")
        {
            cvBlackBoard = default;
            for (int i = 0; i < mBBsConnectedCtrAndModel.Count; i++)
            {
                var connect = mBBsConnectedCtrAndModel[i];
                if (connect.IsEqual(controller, viewer, name))
                {
                    cvBlackBoard = connect;
                    return true;
                }
            }

            return false;
        }

        public static bool HasCVBlackBoard(this ModelModule modelModule, IModelController controller, IViewer viewer)
        {
            for (int i = 0; i < mBBsConnectedCtrAndModel.Count; i++)
            {
                var connect = mBBsConnectedCtrAndModel[i];
                if (connect.IsEqual(controller, viewer))
                {
                    return true;
                }
            }

            return false;
        }

        public static CVBlackBoard GetCVBlackBoard(this ModelModule modelModule, IModelController controller,
            IViewer viewer, string connectionName = "")
        {
            for (int i = 0; i < mBBsConnectedCtrAndModel.Count; i++)
            {
                var connect = mBBsConnectedCtrAndModel[i];
                if (connect.IsEqual(controller, viewer, connectionName))
                {
                    return connect;
                }
            }

            return null;
        }

        public static CVBlackBoard[] GetCVBlackBoards(this ModelModule modelModule, IModelController controller,
            IViewer viewer, string connectionName = "")
        {
            List<CVBlackBoard> res = new List<CVBlackBoard>();
            for (int i = 0; i < mBBsConnectedCtrAndModel.Count; i++)
            {
                var connect = mBBsConnectedCtrAndModel[i];
                if (connect.IsEqual(controller, viewer, connectionName))
                {
                    res.Add(connect);
                }
            }

            return res.ToArray();
        }
    }
}