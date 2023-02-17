using System.Collections;
using System.Collections.Generic;
using org.flaver.model;
using UnityEngine;

namespace org.flaver.controller {
    public class CharacterSpriteController : MonoBehaviour
    {
        private Dictionary<Character, GameObject> mappedCharacters;
        private Dictionary<string, Sprite> mappedCharacterSprites;
        private World world { get { return WorldController.Instance.World; } }

        private void Start()
        {
            LoadSprites();
                        
            // Init map of which tile belongs to which gameobject
            mappedCharacters = new Dictionary<Character, GameObject>();

            // Register world callbacks
            world.RegisterCharacterCreated(OnCharacterCreated);

            foreach(Character item in world.Characters)
            {
                OnCharacterCreated(item);
            }
        }

        public void OnCharacterCreated(Character character)
        {
            Debug.Log("OnCharacterCreated");
            // Create a visual game object lined to the data
            GameObject characterGameObject = new GameObject();
            characterGameObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            characterGameObject.name = "Character";

            mappedCharacters.Add(character, characterGameObject); // Register mapped character

            characterGameObject.transform.position = new Vector3(character.X, character.Y, 0f);
            characterGameObject.transform.SetParent(transform, true); // true = stay in world pos
            characterGameObject.AddComponent<SpriteRenderer>().sprite = mappedCharacterSprites["player"];
            characterGameObject.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Character");

            // Object infos changes
            character.RegisterOnCharacterChanged(OnCharacterChanged);
        }

        private void OnCharacterChanged(Character character)
        {
            if (!mappedCharacters.ContainsKey(character))
            {
                Debug.LogError("OnCharacterMoved, can not change visuals, chracter not mapped");
                return;
            }

            GameObject characterGameObject = mappedCharacters[character];
            characterGameObject.transform.position = new Vector3(character.X, character.Y, 0);
        }

        private void LoadSprites()
        {
            mappedCharacterSprites = new Dictionary<string, Sprite>();
            Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Characters");

            Debug.Log("Resources loaded");
            foreach (Sprite item in sprites)
            {
                Debug.Log(item.name);
                mappedCharacterSprites[item.name] = item;
            }
        }
    }
}