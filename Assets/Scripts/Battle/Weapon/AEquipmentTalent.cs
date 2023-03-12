using System.Collections;
using System.Collections.Generic;
using LitJson;

public abstract class AEquipmentTalent
{
    public abstract void OnEquiping(Character character);

    public abstract void OnTakingOff(Character character);

    public abstract void OnBattleStart(Character self, List<Character> characters, List<Enemy> enemies);
}
