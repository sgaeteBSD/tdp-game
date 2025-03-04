using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquashNStretch : MonoBehaviour
{
    [Header("Squash and Stretch Core")]
    [SerializeField] private Transform transformToAffect;
    [SerializeField] private SquashStretchAxis axisToAffect = SquashStretchAxis.X;
    [SerializeField, Range(0, 1f)] public float animationDuration = 0.25f;
    [SerializeField] private bool canBeOverwritten = true;
    [SerializeField] private bool playOnStart;
    
    [Flags]
    public enum SquashStretchAxis
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4
    }

    [Header("Animation Settings")] 
    [SerializeField] private float initialScale = 1f;

    [SerializeField] public float maximumScale = 1.3f;
    [SerializeField] private bool resetToInitialScaleAfterAnimation = true;

    [SerializeField] public AnimationCurve squashAndStretchCurve = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.25f, 1f), new Keyframe(1f, 0f));

    private Coroutine squashAndStretchCoroutine;
    private Vector3 initialScaleVector;

    private bool affectX => (axisToAffect & SquashStretchAxis.X) != 0;
    private bool affectY => (axisToAffect & SquashStretchAxis.Y) != 0;
    private bool affectZ => (axisToAffect & SquashStretchAxis.Z) != 0;

    private void Awake()
    {
        if (transformToAffect == null)
        {
            transformToAffect = transform;
        }

        initialScaleVector = transformToAffect.localScale;
    }

    private void Start()
    {
        if (playOnStart)
        {
            CheckForAndStartCoroutine();
        }
    }
    
    [ContextMenu("Play Squash and Stretch")]
    public void PlaySquashAndStretch()
    {
        if (!canBeOverwritten)
        {
            return;
        }
        CheckForAndStartCoroutine();
    }

    public void SetAxis(SquashStretchAxis axis)
    {
        axisToAffect = axis;
    }
    
    private void CheckForAndStartCoroutine()
    {
        if (axisToAffect == SquashStretchAxis.None)
        {
            Debug.Log("Axis to affect is set to None.", gameObject);
            return;
        }

        if (squashAndStretchCoroutine != null)
        {
            StopCoroutine(squashAndStretchCoroutine);
            if (resetToInitialScaleAfterAnimation)
            {
                transform.localScale = initialScaleVector;
            }
        }

        squashAndStretchCoroutine = StartCoroutine(SquashAndStretchEffect());
    }

    private IEnumerator SquashAndStretchEffect()
    {
        float elapsedTime = 0;
        Vector3 originalScale = initialScaleVector;
        Vector3 modifiedScale = originalScale;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float curvePosition = elapsedTime / animationDuration;

            float curveValue = squashAndStretchCurve.Evaluate(curvePosition);
            float remappedValue = initialScale + (curveValue * (maximumScale - initialScale));

            float minimumThreshold = 0.0001f;
            if (Math.Abs(remappedValue) < minimumThreshold)
            {
                remappedValue = minimumThreshold;
            }

            if (affectX)
            {
                modifiedScale.x = originalScale.x * remappedValue;
            }
            else
            {
                modifiedScale.x = originalScale.x / remappedValue;
            }
            
            if (affectY)
            {
                modifiedScale.y = originalScale.y * remappedValue;
            }
            else
            {
                modifiedScale.y = originalScale.y / remappedValue;
            }
            
            if (affectZ)
            {
                modifiedScale.z = originalScale.z * remappedValue;
            }
            else
            {
                modifiedScale.z = originalScale.z / remappedValue;
            }

            transformToAffect.localScale = modifiedScale;

            yield return null;
        }

        if (resetToInitialScaleAfterAnimation)
        {
            transformToAffect.localScale = originalScale;
        }
    }

}
