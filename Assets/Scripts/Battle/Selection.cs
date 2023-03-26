using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Selection : MonoBehaviour
{
    public GameObject selectLeft;
    public GameObject selectRight;

    public AudioClip changeSlelect;
    public AudioClip error;
    public AudioSource audioSource;
    bool isDuringSelection = false;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            RespondtoKeycodeD();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            RespondtoKeycodeA();
        }
    }

    public void ApplyAction(List<TriggerEvent<Character.TalentUponTarget>> before, List<TriggerEvent<Character.TalentUponTarget>> after)
    {
        List<Creature> selectedCreatures = new List<Creature>();
        if (isTargetEnemy)
        {
            List<Enemy> selectedEnemies = new List<Enemy>();
            foreach (int i in selectedCreatureIndices)
            {
                selectedEnemies.Add(BattleManager.Instance.enemies[i]);
                selectedCreatures.Add(BattleManager.Instance.enemies[i]);
            }
            foreach(var t in before)
            {
                t.trigger(selectedCreatures);   
            }
            enemyAction(selectedEnemies);
            foreach (var t in after)
            {
                t.trigger(selectedCreatures);
            }
        }
        else
        {
            List<Character> selectedCharacters = new List<Character>();
            foreach (int i in selectedCreatureIndices)
            {
                selectedCharacters.Add(BattleManager.Instance.characters[i]);
                selectedCreatures.Add(BattleManager.Instance.characters[i]);
            }
            foreach (var t in before)
            {
                t.trigger(selectedCreatures);
            }
            characterAction(selectedCharacters);
            foreach (var t in after)
            {
                t.trigger(selectedCreatures);
            }
        }
        ClearSelection();
        isDuringSelection = false;
        selectLeft.SetActive(false);
        selectRight.SetActive(false);
    }

    public List<int> selectedCreatureIndices = new List<int>();
    public delegate void ActionUponCharacter(List<Character> characters);
    public delegate void ActionUponEnemy(List<Enemy> enemies);

    ActionUponCharacter characterAction;
    ActionUponEnemy enemyAction;
    bool isTargetEnemy = false;
    SelectionType selectionType;
    int curCharacterIndex = 0;
    List<Character> characters { get { return BattleManager.Instance.characters; } }
    Character curCharacter { get { return BattleManager.Instance.curCharacter; } }

    public void StartCharacterSelection(SelectionType type, ActionUponCharacter action)
    {
        ClearSelection();
        isTargetEnemy = false;
        selectionType = type;
        characterAction = action;
        enemyAction = null;
        isDuringSelection = true;
        selectRight.SetActive(true);
        selectLeft.SetActive(true);
        curCharacterIndex = characters.FindIndex(c => c == curCharacter);
        switch (type)
        {
            case SelectionType.Self:
                curCharacter.mono.SetSelected();
                selectedCreatureIndices.Add(curCharacterIndex);
                break;
            case SelectionType.One:
                characters[0].mono.SetSelected();
                selectedCreatureIndices.Add(0);
                break;
            case SelectionType.OneExceptSelf:
                for(int i = 0;i<characters.Count; ++i)
                {
                    Character c = characters[i];
                    if(c != curCharacter)
                    {
                        c.mono.SetSelected();
                        selectedCreatureIndices.Add(i);
                        break;
                    }
                }
                break;
            case SelectionType.All:
                for (int i = 0; i < characters.Count; ++i)
                {
                    Character c = characters[i];
                    c.mono.SetSelected();
                    selectedCreatureIndices.Add(i);
                }
                break;
            case SelectionType.AllExceptSelf:
                for (int i = 0; i < characters.Count; ++i)
                {
                    Character c = characters[i];
                    if (c != curCharacter)
                    {
                        c.mono.SetSelected();
                        selectedCreatureIndices.Add(i);
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
        selectRight.SetActive(true);
        selectLeft.SetActive(true);
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
                selectedCreatureIndices.Add(0);
                break;
            case SelectionType.All:
                for (int i = 0;i<enemies.Count;++i)
                {
                    enemies[i].SetSelected();
                    selectedCreatureIndices.Add(i);
                }
                break;
        }
    }

    void ClearSelection()
    {
        foreach (Character c in characters)
        {
            c.mono.SetUnselected();
        }
        foreach (Enemy e in BattleManager.Instance.enemies)
        {
            e.mono.SetUnselected();
        }
        selectedCreatureIndices.Clear();
    }


    public void RespondtoKeycodeD()
    {
        if (!isDuringSelection)
            return;

        if (selectionType == SelectionType.One)
        {
            if (isTargetEnemy)
            {
                List<EnemyMono> enemies = new List<EnemyMono>();
                foreach (Enemy e in BattleManager.Instance.enemies)
                {
                    enemies.Add(e.mono);
                }
                int curSelected = selectedCreatureIndices[0];


                bool change = curSelected > 0;
                PlayAudio(change);
                if (change)
                {
                    enemies[curSelected].SetUnselected();
                    --curSelected;
                    enemies[curSelected].SetSelected();
                    selectedCreatureIndices[0] = curSelected;
                }

            }
            else
            {
                int curSelected = selectedCreatureIndices[0];

                bool change = curSelected < characters.Count - 1;
                PlayAudio(change);
                if (change)
                {
                    characters[curSelected].mono.SetUnselected();
                    ++curSelected;
                    characters[curSelected].mono.SetSelected();
                    selectedCreatureIndices[0] = curSelected;
                }

            }
        }
        else if (selectionType == SelectionType.OneExceptSelf)
        {
            int curSelected = selectedCreatureIndices[0];
            if (curSelected > 0)
            {
                if (curSelected - 1 == curCharacterIndex)
                {
                    if (curSelected - 2 >= 0)
                    {
                        characters[curSelected].mono.SetUnselected();
                        curSelected -= 2;
                        characters[curSelected].mono.SetSelected();
                        selectedCreatureIndices[0] = curSelected;
                    }
                }
                else
                {
                    characters[curSelected].mono.SetUnselected();
                    --curSelected;
                    characters[curSelected].mono.SetSelected();
                    selectedCreatureIndices[0] = curSelected;
                }
            }

        }
    }

    public void RespondtoKeycodeA()
    {
        if (!isDuringSelection)
            return;

        if (selectionType == SelectionType.One)
        {
            if (isTargetEnemy)
            {
                List<EnemyMono> enemies = new List<EnemyMono>();
                foreach (Enemy e in BattleManager.Instance.enemies)
                {
                    enemies.Add(e.mono);
                }
                int curSelected = selectedCreatureIndices[0];

                bool change = curSelected < enemies.Count - 1;
                PlayAudio(change);
                if (change)
                {
                    enemies[curSelected].SetUnselected();
                    ++curSelected;
                    enemies[curSelected].SetSelected();
                    selectedCreatureIndices[0] = curSelected;
                    audioSource.Play();
                }

            }
            else
            {
                int curSelected = selectedCreatureIndices[0];
                bool change = curSelected > 0;
                PlayAudio(change);
                if (change)
                {
                    characters[curSelected].mono.SetUnselected();
                    --curSelected;
                    characters[curSelected].mono.SetSelected();
                    selectedCreatureIndices[0] = curSelected;
                }

            }
        }
        else if (selectionType == SelectionType.OneExceptSelf)
        {
            int curSelected = selectedCreatureIndices[0];
            if (curSelected < characters.Count - 1)
            {
                if (curSelected + 1 == curCharacterIndex)
                {
                    if (curSelected + 2 < characters.Count)
                    {
                        characters[curSelected].mono.SetUnselected();
                        curSelected += 2;
                        characters[curSelected].mono.SetSelected();
                        selectedCreatureIndices[0] = curSelected;
                    }
                }
                else
                {
                    characters[curSelected].mono.SetUnselected();
                    ++curSelected;
                    characters[curSelected].mono.SetSelected();
                    selectedCreatureIndices[0] = curSelected;
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
