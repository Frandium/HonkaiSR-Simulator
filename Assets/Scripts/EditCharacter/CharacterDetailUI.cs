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

    public Text chaName;
    public Image background;
    public GameObject attrLine;
    public GameObject attrScroll;
    public GameObject[] detailPages;
    public GameObject[] talentItems;
    public GameObject[] constellaItems;
    public GameObject buffContent;

    Color[] attrLineColor = new Color[] { new Color(0, .75f, 1, .25f), new Color(0, .75f, 1, .5f), new Color(0, .25f, 1, .25f) };
    public void ShowDetail(Character c)
    {
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
        texts[0].text = c.atkName + " Lv.<color=#0f8>" + c.atkLevel + "</color>";
        texts[1].text = c.atkDescription;

        imgs = talentItems[1].GetComponentsInChildren<Image>();
        imgs[1].sprite = Resources.Load<Sprite>(c.dbname + "/skill");
        texts = talentItems[1].GetComponentsInChildren<Text>();
        texts[0].text = c.skillName + " Lv.<color=#0f8>" + c.skillLevel + "</color>";
        texts[1].text = c.skillDescription;

        imgs = talentItems[2].GetComponentsInChildren<Image>();
        imgs[1].sprite = Resources.Load<Sprite>(c.dbname + "/burst");
        texts = talentItems[2].GetComponentsInChildren<Text>();
        texts[0].text = c.burstName + " Lv.<color=#0f8>" + c.burstLevel + "</color>";
        texts[1].text = c.burstDescription;

        imgs = talentItems[3].GetComponentsInChildren<Image>();
        imgs[1].sprite = Resources.Load<Sprite>(c.dbname + "/talent");
        texts = talentItems[3].GetComponentsInChildren<Text>();
        texts[0].text = c.talentName + " Lv.<color=#0f8>" + c.talentLevel + "</color>";
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
            bool activated = c.constellaLevel >= i + 1;
            texts[0].text = (string)c.metaData["constellation"][i]["name"] + (activated? "" : "  未激活");
            texts[0].color = activated ? Color.black : Color.grey;
            texts[1].text = (string)c.metaData["constellation"][i]["content"];
            texts[1].color = activated ? Color.black : Color.grey;
        }

        // Artifacts 界面

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
}
