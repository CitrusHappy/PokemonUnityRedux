﻿/// Source: Pokémon Unity Redux
/// Purpose: NPC sprite for Pokémon Unity frontend
/// Author: IIColour_Spectrum
/// Contributors: TeamPopplio
using UnityEngine;
using System.Collections;
using PokemonUnity.Backend.Serializables;
using PokemonUnity.Backend.Databases;
using PokemonUnity.Frontend.Overworld.Mapping;
using PokemonUnity.Frontend.UI.Scenes;
using PokemonUnity.Frontend.UI;
using Sirenix.OdinInspector;
using UnityEditor;

namespace PokemonUnity.Frontend.Overworld {
public class NPC : CharacterBase
{
    private Player Player;

    [BoxGroup("Basic Information")]
    public int pokemonID = 0;

    public enum NPCBehaviour
    {
        Idle,
        Walk,
        Patrol
    }

    [BoxGroup("Behavior"), EnumToggleButtons]
    public NPCBehaviour npcBehaviour;
    private Sprite[] lightSheet;

    [BoxGroup("Behavior"), ShowIf("npcBehaviour", NPCBehaviour.Patrol)]
    public WalkCommand[] patrol = new WalkCommand[1];

    [BoxGroup("Behavior"), ShowIf("npcBehaviour", NPCBehaviour.Walk)]
    public Vector2 walkRange;
    private Vector3 initialPosition;

    [BoxGroup("Debug"), ReadOnly]
    public bool trainerSurfing = false;

    private bool overrideBusy = false;

    private SpriteRenderer pawnLightSprite;
    private SpriteRenderer pawnLightReflectionSprite;

    private Light npcLight;


    [HideInInspector]
    public MapCollider destinationMap;

    [HideInInspector]
    public GameObject exclaim;

    [BoxGroup("Dialogue")]
    public string[] dialogue;

    [BoxGroup("Pokemon Party")]
    public PokemonInitialiser[] trainerParty = new PokemonInitialiser[1];

    [OnInspectorGUI]
	[PropertyOrder(-10), HorizontalGroup("Split", 100), BoxGroup("Pokemon Party")]
	private void ShowImage()
	{
		//GUILayout.Label(AssetDatabase.LoadAssetAtPath("Assets/Plugins/Sirenix/Assets/Editor/Odin Inspector Logo.png", typeof(Sprite)));
	}

    [HideInInspector]
    public Pokemon[] party;

    [BoxGroup("Basic Information")]
    public string trainerName;
    

    //this is not being executed by trainers need to fix that
    public override void Awake()
    {
        party = new Pokemon[trainerParty.Length];

        Player = Player.player;
        pawnSprite = transform.Find("Pawn").GetComponent<SpriteRenderer>();
        pawnReflectionSprite = transform.Find("PawnReflection").GetComponent<SpriteRenderer>();

        if (pokemonID != 0)
        {
            pawnLightSprite = transform.Find("PawnLight").GetComponent<SpriteRenderer>();
            pawnLightReflectionSprite = transform.Find("PawnLightReflection").GetComponent<SpriteRenderer>();
            npcLight = transform.Find("Point light").GetComponent<Light>();
        }

        hitBox = transform.Find("NPC_Object");
        if (pokemonID == 0)
        {
            spriteSheet = Resources.LoadAll<Sprite>("OverworldNPCSprites/" + spriteName);
        }
        else
        {
            spriteSheet = Pokemon.GetSpriteFromID(pokemonID, false, false);

            npcLight.intensity = PokemonDatabase.getPokemon(pokemonID).getLuminance();
            npcLight.color = PokemonDatabase.getPokemon(pokemonID).getLightColor();
            lightSheet = Pokemon.GetSpriteFromID(pokemonID, false, true);
        }

        exclaim = transform.Find("Exclaim").gameObject;
    }

    //this is not being executed by trainers need to fix that
    public override void Start()
    {
        for (int i = 0; i < trainerParty.Length; i++)
        {
            party[i] = new Pokemon(trainerParty[i].ID, trainerParty[i].gender, trainerParty[i].level, "Poké Ball", trainerParty[i].heldItem, trainerName, trainerParty[i].ability);
        }

        initialPosition = hitBox.position;

        hitBox.localPosition = new Vector3(0, 0, 0);

        exclaim.SetActive(false);



        StartCoroutine("animateSprite");

        if(npcBehaviour == NPCBehaviour.Idle)
        animPause = true;


        //Check current map
        RaycastHit[] hitRays = Physics.RaycastAll(transform.position + Vector3.up, Vector3.down);
        int closestIndex = -1;
        float closestDistance = float.PositiveInfinity;
        if (hitRays.Length > 0)
        {
            for (int i = 0; i < hitRays.Length; i++)
            {
                if (hitRays[i].collider.gameObject.GetComponent<MapCollider>() != null)
                {
                    if (hitRays[i].distance < closestDistance)
                    {
                        closestDistance = hitRays[i].distance;
                        closestIndex = i;
                    }
                }
            }
        }
        if (closestIndex != -1)
        {
            currentMap = hitRays[closestIndex].collider.gameObject.GetComponent<MapCollider>();
        }
        else
        {
            Debug.Log("no map found for: " + gameObject.name);
        }


        if (npcBehaviour == NPCBehaviour.Walk)
        {
            StartCoroutine("walkAtRandom");
        }
        else if (npcBehaviour == NPCBehaviour.Patrol)
        {
            StartCoroutine("patrolAround");
        }
    }





    public override IEnumerator animateSprite()
    {
        if (pokemonID == 0)
        {
            frame = 0;
            frames = 4;
            framesPerSec = 7;
            secPerFrame = 1f / (float) framesPerSec;
            while (true)
            {
                for (int i = 0; i < 4; i++)
                {
                    while (Player.player.busyWith != null && Player.player.busyWith != this.gameObject &&
                           !overrideBusy)
                    {
                        yield return null;
                    }
                    if (animPause && frame % 2 != 0)
                    {
                        frame -= 1;
                    }
                    //Debug.Log((int)direction);
                    pawnSprite.sprite = spriteSheet[(int)direction * frames + frame];
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
        else
        {
            frame = 0;
            while (true)
            {
                for (int i = 0; i < 6; i++)
                {
                    pawnSprite.sprite = spriteSheet[(int)direction * 2 + frame];
                    pawnLightSprite.sprite = lightSheet[(int)direction * 2 + frame];

                    pawnReflectionSprite.sprite = pawnSprite.sprite;
                    pawnLightReflectionSprite.sprite = pawnLightSprite.sprite;
                    if (i > 2)
                    {
                        pawnSprite.transform.localPosition = new Vector3(0, 0.17f, -0.36f);
                        pawnLightSprite.transform.localPosition = new Vector3(0, 0.171f, -0.36f);
                    }
                    else
                    {
                        pawnSprite.transform.localPosition = new Vector3(0, 0.2f, -0.305f);
                        pawnLightSprite.transform.localPosition = new Vector3(0, 0.201f, -0.305f);
                    }
                    yield return new WaitForSeconds(0.055f);
                }
                frame = (frame == 0) ? 1 : 0;
            }
        }
    }

    public void setFrameStill()
    {
        if (frame % 2 != 0)
        {
            frame -= 1;
        }
        pawnSprite.sprite = spriteSheet[(int)direction * frames + frame];
        pawnReflectionSprite.sprite = pawnSprite.sprite;
    }

    public void setDirection(Direction newDirection)
    {
        direction = newDirection;
        pawnSprite.sprite = spriteSheet[(int)direction * frames + frame];
        pawnReflectionSprite.sprite = pawnSprite.sprite;
    }

    private IEnumerator walkAtRandom()
    {
        float waitTime;
        int newDirection;
        int walkDistance;
        while (true)
        {
            while (!busy)
            {
                waitTime = Random.Range(-0.8f, 1.6f);
                waitTime = 1.1f + (waitTime * waitTime * waitTime);

                newDirection = Random.Range(0, 4);
                walkDistance = Random.Range(0, 5);
                if (walkDistance > 1)
                {
                    //make movements of 1 more likely than others. 
                    walkDistance -= 1;
                }

                while (busy)
                {
                    yield return null;
                }
                direction = (Direction)newDirection;
                yield return null; //2 frame delay to prevent taking a step before initialising battle.
                yield return null;

                //walk
                for (int i = 0; i < walkDistance; i++)
                {
                    bool atEdge = false;
                    if (newDirection == 0)
                    {
                        if (hitBox.position.z >= (initialPosition.z + walkRange.y))
                        {
                            atEdge = true;
                        }
                    }
                    else if (newDirection == 1)
                    {
                        if (hitBox.position.x >= (initialPosition.x + walkRange.x))
                        {
                            atEdge = true;
                        }
                    }
                    else if (newDirection == 2)
                    {
                        if (hitBox.position.z <= (initialPosition.z - walkRange.y))
                        {
                            atEdge = true;
                        }
                    }
                    else if (newDirection == 3)
                    {
                        if (hitBox.position.x <= (initialPosition.x - walkRange.x))
                        {
                            atEdge = true;
                        }
                    }

                    if (!atEdge)
                    {
                        Vector3 movement = getForwardsVector();
                        if (movement != new Vector3(0, 0, 0))
                        {
                            yield return StartCoroutine(move(movement));
                        }
                    }
                }

                yield return new WaitForSeconds(waitTime);
            }
            yield return null;
        }
    }

    private IEnumerator patrolAround()
    {
        while (true)
        {
            for (int i = 0; i < patrol.Length; i++)
            {
                while (busy)
                {
                    yield return null;
                }
                direction = (Direction)patrol[i].direction;
                yield return null; //2 frame delay to prevent taking a step before initialising battle.
                yield return null;

                for (int i2 = 0; i2 < patrol[i].steps; i2++)
                {
                    Vector3 movement = getForwardsVector();
                    while (movement == new Vector3(0, 0, 0))
                    {
                        movement = getForwardsVector();
                        yield return new WaitForSeconds(0.1f);
                    }

                    while (busy)
                    {
                        yield return null;
                    }

                    yield return StartCoroutine(move(movement));
                }

                if (patrol[i].endWait > 0)
                {
                    yield return new WaitForSeconds(patrol[i].endWait);
                }

                i = patrol.Length;
            }
            transform.position = initialPosition;
            yield return null;
        }
    }

    public Vector3 getForwardsVector()
    {
        return getForwardsVector(false);
    }

    public Vector3 getForwardsVector(bool noClip)
    {
        Vector3 forwardsVector = new Vector3(0, 0, 0);
        if (direction == Direction.Up)
        {
            forwardsVector = new Vector3(0, 0, 1f);
        }
        else if (direction == Direction.Right)
        {
            forwardsVector = new Vector3(1f, 0, 0);
        }
        else if (direction == Direction.Down)
        {
            forwardsVector = new Vector3(0, 0, -1f);
        }
        else if (direction == Direction.Left)
        {
            forwardsVector = new Vector3(-1f, 0, 0);
        }

        Vector3 movement = forwardsVector;

        //Check destination map																	//0.5f to adjust for stair height
        //cast a ray directly downwards from the position directly in front of the npc			//1f to check in line with player's head
        RaycastHit[] mapHitColliders = Physics.RaycastAll(transform.position + movement + new Vector3(0, 1.5f, 0),
            Vector3.down);
        RaycastHit mapHit = new RaycastHit();
        //cycle through each of the collisions
        if (mapHitColliders.Length > 0)
        {
            for (int i = 0; i < mapHitColliders.Length; i++)
            {
                //if a collision's gameObject has a mapCollider, it is a map. set it to be the destination map.
                if (mapHitColliders[i].collider.gameObject.GetComponent<MapCollider>() != null)
                {
                    mapHit = mapHitColliders[i];
                    destinationMap = mapHit.collider.gameObject.GetComponent<MapCollider>();
                    i = mapHitColliders.Length;
                }
            }
        }

        //check for a bridge at the destination
        RaycastHit bridgeHit =
            MapCollider.getBridgeHitOfPosition(transform.position + movement + new Vector3(0, 1.5f, 0));
        if (bridgeHit.collider != null)
        {
            //modify the forwards vector to align to the bridge.
            movement -= new Vector3(0, (transform.position.y - bridgeHit.point.y), 0);
        }
        //if no bridge at destination
        else if (mapHit.collider != null)
        {
            //modify the forwards vector to align to the mapHit.
            movement -= new Vector3(0, (transform.position.y - mapHit.point.y), 0);
        }


        float currentSlope = Mathf.Abs(MapCollider.getSlopeOfPosition(transform.position, (int)direction));
        float destinationSlope =
            Mathf.Abs(MapCollider.getSlopeOfPosition(transform.position + forwardsVector, (int)direction));
        float yDistance = Mathf.Abs((transform.position.y + movement.y) - transform.position.y);
        yDistance = Mathf.Round(yDistance * 100f) / 100f;

        //if either slope is greater than 1 it is too steep.
        if (currentSlope <= 1 && destinationSlope <= 1)
        {
            //if yDistance is greater than both slopes there is a vertical wall between them
            if (yDistance <= currentSlope || yDistance <= destinationSlope)
            {
                //check destination tileTag for impassibles unless NoClipping
                if (!noClip)
                {
                    int destinationTileTag = destinationMap.getTileTag(transform.position + movement);
                    if (destinationTileTag == 1)
                    {
                        return Vector3.zero;
                    }
                    else
                    {
                        if (trainerSurfing)
                        {
                            //if a surf trainer, normal tiles are impassible
                            if (destinationTileTag != 2)
                            {
                                return Vector3.zero;
                            }
                        }
                        else
                        {
                            //if not a surf trainer, surf tiles are impassible
                            if (destinationTileTag == 2)
                            {
                                return Vector3.zero;
                            }
                        }
                    }
                }

                bool destinationPassable = true;
                if (!noClip)
                {
                    //check destination for objects/player/follower
                    Collider[] hitColliders = Physics.OverlapSphere(transform.position + movement, 0.4f);
                    if (hitColliders.Length > 0)
                    {
                        for (int i = 0; i < hitColliders.Length; i++)
                        {
                            if (hitColliders[i].name == "Player_Transparent" ||
                                hitColliders[i].name == "Follower_Transparent" ||
                                hitColliders[i].name.ToLowerInvariant().Contains("_object"))
                            {
                                destinationPassable = false;
                            }
                        }
                    }
                }

                if (destinationPassable)
                {
                    return movement;
                }
            }
        }
        return Vector3.zero;
    }


    public IEnumerator move(Vector3 movement)
    {
        yield return StartCoroutine(move(movement, 1));
    }

    public IEnumerator move(Vector3 movement, float speedMod)
    {
        float increment = 0f;

        if (speedMod <= 0)
        {
            speedMod = 1f;
        }
        float speed = Player.player.walkSpeed / speedMod;
        framesPerSec = Mathf.RoundToInt(7f * speedMod);

        Vector3 startPosition = transform.position;
        Vector3 destinationPosition = startPosition + movement;

        animPause = false;
        while (increment < 1f)
        {
            //increment increases slowly to 1 over the frames
            if (Player.player.busyWith == null || Player.player.busyWith == this.gameObject ||
                overrideBusy)
            {
                increment += (1f / speed) * Time.deltaTime;
                    //speed is determined by how many squares are crossed in one second
                if (increment > 1)
                {
                    increment = 1;
                }
                transform.position = startPosition + (movement * increment);
                hitBox.position = destinationPosition;
            }
            yield return null;
        }
        animPause = true;
    }

    public void setOverrideBusy(bool set)
    {
        overrideBusy = set;
    }

    public IEnumerator interact()
    {
        if (Player.setCheckBusyWith(this.gameObject))
        {
            if (npcBehaviour == NPCBehaviour.Walk)
            {
                StopCoroutine("walkAtRandom");
            }
            else if (npcBehaviour == NPCBehaviour.Patrol)
            {
                StopCoroutine("patrolAround");
            }

            
            //calculate Player's position relative to target object's and set direction accordingly. (Face the player)
            float xDistance = this.transform.position.x - Player.gameObject.transform.position.x;
            float zDistance = this.transform.position.z - Player.gameObject.transform.position.z;
            if (xDistance >= Mathf.Abs(zDistance))
            {
                //Mathf.Abs() converts zDistance to a positive always.
                direction = Direction.Left; //this allows for better accuracy when checking orientation.
            }
            else if (xDistance <= Mathf.Abs(zDistance) * -1)
            {
                direction = Direction.Right;
            }
            else if (zDistance >= Mathf.Abs(xDistance))
            {
                direction = Direction.Down;
            }
            else
            {
                direction = Direction.Up;
            }

            //SaveData.currentSave.PC.boxes[0][followerIndex].getName() +
            foreach (string t in dialogue)
            {
                while (!Input.GetButtonDown("Select") && !Input.GetButtonDown("Back"))
                {
                    yield return null;
                }
                if (Input.GetButtonDown("Select") || Input.GetButtonDown("Back"))
                {
                    SceneScript.main.Dialog.DrawDialogBox();
                    yield return SceneScript.main.Dialog.StartCoroutine(SceneScript.main.Dialog.DrawText(t));
                }
                
            }

            while (!Input.GetButtonDown("Select") && !Input.GetButtonDown("Back"))
            {
                yield return null;
            }

            SceneScript.main.Dialog.UnDrawDialogBox();
            yield return new WaitForSeconds(0.2f);
            Player.unsetCheckBusyWith(this.gameObject);


            if (npcBehaviour == NPCBehaviour.Walk)
            {
                StartCoroutine("walkAtRandom");
            }
            else if (npcBehaviour == NPCBehaviour.Patrol)
            {
                StartCoroutine("patrolAround");
            }
        }
    }





}


[System.Serializable]
public class WalkCommand
{
    public int direction;
    public int steps;
    public float endWait;
}

[System.Serializable]
public class PokemonInitialiser
{
    public int ID;
    public int level;
    public Pokemon.Gender gender;
    public string heldItem;
    public int ability;
}
}
