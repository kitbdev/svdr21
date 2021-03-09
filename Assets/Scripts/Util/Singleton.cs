using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Singleton behaviour class, used for components that should only have one instance
/// </summary>
/// <typeparam name="T"></typeparam>
[DefaultExecutionOrder(-50)]
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;

    public static T Instance
    {
        get {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<T>();
            }
            return _instance;
        }
    }

    /// <summary>
    ///     Returns whether the instance has been initialized or not.
    /// </summary>
    // public static bool IsInitialized
    // {
    //     get { return Instance != null; }
    // }

    /// <summary>
    ///     Base awake method that sets the singleton's unique instance.
    /// </summary>
    protected virtual void Awake()
    {
        if (GameObject.FindObjectsOfType<T>().Length > 1)
        {
            Debug.LogErrorFormat("Trying to instantiate a second instance of singleton class {0}", GetType().Name);
        }
        // if (_instance != null)
        // else
        //     _instance = (T)this;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}
