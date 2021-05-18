/// Source: Pokémon Unity Redux
/// Purpose: Trainer data class for Pokémon Unity backend
/// Author: IIColour_Spectrum
/// Contributors: TeamPopplio
using UnityEngine;
using System.Collections;
using System;
using PokemonUnity.Backend.Serializables;
using PokemonUnity.Frontend.UI.Scenes;
using PokemonUnity.Frontend.UI;
using PokemonUnity.Frontend.Overworld;
using PokemonUnity.Frontend.Global;
using Sirenix.OdinInspector;

namespace PokemonUnity.Backend.Datatypes {

public class Trainer : PokemonUnity.Frontend.Overworld.NPC
{
    public enum Class
    {
        Trainer,
        AceTrainer,
        Surfer,
        Gamer,
        EGirl,
        Skater,
        Joestar,

    };

    private static string[] classString = new string[]
    {
        "Trainer",
        "Ace Trainer",
        "Surfer",
        "Gamer",
        "EGirl",
        "Skater",
        "Joestar",
    };

    private static int[] classPrizeMoney = new int[]
    {
        100,
        60
    };

    [HideInInspector]
    public Sprite[] uniqueSprites = new Sprite[0];

    [BoxGroup("Basic Information")]
    public Class trainerClass;

    [BoxGroup("Basic Information")]
    public int customPrizeMoney = 0;

    [BoxGroup("Basic Information")]
    public bool isFemale = false;

    [BoxGroup("Behavior")]
    public int viewDistance;

    [BoxGroup("Music")]
    public AudioClip battleBGM;

    [BoxGroup("Music")]
    public int samplesLoopStart;

    [BoxGroup("Music")]
    public AudioClip victoryBGM;

    [BoxGroup("Music")]
    public int victorySamplesLoopStart;

    [BoxGroup("Dialogue")]
    public string[] tightSpotDialog;

    [BoxGroup("Dialogue")]
    public string[] playerVictoryDialog;

    [BoxGroup("Dialogue")]
    public string[] playerLossDialog;

    [BoxGroup("Dialogue")]
    public string startBattleDialogue;

    private int distance;

    public Trainer(Pokemon[] party)
    {
        trainerClass = Class.Trainer;
        trainerName = "";

        this.party = party;
    }


    
    public void Update()
    {
        if(!busy && PokemonUnity.Frontend.Overworld.Player.player.moving == false)
        CheckForPlayer();
    }
    


    public Pokemon[] GetParty()
    {
        return party;
    }

    public string GetName()
    {
        return (!string.IsNullOrEmpty(trainerName))
            ? classString[(int) trainerClass] + " " + trainerName
            : classString[(int) trainerClass];
    }

    public Sprite[] GetSprites()
    {
        Sprite[] sprites = new Sprite[0];
        if (uniqueSprites.Length > 0)
        {
            sprites = uniqueSprites;
        }
        else
        {
            //Try to load female sprite if female
            if (isFemale)
            {
                sprites = Resources.LoadAll<Sprite>("NPCSprites/" + classString[(int) trainerClass] + "_f");
            }
            //Try to load regular sprite if male or female load failed
            if (!isFemale || (isFemale && sprites.Length < 1))
            {
                sprites = Resources.LoadAll<Sprite>("NPCSprites/" + classString[(int) trainerClass]);
            }
        }
        //if all load calls failed, load null as an array
        if (sprites.Length == 0)
        {
            sprites = new Sprite[] {Resources.Load<Sprite>("null")};
        }
        return sprites;
    }

    public int GetPrizeMoney()
    {
        int prizeMoney = (customPrizeMoney > 0) ? customPrizeMoney : classPrizeMoney[(int) trainerClass];
        int averageLevel = 0;
        for (int i = 0; i < party.Length; i++)
        {
            averageLevel += party[i].getLevel();
        }
        averageLevel = Mathf.CeilToInt((float) averageLevel / (float) party.Length);
        return averageLevel * prizeMoney;
    }

    public void CheckForPlayer()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position + new Vector3(0,.5f,0) +getForwardsVector()/2, getForwardsVector(), Color.green, 1f);
        if (Physics.Raycast(transform.position + new Vector3(0,.5f,0) +getForwardsVector()/2, getForwardsVector(), out hit, viewDistance))
        {
            //Debug.Log(hit.collider.name);
            if (hit.collider.gameObject.tag == "Player" && busy == false)
            {
                busy = true;
                distance = Convert.ToInt32(hit.distance);
                Debug.Log("distance from player is: " + distance);
                StartCoroutine("exclaimAnimation");
            }
        }
    }

    public IEnumerator trainerBattle(PokemonUnity.Backend.Datatypes.Trainer trainer)
    {
        BgmHandler.main.PlayOverlay(battleBGM, samplesLoopStart);

        yield return StartCoroutine(ScreenFade.main.FadeCutout(false, ScreenFade.slowedSpeed, null));
        //yield return new WaitForSeconds(sceneTransition.FadeOut(1f));
        SceneScript.main.Battle.gameObject.SetActive(true);
        StartCoroutine(SceneScript.main.Battle.Control(trainer));
        while (SceneScript.main.Battle.gameObject.activeSelf)
        {
            yield return null;
        }
        //yield return new WaitForSeconds(sceneTransition.FadeIn(0.4f));
        yield return StartCoroutine(ScreenFade.main.Fade(true, 0.4f));
        Player.player.canInput = true;
    }


    //Better exclaimation not yet implemented
    public IEnumerator exclaimAnimation()
    {
        float increment = -1f;
        float speed = 0.15f;

        exclaim.SetActive(true);
        Player.player.canInput = false;

        while (increment < 0.3f)
        {
            increment += (1 / speed) * Time.deltaTime;
            if (increment > 0.3f)
            {
                increment = 0.3f;
            }
            exclaim.transform.localScale = new Vector3(1, 1.3f + (-1.3f * increment * increment), 1);
            yield return null;
        }

        exclaim.transform.localScale = new Vector3(1, 1, 1);

        yield return new WaitForSeconds(1.2f);
        exclaim.SetActive(false);

        //walk over to player
        Vector3 moveToPlayer = getForwardsVector() * distance;
        Debug.Log(moveToPlayer);
        yield return StartCoroutine(move(moveToPlayer));


        //say dialogue
        SceneScript.main.Dialog.DrawDialogBox();
        yield return SceneScript.main.Dialog.StartCoroutine(SceneScript.main.Dialog.DrawText(startBattleDialogue));

        while (!Input.GetButtonDown("Select") && !Input.GetButtonDown("Back"))
        {
            yield return null;
        }
        if (Input.GetButtonDown("Select") || Input.GetButtonDown("Back"))
        {
            SceneScript.main.Dialog.UnDrawDialogBox();
        }
        yield return new WaitForSeconds(0.2f);

        //start battle
        StartCoroutine(trainerBattle(this));
    }

}



}
