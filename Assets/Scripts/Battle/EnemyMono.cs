using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using LitJson;

public class EnemyMono : CreatureMono
{
    new Enemy self;

    public Image weakFilling;
    public List<Image> weakpointImage;
    private void Update()
    {
        if (isMyTurn || isSelected)
        {
            alpha += Time.deltaTime * alphaSpeed * alphaDirection;
            if (alpha > 1) alphaDirection = -1;
            else if (alpha < 0) alphaDirection = 1;
            if (isSelected) selectedSR.color = new Color(1, 0, 0, alpha);
            else if (isMyTurn) selectedSR.color = new Color(0, 0, 1, alpha);
        }
    }

    readonly float elementSize = .6f;
    public void Initialize(Enemy e)
    {
        base.Initialize(e);
        self = e;
        int i = 1;
        AudioClip a = Resources.Load<AudioClip>(e.dbname + "/attack" + i);
        while (a != null)
        {
            attackAudios.Add(a);
            i++;
            a = Resources.Load<AudioClip>(e.dbname + "/attack" + i);
        }
        int weakCount = self.weakPoint.Count;
        float left = - elementSize * .6f * (weakCount - 1);
        for (i = 0; i < weakCount; ++i)
        {
            GameObject go = new GameObject("weakpoint" + i);
            RectTransform rect = go.AddComponent<RectTransform>();
            go.transform.SetParent(canvas, false);
            rect.anchorMin = new Vector2(.5f, 1);
            rect.anchorMax = new Vector2(.5f, 1);
            rect.sizeDelta = new Vector2(elementSize, elementSize);
            rect.anchoredPosition = new Vector3(left + elementSize * 1.2f * i, - elementSize / 2.0f, 0);
            go.AddComponent<Image>().sprite = BattleManager.Instance.elementSymbols[(int)self.weakPoint[i]];
        }
        weakFilling.fillAmount = self.weakHp / self.weakMaxHp;
    }

    public override void OnDying()
    {
        BattleManager.Instance.RemoveEnemy(self);
        Destroy(gameObject);
    }

    public override void TakeDamage(float value, Element e)
    {
        weakFilling.fillAmount = self.weakHp / self.weakMaxHp;
        base.TakeDamage(value, e);
    }

    //protected override IEnumerator TakeDamangeAnim(int dmg)
    //{
    //    isAnimFinished = false;
    //    PlayAudio(AudioType.TakeDamage);
    //    dmgGO.SetActive(true);
    //    dmgText.text =  dmg.ToString();
    //    RectTransform rect = dmgGO.GetComponent<RectTransform>();
    //    Image dmgBgImg = dmgGO.GetComponent<Image>();
    //    dmgBgImg.color = new Color(0, 0, 0, dmgBgBaseAlpha);
    //    rect.localPosition = new Vector3(0, 0f, 0);
    //    dmgText.color = Color.white;
    //    float dmgAlpha = 1;
    //    float alphaFadeSpeed = 1 / dmgAnimTime;
    //    float dmgBgSpeed = (6 - 0f) / dmgAnimTime;
    //    while (rect.localPosition.y < 6)
    //    {
    //        rect.localPosition += Vector3.up * dmgBgSpeed * Time.deltaTime;
    //        dmgBgImg.color = new Color(0, 0, 0, dmgBgBaseAlpha * dmgAlpha);
    //        dmgText.color = new Color(1, 1, 1, dmgAlpha);
    //        dmgAlpha -= alphaFadeSpeed * Time.deltaTime;
    //        yield return new WaitForEndOfFrame();
    //    }
    //    dmgGO.SetActive(false);
    //    isAnimFinished = true;
    //}
}
