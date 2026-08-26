namespace CDTU.Utils
{
    /// <summary>
    /// Persistent singleton convenience base.
    /// </summary>
    public abstract class SingletonDD<T> : Singleton<T> where T : SingletonDD<T>
    {
        protected sealed override bool PersistAcrossScenes => true;
    }
}
