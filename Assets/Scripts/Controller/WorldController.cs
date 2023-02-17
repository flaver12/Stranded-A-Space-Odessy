using System.IO;
using System.Xml.Serialization;
using org.flaver.model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace org.flaver.controller
{
    public class WorldController : MonoBehaviour
    {
        public static WorldController Instance { get; private set; }
        public World World { get; private set; }

        private static bool loadWorld = false;

        private void OnEnable()
        {
            Debug.Log("WorldController init");
            if (Instance != null)
            {
                Debug.LogError("Multiple instances of WorldController");
            }
            
            Instance = this;

            if (loadWorld)
            {
                CreateWorldFromSaveFile();
                loadWorld = false;
                return;
            }

            CreateEmptyWorld();

        }

        private void Update()
        {
            // TODO add pause, unpause, speed etc....
            World.Tick(Time.deltaTime);
        }

        public void NewWorld()
        {
            Debug.Log("NewWorld button press");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        }

        public void SaveWorld()
        {
            Debug.Log("SaveWorld button press");

            XmlSerializer serializer = new XmlSerializer(typeof(World));
            TextWriter writer = new StringWriter();
            serializer.Serialize(writer, World);
            writer.Close();
            Debug.Log( writer.ToString() );
            GUIUtility.systemCopyBuffer = writer.ToString();

            PlayerPrefs.SetString("SaveGame00", writer.ToString());
        }

        public void LoadWorld()
        {
            Debug.Log("LoadWorld button press");

            loadWorld = true;
            // Reload the scene to purge all references
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public Tile GetTileAtWorldCord(Vector3 position)
        {
            int x = Mathf.FloorToInt(position.x + 0.5f);
            int y = Mathf.FloorToInt(position.y + 0.5f);

            return World.GetTileByPosition(x, y);
        }

        private void CreateEmptyWorld()
        {
            World = new World(100, 100);

            // center the camera
            Camera.main.transform.position = new Vector3( World.Width / 2, World.Height / 2,  Camera.main.transform.position.z);
        }

        private void CreateWorldFromSaveFile()
        {
            Debug.Log("CreateWorldFromSaveFile");
            // Create the world from the save file data
            XmlSerializer serializer = new XmlSerializer(typeof(World));
            TextReader reader = new StringReader(PlayerPrefs.GetString("SaveGame00"));
            Debug.Log( reader.ToString() ); 
            GUIUtility.systemCopyBuffer = reader.ToString();
            World = (World)serializer.Deserialize(reader);
            reader.Close();

            // center the camera
            Camera.main.transform.position = new Vector3( World.Width / 2, World.Height / 2,  Camera.main.transform.position.z);
        }
    }
}