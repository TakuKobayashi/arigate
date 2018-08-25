using UnityEngine;

public abstract class SingletonBehaviour<T> : MonoBehaviour
    where T : MonoBehaviour
{
    private static T instance;

    [SerializeField]
    bool dontDestroyOnLoad;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType(typeof(T));
                if (instance == null) throw new System.Exception("Error: " + typeof(T) + " is nothing.");
            }
            return instance;
        }
    }

    void Awake()
    {
        CheckInstance();
    }

    protected bool CheckInstance()
    {
        if (this == Instance)
        {
            if (dontDestroyOnLoad) DontDestroyOnLoad(this.gameObject);
            SingleAwake();
            return true;
        }
        Destroy(this.gameObject);
        return false;
    }

    public virtual void SingleAwake() { }

    public static bool Exist
    {
        get
        {
            return instance != null;
        }
    }
}
