using UnityEngine;
using System.Collections;

public class LuzMov : MonoBehaviour
{

    public float[] vel;
    float temp;
    public Transform[] limit;

	// Use this for initialization
	void Start ()
    {
        temp = Random.Range(vel[0], vel[1]);
    }
	
	// Update is called once per frame
	void Update ()
    {
        transform.Translate(temp * Time.deltaTime, 0, 0);

        if(transform.position.x > limit[0].position.x && temp > 0)
        {
            temp *= -1;
        }
        else if(transform.position.x < limit[1].position.x && temp < 0)
        {
            temp *= -1;
        }
    }
}
