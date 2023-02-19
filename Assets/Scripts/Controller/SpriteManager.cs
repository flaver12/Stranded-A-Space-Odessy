using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Xml;

namespace org.flaver.controller
{
    public class SpriteManager : MonoBehaviour
    {
        public static SpriteManager Instance { get; private set; }
        private Dictionary<string, Sprite> sprites;
        
        private void Awake()
        {
            Debug.Log("SpriteManager init");
            Instance = this;

            LoadSprites();
        }

        public Sprite GetSprite(string spriteCategory, string spriteName)
        {
            string key = spriteCategory + "/" + spriteName;
            if (!sprites.ContainsKey(key))
            {
                Debug.LogError($"Tried to load sprite with name: {key}, could not find it");
                // TODO dummy texture?
                return null;
            }

            return sprites[key];
        }

        private void LoadSprites()
        {
            sprites = new Dictionary<string, Sprite>();

            string filePath = Path.Combine(Application.streamingAssetsPath, "Images");
            LoadSpritesFromDirectory(filePath);
        }

        private void LoadSpritesFromDirectory(string path)
        {
            Debug.Log("LoadSpritesFromDirectory: " + path);
            
            // Go to the tree and load all sprites from it
            string[] subDirs = Directory.GetDirectories(path);
            foreach(string subDir in subDirs)
            {
                LoadSpritesFromDirectory(subDir);
            }

            // Load the sprites from the current one
            string[] filesInDirectory = Directory.GetFiles(path);
            foreach (string fileName in filesInDirectory)
            {
                // Try to load the image
                string spriteCategory = new DirectoryInfo(path).Name;
                LoadImage(spriteCategory, fileName);    
            }
        }

        private void LoadImage(string spriteCategory, string filePath)
        {
            Debug.Log($"LoadImage: {filePath}");
            byte[] imageData = File.ReadAllBytes(filePath);

            // TODO dummy fix
            if (filePath.Contains(".xml") || filePath.Contains(".meta"))
            {
                return;
            }

            // Init a dummy texture, the we can pass the byte data to it
            Texture2D imageTexture = new Texture2D(2, 2);
            
            // We could load the texture
            if(imageTexture.LoadImage(imageData))
            {
                string baseSpriteName = Path.GetFileNameWithoutExtension(filePath);
                string basePath = Path.GetDirectoryName(filePath);

                // Attemp to load the xml file
                // To get information on the sprites
                string xmlPath = Path.Combine(basePath, baseSpriteName + ".xml");
                
                if (File.Exists(xmlPath))
                {
                    string xmlData = File.ReadAllText(xmlPath);
                    XmlTextReader reader = new XmlTextReader( new StringReader(xmlData));

                    if(reader.ReadToDescendant("Sprites") && reader.ReadToDescendant("Sprite"))
                    {

                        do
                        {
                            ReadSpriteFromXml(spriteCategory, reader, imageTexture);
                        } while (reader.ReadToNextSibling("Sprite"));
                    }
                    else
                    {
                        Debug.LogError("Could not find a sprites tag");
                        return;
                    }
                }
                else
                {
                    // File was invalid
                    // We just assume, this is a single image
                    LoadSprite(spriteCategory, baseSpriteName, imageTexture, new Rect(0,0,imageTexture.width, imageTexture.height), 32);
                }
            } // If not move on!

        }

        private void ReadSpriteFromXml(string spriteCategory, XmlReader reader, Texture2D imageTexture)
        {
            Debug.Log("ReadSpriteFromXml");
            string spriteName = reader.GetAttribute("name");
            int x = int.Parse(reader.GetAttribute("x"));
            int y =int.Parse(reader.GetAttribute("y"));
            int width = int.Parse(reader.GetAttribute("w"));
            int height = int.Parse(reader.GetAttribute("h"));
            int pixelsPerUnit = int.Parse(reader.GetAttribute("pixelPerUnit"));

            LoadSprite(spriteCategory, spriteName, imageTexture, new Rect(x,y, width, height), pixelsPerUnit);
        }

        private void LoadSprite(string spriteCategory, string spriteName, Texture2D imageTexture, Rect spriteCordinates, int pixelsPerUnit)
        {
            spriteName = spriteCategory + "/" + spriteName;
            Debug.Log("LoadSprite: "+ spriteName);
            // Create a pivot point for the sprite
            Vector2 pivotPoint = new Vector2(0.5f, 0.5f); // center, Ranges from 0-1 / 0.5 == center

            Sprite sprite = Sprite.Create(imageTexture, spriteCordinates, pivotPoint, pixelsPerUnit);

            sprites[spriteName] = sprite;

            Debug.Log($"LoadSprite: {spriteName} was loaded");
        }
    }
}
