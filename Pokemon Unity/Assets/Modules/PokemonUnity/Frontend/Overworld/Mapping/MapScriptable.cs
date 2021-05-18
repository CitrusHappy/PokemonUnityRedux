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

[CreateAssetMenu]    
public class MapScriptable : SerializedScriptableObject
{
    //Collision Map String provided by DeKay's Collision Map Compiler for Pokémon Essentials
    //See TOOLS folder for details

    [TitleGroup("Map Compiling"), OdinSerialize, HideInInspector, ReadOnly]
    public Dictionary<(int, int), int> collisionMap;

    [TitleGroup("Map Compiling")]
    public LayerMask mask;

    [TitleGroup("Map Data"), ReadOnly]
    public int width;

    [TitleGroup("Map Data"), ReadOnly]
    public int length;

    [TitleGroup("Map Data")]
    public int height;

    [HideLabel]
    private Material currentTile;

    [TitleGroup("Material Definition")]
    [HorizontalGroup("Material Definition/Split")]
    [VerticalGroup("Material Definition/Split/Left")]
    [PreviewField]
    public List<Material> waterMats, impassibleMats, sandMats;


    [VerticalGroup("Material Definition/Split/Right")]
    [PreviewField]
    public List<Material> walkableMats, tallGrassMats;


    [TitleGroup("Map Compiling"), Button]
    public void CompileMapInCurrentScene()
    {
        Compile(GameObject.FindGameObjectWithTag("MapCollider"));
        calculateCollisionMap();
        Debug.Log(collisionMap);
    }

    public void Compile(GameObject compileObj)
    {
        Debug.Log("Compiling map...");
        Mesh map = new Mesh();
        if(!compileObj.GetComponent<MeshFilter>())
        {
            compileObj.AddComponent<MeshFilter>();
        }
        if(!compileObj.GetComponent<MeshCollider>())
        {
            compileObj.AddComponent<MeshCollider>();
        }
        compileObj.GetComponent<MeshFilter>().mesh = null;
        MeshFilter[] meshFilters = compileObj.GetComponentsInChildren<MeshFilter>();
        List<CombineInstance> subCombine = new List<CombineInstance> ();

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++) {

            if (meshFilters[i].sharedMesh == null) 
            {
                continue;
            }
                

            for (int j = 0; j < meshFilters[i].sharedMesh.subMeshCount; j++)
            {
 
                 CombineInstance ci = new CombineInstance ();
 
                 ci.mesh = meshFilters[i].sharedMesh;
                 ci.subMeshIndex = j;
                 ci.transform = meshFilters[i].transform.localToWorldMatrix;
 
                 subCombine.Add (ci);
            }

        }
		map.CombineMeshes(subCombine.ToArray(), true, true);
        compileObj.GetComponent<MeshFilter>().sharedMesh = map;
		compileObj.GetComponent<MeshCollider>().sharedMesh = compileObj.GetComponent<MeshFilter>().sharedMesh;
        compileObj.SetActive(true);
    }

    private bool ValidateCompile()
    {
        // Return false if no gameobject is selected.
        return Selection.activeGameObject != null;
    }

    
    public void calculateCollisionMap()
    {
        Debug.Log("Calculating TileTags...");
        GameObject map = GameObject.FindGameObjectWithTag("Map");
        Mesh mapMesh = map.GetComponent<MeshFilter>().sharedMesh;
        Bounds bounds = mapMesh.bounds;

        width = Mathf.RoundToInt(bounds.size.x);
        length = Mathf.RoundToInt(bounds.size.y);
        //height = Mathf.RoundToInt(bounds.size.z);

        collisionMap = new Dictionary<(int, int), int>();

        Debug.Log("width: " + bounds.size.x + " |    length: " + bounds.size.y );


        int tag;
        RaycastHit tileHit; 
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                // see what material the player will be walking on next
                if(Physics.Raycast(new Vector3(-width/2 + x, height, length/2 - z), Vector3.down, out tileHit, Mathf.Infinity, mask))
                {
                    Debug.DrawRay(new Vector3(-width/2 + x, height, length/2 - z), Vector3.down, Color.cyan, 9999f);
                    //Debug.Log(tileHit.collider);

                    if (tileHit.collider.GetComponent<MeshFilter>() != null)
                    {
                        Mesh mesh = tileHit.collider.GetComponent<MeshFilter>().sharedMesh;
                        int triangleIdx = tileHit.triangleIndex;
                        int subMeshesNr = mesh.subMeshCount;
                        int materialIdx = -1;

                        int lookupIdx1 = mesh.triangles[triangleIdx * 3];
                        int lookupIdx2 = mesh.triangles[triangleIdx * 3 + 1];
                        int lookupIdx3 = mesh.triangles[triangleIdx * 3 + 2];

                        for(int g=0; g<subMeshesNr ; g++) 
                        {
                            int[] tr= mesh.GetTriangles(g);
                            for(int j=0; j<tr.Length; j++) 
                            {
                                if (tr[j] == lookupIdx1 && tr[j+1] == lookupIdx2 && tr[j+2] == lookupIdx3) 
                                {
                                    materialIdx = g;
                                    break;
                                }
                            }
                            if (materialIdx != -1) 
                            {
                                currentTile = tileHit.collider.GetComponent<Renderer>().sharedMaterials[materialIdx];
                                //Debug.Log(currentTile);
                                break;
                            }
                        }
                    }

                    if (waterMats.Contains(currentTile) == true)
                    {
                        tag = 2;
                    }
                    else if(impassibleMats.Contains(currentTile) == true)
                    {
                        tag = 1;
                    }
                    else if(walkableMats.Contains(currentTile) == true)
                    {
                        tag = 3;
                    }
                    else if(tallGrassMats.Contains(currentTile) == true)
                    {
                        tag = 4;
                    }
                    else if(sandMats.Contains(currentTile) == true)
                    {
                        tag = 5;
                    }
                    else
                    {
                        tag = 0;
                    }
                }
                else
                {
                    //tile not found
                    tag = -1;
                }

                //Debug.Log("at point (" + x + ", " + z + ") is " + currentTile );
                //Debug.Log(tag);
                collisionMap.Add((x, z), tag);
            }
        }

        Debug.Log("Finished Tagging Map");
    }
    
    }
}
