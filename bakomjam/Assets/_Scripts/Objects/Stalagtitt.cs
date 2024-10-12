using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stalagtitt : MonoBehaviour
{
    PlayerMovement player;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Loke"))
        {
            Destroy(this.gameObject);
        }
        if (collision.gameObject.CompareTag("Basket"))
        {
            if(player != null)
            {
                player.StunPlayer();
            }
            Destroy(this.gameObject);
        }
    }
}
