using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Callbacks;
#endif

namespace Managers
{
    [CreateAssetMenu(menuName = "Scene Management/" + nameof(SceneLoader))]
    public class SceneLoader : ScriptableObject
    {
        [SerializeField, HideInInspector] string[] sceneNames;
        [SerializeField, HideInInspector] string activeSceneName;
        List<AsyncOperation> _asyncOperations;
        
        public float Progress { get; private set; }
        
        public IEnumerator LoadLevel()
        {
            LoadSceneMode mode = LoadSceneMode.Additive;
            _asyncOperations = new List<AsyncOperation>();

            Progress = 0;
            
            foreach (string scene in sceneNames)
            {
                AsyncOperation async = SceneManager.LoadSceneAsync(scene, mode);

                if(scene.Equals(activeSceneName))
                    async.completed += OnActiveSceneLoaded;
                
                //mode = LoadSceneMode.Additive;

                _asyncOperations.Add(async);
            }

            float totalProgress = 0;
            float progressScene = 1f / sceneNames.Length;
            
            foreach (AsyncOperation operation in _asyncOperations)
            {
                operation.allowSceneActivation = true;
                
                while (!operation.isDone)
                {
                    Progress = totalProgress + operation.progress / sceneNames.Length;
                    
                    yield return null;
                }

                totalProgress += progressScene;

                //operation.allowSceneActivation = true;
            }

            Progress = 1;

            // Only activate when all are finished
            // foreach (AsyncOperation operation in asyncOperations)
            // {
            //     operation.allowSceneActivation = true;
            //     Debug.Log("Activate scene");
            // }
            
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(activeSceneName));
        }

        void OnActiveSceneLoaded(AsyncOperation obj)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(activeSceneName));
        }

#if UNITY_EDITOR
        
        public SceneAsset[] level;
        public SceneAsset activeScene;
        public SceneAsset globalScene;

        [OnOpenAssetAttribute]
        public static bool LoadLevelEditor(int instanceID, int line)
        {
            SceneLoader sceneLoader = EditorUtility.InstanceIDToObject(instanceID) as SceneLoader;

            if (sceneLoader == null)
                return false;
            
            foreach (SceneSetup sceneSetup in EditorSceneManager.GetSceneManagerSetup())
            {
                Scene scene = SceneManager.GetSceneByPath(sceneSetup.path);
                if (!scene.isDirty) 
                    continue;

                switch (EditorUtility.DisplayDialogComplex("Scene modified",
                    $"{scene.name} was modified. Do you wish to save it?", "Yes", "Cancel", "Don't Save"))
                {
                    case 0:
                        EditorSceneManager.SaveScene(scene);
                        break;
                    case 1:
                        return true;
                    case 2:
                        break;
                }
            }

            string globalScenePath = AssetDatabase.GetAssetPath(sceneLoader.globalScene); 
            EditorSceneManager.OpenScene(globalScenePath, OpenSceneMode.Single);
            
            foreach (SceneAsset scene in sceneLoader.level)
            {
                string path = AssetDatabase.GetAssetPath(scene); 
                EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneLoader.activeScene.name));
            
            return true;
        }

        void OnValidate()
        {
            sceneNames = new string[level.Length];
            
            for (int index = 0; index < level.Length; index++)
            {
                sceneNames[index] = level[index].name;
            }

            activeSceneName = activeScene.name;
            
            if (!level.Contains(activeScene))
                Debug.LogError("Active scene must be in the level scenes");
        }

#endif
        
    }
}
