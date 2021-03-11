using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

[AddComponentMenu("_Util/HoverTween")]
public class HoverTween : MonoBehaviour
{
    [ContextMenuItem("Reset", "ResetPos")]
    public Vector3 localTargetPos = Vector3.up * 0.1f;
    [ContextMenuItem("Reset", "ResetScale")]
    public Vector3 localTargetScale = Vector3.one;
    [ContextMenuItem("Reset", "ResetRot")]
    public Vector3 localTargetRotateEuler = Vector3.zero;
    [Min(0f)]
    public float delay = 0;
    [Min(0f)]
    public float duration = 2;
    public Ease moveEase = Ease.InOutSine;
    public Ease scaleEase = Ease.InOutSine;

    bool canTweenPos => localTargetPos != Vector3.zero;
    bool canTweenScale => localTargetScale != Vector3.one;
    bool canTweenRotate => localTargetRotateEuler != Vector3.zero;

    Tween moveTween;
    Tween scaleTween;
    Tween rotTween;

    private void Start()
    {
        StartMoving();
    }
    void ResetPos()
    {
        localTargetPos = Vector3.zero;
    }
    void ResetScale()
    {
        localTargetScale = Vector3.one;
    }
    void ResetRot()
    {
        localTargetRotateEuler = Vector3.zero;
    }
    public void StartMoving()
    {
        if (canTweenPos)
        {
            // Vector3[] path = new Vector3[] { transform.localPosition, transform.localPosition + localTargetPos };
            // var dt = transform.DOLocalPath(path, duration);
            // transform.TransformPoint(localTargetPos)
            moveTween = transform.DOLocalMove(transform.localPosition + localTargetPos, duration);
            moveTween.SetEase(moveEase);
            moveTween.SetLoops(-1, LoopType.Yoyo);
            moveTween.SetDelay(delay);
            moveTween.Play();
        }
        if (canTweenScale)
        {
            scaleTween = transform.DOScale(Vector3.Scale(transform.localScale, localTargetScale), duration);
            scaleTween.SetEase(scaleEase);
            scaleTween.SetLoops(-1, LoopType.Yoyo);
            scaleTween.SetDelay(delay);
            scaleTween.Play();
        }
        if (canTweenRotate)
        {
            rotTween = transform.DOLocalRotate(localTargetRotateEuler, duration);
            rotTween.SetEase(Ease.Linear);
            rotTween.SetLoops(-1, LoopType.Incremental);
            rotTween.SetDelay(delay);
            rotTween.Play();
        }
    }
    public void PauseTween()
    {
        moveTween?.Pause();
        scaleTween?.Pause();
        rotTween?.Pause();
    }
    public void ResumeTween()
    {
        moveTween?.Play();
        scaleTween?.Play();
        rotTween?.Play();
    }
    public void StopMoving()
    {
        DOTween.RestartAll();
        DOTween.Clear();
        moveTween = null;
        scaleTween = null;
        rotTween = null;
    }
    public void Punch(Vector3 dir)
    {
        transform.DOPunchPosition(dir, 0.2f);
    }
    public void Shake()
    {
        transform.DOShakePosition(0.1f, 1);
    }
    private void OnDrawGizmosSelected()
    {
        if (canTweenPos)
        {
            Gizmos.color = Color.cyan;
            Vector3 targ = transform.position + Vector3.Scale(localTargetPos, transform.lossyScale);
            Gizmos.DrawSphere(targ, 0.005f);
        }
    }
}