using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float velocity;
    [SerializeField] private float jumpVelocity;
    [SerializeField] private float dashVelocity;
    [SerializeField] private float dashLength;
    private FakeHeight fakeHeight;

    private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    private Vector2 lastMoveInput = Vector2.right;
    public bool dashing = false;
    public bool endDash = false;
    public bool takingDamage = false;

    public Vector2 moveInput;

    [SerializeField] public ContactFilter2D groundedFilter;
    private bool isCharacterGrounded => rb.IsTouching(groundedFilter);

    private SquashNStretch sns;
    private Vector2 offset;
    public bool hasFallen = false;
    private bool allowInputs = true;

    private PlayerSFX sfx;
    private PlayerHealth health;

    private bool canInteract = false;
    private Interactable interactedObj;
    private Grabbable grabbedObj;

    [SerializeField] private bool isHoldingObj = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sns = GetComponentInChildren<SquashNStretch>();
        fakeHeight = GetComponent<FakeHeight>();
        sfx = GetComponent<PlayerSFX>();
        health = GameObject.FindGameObjectWithTag("PlayerHealth").GetComponent<PlayerHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        if (allowInputs)
        {
            MiscInputs();
            MoveInput();
            if (!isHoldingObj)
            {
                JumpInput();
                DashInput();
            }
        }
        CheckIfFalling(false);
    }

    void FixedUpdate()
    {
        Move();
        FinishDash();
    }

    void MiscInputs()
    {
        if (moveInput != Vector2.zero)
        {
            lastMoveInput = moveInput;
        }
        if (Input.GetButtonDown("Submit"))
        {
            if (!isHoldingObj)
            {
                Interact();
            }
            else
            {
                if (!grabbedObj.IsDroppable())
                {
                    return;
                }
                StartCoroutine(grabbedObj.Drop(5, 5));
                sfx.Drop();
            }
        }
        if (isHoldingObj && !grabbedObj.IsGrabAnimOn() && Input.GetButtonDown("Jump") && grabbedObj.IsDroppable())
        {
            StartCoroutine(grabbedObj.Drop(5, 5));
            sfx.Drop();
        }
        if (isHoldingObj && !grabbedObj.IsGrabAnimOn() && Input.GetButtonDown("Dash") && grabbedObj.IsDroppable())
        {
            StartCoroutine(grabbedObj.Drop(12.5f, 12.5f));
            sfx.Throw();
        }
    }

    void MoveInput()
    {
        float horiz = Input.GetAxisRaw("Horizontal");
        float vert = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(horiz, vert).normalized;

        if (moveInput != Vector2.zero && fakeHeight.isGrounded && !dashing && !endDash && !CheckIfFalling(false))
        {
            sfx.Walk();
        }
        else
        {
            sfx.StopWalk();
        }
    }

    void Move()
    {
        if (!dashing && !endDash && !takingDamage)
        {
            fakeHeight.SetGroundVelocity(Vector2.zero);
            rb.velocity = moveInput * velocity + offset;
        }
        else if (!takingDamage)
        {
            if (!fakeHeight.colliding)
            {
                fakeHeight.SetGroundVelocity(moveInput * velocity);
            }

            if (moveInput.x != 0)
            {
                float inputMuliplier = moveInput.x;
                if (moveInput.x < 0)
                {
                    inputMuliplier = Mathf.Floor(inputMuliplier);
                }
                else if (moveInput.x > 0)
                {
                    inputMuliplier = Mathf.Ceil(inputMuliplier);
                }
                rb.velocity = new Vector2(Mathf.Abs(rb.velocity.x) * inputMuliplier, rb.velocity.y);
            }

            if (moveInput.y != 0)
            {
                float inputMuliplier = moveInput.y;
                if (moveInput.y < 0)
                {
                    inputMuliplier = Mathf.Floor(inputMuliplier);
                }
                else if (moveInput.y > 0)
                {
                    inputMuliplier = Mathf.Ceil(inputMuliplier);
                }
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Abs(rb.velocity.y) * inputMuliplier);
            }
        }
    }

    void JumpInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (fakeHeight.isGrounded && jumpBufferCounter > 0f)
        {
            sns.PlaySquashAndStretch();
            sfx.Jump();
            fakeHeight.Launch(Vector2.zero, jumpVelocity);
        }
    }

    void DashInput()
    {
        if (Input.GetButtonDown("Dash") && !dashing)
        {
            StartCoroutine("Dash");
        }
    }

    IEnumerator Dash()
    {
        if (fakeHeight.isGrounded)
        {
            sfx.Roll();
        }
        else
        {
            sfx.Dash();
        }
        dashing = true;
        rb.velocity = Vector2.zero;
        rb.AddForce(lastMoveInput * dashVelocity, ForceMode2D.Impulse);
        yield return new WaitForSeconds(dashLength);
        dashing = false;
        endDash = true;
    }

    void FinishDash()
    {
        if (endDash)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, moveInput * velocity, 0.4f);
            if ((Mathf.Abs(rb.velocity.x - (moveInput.x * velocity)) < 1f) && (Mathf.Abs(rb.velocity.y - (moveInput.y * velocity)) < 1f))
            {
                rb.velocity = moveInput * velocity;
            }

            if (rb.velocity == moveInput * velocity)
            {
                endDash = false;
            }

            if (fakeHeight.colliding)
            {
                endDash = false;
            }
        }
    }

    void Interact()
    {
        if (canInteract && interactedObj != null)
        {
            interactedObj.Interact();
        }
    }

    public bool CheckIfFalling(bool justJumped)
    {
        if ((!isCharacterGrounded && !dashing && fakeHeight.isGrounded && !hasFallen) || (!isCharacterGrounded && !dashing && !hasFallen && justJumped))
        {
            GameObject shadow = GameObject.FindGameObjectWithTag("PlayerShadow");
            shadow.SetActive(false);

            health.Damaged();
            moveInput = Vector2.zero;
            rb.velocity = Vector2.zero;
            allowInputs = false;
            hasFallen = true;
            return true;
        }
        return false;
    }

    public bool CheckIfGrounded()
    {
        if (!dashing && fakeHeight.isGrounded)
        {
            return true;
        }
        return false;
    }

    public void SetOffset(Vector2 offset)
    {
        this.offset = offset;
    }

    public IEnumerator Respawn()
    {
        jumpBufferCounter = 0f;
        transform.position = new Vector2(-4, 0);
        GameObject.FindGameObjectWithTag("PlayerSprite").transform.localScale = new Vector3(1, 1, 0);
        yield return new WaitForSeconds(0.1f);
        allowInputs = true;
        hasFallen = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Obstacle") && fakeHeight.isGrounded)
        {
            ObstacleDamage(other.gameObject);
        }
        if (other.gameObject.CompareTag("Interactable"))
        {
            canInteract = true;
            interactedObj = other.gameObject.GetComponent<Interactable>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Interactable"))
        {
            canInteract = false;
            interactedObj = null;
        }
    }

    void ObstacleDamage(GameObject obstacle)
    {
        sfx.Hurt();
        Camera.main.GetComponent<CameraController>().Shake();
        moveInput = Vector2.zero;
        rb.velocity = Vector2.zero;
        health.Damaged();
        obstacle.GetComponent<Obstacle>().Knockback();
        StartCoroutine("TakingDamage");
    }

    IEnumerator TakingDamage()
    {
        takingDamage = true;
        allowInputs = false;
        yield return new WaitForSeconds(0.25f);
        takingDamage = false;
        allowInputs = true;
    }

    public Vector2 GetLastInput()
    {
        return lastMoveInput;
    }

    public void SetIsHolding(bool isHoldingObj)
    {
        this.isHoldingObj = isHoldingObj;
        if (isHoldingObj)
        {
            sfx.Grab();
        }
    }

    public void SetGrabbedObj(Grabbable grabbedObj)
    {
        this.grabbedObj = grabbedObj;
    }

    public Grabbable GetGrabbedObj()
    {
        return grabbedObj;
    }
}