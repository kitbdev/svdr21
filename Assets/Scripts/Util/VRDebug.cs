using System.Collections.Generic;
using UnityEngine;
using TMPro;

[DefaultExecutionOrder(-5)]
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
                i--;
            }
        }
    }
    public static void Log(string msg, float timeout = -1, bool alsoDebug = true)
    {
        Instance.ILog(msg, timeout, alsoDebug);
    }
    public static void Clear()
    {
        Instance.IClear();
    }
    void ILog(string msg, float timeout = -1, bool alsoDebug = true)
    {
        msgs.Add(msg);
        if (alsoDebug)
        {
            Debug.Log("VRLOG:" + msg);
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
            msgTimeouts.RemoveRange(0, msgs.Count - maxMsgs);
        }
        string fullMsg = "";
        foreach (var msg in msgs)
        {
            fullMsg += msg + "\n";
        }
        textEl.text = fullMsg;
    }
}