using UnityEngine;
using System.Collections;

/*Movimenta o chao para acompanhar o player */

public class ChaoMov : MonoBehaviour
{

    public Transform limit;
    public Transform atras;

    public float soma;

	void Update ()
    {
        if(transform.position.x < limit.position.x)
        {
            transform.position = new Vector3((atras.position.x + soma), transform.position.y, transform.position.z);
        }
	}
}
