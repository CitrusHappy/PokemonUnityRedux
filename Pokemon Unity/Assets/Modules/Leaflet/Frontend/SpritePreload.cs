using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PokemonUnity.Frontend.Overworld;
using PokemonUnity.Backend.Serializables;

[ExecuteInEditMode]
public class SpritePreload : MonoBehaviour
{
    #if UNITY_EDITOR
    public Sprite[] spriteSheet;
    public NPC npc;
    // Start is called before the first frame update
    void Awake()
    {
        npc = gameObject.GetComponentInParent<NPC>();
    }
    void Update()
    {
        if (gameObject.GetComponentInParent<NPC>().pokemonID == 0)
        {
            spriteSheet = Resources.LoadAll<Sprite>("OverworldNPCSprites/" + gameObject.GetComponentInParent<CharacterBase>().spriteName);
            gameObject.GetComponent<SpriteRenderer>().sprite = spriteSheet[10];
        }
        else
        {
            spriteSheet = Pokemon.GetSpriteFromID(npc.pokemonID, false, false);
            gameObject.GetComponent<SpriteRenderer>().sprite = spriteSheet[10];
        }
        //Debug.Log(spriteSheet);
        
        
    }
    #endif
}
