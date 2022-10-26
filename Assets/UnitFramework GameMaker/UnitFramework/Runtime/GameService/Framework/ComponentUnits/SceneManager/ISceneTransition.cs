namespace UnitFramework.Runtime
{
    public interface ISceneTransition
    {
        public void OnStart();

        public void OnEnd();
        public void OnProgressUpdate(float progress);
    }
}