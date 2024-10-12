using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseShoe : MonoBehaviour
{
    private FallingObjectsManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<FallingObjectsManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Loke"))
        {
            Destroy(this.gameObject);
        }
        if (collision.gameObject.CompareTag("Basket"))
        {
            Destroy(this.gameObject);
            gameManager.SpeedBoostCaught();
        }
    }
}
