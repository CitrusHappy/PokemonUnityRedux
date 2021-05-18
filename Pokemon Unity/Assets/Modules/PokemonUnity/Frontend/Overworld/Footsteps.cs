using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(1f);

        for(float t = 1f; t >= 0f; t -= 0.01f)
        {
            Color newColor = gameObject.GetComponentInChildren<Renderer>().material.color;
            newColor.a = t;
            gameObject.GetComponentInChildren<Renderer>().material.color = newColor;
            yield return null;
        }

        Destroy(gameObject);
    }
}
