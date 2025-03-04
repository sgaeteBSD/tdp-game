using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    private Rigidbody2D rb;
    public float speed;
    public List<Vector2> routes;
    private Vector2 currentRoute;
    private Vector2 currentRouteDir;
    public int currentRouteNum;

    private PlayerController player;

    private bool movePlayer = false;
    private bool moveItem = false;

    private List<Rigidbody2D> items = new List<Rigidbody2D>();
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        
        rb = GetComponent<Rigidbody2D>();
        currentRouteNum = 0;
        
        currentRoute = transform.position;
        if (routes.Count > 0)
        {
            currentRoute = routes[0];
        }
        
        currentRouteDir = (currentRoute - (Vector2)transform.position).normalized;
    }
    
    void Update()
    {
    }

    void FixedUpdate()
    {
        rb.velocity = currentRouteDir * speed;
        MoveObjects();
        NextRoute();
    }

    private bool routeUpdated = false;
    void NextRoute()
    {
        
        if (!routeUpdated && (Vector2.Distance((currentRoute - (Vector2)transform.position).normalized, currentRouteDir) > 0.025f || currentRouteNum == 0))
        {
            currentRouteNum++;
            currentRoute = routes[currentRouteNum % (routes.Count)];
            currentRouteDir = (currentRoute - (Vector2)transform.position).normalized;
            routeUpdated = true;
        }
        
        if (Vector2.Distance(transform.position, currentRoute) < 0.25f)
        {
            routeUpdated = false;
        }
    }

    void MoveObjects()
    {
        if (movePlayer)
        {
            player.SetOffset(rb.velocity);
        }
        if (moveItem && items.Count > 0) {
            foreach (var item in items) {
                item.velocity = rb.velocity;
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && player.CheckIfGrounded() && !player.hasFallen)
        {
            movePlayer = true;
        }
        else if (other.gameObject.CompareTag("Interactable")) {
            if (other.gameObject.GetComponent<Grabbable>().IsGrounded()) {
                moveItem = true;
                if (items?.Contains(other.GetComponent<Rigidbody2D>()) == false) {
                    items.Add(other.GetComponent<Rigidbody2D>());
                }
            }
        }
        else
        {
            movePlayer = false;
            player.SetOffset(Vector2.zero);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            movePlayer = false;
            player.SetOffset(Vector2.zero);
        }
        if (other.gameObject.CompareTag("Interactable")) 
        {
            moveItem = false;
            if (items.Count > 0 && items?.Contains(other.GetComponent<Rigidbody2D>()) == true) {
                items.Remove(other.GetComponent<Rigidbody2D>());
                other.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            }
        }
    }
}
