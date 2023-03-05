using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        cover.gameObject.SetActive(true);
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
        StartCoroutine(LoadSceneAnim(ao));
    }

    public Image cover;
    public Image loading;

    IEnumerator LoadSceneAnim(AsyncOperation ao)
    {
        cover.gameObject.SetActive(true);
        loading.fillAmount = 0;
        while (!ao.isDone)
        {
            loading.fillAmount = ao.progress;
            yield return new WaitForEndOfFrame();
        }
    }
}
