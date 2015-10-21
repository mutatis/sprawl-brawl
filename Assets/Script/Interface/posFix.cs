using UnityEngine;
using System.Collections;

public class posFix : MonoBehaviour
{
    public Transform pos;
    Transform temp;

	// Use this for initialization
	void Start ()
    {
        temp = pos;
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.position = temp.position;
	}
}
