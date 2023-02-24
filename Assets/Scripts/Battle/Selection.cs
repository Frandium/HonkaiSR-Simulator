using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selection : MonoBehaviour
{
    public AudioClip changeSlelect;
    public AudioClip error;
    public AudioSource audioSource;
    bool isDuringSelection = false;
    // Update is called once per frame
    void Update()
    {
        if (isDuringSelection)
        {
            ResponseSelectionChange();
        }
    }

    public void ApplyAction(Dictionary<string, Character.TalentUponTarget> talent)
    {
        if (isTargetEnemy)
        {
            List<Enemy> selectedEnemies = new List<Enemy>();
            foreach (int i in selectedCreatures)
            {
                selectedEnemies.Add(BattleManager.Instance.enemies[i]);
            }
            enemyAction(selectedEnemies);
            foreach (var t in talent.Values)
            {
                foreach(Enemy e in selectedEnemies)
                {
                    t(e);
                }
            }

        }
        else
        {
            List<Character> selectedCharacters = new List<Character>();
            foreach (int i in selectedCreatures)
            {
                selectedCharacters.Add(BattleManager.Instance.characters[i]);
            }
            characterAction(selectedCharacters);
            foreach (var t in talent.Values)
            {
                foreach (Character c in selectedCharacters)
                {
                    t(c);
                }
            }
        }
        ClearSelection();
        isDuringSelection = false;
    }

    public List<int> selectedCreatures = new List<int>();
    public delegate void ActionUponCharacter(List<Character> characters);
    public delegate void ActionUponEnemy(List<Enemy> enemies);

    ActionUponCharacter characterAction;
    ActionUponEnemy enemyAction;
    bool isTargetEnemy = false;
    SelectionType selectionType;
    int curCharacterIndex = 0;
    CharacterMono curCharacter;

    public void StartNewTurn(CharacterMono curC)
    {
        curCharacter = curC;
    }

    public void StartCharacterSelection(SelectionType type, ActionUponCharacter action)
    {
        ClearSelection();
        isTargetEnemy = false;
        selectionType = type;
        characterAction = action;
        enemyAction = null;
        isDuringSelection = true;
        List<CharacterMono> characters = BattleManager.Instance.cMonos;
        curCharacterIndex = characters.FindIndex(c => c == curCharacter);
        switch (type)
        {
            case SelectionType.Self:
                curCharacter.SetSelected();
                selectedCreatures.Add(curCharacterIndex);
                break;
            case SelectionType.One:
                characters[0].SetSelected();
                selectedCreatures.Add(0);
                break;
            case SelectionType.OneExceptSelf:
                for(int i = 0;i<characters.Count; ++i)
                {
                    CharacterMono c = characters[i];
                    if(c != curCharacter)
                    {
                        c.SetSelected();
                        selectedCreatures.Add(i);
                        break;
                    }
                }
                break;
            case SelectionType.All:
                for (int i = 0; i < characters.Count; ++i)
                {
                    CharacterMono c = characters[i];
                    c.SetSelected();
                    selectedCreatures.Add(i);
                }
                break;
            case SelectionType.AllExceptSelf:
                for (int i = 0; i < characters.Count; ++i)
                {
                    CharacterMono c = characters[i];
                    if (c != curCharacter)
                    {
                        c.SetSelected();
                        selectedCreatures.Add(i);
                    }
                }
                break;
        }
    }

    public void StartEnemySelection(SelectionType type, ActionUponEnemy action)
    {
        ClearSelection();
        isTargetEnemy = true;
        enemyAction = action;
        selectionType = type;
        characterAction = null;
        isDuringSelection = true; 
        List<EnemyMono> enemies = new List<EnemyMono>();
        foreach (Enemy e in BattleManager.Instance.enemies)
        {
            enemies.Add(e.mono);
        }
        switch (type)
        {
            case SelectionType.Self:
            case SelectionType.OneExceptSelf:
            case SelectionType.AllExceptSelf:
                Debug.LogError($"Wrong Selection type detected in StartEnemySelection : {selectionType}.");
                break;
            case SelectionType.One:
                enemies[0].SetSelected();
                selectedCreatures.Add(0);
                break;
            case SelectionType.All:
                for (int i = 0;i<enemies.Count;++i)
                {
                    enemies[i].SetSelected();
                    selectedCreatures.Add(i);
                }
                break;
        }
    }

    void ClearSelection()
    {
        foreach (CharacterMono c in BattleManager.Instance.cMonos)
        {
            c.SetUnselected();
        }
        foreach (Enemy e in BattleManager.Instance.enemies)
        {
            e.mono.SetUnselected();
        }
        selectedCreatures.Clear();
    }

    void ResponseSelectionChange()
    {

        if (selectionType == SelectionType.One)
        {
            if (isTargetEnemy)
            {
                List<EnemyMono> enemies = new List<EnemyMono>();
                foreach(Enemy e in BattleManager.Instance.enemies)
                {
                    enemies.Add(e.mono);
                }
                int curSelected = selectedCreatures[0];
                if (Input.GetKeyDown(KeyCode.D))
                {
                    bool change = curSelected > 0;
                    PlayAudio(change);
                    if (change)
                    {
                        enemies[curSelected].SetUnselected();
                        --curSelected;
                        enemies[curSelected].SetSelected();
                        selectedCreatures[0] = curSelected;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    bool change = curSelected < enemies.Count - 1;
                    PlayAudio(change);
                    if (change)
                    {
                        enemies[curSelected].SetUnselected();
                        ++curSelected;
                        enemies[curSelected].SetSelected();
                        selectedCreatures[0] = curSelected;
                        audioSource.Play();
                    }
                }
            }
            else
            {
                List<CharacterMono> characters = BattleManager.Instance.cMonos;
                int curSelected = selectedCreatures[0];
                if (Input.GetKeyDown(KeyCode.D))
                {
                    bool change = curSelected < characters.Count - 1;
                    PlayAudio(change);
                    if (change)
                    {
                        characters[curSelected].SetUnselected();
                        ++curSelected;
                        characters[curSelected].SetSelected();
                        selectedCreatures[0] = curSelected;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    bool change = curSelected > 0;
                    PlayAudio(change);
                    if (change)
                    {
                        characters[curSelected].SetUnselected();
                        --curSelected;
                        characters[curSelected].SetSelected();
                        selectedCreatures[0] = curSelected;
                    }
                }
            }
        }
        else if (selectionType == SelectionType.OneExceptSelf)
        {
            List<CharacterMono> characters = BattleManager.Instance.cMonos;
            int curSelected = selectedCreatures[0];
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (curSelected < characters.Count - 1)
                {
                    if (curSelected + 1 == curCharacterIndex)
                    {
                        if (curSelected + 2 < characters.Count)
                        {
                            characters[curSelected].SetUnselected();
                            curSelected += 2;
                            characters[curSelected].SetSelected();
                            selectedCreatures[0] = curSelected;
                        }
                    }
                    else
                    {
                        characters[curSelected].SetUnselected();
                        ++curSelected;
                        characters[curSelected].SetSelected();
                        selectedCreatures[0] = curSelected;
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                if (curSelected > 0)
                {
                    if (curSelected - 1 == curCharacterIndex)
                    {
                        if (curSelected - 2 >= 0)
                        {
                            characters[curSelected].SetUnselected();
                            curSelected -= 2;
                            characters[curSelected].SetSelected();
                            selectedCreatures[0] = curSelected;
                        }
                    }
                    else
                    {
                        characters[curSelected].SetUnselected();
                        --curSelected;
                        characters[curSelected].SetSelected();
                        selectedCreatures[0] = curSelected;
                    }
                }
            }
        }
    }

    void PlayAudio(bool isChangeOrError)
    {
        if (isChangeOrError)
        {
            audioSource.clip = changeSlelect;
        }
        else
        {
            audioSource.clip = error;
        }
        audioSource.Play();
    }
}
