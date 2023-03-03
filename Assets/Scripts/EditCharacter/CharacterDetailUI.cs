using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDetailUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    List<string> artifactDisnames;
    List<string> artifactDbnames;
    List<string> attributes;
    List<string> valueTypes;

    public Text chaName;
    public Image background;
    public GameObject attrLine;
    public GameObject attrScroll;
    public GameObject[] detailPages;
    public GameObject[] talentItems;
    public GameObject[] constellaItems;
    public GameObject buffContent;
    public GameObject[] artifactInfos;

    Character curCharacter;
    bool enableChange = false;

    Color[] attrLineColor = new Color[] { new Color(0, .75f, 1, .25f), new Color(0, .75f, 1, .5f), new Color(0, .25f, 1, .25f) };
    public void ShowDetail(Character c, bool _enableChange = false)
    {
        curCharacter = c;
        enableChange = _enableChange;

        background.sprite = Resources.Load<Sprite>(c.dbname + "/splash");
        Text[] texts;
        // Attribute 页面
        chaName.text = c.disname + "  Lv." + c.level + "  突破" + c.breakLevel + "  " + Utils.ElementName[(int)c.element]
            + "  " + Utils.CareerName[(int)c.career];
        attrScroll.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 30 * (int)CommonAttribute.Count);

        for (int i = 0; i < attrScroll.transform.childCount; i++)
        {
            Destroy(attrScroll.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < (int)CommonAttribute.Count; ++i)
        {
            string attrName = Utils.attributeNames[i];
            GameObject go = Instantiate(attrLine, attrScroll.transform);
            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -30 * i - 15);
            go.GetComponent<Image>().color = attrLineColor[i % 3];
            go.name = attrName;
            texts = go.GetComponentsInChildren<Text>();
            texts[0].text = attrName;
            float b = c.GetBaseAttr((CommonAttribute)i);
            float f = c.GetFinalAttr((CommonAttribute)i);
            texts[1].text = b + " + <color=green>" + (f - b) + "</color> = " + f;
        }

        // Talent 页面
        Image[] imgs;

        imgs = talentItems[0].GetComponentsInChildren<Image>();
        imgs[1].sprite = Resources.Load<Sprite>(c.dbname + "/attack");
        texts = talentItems[0].GetComponentsInChildren<Text>();
        Slider slider = talentItems[0].GetComponentInChildren<Slider>();
        slider.SetValueWithoutNotify(c.config.atkLevel);
        string ncolor = " Lv.<color=#000>";
        string tcolor = " Lv.<color=#0f8>";
        texts[0].text = c.atkName + (c.atkLevel > c.config.atkLevel ? tcolor : ncolor) + c.atkLevel + "</color>";
        texts[1].text = c.atkDescription;

        imgs = talentItems[1].GetComponentsInChildren<Image>();
        imgs[1].sprite = Resources.Load<Sprite>(c.dbname + "/skill");
        texts = talentItems[1].GetComponentsInChildren<Text>();
        slider = talentItems[1].GetComponentInChildren<Slider>();
        slider.SetValueWithoutNotify(c.config.skillLevel);
        texts[0].text = c.skillName + (c.skillLevel > c.config.skillLevel ? tcolor : ncolor) + c.skillLevel + "</color>";
        texts[1].text = c.skillDescription;

        imgs = talentItems[2].GetComponentsInChildren<Image>();
        imgs[1].sprite = Resources.Load<Sprite>(c.dbname + "/burst");
        texts = talentItems[2].GetComponentsInChildren<Text>();
        slider = talentItems[2].GetComponentInChildren<Slider>();
        slider.SetValueWithoutNotify(c.config.burstLevel);
        texts[0].text = c.burstName + (c.burstLevel > c.config.burstLevel ? tcolor : ncolor) + c.burstLevel + "</color>";
        texts[1].text = c.burstDescription;

        imgs = talentItems[3].GetComponentsInChildren<Image>();
        imgs[1].sprite = Resources.Load<Sprite>(c.dbname + "/talent");
        texts = talentItems[3].GetComponentsInChildren<Text>();
        slider = talentItems[3].GetComponentInChildren<Slider>();
        slider.SetValueWithoutNotify(c.config.talentLevel);
        texts[0].text = c.talentName + (c.talentLevel > c.config.talentLevel ? tcolor : ncolor) + c.talentLevel + "</color>";
        texts[1].text = c.talentDescription;

        imgs = talentItems[4].GetComponentsInChildren<Image>();
        imgs[1].sprite = Resources.Load<Sprite>(c.dbname + "/mystery");
        texts = talentItems[4].GetComponentsInChildren<Text>();
        texts[0].text = c.mysteryName;
        texts[1].text = c.mysteryDescription;

        for (int i = 0; i < 3; ++i)
        {
            imgs = talentItems[5 + i].GetComponentsInChildren<Image>();
            imgs[1].sprite = Resources.Load<Sprite>(c.dbname + "/ability" + i);
            texts = talentItems[5 + i].GetComponentsInChildren<Text>();
            bool activated = c.config.abilityActivated[i];
            texts[0].text = (string)c.metaData["ability"][i]["name"] + (activated ? "" : " 未激活");
            texts[1].text = (string)c.metaData["ability"][i]["description"];
            if (!activated)
            {
                texts[0].color = Color.grey;
                texts[1].color = Color.grey;
            }
            else
            {
                texts[0].color = Color.black;
                texts[1].color = Color.black;
            }
        }

        // Weapon 页面
        Image weaponImage = detailPages[2].GetComponentInChildren<Image>();
        weaponImage.sprite = Resources.Load<Sprite>("weapons/" + c.weapon.dbName);
        texts = detailPages[2].GetComponentsInChildren<Text>();

        texts[0].text = c.weapon.disName + "  " + Utils.CareerName[(int)c.weapon.career];
        texts[1].text = "Lv.<color=green>" + c.weapon.level + "</color>";
        texts[3].text = Mathf.RoundToInt(c.weapon.maxHp).ToString();
        texts[5].text = Mathf.RoundToInt(c.weapon.atk).ToString();
        texts[7].text = Mathf.RoundToInt(c.weapon.def).ToString();
        texts[8].text = c.weapon.effectName + "  叠影<color=green>" + c.weapon.refine + "</color>级";
        texts[9].text = c.weapon.effectDescription;

        // Constellation 页面
        for(int i = 0; i < 6; ++i)
        {
            constellaItems[i].GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>(c.dbname + "/c" + i);
            texts = constellaItems[i].GetComponentsInChildren<Text>();
            constellaItems[i].GetComponentInChildren<Button>().interactable = _enableChange;
            bool activated = c.constellaLevel >= i + 1;
            texts[0].text = (string)c.metaData["constellation"][i]["name"] + (activated? "" : "  未激活");
            texts[0].color = activated ? Color.black : Color.grey;
            texts[1].text = (string)c.metaData["constellation"][i]["content"];
            texts[1].color = activated ? Color.black : Color.grey;
            if(i < curCharacter.constellaLevel)
                texts[2].text = "关闭";
            else
                texts[2].text = "开启";
        }

        // Artifacts 界面
        if(artifactDisnames == null)
        {
            artifactDisnames = new List<string>();
            foreach (var v in ArtifactDescription.GetAllArtifactSuits().Values)
            {
                artifactDisnames.Add(v.disname);
            }
            artifactDbnames = new List<string>();
            foreach (var v in ArtifactDescription.GetAllArtifactSuits().Values)
            {
                artifactDbnames.Add(v.dbname);
            }
            attributes = new List<string>(Utils.attributeNames);
            attributes.RemoveAt(Utils.attributeNames.Length - 1);
            valueTypes = new List<string>(Utils.valueTypeName);
            valueTypes.RemoveAt(Utils.valueTypeName.Length - 1);


            for (int i = 0; i < artifactInfos.Length; ++i)
            {
                GameObject art = artifactInfos[i];
                Dropdown[] dropdowns = art.GetComponentsInChildren<Dropdown>();
                InputField[] inputFields = art.GetComponentsInChildren<InputField>();
                dropdowns[0].ClearOptions();
                dropdowns[0].AddOptions(artifactDisnames);
                int num = i;
                dropdowns[0].onValueChanged.AddListener(v => {
                    Debug.Log("i = " + num);
                    Debug.Log("v = " + v);
                    curCharacter.config.artifacts[num].suitName = artifactDbnames[v];
                });

                for (int j = 1; j <= 10; j += 2)
                {
                    dropdowns[j].ClearOptions();
                    dropdowns[j].AddOptions(attributes);
                    if (j == 1)
                    {
                        dropdowns[j].onValueChanged.AddListener(v =>
                        {
                            curCharacter.config.artifacts[num].mainPhrase.attr = (CommonAttribute)v;
                        });
                    }
                    else
                    {
                        int num2 = j;
                        dropdowns[j].onValueChanged.AddListener(v =>
                        {
                            curCharacter.config.artifacts[num].vicePhrases[(num2 - 3) / 2].attr = (CommonAttribute)v;
                        });
                    }
                }

                for (int j = 2; j <= 10; j += 2)
                {
                    dropdowns[j].ClearOptions();
                    dropdowns[j].AddOptions(valueTypes);
                    if (j == 2)
                    {
                        dropdowns[j].onValueChanged.AddListener(v =>
                        {
                            curCharacter.config.artifacts[num].mainPhrase.type = (ValueType)v;
                        });
                    }
                    else
                    {
                        int num2 = j;
                        dropdowns[j].onValueChanged.AddListener(v =>
                        {
                            curCharacter.config.artifacts[num].vicePhrases[(num2 - 3) / 2].attr = (CommonAttribute)v;
                        });
                    }
                }

                for (int j = 0; j < 5; ++j)
                {
                    if (j == 0)
                    {
                        inputFields[j].onValueChanged.AddListener(s =>
                        {
                            curCharacter.config.artifacts[num].mainPhrase.value = double.Parse(s);
                        });
                    }
                    else
                    {
                        int num2 = j;
                        inputFields[j].onValueChanged.AddListener(s =>
                        {
                            curCharacter.config.artifacts[num].vicePhrases[num2 - 1].value = double.Parse(s);
                        });
                    }
                }
            }
        }

        for (int i = 0; i < artifactInfos.Length; ++i)
        {
            GameObject art = artifactInfos[i];
            Dropdown[] dropdowns = art.GetComponentsInChildren<Dropdown>();
            InputField[] inputFields = art.GetComponentsInChildren<InputField>();
            dropdowns[0].SetValueWithoutNotify(artifactDbnames.IndexOf(c.config.artifacts[i].suitName));
            dropdowns[0].interactable = _enableChange;

            for (int j = 1; j <= 10; j += 2)
            {
                dropdowns[j].interactable = _enableChange;
                if (j == 1)
                
                    dropdowns[j].SetValueWithoutNotify((int)c.config.artifacts[i].mainPhrase.attr);
                
                else
                
                    dropdowns[j].SetValueWithoutNotify((int)c.config.artifacts[i].vicePhrases[(j - 3) / 2].attr);
                
            }

            for (int j = 2; j <= 10; j += 2)
            {
                dropdowns[j].interactable = _enableChange;
                if (j == 2)
                
                    dropdowns[j].SetValueWithoutNotify((int)c.config.artifacts[i].mainPhrase.type);
                
                else
                
                    dropdowns[j].SetValueWithoutNotify((int)c.config.artifacts[i].vicePhrases[(j - 4) / 2].type);
                
            }

            for (int j = 0; j < 5; ++j)
            {
                inputFields[j].interactable = _enableChange;
                if (j == 0)
                {
                    inputFields[j].SetTextWithoutNotify(c.config.artifacts[i].mainPhrase.value.ToString());
                }
                else
                {
                    inputFields[j].SetTextWithoutNotify(c.config.artifacts[i].vicePhrases[j - 1].value.ToString());
                }
            }
        }


        // Buff 界面
        buffContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 30 * (int)CommonAttribute.Count);

        for (int i = 0; i < buffContent.transform.childCount; i++)
        {
            Destroy(buffContent.transform.GetChild(i).gameObject);
        }
        for(int i = 0; i < c.buffs.Count; ++i)
        {
            Buff b = c.buffs[i];
            string attrName = b.tag;
            GameObject go = Instantiate(attrLine, buffContent.transform);
            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -30 * i - 15);
            go.GetComponent<Image>().color = attrLineColor[i % 2];
            go.name = attrName;
            texts = go.GetComponentsInChildren<Text>();
            texts[0].text = attrName + " " + b.buffType + " " + Utils.attributeNames[(int)b.targetAttribute];
            if (b.buffType == BuffType.Permanent)
                texts[1].text = "永久";
            else
                texts[1].text = "剩余 <color=#80f>" +  b.times + "</color> 回合";
        }
    }

    public void ChangePage(int i)
    {
        for(int j = 0; j < detailPages.Length; j++)
        {
            if(j == i)
            {
                detailPages[j].SetActive(true);
            }
            else
            {
                detailPages[j].SetActive(false);
            }
        }
    }

    public void ChangeConstellation(int n)
    {
        if(curCharacter.config.constellaLevel >= n)
        {
            curCharacter.config.constellaLevel = n - 1;
        }
        else
        {
            curCharacter.config.constellaLevel = n;
        }
        Refresh();
    }

    public void Refresh()
    {
        curCharacter.SaveConfig();
        ShowDetail(curCharacter, enableChange);
    }

    public void ChangeTalentLevel(Slider slider)
    {
        if (curCharacter == null)
            return;
        curCharacter.config.talentLevel = (int)slider.value;
        Refresh();
    }
    public void ChangeNormalAttackLevel(Slider slider)
    {
        if (curCharacter == null)
            return;
        curCharacter.config.atkLevel = (int)slider.value;
        Refresh();
    }
    public void ChangeSkillLevel(Slider slider)
    {
        if (curCharacter == null)
            return;
        curCharacter.config.skillLevel = (int)slider.value;
        Refresh();
    }
    public void ChangeBurstLevel(Slider slider)
    {
        if (curCharacter == null)
            return;
        curCharacter.config.burstLevel = (int)slider.value;
        Refresh();
    }
}
