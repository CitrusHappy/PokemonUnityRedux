/// Source: Pokémon Unity Redux
/// Purpose: Map collider for Pokémon Unity frontend
/// Author: IIColour_Spectrum
/// Contributors: TeamPopplio
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;

namespace PokemonUnity.Frontend.Overworld.Mapping {
public class MapCollider : MonoBehaviour
{
    //Collision Map String provided by DeKay's Collision Map Compiler for Pokémon Essentials
    //See TOOLS folder for details
    
    //Tile Tags:
    //0 - Default Environment
    //1 - Impassable
    //2 - Surf Water
    //3 - Decor/Ignore Slope (walkable)
    //4 - Tallgrass (wild encounter)
    //5 - sand

    public MapScriptable map;

    public int getTileTag(Vector3 position)
    {
        int mapX =
            Mathf.RoundToInt(Mathf.Round(position.x) - Mathf.Round(transform.position.x) +
                             Mathf.Floor((float) map.width / 2f));
        int mapZ =
            Mathf.RoundToInt(map.length -
                             (Mathf.Round(position.z) - Mathf.Round(transform.position.z) +
                              Mathf.Floor((float) map.length / 2f)));
        //Debug.Log (mapX +" "+ mapZ +", "+ width +" "+ length);
        if (mapX < 0 || mapX >= map.width ||
            mapZ < 0 || mapZ >= map.length)
        {
            //Debug.Log (mapX +" "+ mapZ +", "+ width +" "+ length);
            return -1;
        }
        int tag = map.collisionMap[(Mathf.FloorToInt(mapX), Mathf.FloorToInt(mapZ))];
        return tag;
    }

    /// if bridge was found, returned RaycastHit will have a collider
    public static RaycastHit getBridgeHitOfPosition(Vector3 position)
    {
        //Check for bridges below inputted position
        //cast a ray directly downwards from the position entered
        RaycastHit[] bridgeHitColliders = Physics.RaycastAll(position, Vector3.down, 3f);
        RaycastHit bridgeHit = new RaycastHit();
        //cycle through each of the collisions
        for (int i = 0; i < bridgeHitColliders.Length; i++)
        {
            //if a collision's gameObject has a bridgeHandler, it is a bridge.
            if (bridgeHitColliders[i].collider.gameObject.GetComponent<Bridge>() != null)
            {
                bridgeHit = bridgeHitColliders[i];
                i = bridgeHitColliders.Length;
            }
        }

        return bridgeHit;
    }

    /// returns the slope of the map geometry on the tile of the given position (in the given direction) 
    public static float getSlopeOfPosition(Vector3 position, int direction, bool checkForBridge = true)
    {
        //set vector3 based off of direction
        Vector3 movement = new Vector3(0, 0, 0);
        if (direction == 0)
        {
            movement = new Vector3(0, 0, 1f);
        }
        else if (direction == 1)
        {
            movement = new Vector3(1f, 0, 0);
        }
        else if (direction == 2)
        {
            movement = new Vector3(0, 0, -1f);
        }
        else if (direction == 3)
        {
            movement = new Vector3(-1f, 0, 0);
        }

        //cast a ray directly downwards from the edge of the tile, closest to original position (1.5f height to account for stairs)
        RaycastHit[] mapHitColliders = Physics.RaycastAll(position - (movement * 0.45f) + new Vector3(0, 1.5f, 0),
            Vector3.down);
        RaycastHit map1Hit = new RaycastHit();

        float shortestHit = Mathf.Infinity;
        int shortestHitIndex = -1;
        //cycle through each of the collisions
        for (int i = 0; i < mapHitColliders.Length; i++)
        {
            //if a collision's gameObject has a MapCollider or a Bridge, it is a valid tile.
            if (checkForBridge)
            {
                if (mapHitColliders[i].collider.gameObject.GetComponent<Bridge>() != null ||
                    mapHitColliders[i].collider.gameObject.GetComponent<MapCollider>() != null)
                {
                    //check if distance is shorter than last recorded shortest
                    if (mapHitColliders[i].distance < shortestHit)
                    {
                        shortestHit = mapHitColliders[i].distance;
                        shortestHitIndex = i;
                    }
                }
            }
            else
            {
                if (mapHitColliders[i].collider.gameObject.GetComponent<MapCollider>() != null)
                {
                    //check if distance is shorter than last recorded shortest
                    if (mapHitColliders[i].distance < shortestHit)
                    {
                        shortestHit = mapHitColliders[i].distance;
                        shortestHitIndex = i;
                    }
                }
            }
        }
        //if index is not -1, a map/bridge was found
        if (shortestHitIndex != -1)
        {
            map1Hit = mapHitColliders[shortestHitIndex];
        }


        //cast another ray at the edge of the tile, further from original position (1.5f height to account for stairs)
        mapHitColliders = Physics.RaycastAll(position + (movement * 0.45f) + new Vector3(0, 1.5f, 0), Vector3.down);
        RaycastHit map2Hit = new RaycastHit();

        shortestHit = Mathf.Infinity;
        shortestHitIndex = -1;
        //cycle through each of the collisions
        for (int i = 0; i < mapHitColliders.Length; i++)
        {
            //if a collision's gameObject has a MapCollider or a Bridge, it is a valid tile.
            if (checkForBridge)
            {
                if (mapHitColliders[i].collider.gameObject.GetComponent<Bridge>() != null ||
                    mapHitColliders[i].collider.gameObject.GetComponent<MapCollider>() != null)
                {
                    //check if distance is shorter than last recorded shortest
                    if (mapHitColliders[i].distance < shortestHit)
                    {
                        shortestHit = mapHitColliders[i].distance;
                        shortestHitIndex = i;
                    }
                }
            }
            else
            {
                if (mapHitColliders[i].collider.gameObject.GetComponent<MapCollider>() != null)
                {
                    //check if distance is shorter than last recorded shortest
                    if (mapHitColliders[i].distance < shortestHit)
                    {
                        shortestHit = mapHitColliders[i].distance;
                        shortestHitIndex = i;
                    }
                }
            }
        }
        //if index is not -1, a map/bridge was found
        if (shortestHitIndex != -1)
        {
            map2Hit = mapHitColliders[shortestHitIndex];
        }


        if (map1Hit.collider == null || map2Hit.collider == null)
        {
            if (map1Hit.collider == null && map2Hit.collider == null)
            {
                Debug.Log("DEBUG: No Map1Hit or Map2Hit!");
            }
            else if (map1Hit.collider == null)
            {
                Debug.Log("DEBUG: No Map1Hit!");
            }
            else
            {
                Debug.Log("DEBUG: No Map1Hit!");
            }

            return 0;
        }

        //flatten the hit.point along the y, so that the distance between them will only calculate using x and z
        Vector3 flatHitPoint1 = new Vector3(map1Hit.point.x, 0, map1Hit.point.z);
        Vector3 flatHitPoint2 = new Vector3(map2Hit.point.x, 0, map2Hit.point.z);

        float rise = Mathf.Abs(map2Hit.point.y - map1Hit.point.y);
        float run = 0.9f;
        float slope = rise / run;

        return Mathf.Round(slope * 100f) / 100f;
    }

}
}
