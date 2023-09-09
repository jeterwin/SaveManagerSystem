using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

public class SaveManagerTest : MonoBehaviour
{
    public static SaveManagerTest Instance;

    public Transform SavesGrid;

    public GameObject SaveSlotPrefab;

    public GameObject UIPrompt;

    public int Health, SaveSlot, Checkpoint;
    
    public int MaxSavesAllowed;

    public int SaveToBeDeletedID;

    private string SaveFolderPath = "Saves/";

    [HideInInspector]
    public SaveData CurrentSave;

    [System.Serializable]
    public class SaveData
    {
        // Add other game data as needed
        public Dictionary<string, object> Data = new()
        {
            { "Playtime", 0 },
            { "PlayerHealth", 100 },
            { "SavedDate", DateTime.UtcNow.ToString() }
        };
        int SaveID;
    }
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        //This for unlimited saves
        //for(int SaveSlot = 1; SaveSlot <= Directory.GetDirectories(saveFolderPath).Length; SaveSlot++)
        for (int SaveSlot = 1; SaveSlot <= MaxSavesAllowed; SaveSlot++)
        {
            string SaveSlotFolder = Path.Combine(SaveFolderPath, "SaveSlot" + SaveSlot.ToString());
            if (Directory.Exists(SaveSlotFolder))
            {
                //Check if some player prefs are empty, if they are, pick the save with
                //the most progress (or maybe the latest save)

                // Check each checkpoint folder within the save slot
                string[] CheckpointFolders = Directory.GetDirectories(SaveSlotFolder);
                foreach (string CheckpointFolder in CheckpointFolders)
                {
                    GetGameData(CheckpointFolder);
                }
                PopulateSaveSlots(CheckpointFolders, SaveSlot);
            }
        }
    }

    protected void PopulateSaveSlots(string[] NumberOfCheckpoints, int SaveSlot)
    {
        //Assign all the info here, progress, chapter image, etc, etc.
        //Item holder is the script that holds all the texts that will be changed
        //and is placed on the save slot itself.
        GameObject GameObject = Instantiate(SaveSlotPrefab, SavesGrid);
        ItemHolder SaveItemHolder = GameObject.GetComponent<ItemHolder>();
        SaveItemHolder.SaveNumberText.text = "Save " + SaveSlot.ToString();
        object value = 0;
        GetGameData(NumberOfCheckpoints[0]).Data.TryGetValue("Playtime", out value);
        SaveItemHolder.SavePlaytimeText.text = value == null ? "00:00:00" : "00:00:00";
        //SaveItemHolder.SavePlaytimeText.text = GetGameData(NumberOfCheckpoints[0]).Data.TryGetValue("Playtime", out value) == 0 ? 
            //"00:00:00" : FormatTime(GetGameData(NumberOfCheckpoints[0]).Playtime);
        //SaveItemHolder.SaveDateTimeText.text = GetGameData(NumberOfCheckpoints[0]).SavedDate;
        SaveItemHolder.SaveDateTimeText.text = DateTime.UtcNow.ToString();
        SaveItemHolder.SaveNumber = SaveSlot;



        //If wish to populate checkpoints
        //PopulateCheckpointsPerSaveSlot(NumberOfCheckpoints, SaveSlot);
    }

    protected void PopulateCheckpointsPerSaveSlot(string[] NumberOfCheckpoints, int SaveSlot)
    {
        foreach (string Checkpoint in NumberOfCheckpoints)
        {
            GameObject GameObject = Instantiate(SaveSlotPrefab, SavesGrid);
            ItemHolder SaveItemHolder = GameObject.GetComponent<ItemHolder>();
            SaveItemHolder.SaveNumberText.text = "Checkpoint " + SaveSlot.ToString();
        }
    }

    public void NewGame()
    {
        string[] NewSaveSlot = Directory.GetDirectories(SaveFolderPath);
        if(NewSaveSlot.Length <= 29)
        {
            Directory.CreateDirectory(Path.Combine(SaveFolderPath, "SaveSlot" + (NewSaveSlot.Length + 1)));
            //Set SaveSlot and Checkpoint to PlayerPrefs so everything saves on that
        }
        //else Write too many saves
    }

    // Save the game data to a specific slot and checkpoint
    public void SaveGame(int SaveSlot, int Checkpoint)
    {
        //Save SaveSlot and Checkpoint to playerprefs, then use to save on current slot
        // Create the save folder if it doesn't exist
        string SaveSlotFolder = Path.Combine(SaveFolderPath, "SaveSlot" + SaveSlot.ToString());
        Directory.CreateDirectory(SaveSlotFolder);

        // Create the checkpoint folder if it doesn't exist, if it does, overwrite the file
        string CheckpointFolder = Path.Combine(SaveSlotFolder, "Checkpoint" + Checkpoint.ToString());
        Directory.CreateDirectory(CheckpointFolder);

        // Create a SaveData object with your game's data
        SaveData SaveData = new SaveData();
        SaveData.Data["PlayerHealth"] = Health;
        Health += 10;
        //saveData.playerPosition = /* Get player position */;
        //saveData.playerHealth = /* Get player health */;
        //saveData.inventory = /* Get player inventory */;

        // Serialize and save the data to a JSON file
        string SaveFilePath = Path.Combine(CheckpointFolder, "saveData.json");
        string JsonData = JsonConvert.SerializeObject(SaveData);
        File.WriteAllText(SaveFilePath, JsonData);
    }

    // Load the game data from a specific slot and checkpoint
    public void LoadGame(int SaveSlot, int Checkpoint)
    {
        string SaveSlotFolder = Path.Combine(SaveFolderPath, "SaveSlot" + SaveSlot.ToString());
        string CheckpointFolder = Path.Combine(SaveSlotFolder, "Checkpoint" + Checkpoint.ToString());
        string SaveFilePath = Path.Combine(CheckpointFolder, "saveData.json");

        if (File.Exists(SaveFilePath))
        {
            // Read the JSON data and deserialize it into a SaveData object
            string jsonData = File.ReadAllText(SaveFilePath);
            SaveData saveData = JsonConvert.DeserializeObject<SaveData>(jsonData);

            // Use the loaded data to restore the game state
            /* Set player position, health, inventory, etc. */
        }
        else
        {
            Debug.LogError("Save file not found: " + SaveFilePath);
        }
    }

    public void DeleteSaveSlot()
    {
        //Make prompt YoU SUrE YoU WaNt To DeLEtE saVE?
        Debug.Log(Path.Combine(SaveFolderPath, "SaveSlot" + SaveToBeDeletedID));
        UIPrompt.SetActive(false);
        //Directory.Delete(Path.Combine(SaveFolderPath, "SaveSlot" + SaveToBeDeletedID), true);
    }
    public void ClosePrompt()
    {
        UIPrompt.SetActive(false);
    }
    protected SaveData GetGameData(string CheckpointFolder)
    {
        // Load the data from a specific checkpoint
        string SaveFilePath = Path.Combine(CheckpointFolder, "saveData.json");

        if (File.Exists(SaveFilePath))
        {
            string JsonData = File.ReadAllText(SaveFilePath);
            return JsonConvert.DeserializeObject<SaveData>(JsonData);
            // Use the loaded data to restore the game state
            /* Set player position, health, inventory, etc. */
        }
        else
        {
            return null;
        }
    }
    protected int GetNumberOfCheckpoints(string SaveSlotFolder)
    {
        string MetadataFilePath = Path.Combine(SaveSlotFolder, "metadata.json");
        if (File.Exists(MetadataFilePath))
        {
            string MetadataJson = File.ReadAllText(MetadataFilePath);
            var Metadata = JsonConvert.DeserializeObject<Dictionary<string, int>>(MetadataJson);

            if (Metadata.ContainsKey("NumberOfCheckpoints"))
            {
                return Metadata["NumberOfCheckpoints"];
            }
        }

        return 0;
    }    
    protected string FormatTime(float TimeInSeconds)
    {
        int Hours = Mathf.FloorToInt(TimeInSeconds / 3600);
        int Minutes = Mathf.FloorToInt((TimeInSeconds % 3600) / 60);
        int Seconds = Mathf.FloorToInt(TimeInSeconds % 60);

        return string.Format("{0:D2}:{1:D2}:{2:D2}", Hours, Minutes, Seconds);
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            SaveGame(SaveSlot, Checkpoint);
            //CheckpointManager.instance.SaveAtCheckpoint(lastCheckpoint);
            //lastCheckpoint++;
        }
        if(Input.GetKeyDown(KeyCode.L))
        {
            //LoadCheckpointData(SaveIndex, lastCheckpoint);
        }
    }
}
