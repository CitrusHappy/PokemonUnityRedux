/// Source: Pokémon Unity Redux
/// Purpose: Map name box UI for Pokémon Unity frontend
/// Author: IIColour_Spectrum
/// Contributors: TeamPopplio
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using PokemonUnity.Frontend.Overworld.Mapping;
using BrunoMikoski.TextJuicer;
namespace PokemonUnity.Frontend.UI {
public class MapNameBoxHandler : MonoBehaviour
{
    private MapSettings map;
    private Transform mapName;
    private Image mapNameBox;
    private TMP_Text mapNameText;
    private TMP_Text mapNameTextShadow;

    private TMP_TextJuicer text;
    private TMP_TextJuicer textShadow;

    private Coroutine mainDisplay;

    public float duration;

    

    void Awake()
    {
        map = GameObject.Find("Map").GetComponent<MapSettings>();
        mapName = gameObject.transform;
        mapNameBox = mapName.Find("BoxImage").GetComponent<Image>();
        mapNameText = mapName.Find("BoxText").GetComponent<TMP_Text>();
        mapNameTextShadow = mapName.Find("BoxTextShadow").GetComponent<TMP_Text>();

        text = mapName.Find("BoxText").GetComponent<TMP_TextJuicer>();
        textShadow = mapName.Find("BoxTextShadow").GetComponent<TMP_TextJuicer>();
    }

    void Start()
    {
        //display(map.mapName);
        mapNameText.alpha = 0;
        mapNameTextShadow.alpha = 0;
        gameObject.GetComponent<CanvasGroup>().alpha = 0;

        if (map.showMapName == true)
        {
            display(map.mapName);
        }
    }

    public void display(string name)
    {
        if (mainDisplay != null)
        {
            StopCoroutine(mainDisplay);
        }
        mainDisplay = StartCoroutine(displayCoroutine(name));
    }

    private IEnumerator displayCoroutine(string name)
    {
        mapNameText.text = name;
        mapNameTextShadow.text = name;

        mapNameText.alpha = 0;
        mapNameTextShadow.alpha = 0;

        for(float i = 0f; i < duration; i += 0.05f)
        {
            gameObject.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(0,1, i/duration);
            yield return null;
        }

        
        mapNameText.alpha = 1;
        mapNameTextShadow.alpha = 1;
        //slide text in here
        text.Play();
        textShadow.Play();
        while(text.Progress < 1 && textShadow.Progress < 1)
        {
            yield return null;
        }
        
        
        
        

        yield return new WaitForSeconds(.7f);

        for(float i = 0f; i < duration; i += 0.05f)
        {
            gameObject.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(1,0, i/duration);
            yield return null;
        }
    }
}
}
