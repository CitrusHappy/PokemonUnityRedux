using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SeasonChanger : MonoBehaviour
{
    public Material newMat;
    public GameObject objectToChange;



    void Start()
    {

    }

    void Awake()
    {
        

        /**
        foreach (Renderer rend in children)
        {
            var mats = new Material[rend.materials.Length];
            for (var i = 0; i < rend.materials.Length; i++)
            {
                mats[i] = Resources.Load<Material>("Materials/Textures/" + children.name + gameObject.GetComponent<SeasonHandler>().getSeason());
                Debug.Log("Materials/Textures/" + gameObject.name + map.GetComponent<SeasonHandler>().getSeason());
            }
            rend.materials = mats;
        }
        */
    }
}
