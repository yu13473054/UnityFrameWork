
public class Singleton<T> where T : new()
{
    private static T _inst;
    private static readonly object locker = new object();

    public static T Inst
    {
        get
        {
            if (_inst == null)
            {
                lock (locker)
                {
                    if (_inst == null)
                        _inst = new T();
                }
            }
            
            return _inst;
        }
    }

    public virtual void Init()
    {
        
    }

    public virtual void OnDestroy()
    {
        _inst = default(T);
    }
}
