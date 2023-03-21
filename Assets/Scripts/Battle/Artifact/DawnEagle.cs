using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DawnEagle : AArtifactTalent
{
    public DawnEagle(int c) : base(c)
    {

    }

    int locationUpTurn = -1;
    bool talentActivated = false;
    public override void OnEquiping(Character character)
    {
        if (count < 2)
            return;
        character.AddBuff("dawnEagle2", BuffType.Permanent, CommonAttribute.AnemoBonus, ValueType.InstantNumber, .1f);

        if (count < 4)
            return;
        character.afterBurst.Add(new TriggerEvent<Character.TalentUponTarget>("dawnEagle4", t =>
        {
            if(locationUpTurn != BattleManager.Instance.curTurnNumber)
            {
                talentActivated = true;
                locationUpTurn = BattleManager.Instance.curTurnNumber;
            }
        }, countdownType: CountDownType.Permanent));
        character.onTurnEnd.Add(new TriggerEvent<Creature.TurnStartEndEvent>("dawnEagle4LocationUp", () =>
        {
            if (talentActivated)
            {
                character.ChangePercentageLocation(.25f);
                talentActivated = false;
            }
        }, countdownType: CountDownType.Permanent));
    }


    public override void OnEnemyRefresh(Character self, List<Enemy> enemies)
    {
        foreach (Enemy e in enemies)
        {
            e.onBreak.Add(new TriggerEvent<Enemy.OnBreak>("meteorThief4Enegy", (s, d) =>
            {
                if (s == self)
                {
                    self.ChangeEnergy(3);
                }
            }));
        }
    }

    public override void OnTakingOff(Character character)
    {
        character.RemoveBuff("dawnEagle2");
        character.afterBurst.RemoveAll(t => t.tag == "dawnEagle4");
        character.onTurnEnd.RemoveAll(t => t.tag == "dawnEagle4LocationUp");
    }
}
