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
    readonly Vector3 enemyActionPos = new Vector3(179.2f, 1.44f, 92.23f);
    readonly Quaternion enemyActionRot = Quaternion.Euler(new Vector3(0, 180, 0));


    private void Start()
    {
        
    }

    private void Update()
    {
        if (isMyTurn || isSelected)
        {
            alpha += Time.deltaTime * alphaSpeed * alphaDirection;
            if (alpha > 1) alphaDirection = -1;
            else if (alpha < 0) alphaDirection = 1;
            cardSR.material.SetFloat("_alpha", alpha);
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

    public override void TakeDamage(Damage d)
    {
        weakFilling.fillAmount = self.weakHp / self.weakMaxHp;
        base.TakeDamage(d);
    }

    public void MoveToSpot()
    {
        transform.position = enemyActionPos;
        transform.rotation = enemyActionRot;
    }


    public override void UpdateHpLine()
    {
        weakFilling.fillAmount = self.weakHp / self.weakMaxHp;
        base.UpdateHpLine();
    }
}
