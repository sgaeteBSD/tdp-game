using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{

    [SerializeField] private bool givesKnockback;
    private GameObject player;
    private PlayerController pc;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        pc = player.GetComponent<PlayerController>();
    }
    
    public void Knockback()
    {
        if (givesKnockback)
        {
            player.GetComponent<FakeHeight>().Launch(-pc.GetLastInput() * 7.5f, 20f);
        }
    }
}
