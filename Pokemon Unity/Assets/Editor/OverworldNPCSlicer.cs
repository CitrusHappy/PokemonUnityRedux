 using UnityEngine;
 using UnityEditor;
 using System.Collections;
 using System.Collections.Generic;
 
 public class OverworldNPCSlicer : MonoBehaviour
 {
    [MenuItem("Pokémon Unity/Sprites/RearrangeNPCSprites")]

    static void ReorderSprites()
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
            string path = AssetDatabase.GetAssetPath(textures[z]);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            SpriteMetaData[] sliceMetaData = textureImporter.spritesheet;

            int index = 0;
            foreach (SpriteMetaData individualSliceData in sliceMetaData)
            {
                string rawName = sliceMetaData[index].name;
                rawName = rawName.Substring(0, rawName.LastIndexOf("_") + 1);
                if(index <= 7)
                {
                    sliceMetaData[index].name = string.Format(rawName + "{0}", index + 8);
                }
                else if (index >= 8 && index <=11)
                {
                    sliceMetaData[index].name = string.Format(rawName + "{0}", index - 4);
                }
                else if (index >= 12 && index <=15)
                {
                    sliceMetaData[index].name = string.Format(rawName + "{0}", index - 12);
                }
                
                //print(sliceMetaData[index].name);

                index++;
            }

            textureImporter.spritesheet = sliceMetaData;
            EditorUtility.SetDirty (textureImporter);
            textureImporter.SaveAndReimport ();

            AssetDatabase.ImportAsset (path, ImportAssetOptions.ForceUpdate);
         }
         Debug.Log("Done renaming!");
     }
 }