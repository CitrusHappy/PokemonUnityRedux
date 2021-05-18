using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PokemonUnity.Frontend.Overworld;

[ExecuteAlways]
public class SpritePreload : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log(gameObject.GetComponentInParent<CharacterBase>().spriteSheet[10]);
        gameObject.GetComponent<SpriteRenderer>().sprite = gameObject.GetComponentInParent<CharacterBase>().spriteSheet[10];
        
    }
}
