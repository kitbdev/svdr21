using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Shows debug logs in VR
/// </summary>
[DefaultExecutionOrder(-50)]
public class VRDebug : Singleton<VRDebug>
{
    public bool canLog = true;
    public int maxMsgs = 10;
    TMP_Text textEl;
    List<string> msgs = new List<string>();
    List<float> msgTimeouts = new List<float>();

    protected override void Awake()
    {
        base.Awake();
        textEl = GetComponentInChildren<TMP_Text>();
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
    /// <summary>
    /// Log for only a frame
    /// </summary>
    /// <param name="msg">message content</param>
    /// <param name="debugContext">debug log context object</param>
    public static void LogFrame(string msg, Object debugContext = null)
    {
        LogP(msg, 0.01f, true, debugContext);
    }
    /// <summary>
    /// Log a message to the VR canvas
    /// </summary>
    /// <param name="msg">message content</param>
    /// <param name="timeout"> duration to display the message (default:2)</param>
    /// <param name="debugContext">debug log context object</param>
    public static void Log(string msg, float timeout = -1, Object debugContext = null)
    {
        LogP(msg, timeout > 0 ? timeout : 2, true, debugContext);
    }
    /// <summary>
    /// Log a message to the VR canvas default permanent
    /// </summary>
    /// <param name="msg">message content</param>
    /// <param name="timeout"> duration to display the message (default: -1, forever)</param>
    /// <param name="alsoDebug">Should log to unity console?</param>
    /// <param name="debugContext">debug log context object (only for unity console)</param>
    public static void LogP(string msg, float timeout = -1, bool alsoDebug = true, Object debugContext = null)
    {
        Instance.ILog(msg, timeout, alsoDebug, debugContext);
    }
    // todo log warn in bolder color
    /// <summary>
    /// Clears the VR log
    /// </summary>
    public static void Clear()
    {
        Instance.IClear();
    }
    /// <summary>
    /// Simple instance log. For inspector UnityEvents and such
    /// </summary>
    /// <param name="msg"></param>
    public void LogSimple(string msg)
    {
        Instance.ILog(msg, 2);
    }
    // protected static IEnumerator waitToLog(string msg, float timeout = -1, bool alsoDebug = true)
    // {
    //     yield return new WaitForEndOfFrame();
    //     Instance.ILog(msg, timeout, alsoDebug);
    // }
    void ILog(string msg, float timeout = -1, bool alsoDebug = true, Object debugContext = null)
    {
        if (!canLog)
        {
            return;
        }
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