using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DevLevelSelector : MonoBehaviour
{
   public GameObject buttonPrefab; // Assign the button prefab in the Unity Editor
   public Transform content; // Assign the Content child object of the Scroll View in the Unity Editor

   void Start()
   {
      for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
      {
         string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
         string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

         if (IsSceneEnabledInBuildSettings(sceneName))
         {
            GameObject button = Instantiate(buttonPrefab, content);
            button.GetComponentInChildren<Text>().text = sceneName;
            button.GetComponent<Button>().onClick.AddListener(() => LoadLevel(sceneName));
         }
      }
   }

   void LoadLevel(string sceneName)
   {
      SceneManager.LoadScene(sceneName);
   }

   bool IsSceneEnabledInBuildSettings(string sceneName)
   {
      for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
      {
         string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
         string buildSceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

         if (sceneName == buildSceneName)
         {
            return true;
         }
      }
      return false;
   }
}
