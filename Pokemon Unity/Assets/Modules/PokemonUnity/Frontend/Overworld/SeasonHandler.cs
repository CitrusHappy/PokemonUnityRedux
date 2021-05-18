using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonHandler : MonoBehaviour
{
    public string season;

    //find all materials on all children objects
    //set source images to materialname + _ + season

    // Start is called before the first frame update
    void Start()
    {
        season = CalculateSeason();
        Debug.Log(season);
    }

    string CalculateSeason()
    {
        float value = System.DateTime.Now.Month + System.DateTime.Now.Day / 100f;
        if (value < 3.21 || value >= 12.21) return "winter";
        if (value < 6.21) return "spring";
        if (value < 9.23) return "summer";
        return "autumn";
    }

    public string getSeason()
    {
        return season;
    }
}
