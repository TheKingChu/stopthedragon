using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Acid : MonoBehaviour
{
    private Spawner gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<Spawner>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Loke"))
        {
            Destroy(this.gameObject);
            gameManager.AcidDropMissed();
        }
        if (collision.gameObject.CompareTag("Basket"))
        {
            Destroy(this.gameObject);
            gameManager.AcidDropCaught();
        }
    }
}
