namespace UnitFramework.Runtime
{
    public interface IModelController : IController
    {
        
        public void OnCtrViewModelConnected(CVBlackBoard cvBlackBoard);
        
        public void OnCtrViewModelDisConnected(CVBlackBoard cvBlackBoard);
        public void OnViewModelConnected(IViewer viewer);
        public void OnViewModelDisConnected(IViewer viewer);
    }
}