using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class IndexUI : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
    }

    public void LoadBattleScene()
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync("ChooseBattle");
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
