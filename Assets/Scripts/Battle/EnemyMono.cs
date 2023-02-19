using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using LitJson;

public class EnemyMono : CreatureMono
{
    new Enemy self;
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
    }

    public override void OnDying()
    {
        BattleManager.Instance.RemoveEnemy(self);
        Destroy(gameObject);
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
