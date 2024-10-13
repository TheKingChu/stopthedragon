using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stalagtitt : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Loke"))
        {
            Destroy(this.gameObject);
        }
        else if (collision.gameObject.CompareTag("Basket"))
        {
            PlayerMovement player = FindObjectOfType<PlayerMovement>();

            if(player != null)
            {
                player.OnHitByStalactite();
            }

            Destroy(this.gameObject);
        }
    }
}
