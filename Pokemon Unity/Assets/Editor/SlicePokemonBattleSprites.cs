 using UnityEngine;
 using UnityEditor;
 using System.Collections;
 using System.Collections.Generic;
 
 public class SlicePokemonBattleSprites : MonoBehaviour
 {
     [MenuItem("Pokémon Unity/Sprites/SlicePokemonBattleSprites")]
     static void SliceSprites()
     {
         List<Texture2D> textures = new List<Texture2D>();

         foreach(Object o in Selection.objects)
         {
             if (o.GetType() == typeof(Texture2D))
             {
                 textures.Add((Texture2D)o);
             }
         }
 
         for (int z = 0; z < textures.Count; z++)
         {
             Debug.Log(textures[z]);
             
             string path = AssetDatabase.GetAssetPath(textures[z]);

             Debug.Log(path);
             TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
             ti.isReadable = true;
             ti.filterMode = FilterMode.Point;

            //wipe all previous spritesheet data
             if (ti.spriteImportMode == SpriteImportMode.Multiple)
             {
                 ti.spriteImportMode = SpriteImportMode.Single;
                 AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
             }

             ti.spriteImportMode = SpriteImportMode.Multiple;
 
             List<SpriteMetaData> newData = new List<SpriteMetaData>();
 
             int SliceWidth = 160;
             int SliceHeight = 160;
 
             for (int i = 0; i < textures[z].width; i += SliceWidth)
             {
                 for (int j = 0; j < textures[z].height; j += SliceHeight)
                 {
                    bool hasSprite = false;
                    Color[] pixels = textures[z].GetPixels(i, j, SliceWidth, SliceHeight);
                    foreach (Color p in pixels)
                    {
                        
                        if(p.a != 0)
                        {
                            hasSprite = true;
                        }
                    }

                    if (hasSprite == true)
                    {
                        SpriteMetaData smd = new SpriteMetaData();
                        smd.pivot = new Vector2(0.5f, 0.5f);
                        smd.alignment = 0;
                        smd.name = (textures[z].height - j) / SliceHeight + ", " + i / SliceWidth;
                        smd.rect = new Rect(i, j, SliceWidth, SliceHeight);

                        //Debug.Log(smd);
                        newData.Add(smd);
                    }
                 }
             }
 
             ti.spritesheet = newData.ToArray();
             AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
         }
         Debug.Log("Done Slicing!");
     }
 }