using UnityEngine;
using System.Collections;

public class PlayEnemy : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            other.gameObject.GetComponent<Enemy>().enabled = true;
        }
    }
}
