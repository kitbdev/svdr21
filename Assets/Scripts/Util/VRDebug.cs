using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[DefaultExecutionOrder(-50)]
public class VRDebug : Singleton<VRDebug>
{
    public int maxMsgs = 10;
    TMP_Text textEl;
    List<string> msgs = new List<string>();
    List<float> msgTimeouts = new List<float>();

    protected override void Awake()
    {
        base.Awake();
        textEl = GetComponent<TMP_Text>();
        Clear();
    }
    private void Update()
    {
        for (int i = 0; i < msgTimeouts.Count; i++)
        {
            float timeout = msgTimeouts[i];
            if (timeout > 0 && Time.unscaledTime > timeout)
            {
                // remove that msg
                msgs.RemoveAt(i);
                msgTimeouts.RemoveAt(i);
                UpdateText();
                i--;
            }
        }
    }
    public static void Log(string msg, float timeout = -1, Object debugContext = null)
    {
        LogP(msg, timeout > 0 ? timeout : 2, true, debugContext);
    }
    public static void LogP(string msg, float timeout = -1, bool alsoDebug = true, Object debugContext = null)
    {
        Instance.ILog(msg, timeout, alsoDebug, debugContext);
    }
    public static void Clear()
    {
        Instance.IClear();
    }
    // protected static IEnumerator waitToLog(string msg, float timeout = -1, bool alsoDebug = true)
    // {
    //     yield return new WaitForEndOfFrame();
    //     Instance.ILog(msg, timeout, alsoDebug);
    // }
    void ILog(string msg, float timeout = -1, bool alsoDebug = true, Object debugContext = null)
    {
        msgs.Add(msg);
        if (alsoDebug)
        {
            if (debugContext != null)
            {
                Debug.Log("VRLOG:" + msg, debugContext);
            } else
            {
                Debug.Log("VRLOG:" + msg);
            }
        }
        msgTimeouts.Add(timeout > 0 ? Time.unscaledTime + timeout : -1);
        UpdateText();
    }
    void IClear()
    {
        msgs.Clear();
        msgTimeouts.Clear();
        UpdateText();
    }
    void UpdateText()
    {
        if (msgs.Count > maxMsgs)
        {
            msgs.RemoveRange(0, msgs.Count - maxMsgs);
            msgTimeouts.RemoveRange(0, msgTimeouts.Count - maxMsgs);
        }
        string fullMsg = "";
        foreach (var msg in msgs)
        {
            fullMsg += msg + "\n";
        }
        textEl.text = fullMsg;
    }
}