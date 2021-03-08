using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.XR.Haptics;
// using UnityEngine.XR;
// using UnityEngine.XR.Interaction.Toolkit;

public class HapticManager : Singleton<HapticManager>
{

    /// <summary>
    /// Haptic information for<see cref="HapticManager"/>
    /// </summary>
    [System.Serializable]
    public class HapticPattern
    {
        // todo custom drawer (or window?)
        public HapticPattern(float amplitude = 1, float duration = 0.1f, HapticType curveType = HapticType.SINGLE)
        {
            this.hapticType = curveType;
            this.amplitude = amplitude;
            this.duration = duration;
        }
        public HapticType hapticType = HapticType.SINGLE;
        public float amplitude = 1;
        public float duration = 0.1f;
        public float waitDuration = 0.05f;
        [Tooltip("Callback for continuous type to stop")]
        /// <summary>
        /// Set to true to stop looping.
        /// </summary>
        public bool stop = false;
        /// <summary>
        /// This delegate is called before each time the pattern is played for HapticType REPEAT and CONTINUOUS.
        /// pattern.loopFunc = (ref HapticManager.HapticPattern p, int i) => {/*code here*/}
        /// </summary>
        /// <param name="pattern">the reference pattern to modify</param>
        /// <param name="iteration">the current loop iteration, starting at 0</param>
        /// <returns></returns>
        public LoopFunc loopFunc = new LoopFunc((ref HapticPattern pattern, int iteration) => { });
        public delegate void LoopFunc(ref HapticPattern pattern, int iteration);
        // ? make asdr envelope
        /// <summary>
        /// the number of times to play the pattern for HapticType REPEAT
        /// </summary>
        public int repeat = 2;

        /// <summary>
        /// Sets the frequency of the pattern
        /// </summary>
        /// <param name="freq">frequency in HZ [0.5, 1000]</param>
        public void SetFrequency(float freq)
        {
            if (freq <= 0) return;
            float ifreq = Mathf.Clamp(1 / freq, 0.001f, maxDur);
            duration = ifreq / 2;
            waitDuration = ifreq / 2;
        }
        public float GetFrequency()
        {
            if (duration <= 0) return 0;
            return 1 / (duration + waitDuration);
        }
        public void Stop()
        {
            stop = true;
            repeat = 0;
        }
        public void Resume()
        {
            stop = false;
        }
        public void Play(bool rightHand)
        {
            HapticManager.Play(this, rightHand);
        }

        public static LoopFunc SimpleInc = new LoopFunc((ref HapticPattern p, int i) => { p.amplitude *= 1.2f; });
        public static LoopFunc SimpleDec = new LoopFunc((ref HapticPattern p, int i) => { p.amplitude *= 0.8f; });
        public static HapticPattern LOW = new HapticPattern(0.2f, 0.1f);
        public static HapticPattern MED = new HapticPattern(0.5f, 0.1f);
        public static HapticPattern HIGH = new HapticPattern(0.8f, 0.1f);
    }
    public enum HapticType
    {// todo remove?
        /// <summary>
        /// single pulse
        /// </summary>
        SINGLE,
        /// <summary>
        /// repeat amount of repeated pulses
        /// </summary>
        REPEAT,
        /// <summary>
        /// haptics will continue until told to stop
        /// </summary>
        CONTINUOUS
    }

    public HapticPattern simpleImpulsePattern = HapticPattern.MED;
    public HapticPattern simpleAcceptPattern = HapticPattern.MED;
    public HapticPattern simpleRefusePattern = HapticPattern.MED;
    [SerializeField] float maxDuration = 2;

    public static float maxDur = 2;
    public static float minWaitDur = 0.01f;
    static bool isRepeatingRight = false;
    static bool isRepeatingLeft = false;
    static bool isPlayingLeft = false;
    static bool isPlayingRight = false;
    // public List<HapticPattern> currentlyPlayingPatterns = new List<HapticPattern>();

    // public static InputAction rightHaptic = new InputAction("RightHaptic", InputActionType.PassThrough, "<XRController>{RightHand}/" + "*");
    // public static InputAction leftHaptic = new InputAction("LeftHaptic", InputActionType.PassThrough, "<XRController>{LeftHand}/" + "*");

    /*
    what do I need haptics for?
    single impluses when something happens (arrow fired, menu hit)
     should be noticably different (hit with arrow, release arrow)
    sustained rumble for unknown amounts of time (quiver position)?
    rumble for fixed time (earthquake from nearby attacks)
    dynamically changing frequency??
    todo major refactor of this
    */
    protected override void Awake()
    {
        base.Awake();
        // rightHaptic.Enable();
        // leftHaptic.Enable();
        maxDur = maxDuration;
    }
    private void Start()
    {
        // increase amp
        simpleAcceptPattern.loopFunc = (ref HapticPattern p, int i) => { };
        // decrease amp
    }
    void SetIsPlaying(bool playing, float dur, bool rightHand)
    {
        if (playing)
        {
            if (rightHand)
            {
                CancelInvoke("ResetIsPlayingRight");
                isPlayingRight = true;
                Invoke("ResetIsPlayingRight", dur);
            } else
            {
                CancelInvoke("ResetIsPlayingLeft");
                isPlayingLeft = true;
                Invoke("ResetIsPlayingLeft", dur);
            }
        } else
        {
            if (rightHand)
            {
                isPlayingLeft = false;
            } else
            {
                isPlayingLeft = false;
            }
        }
    }
    void ResetIsPlayingRight()
    {
        isPlayingRight = false;
    }
    void ResetIsPlayingLeft()
    {
        isPlayingLeft = false;
    }
    public static bool IsPlaying(bool rightHand)
    {
        if (rightHand)
        {
            return isPlayingRight;
        } else
        {
            return isPlayingLeft;
        }
    }
    public static bool IsPlaying()
    {
        return isPlayingLeft || isPlayingRight;
    }
    public static bool IsPlayingRepeat(bool rightHand)
    {
        if (rightHand)
        {
            return isRepeatingRight;
        } else
        {
            return isRepeatingLeft;
        }
    }
    public static bool IsPlayingRepeat()
    {
        return isRepeatingLeft || isRepeatingRight;
    }
    public static void StopAllImpulses()
    {
        InputSystem.ResetHaptics();
        Instance.StopAllCoroutines();
        isPlayingLeft = false;
        isPlayingRight = false;
        isRepeatingLeft = false;
        isRepeatingRight = false;
    }
    static bool GetController(bool rightHand, out XRControllerWithRumble rumbleController)
    {
        bool valid = false;
        rumbleController = null;
        var controller = rightHand ? XRControllerWithRumble.rightHand : XRControllerWithRumble.leftHand;
        if (controller != null && controller is XRControllerWithRumble rc)
        {
            rumbleController = rc;
            valid = true;
        }
        // todo test
        // InputAction hapticAction = rightHand ? rightHaptic : leftHaptic;
        // if (hapticAction?.activeControl?.device is XRControllerWithRumble rc) {
        //     rumbleController = rc;
        //     valid = true;
        // }
        return valid;
    }
    public static void StopImpulse(bool rightHand)
    {
        if (GetController(rightHand, out var controller))
        {
            controller.SendImpulse(0, 0);
        }
    }
    // public void SimpleImpulseRight() {
    //     SimpleImpulse(true);
    // }
    // public void SimpleImpulseLeft() {
    //     SimpleImpulse(false);
    // }
    // public void SimpleImpulse(bool rightHand) {
    //     // todo to be called via UI unity events

    // }
    public static void Impulse(float amplitude, float duration, bool rightHand)
    {
        if (amplitude <= 0 || duration <= 0)
        {
            return;
        }
        amplitude = Mathf.Clamp(amplitude, 0f, 1f);
        duration = Mathf.Min(duration, maxDur);
        // Debug.Log("Haptic "+(rightHand?"right":"left")+" hand for "+amplitude+" "+duration+"s");
        if (GetController(rightHand, out var controller))
        {
            controller.SendImpulse(amplitude, duration);
            Instance.SetIsPlaying(true, duration, rightHand);
        }
    }
    IEnumerator PlayImpulsesRepeat(HapticPattern pattern, bool rightHand)
    {
        if (pattern.repeat <= 0) yield break;
        float repeatAmount = Mathf.Clamp(pattern.repeat, 0, 1000);
        for (int i = 0; i < repeatAmount; i++)
        {
            pattern.loopFunc.Invoke(ref pattern, i);
            if (pattern.stop) yield break;
            if (pattern.repeat <= 0) yield break;
            if (pattern.amplitude > 0)
            {
                Impulse(pattern.amplitude, pattern.duration, rightHand);
            }
            float waitDur = pattern.duration + pattern.waitDuration;
            if (waitDur <= 0) waitDur = minWaitDur;
            yield return new WaitForSeconds(waitDur);
        }
    }
    IEnumerator PlayImpulsesInf(HapticPattern pattern, bool rightHand)
    {
        int i = 0;
        while (!pattern.stop)
        {
            pattern.loopFunc.Invoke(ref pattern, i);
            if (pattern.stop) continue;
            if (pattern.amplitude > 0)
            {
                Impulse(pattern.amplitude, pattern.duration, rightHand);
            }
            float waitDur = pattern.duration + pattern.waitDuration;
            if (waitDur <= 0) waitDur = minWaitDur;
            yield return new WaitForSeconds(waitDur);
            i++;
        }
    }
    /// <summary>
    /// Stops repeating the pattern on this hand.
    /// </summary>
    public static void StopPatternRepeat(bool rightHand)
    {
        StopImpulse(rightHand);
        if (rightHand)
        {
            isRepeatingRight = false;
        } else
        {
            isRepeatingLeft = false;
        }
    }
    public static void PlayPatternRepeatBothHands(HapticPattern pattern)
    {
        PlayPatternRepeat(pattern, true);
        PlayPatternRepeat(pattern, false);
    }
    /// <summary>
    /// Play the specified pattern repeatedly until StopPatternRepeat is called
    /// </summary>
    public static void PlayPatternRepeat(HapticPattern pattern, bool rightHand)
    {
        if (rightHand)
        {
            if (isRepeatingRight)
            {
                // Debug.Log("Already playing a repeating pattern on Right!");
                return;
            }
            isRepeatingRight = true;
        } else
        {
            if (isRepeatingLeft)
            {
                // Debug.Log("Already playing a repeating pattern on Left!");
                return;
            }
            isRepeatingLeft = true;
        }
        if (pattern.hapticType == HapticType.CONTINUOUS)
        {
            pattern.hapticType = HapticType.SINGLE;
        }
        Instance.PlayRepeat(pattern, rightHand);
    }
    void PlayRepeat(HapticPattern pattern, bool rightHand)
    {
        StartCoroutine(PlayRepeatCo(pattern, rightHand));
    }
    IEnumerator PlayRepeatCo(HapticPattern pattern, bool rightHand)
    {
        float waitS = pattern.duration + pattern.waitDuration;
        if (pattern.hapticType == HapticType.REPEAT)
        {
            waitS *= pattern.repeat;
        }
        WaitForSeconds waitDur = new WaitForSeconds(waitS);
        while (rightHand ? isRepeatingRight : isRepeatingLeft)
        {
            Play(pattern, rightHand);
            yield return waitDur;
        }
    }
    public static void PlayBothHands(HapticPattern pattern)
    {
        Play(pattern, true);
        Play(pattern, false);
    }
    /// <summary>
    /// Play a haptic pattern
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="rightHand"></param>
    public static void Play(HapticPattern pattern, bool rightHand)
    {
        switch (pattern.hapticType)
        {
            case HapticType.SINGLE:
                Impulse(pattern.amplitude, pattern.duration, rightHand);
                break;
            case HapticType.REPEAT:
                Instance.StartCoroutine(Instance.PlayImpulsesRepeat(pattern, rightHand));
                break;
            case HapticType.CONTINUOUS:
                Instance.StartCoroutine(Instance.PlayImpulsesInf(pattern, rightHand));
                break;
            default:
                break;
        }
    }
}