using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunwayAvatar : MonoBehaviour
{
    public RectTransform rect;
    public Image avatarImage;
    public Text turnToEnd;

    public Creature creature { get; private set; }
    public bool IsBurst { get; private set; }
    private bool firstMove = true;
    
    public delegate void AnyAction();

    public void SetCreature(Creature _c, bool isBurst)
    {
        creature = _c;
        IsBurst = isBurst;
        if(isBurst)
        {
            turnToEnd.transform.parent.gameObject.SetActive(false);
        }
        avatarImage.sprite = creature.mono.runwayAvatar;
    }

    public void MoveTowards(Vector3 pos, AnyAction nextToDo = null)
    {
        StopAllCoroutines();
        if (!IsBurst)
        {
            float location = creature.location;
            float speeed = creature.GetFinalAttr(CommonAttribute.Speed, true);
            turnToEnd.text = Mathf.CeilToInt((Runway.Length - location) / speeed).ToString();
        }
        StartCoroutine(AvatarAnim(pos, nextToDo));
    }

    private IEnumerator AvatarAnim(Vector3 endPos, AnyAction nextToDo)
    {
        Vector3 dir = (endPos - rect.anchoredPosition3D).normalized;
        float x = Vector3.Distance(rect.anchoredPosition3D, endPos);
        float dis = Vector3.Distance(rect.anchoredPosition3D, endPos);
        while (dis > .1f)
        {
            yield return new WaitForEndOfFrame();
            rect.Translate(dir * dis * Time.deltaTime * 5);
            dir = (endPos - rect.anchoredPosition3D).normalized;
            if (firstMove)
            {
                float alpha = 1 - dis / x;
                avatarImage.color = new Color(1, 1, 1, alpha);
            }
            dis = Vector3.Distance(rect.anchoredPosition3D, endPos);
        }
        firstMove = false;
        nextToDo?.Invoke();
    }

}
