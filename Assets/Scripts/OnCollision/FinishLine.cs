using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishLine : MonoBehaviour
{

    // Bool
    public bool first;
    public bool secound;

    private float timePast;

    public GameObject[] fireworks;

	// Use this for initialization
	void Start ()
    {   timePast = 1;

        first = false;
        secound = false;
    }

    private void FixedUpdate()
    {
        timePast +=  Time.deltaTime;
        // Debug.Log(timePast);
    }

    public void OnTriggerEnter()
    {
        if (secound == true && timePast >= 50)
        {
            // fireworks = gameObject.SetActive(true);


            foreach (GameObject fire in fireworks)
            {
                fire.SetActive(true);
            }

        }
        else if (first = true & timePast >= 25)
        {
            secound = true;
        }
        else
            first = true;
    }

}
