using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarRailNight : AWeaponTalent
{
    public StarRailNight(JsonData c ,int r):base(c, r)
    {

    }

    float dmgUp, atk;
    public override void OnEquiping(Character character)
    {
        dmgUp = (float)(double)config["effect"]["dmgUp"]["value"][refine];
        atk = (float)(double)config["effect"]["atk"]["value"][refine];
    }

    public override void OnBattleStart(Character self, List<Character> characters)
    {
        self.AddBuff("starRailNightATK", BuffType.Permanent, CommonAttribute.ATK, (s, t, dc) =>
        {
            return Mathf.Min(5, BattleManager.Instance.enemies.Count) * atk * self.GetBaseAttr(CommonAttribute.ATK);
        }, null);
        
    }

    public override void OnEnemyRefresh(Character self, List<Enemy> enemies)
    {
        foreach (Enemy enemy in enemies) {
            enemy.onBreak.Add(new TriggerEvent<Enemy.OnBreak>("starRailNight", (s, d) =>
            {
                self.AddBuff("starRainNightDmgUp", BuffType.Buff, CommonAttribute.GeneralBonus, ValueType.InstantNumber, dmgUp, null, 1);
            }));
        }
    }

    public override void OnTakingOff(Character character)
    {

    }
}
