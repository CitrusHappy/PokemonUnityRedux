/// Source: Pokémon Unity Redux
/// Purpose: Overworld character sprite base for Pokémon Unity frontend
/// Author: IIColour_Spectrum
/// Contributors: TeamPopplio
using UnityEngine;
using System.Linq;
using PokemonUnity.Frontend.Overworld.Mapping;
using System.Collections;
using System;
using Sirenix.OdinInspector;

namespace PokemonUnity.Frontend.Overworld {
public class CharacterBase : MonoBehaviour
{
    [BoxGroup("Basic Information")]
    public string spriteName;

    [ReadOnly, BoxGroup("Debug")]
    public bool busy = true;
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    [BoxGroup("Debug")]
    public Direction direction = Direction.Down;

    [HideInInspector]
    public MapCollider currentMap;

    [HideInInspector]
    public Transform pawn;

    [HideInInspector]
    public Transform pawnReflection;
    //public Material pawnReflectionSprite;

    [BoxGroup("Basic Information"), ReadOnly]
    public SpriteRenderer pawnSprite;

    [HideInInspector]
    public SpriteRenderer pawnReflectionSprite;

    [HideInInspector]
    public Transform hitBox;

    [BoxGroup("Basic Information"), ReadOnly]
    public Sprite[] spriteSheet;

    [HideInInspector]
    public int frame;

    [HideInInspector]
    public int frames;

    [HideInInspector]
    public int framesPerSec;

    [HideInInspector]
    public float secPerFrame;

    [HideInInspector]
    public bool animPause;
    public virtual void Awake()
    {
        pawnSprite = transform.Find("Pawn").GetComponent<SpriteRenderer>();
        pawnReflectionSprite = transform.Find("PawnReflection").GetComponent<SpriteRenderer>();
        hitBox = transform.Find("Character_Object");
        spriteSheet = Resources.LoadAll<Sprite>("OverworldNPCSprites/" + spriteName);
    }

    public virtual void Start()
    {
        hitBox.localPosition = new Vector3(0, 0, 0);

        StartCoroutine("animateSprite");

        //Check current map
        RaycastHit[] hitRays = Physics.RaycastAll(transform.position + Vector3.up, Vector3.down);
        int closestIndex;
        float closestDistance;

        CheckHitRaycastDistance(hitRays, out closestIndex, out closestDistance);

        if (closestIndex >= 0)
        {
            currentMap = hitRays[closestIndex].collider.gameObject.GetComponent<MapCollider>();
        }
        else
        {
            //if no map found
            //Check for map in front of character's direction
            hitRays = Physics.RaycastAll(transform.position + Vector3.up + getForwardVectorRaw(), Vector3.down);

            CheckHitRaycastDistance(hitRays, out closestIndex, out closestDistance);

            if (closestIndex >= 0)
            {
                currentMap = hitRays[closestIndex].collider.gameObject.GetComponent<MapCollider>();
            }
        }
    }

    public virtual IEnumerator animateSprite()
    {
        frame = 0;
        frames = 4;
        framesPerSec = 7;
        secPerFrame = 1f / (float) framesPerSec;
        while (true)
        {
            for (int i = 0; i < 4; i++)
            {
                if (animPause && frame % 2 != 0)
                {
                    frame -= 1;
                }
                pawnSprite.sprite = spriteSheet[(int) direction * frames + frame];
                pawnReflectionSprite.sprite = pawnSprite.sprite;
                yield return new WaitForSeconds(secPerFrame / 4f);
            }
            if (!animPause)
            {
                frame += 1;
                if (frame >= frames)
                {
                    frame = 0;
                }
            }
        }
    }
    public Vector3 getForwardVectorRaw()
    {
        return getForwardVectorRaw((int)direction);
    }

    public Vector3 getForwardVectorRaw(int direction)
    {
        //set vector3 based off of direction
        Vector3 forwardVector = new Vector3(0, 0, 0);
        if (direction == 0)
        {
            forwardVector = new Vector3(0, 0, 1f);
        }
        else if (direction == 1)
        {
            forwardVector = new Vector3(1f, 0, 0);
        }
        else if (direction == 2)
        {
            forwardVector = new Vector3(0, 0, -1f);
        }
        else if (direction == 3)
        {
            forwardVector = new Vector3(-1f, 0, 0);
        }
        return forwardVector;
    }
    public void CheckHitRaycastDistance(RaycastHit[] hitRays, out int closestIndex, out float closestDistance)
    {
        closestIndex = -1;
        float closestDist = closestDistance = float.PositiveInfinity;
        
        foreach(RaycastHit hitRay in hitRays.Where(x => x.collider.gameObject.GetComponent<MapCollider>() != null && x.distance < closestDist))
        {
            closestDistance = hitRay.distance;
            closestIndex = Array.IndexOf(hitRays, hitRay);
        }
    }
}
}
