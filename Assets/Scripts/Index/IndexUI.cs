using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class IndexUI : MonoBehaviour
{
    public Dropdown battleDropDown;
    
    // Start is called before the first frame update
    void Start()
    {
        RefreshDropdown();
    }

    List<string> files;
    public void RefreshDropdown()
    {
        files = new List<string>(Directory.GetFiles(GlobalInfoHolder.Instance.battleDir));
        files.RemoveAll(s => !Path.GetExtension(s).Equals(".json"));
        if (files.Count == 0)
            return;
        battleDropDown.ClearOptions();
        List<Dropdown.OptionData> data = new List<Dropdown.OptionData>();
        foreach (string s in files)
        {
            data.Add(new Dropdown.OptionData(Path.GetFileNameWithoutExtension(s)));
        }
        battleDropDown.AddOptions(data);
        battleDropDown.SetValueWithoutNotify(0);
        OptionChange(0);
    }

    public void OptionChange(int i)
    {
        GlobalInfoHolder.Instance.battleFilePath = files[i];
    }

    public void LoadBattleScene()
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync("Battle");
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
