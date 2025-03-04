using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

[RequireComponent(typeof(SquashNStretch), typeof(FakeHeight), typeof(Rigidbody2D))]
public class Grabbable : MonoBehaviour, Interactable
{

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform body;
    [SerializeField] private Transform shadow;
    private BoxCollider2D bc;
    private PlayerController pc;

    [Header("Grab Aniamation")]
    [SerializeField] private float animationDuration;
    [SerializeField] AnimationCurve grabAnimX;
    [SerializeField] AnimationCurve grabAnimY;

    private float finalXPos = 0;
    private float finalYPos = 0;
    private float initialXPos;
    private float initialYPos;

    private float initialShadowYPos;
    private Vector3 finalShadowPos;
    
    SquashNStretch sns;
    FakeHeight fakeHeight;
    private bool animPlaying = false;

    private bool isDroppable = true;
    private bool justDropped = false;

    // Start is called before the first frame update
    void Start()
    {
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        sns = GetComponent<SquashNStretch>();
        fakeHeight = GetComponent<FakeHeight>();

        rb.isKinematic = true;
        rb.freezeRotation = true;
        rb.gravityScale = 0f;
    }

    // Update is called once per frame
    void Update()
    {

        if (CheckIfGrabbed() && animPlaying) {
            SetShadow();
        }

        bc.isTrigger = CheckIfGrabbed() || !IsGrounded() ? true : false;

        if (IsGrounded() && shadow.transform.localPosition.y != -0.4f && !CheckIfGrabbed()) {
            shadow.transform.localPosition = new Vector3(shadow.transform.localPosition.x, -0.4f, shadow.transform.localPosition.z);
        }

        if (IsGrabbable() && body.GetComponent<SpriteRenderer>().sortingOrder == 1) {
            body.GetComponent<SpriteRenderer>().sortingOrder = 0;
        }
    }

    public void Interact()
    {
        if (IsGrabbable()) {
            Grab();
        }
    }

    public bool IsGrabbable() {
        return IsGrounded() && !CheckIfGrabbed() && !justDropped;
    }

    void Grab() {
        Transform hand = GameObject.FindGameObjectWithTag("PlayerHand").transform;
        initialShadowYPos = shadow.localPosition.y;
        finalShadowPos = new Vector3(0, -(hand.localPosition.y + Mathf.Abs(initialShadowYPos)), 0f);
        transform.parent = hand;
        shadow.parent = hand;
        initialXPos = transform.localPosition.x;
        initialYPos = transform.localPosition.y;
        pc.SetIsHolding(true);
        pc.SetGrabbedObj(this);

        animationDuration = Mathf.Clamp(Vector2.Distance(new Vector2(0, 0), transform.localPosition), 0f, 2f) * 0.175f;
        sns.animationDuration = Mathf.Lerp(0.1f, 0.25f, animationDuration / 0.45f);
        sns.maximumScale = Mathf.Lerp(0.8f, 0.6f, animationDuration / 0.35f);
        
        StartCoroutine("GrabAnim");
    }

    public IEnumerator Drop(float groundVelocity, float verticalVelocity) {
        body.GetComponent<SpriteRenderer>().sortingOrder = 1;
        yield return new WaitForEndOfFrame();
        transform.parent = null;
        fakeHeight.Launch(pc.GetLastInput() * groundVelocity, verticalVelocity);
        yield return new WaitForEndOfFrame();
        justDropped = true;
        pc.SetIsHolding(false);
        pc.SetGrabbedObj(null);
        yield return new WaitForEndOfFrame();
        justDropped = false;
    }

    public IEnumerator PickupCooldown() {
        justDropped = true;
        yield return new WaitForEndOfFrame();
        justDropped = false;
    }

    bool CheckIfGrabbed() {
        return pc.GetGrabbedObj() == this;
    }

    Vector3 z = Vector3.zero;
    void SetShadow() 
    {
        shadow.transform.localPosition = Vector3.SmoothDamp(shadow.transform.localPosition, finalShadowPos, ref z, 0.05f);
    }

    private IEnumerator GrabAnim() {
        isDroppable = false;
        float elapsedTime = 0;
        Vector3 initialPos = new Vector3(initialXPos, initialYPos, 0); 
        Vector3 finalPos = initialPos;

        float yDirFromPlayer = Mathf.Sign(initialYPos + transform.parent.localPosition.y);

        bool snsPlayed = false;

        while (elapsedTime < animationDuration) {
            animPlaying = true;
            elapsedTime += Time.deltaTime;
            
            float curvePos = elapsedTime / animationDuration;
            float xCurveValue = grabAnimX.Evaluate(curvePos);
            float yCurveValue = grabAnimY.Evaluate(curvePos);
            
            float xRemappedValue = initialXPos + (xCurveValue * (finalXPos - initialXPos));
            float yRemappedValue = initialYPos + (yCurveValue * (finalYPos - initialYPos));

            float newXPos = Mathf.Abs(initialPos.x * xRemappedValue) > Mathf.Abs(initialXPos) ? initialXPos : initialPos.x * xRemappedValue * Mathf.Sign(initialXPos);
            float newYPos = Mathf.Abs(initialPos.y * yRemappedValue) > Mathf.Abs(initialYPos) ? initialYPos : initialPos.y * yRemappedValue * Mathf.Sign(initialYPos);

            if (yDirFromPlayer < 0 && initialPos.y * yRemappedValue * Mathf.Sign(initialYPos) > initialYPos) {
                newYPos = initialPos.y * yRemappedValue * Mathf.Sign(initialYPos);
            }

            finalPos.x = newXPos;
            finalPos.y = newYPos;
            transform.localPosition = finalPos;
            shadow.transform.localPosition = new Vector3(finalPos.x, shadow.transform.localPosition.y, 0);

            if (elapsedTime / animationDuration > 0.8f && !snsPlayed) {
                sns.PlaySquashAndStretch();
                snsPlayed = false;
            }

            yield return null;
        }

        animPlaying = false;
        shadow.parent = gameObject.transform;

        yield return new WaitForEndOfFrame();
        isDroppable = true;
    }

    public bool IsGrabAnimOn() {
        return animPlaying;
    }

    public bool IsGrounded() {
        return fakeHeight.isGrounded;
    }

    public bool IsDroppable() {
        return isDroppable;
    }

    public Vector3 GetShadowPos() {
        return new Vector3(0, initialShadowYPos, 0);
    }
}
