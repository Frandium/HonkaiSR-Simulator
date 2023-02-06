using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Creature
{
    public IEnemyAction enemyAction;

    private void Update()
    {
        if (isMyTurn || isSelected)
        {
            alpha += Time.deltaTime * alphaSpeed * alphaDirection;
            if (alpha > 1) alphaDirection = -1;
            else if (alpha < 0) alphaDirection = 1;
            if (isSelected) selected.color = new Color(1, 0, 0, alpha);
            else if (isMyTurn) selected.color = new Color(0, 0, 1, alpha);
        }
    }

    public override void Initialize(string dbN, int id)
    {
        base.Initialize(dbN, id);
        switch (databaseName)
        {
            case "hilichurl":
                uniqueID = id;
                def = 1305;
                atk = 6628;
                speed = 33;
                maxHp = 25664;
                enemyAction = new Hilichurl(this);
                break;
            default:
                break;
        }
        hp = maxHp;
    }


    protected override IEnumerator TakeDamangeAnim(int dmg, Then nextToDo)
    {
        isAnimFinished = false;
        PlayAudio(AudioType.TakeDamage);
        dmgGO.SetActive(true);
        dmgText.text =  dmg.ToString();
        RectTransform rect = dmgGO.GetComponent<RectTransform>();
        Image dmgBgImg = dmgGO.GetComponent<Image>();
        dmgBgImg.color = new Color(0, 0, 0, dmgBgBaseAlpha);
        rect.localPosition = new Vector3(0, 0f, 0);
        dmgText.color = Color.white;
        float dmgAlpha = 1;
        float alphaFadeSpeed = 1 / dmgAnimTime;
        float dmgBgSpeed = (6 - 0f) / dmgAnimTime;
        while (rect.localPosition.y < 6)
        {
            rect.localPosition += Vector3.up * dmgBgSpeed * Time.deltaTime;
            dmgBgImg.color = new Color(0, 0, 0, dmgBgBaseAlpha * dmgAlpha);
            dmgText.color = new Color(1, 1, 1, dmgAlpha);
            dmgAlpha -= alphaFadeSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        dmgGO.SetActive(false);
        isAnimFinished = true;
        nextToDo?.Invoke();
    }

    protected override void OnDying()
    {
        BattleManager.Instance.RemoveEnemy(this);
        base.OnDying();
    }
}
