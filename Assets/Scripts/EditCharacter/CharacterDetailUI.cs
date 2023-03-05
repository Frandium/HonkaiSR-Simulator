using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDetailUI : MonoBehaviour
{
    List<string> artifactDisnames;
    List<string> artifactDbnames;
    List<string> attributes;
    List<string> valueTypes;
    List<string> weaponDbnames;
    List<string> weaponDisnames;

    public Text chaName;
    public Image background;
    public GameObject attrLine;
    public GameObject attrScroll;
    public GameObject[] detailPages;
    public GameObject[] talentItems;
    public GameObject[] constellaItems;
    public GameObject buffContent;
    public GameObject[] artifactInfos;
    public Text[] WeaponPageTexts;
    public Text artiSuitInfo;

    Button[] talentButtons;
    Slider[] talentLevels;
    Slider chaLevel;
    Toggle chaBreak;
    Dropdown weaponList;
    Slider[] weaponSliders;
    Toggle weaponBreak;
    Image weaponImage;


    Character curCharacter;
    bool enableChange = false;

    Color[] attrLineColor = new Color[] { new Color(0, .75f, 1, .25f), new Color(0, .75f, 1, .5f), new Color(0, .25f, 1, .25f) };

    public readonly static string[] TextElementColors = { "ddd", "f30", "0bf", "50f", "093", "01b", "fb3", "000" };
    // Start is called before the first frame update
    void Awake()
    {
        chaLevel = detailPages[0].GetComponentInChildren<Slider>();
        chaBreak = detailPages[0].GetComponentInChildren<Toggle>();
        chaLevel.onValueChanged.AddListener(cl =>
        {
            int l = (int)cl;
            curCharacter.config.level = l;
            if (l < 20)
                curCharacter.config.breakLevel = 0;
            else
                curCharacter.config.breakLevel = (int)cl / 10 - (l % 10 == 0 ? 2 : 1);
            Refresh();
        });
        chaBreak.onValueChanged.AddListener(cb =>
        {
            curCharacter.config.breakLevel = curCharacter.level / 10 - (cb ? 1 : 2);
            Refresh();
        });


        talentButtons = detailPages[1].GetComponentsInChildren<Button>();
        talentLevels = detailPages[1].GetComponentsInChildren<Slider>();


        weaponImage = detailPages[2].GetComponentInChildren<Image>();
        weaponDbnames = new List<string>(Weapon.GetAllWeapons().Keys);
        weaponDisnames = new List<string>();
        foreach (string dbname in weaponDbnames)
        {
            weaponDisnames.Add("(" + Utils.CareerName[Weapon.GetAllWeaponCareers()[dbname]] + ")" + Weapon.GetAllWeapons()[dbname]);
        }
        weaponList = detailPages[2].GetComponentInChildren<Dropdown>();
        weaponList.ClearOptions();
        weaponList.AddOptions(weaponDisnames);
        weaponList.onValueChanged.AddListener(widx =>
        {
            curCharacter.config.weaponConfig.dbname = weaponDbnames[widx];
            Refresh();
        });
        weaponSliders = detailPages[2].GetComponentsInChildren<Slider>();
        weaponBreak = detailPages[2].GetComponentInChildren<Toggle>();
        weaponSliders[0].onValueChanged.AddListener(wl =>
        {
            int l = (int)wl;
            curCharacter.config.weaponConfig.level = l;
            if (l < 20)
                curCharacter.config.weaponConfig.breakLevel = 0;
            else
                curCharacter.config.weaponConfig.breakLevel = l / 10 - (l % 10 == 0 ? 2 : 1);
            Refresh();
        });
        weaponSliders[1].onValueChanged.AddListener(wr =>
        {
            curCharacter.config.weaponConfig.refine = (int)wr;
            Refresh();
        });
        weaponBreak.onValueChanged.AddListener(wb =>
        {
            if (wb)
                curCharacter.config.weaponConfig.breakLevel = curCharacter.weapon.level / 10 - 1;
            else
                curCharacter.config.weaponConfig.breakLevel = curCharacter.weapon.level / 10 - 2;
            Refresh();
        });


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
                int num2 = j;
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
                    dropdowns[j].onValueChanged.AddListener(v =>
                    {
                        curCharacter.config.artifacts[num].vicePhrases[(num2 - 3) / 2].attr = (CommonAttribute)v;
                    });
                }
            }

            for (int j = 2; j <= 10; j += 2)
            {
                int num2 = j;
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
                    dropdowns[j].onValueChanged.AddListener(v =>
                    {
                        curCharacter.config.artifacts[num].vicePhrases[(num2 - 3) / 2].type = (ValueType)v;
                    });
                }
            }

            for (int j = 0; j < 5; ++j)
            {
                int num2 = j;
                if (j == 0)
                {
                    inputFields[j].onValueChanged.AddListener(s =>
                    {
                        curCharacter.config.artifacts[num].mainPhrase.value = double.Parse(s);
                    });
                }
                else
                {
                    inputFields[j].onValueChanged.AddListener(s =>
                    {
                        curCharacter.config.artifacts[num].vicePhrases[num2 - 1].value = double.Parse(s);
                    });
                }
            }
        }
    }

    public void ShowDetail(Character c, bool _enableChange = false)
    {
        curCharacter = c;
        enableChange = _enableChange;

        background.sprite = Resources.Load<Sprite>(c.dbname + "/splash");
        Text[] texts;

        // Attribute 页面
        chaName.text = c.disname + "  " + "<color=#" + TextElementColors[(int)c.element] + ">" + Utils.ElementName[(int)c.element] + "</color>  " + Utils.CareerName[(int)c.career] +
            "  Lv." + c.level;
        chaLevel.SetValueWithoutNotify(c.level);
        chaLevel.interactable = enableChange;
        if(c.level % 10 == 0 && c.level < 80)
        {
            chaBreak.interactable = enableChange;
            chaBreak.SetIsOnWithoutNotify(c.level / 10 - 1 == c.breakLevel);
        }
        else
        {
            chaBreak.interactable = false;
            chaBreak.SetIsOnWithoutNotify(true);
        }
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
            texts[1].text = b + " + <color=green>" + (f - b) + "</color> = " + (f);
        }

        // Talent 页面
        Image[] imgs;
        foreach(Button b in talentButtons)
        {
            b.interactable = enableChange;
        }
        foreach(Slider s in talentLevels)
        {
            s.interactable = enableChange;
        }

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
            texts[2].text = activated ? "关闭" : "开启";
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
        weaponImage.sprite = Resources.Load<Sprite>("weapons/" + c.weapon.dbName);
        weaponList.SetValueWithoutNotify(weaponDbnames.IndexOf(c.weapon.dbName));
        weaponList.interactable = enableChange;
        WeaponPageTexts[0].text = "Lv.<color=green>" + c.weapon.level + "</color>";
        WeaponPageTexts[1].text = Mathf.RoundToInt(c.weapon.maxHp).ToString();
        WeaponPageTexts[2].text = Mathf.RoundToInt(c.weapon.atk).ToString();
        WeaponPageTexts[3].text = Mathf.RoundToInt(c.weapon.def).ToString();
        WeaponPageTexts[4].text = c.weapon.effectName + "  叠影<color=green>" + c.weapon.refine + "</color>级";
        WeaponPageTexts[5].text = c.weapon.effectDescription;
        weaponSliders[0].SetValueWithoutNotify(c.weapon.level);
        weaponSliders[0].interactable = enableChange;
        if(c.weapon.level % 10 == 0 && c.weapon.level < 80)
        {
            weaponBreak.interactable = enableChange;
            weaponBreak.SetIsOnWithoutNotify(c.weapon.breakLevel == (c.weapon.level / 10) - 1);
        }
        else
        {
            weaponBreak.SetIsOnWithoutNotify(true);
            weaponBreak.interactable = false;
        }
        weaponSliders[1].SetValueWithoutNotify(c.weapon.refine);
        weaponSliders[1].interactable = false;


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
        Dictionary<string, int> suitCount = new Dictionary<string, int>();
        for (int i = 0; i < artifactInfos.Length; ++i)
        {
            if (!suitCount.ContainsKey(c.config.artifacts[i].suitName))
            {
                suitCount[c.config.artifacts[i].suitName] = 0;
            }
            suitCount[c.config.artifacts[i].suitName]++;
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
                    inputFields[j].SetTextWithoutNotify(c.config.artifacts[i].mainPhrase.value.ToString());
                else
                    inputFields[j].SetTextWithoutNotify(c.config.artifacts[i].vicePhrases[j - 1].value.ToString());
            }
        }
        string suittext = "套装效果\n\n";
        foreach(var p in suitCount)
        {
            string name = p.Key;
            ArtifactDescription ad = ArtifactDescription.GetArtifactDescriptionByDbname(name);
            suittext += "<color=#000>" + ad.disname + "</color>\n";
            if (p.Value >= 2)
                suittext += "<color=#000>";
            else
                suittext += "<color=#aaa>";
            suittext += "2件套：" + ad.two + "</color>\n";
            if (p.Value >= 4)
                suittext += "<color=#000>";
            else
                suittext += "<color=#aaa>";
            suittext += "4件套：" + ad.four + "</color>\n";
            suittext += "\n";
        }
        artiSuitInfo.text = suittext;

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

    public void ChangeAbility(int i)
    {
        curCharacter.config.abilityActivated[i] = !curCharacter.config.abilityActivated[i];
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
