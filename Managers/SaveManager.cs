using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using SaveData;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Managers
{
    public class SaveManager : MonoBehaviour
    {
        static string Path => $"{Application.persistentDataPath}/save.fs";
        static SaveManager _instance = null;
        static Dictionary<string, ISaveData> _saveElements;
        
        void Awake()
        {
            if (_instance)
                Destroy(gameObject);
            else
                _instance = this;

            //Path = $"{Application.persistentDataPath}/save.fs";

            _saveElements = new Dictionary<string, ISaveData>();
            IEnumerable<ISaveData> elements = Resources.FindObjectsOfTypeAll<Object>().OfType<ISaveData>();

            foreach (ISaveData saveData in elements)
            {
                _saveElements.Add(saveData.Id, saveData);
            }
            
            DontDestroyOnLoad(gameObject);
        }

        public static void EnableSaving()
        {
            DisableSaving();
            Application.quitting += Save;
        }

        static void DisableSaving()
        {
            Application.quitting -= Save;
        }

        [ContextMenu("Save")]
        public void _Save() => Save();

        [ContextMenu("Load")]
        public void _Load() => Load();
        
        static void Save()
        {
            Dictionary<string, object> datas = new Dictionary<string, object>();
            
            foreach (ISaveData data in _saveElements.Values)
            {
                try
                {
                    datas.Add(data.Id, data.Save());
                }
                catch (Exception e)
                {
                    print($"ERROR: {e.Message.ToUpper()}");
                    //throw;
                }
            }

            SaveFile(datas);
        }

        static void SaveFile(Dictionary<string, object> datas)
        {
            using FileStream file = File.Open(Path, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(file, datas);
        }

        public static void Load()
        {
            Dictionary<string, object> datas = LoadFile();

            foreach (ISaveData data in _saveElements.Values)
            {
                data.Load(datas[data.Id]);
            }
        }

        static Dictionary<string, object> LoadFile()
        {
            if (!File.Exists(Path))
                Debug.LogError("Tried to load an non existing save file");

            using FileStream file = new FileStream(Path, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            return (Dictionary<string, object>) formatter.Deserialize(file);
        }

        public static void AddSaveElement(string id, ISaveData data)
        {
            if (!_saveElements.TryGetValue(id, out _))
                _saveElements.Add(id, data);
        }

        public static bool HasFile()
        {
            return File.Exists(Path);
        }
    }
}
