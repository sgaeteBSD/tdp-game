using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FakeHeight : MonoBehaviour
{

    public UnityEvent onGroundHitEvent;
    
    public Transform obj;
    public Transform body;
    public Transform shadow;

    public float gravity = -100;
    private float origGravity;
    public Vector2 groundVelocity;
    public float verticalVelocity;
    private float lastInitialVerticalVelocity;

    public bool isGrounded;
    public bool colliding = false;

    [Header("Jump Variables")]
    public bool hangTime;
    public bool multiplier;
    public float maxFallGravity;

    private Vector3 shadowScale;
    
    void Start()
    {
        isGrounded = true;
        lastInitialVerticalVelocity = verticalVelocity;
        origGravity = gravity;
        shadowScale = shadow.localScale;
    }

    void Update()
    {
        ShadowScale();
        CheckGroundHit();
    }

    void FixedUpdate()
    {
        UpdatePosition();
    }

    public void Launch(Vector2 groundVelocity, float verticalVelocity)
    {
        isGrounded = false;
        this.groundVelocity = groundVelocity;
        this.verticalVelocity = verticalVelocity + (0.5f * Time.fixedDeltaTime * -gravity);
        lastInitialVerticalVelocity = verticalVelocity;
    }

    public void SetGroundVelocity(Vector2 groundVelocity)
    {
        if (colliding)
        {
            groundVelocity = CheckCollision(groundVelocity);
        }
        this.groundVelocity = groundVelocity;
    }

    public void SetVerticalVelocity(float verticalVelocity)
    {
        this.verticalVelocity = verticalVelocity;
    }

    public void SetGravity(float gravity)
    {
        this.gravity = gravity;
    }

    void UpdatePosition()
    {
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
            body.position += new Vector3(0, verticalVelocity, 0) * Time.deltaTime;

            if (verticalVelocity < 1f && verticalVelocity > -1f && hangTime && gravity != 0)
            {
                gravity = origGravity / 2;
            }
            else if (verticalVelocity < -1f && hangTime && gravity != 0)
            {
                float newGrav = origGravity;
                if (multiplier && (gravity > maxFallGravity))
                {
                    newGrav = origGravity * 1.5f;
                }
                gravity = newGrav;
            }
            
        }

        obj.position += (Vector3)groundVelocity * Time.deltaTime;
    }

    void ShadowScale()
    {
        Vector3 newShadowScale = shadowScale;
        if ((Vector3.Distance(body.position, shadow.position) / 2) >= 1f)
        {
            newShadowScale = shadowScale / (Vector3.Distance(body.position, shadow.position) / 2);
        }
        
        shadow.localScale = newShadowScale;
    }

    void CheckGroundHit()
    {
        if (body.position.y < obj.position.y && !isGrounded && !obj.CompareTag("Interactable"))
        {
            gravity = origGravity;
            body.position = obj.position;
            isGrounded = true; 
            GroundHit();

            PlayerController playerController = GetComponentInParent<PlayerController>();
            if (playerController != null)
            {
                playerController.CheckIfFalling(true);
            }
        }
        else if (body.position.y < shadow.position.y + 0.4f && !isGrounded && obj.CompareTag("Interactable")) 
        {
            gravity = origGravity;
            body.position = shadow.position + new Vector3(0, 0.4f, 0);
            obj.position = shadow.position + new Vector3(0, 0.4f, 0);
            body.localPosition = Vector3.zero;
            shadow.localPosition = GetComponent<Grabbable>().GetShadowPos();
            isGrounded = true;
            GroundHit();
        }
    }

    void GroundHit()
    {
        onGroundHitEvent.Invoke();
    }

    public void Stick()
    {
        groundVelocity = Vector2.zero;
    }

    public void Bounce(float divisionFactor) 
    {
        Launch(groundVelocity, lastInitialVerticalVelocity / divisionFactor);
    }

    public void SlowDownGroundVelocity(float divisionFactor) 
    {
        groundVelocity = groundVelocity / divisionFactor;
    }
    
    void OnCollisionStay2D(Collision2D other)
    {
        foreach (var contact in other.contacts)
        {
            colliding = true;
            Collide(contact.normal);
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        colliding = false;
    }

    private bool negX = true;
    private bool posX = true;
    private bool negY = true;
    private bool posY = true;
    void Collide(Vector2 normal)
    {
        if (-normal.x < 0 && groundVelocity.x < 0)
        {
            negX = false;
            groundVelocity.x = 0;
        }
        else
        {
            negX = true;
        }
        
        if (-normal.x > 0 && groundVelocity.x > 0)
        {
            posX = false;
            groundVelocity.x = 0;
        }
        else
        {
            posX = true;
        }
        
        if (-normal.y < 0 && groundVelocity.y < 0)
        {
            negY = false;
            groundVelocity.y = 0;
        }
        else
        {
            negY = true;
        }
        
        
        if (-normal.y > 0 && groundVelocity.y > 0)
        {
            posY = false;
            groundVelocity.y = 0;
        }
        else
        {
            posY = true;
        }
    }

    Vector2 CheckCollision(Vector2 attemptedGroundVelocity)
    {
        
        if (!negX && attemptedGroundVelocity.x < 0)
        {
            attemptedGroundVelocity.x = 0;
        }

        if (!posX && attemptedGroundVelocity.x > 0)
        {
            attemptedGroundVelocity.x = 0;
        }

        if (!negY && attemptedGroundVelocity.y < 0)
        {
            attemptedGroundVelocity.y = 0;
        }

        if (!posY && attemptedGroundVelocity.y > 0)
        {
            attemptedGroundVelocity.y = 0;
        }

        return attemptedGroundVelocity;
    }
}
