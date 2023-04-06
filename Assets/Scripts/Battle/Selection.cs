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
                if (selectionType == SelectionType.OneAndNeighbour)
                {
                    if (i - 1 >= 0)
                    {
                        selectedEnemies.Add(BattleManager.Instance.enemies[i - 1]);
                        selectedCreatures.Add(BattleManager.Instance.enemies[i - 1]);
                    }
                    if (i + 1 < BattleManager.Instance.enemies.Count)
                    {
                        selectedEnemies.Add(BattleManager.Instance.enemies[i + 1]);
                        selectedCreatures.Add(BattleManager.Instance.enemies[i + 1]);
                    }
                }
            }
            foreach (var t in before)
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

    List<Enemy> enemies { get { return BattleManager.Instance.enemies; } }

    public void StartCharacterSelection(SelectionType type, ActionUponCharacter action, bool isAnimControlledByGM = false)
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
        curCharacter.mono.MoveBack();
        foreach (Character character in characters)
        {
            character.mono.cardSR.enabled = true;
        }
        foreach(Enemy e in enemies)
        {
            e.mono.gameObject.SetActive(false);
        }
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
                for(int i = 0; i < characters.Count; ++i)
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

    Vector3 chaSpotSeat = new Vector3(142.4f, 3.4f, 72.9f);
    Quaternion chaSpotRotation = Quaternion.Euler(new Vector3(0, -18, 0));
    public void StartEnemySelection(SelectionType type, ActionUponEnemy action, bool isAnimControlledByGM = false)
    {
        ClearSelection();
        isTargetEnemy = true;
        enemyAction = action;
        selectionType = type;
        characterAction = null;
        isDuringSelection = true;
        selectRight.SetActive(true);
        selectLeft.SetActive(true);
        curCharacter.mono.MoveTo(chaSpotSeat, chaSpotRotation);
        foreach (Character character in characters)
        {
            character.mono.cardSR.enabled = curCharacter == character;
        }
        foreach (Enemy e in enemies)
        {
            e.mono.gameObject.SetActive(true);
        }
        switch (type)
        {
            case SelectionType.Self:
            case SelectionType.OneExceptSelf:
            case SelectionType.AllExceptSelf:
                Debug.LogError($"Wrong Selection type detected in StartEnemySelection : {selectionType}.");
                break;
            case SelectionType.One:
                enemies[0].mono.SetSelected();
                selectedCreatureIndices.Add(0);
                break;
            case SelectionType.All:
                for (int i = 0;i<enemies.Count;++i)
                {
                    enemies[i].mono.SetSelected();
                    selectedCreatureIndices.Add(i);
                }
                break;
            case SelectionType.OneAndNeighbour:
                enemies[0].mono.SetSelected();
                selectedCreatureIndices.Add(0);
                if (enemies.Count >= 2)
                {
                    enemies[1].mono.SetSelected(false);
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
        else if (selectionType == SelectionType.OneAndNeighbour)
        {
            if (isTargetEnemy)
            {
                List<EnemyMono> enemies = new List<EnemyMono>();
                foreach (Enemy e in BattleManager.Instance.enemies)
                {
                    enemies.Add(e.mono);
                }
                int curSelected = selectedCreatureIndices[0];

                // d 会向 id 小的那边选择，所以必须
                bool change = curSelected > 0;
                PlayAudio(change);
                if (change)
                {
                    if (curSelected + 1 < enemies.Count)
                        enemies[curSelected + 1].SetUnselected();
                    enemies[curSelected].SetSelected(false);
                    --curSelected;
                    selectedCreatureIndices[0] = curSelected;
                    enemies[curSelected].SetSelected(true);
                    if (curSelected - 1 >= 0)
                    {
                        enemies[curSelected - 1].SetSelected(false);
                    }
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
                List<Enemy> enemies = BattleManager.Instance.enemies;
                int curSelected = selectedCreatureIndices[0];

                bool change = curSelected < enemies.Count - 1;
                PlayAudio(change);
                if (change)
                {
                    enemies[curSelected].mono.SetUnselected();
                    ++curSelected;
                    enemies[curSelected].mono.SetSelected();
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
        else if (selectionType == SelectionType.OneAndNeighbour)
        {
            if (isTargetEnemy)
            {
                List<Enemy> enemies = BattleManager.Instance.enemies;
                int curSelected = selectedCreatureIndices[0];

                // a 会向 id 大的那边选择，所以必须
                bool change = curSelected < BattleManager.Instance.enemies.Count - 1;
                PlayAudio(change);
                if (change)
                {
                    enemies[curSelected].mono.SetSelected(false);
                    if (curSelected - 1 >= 0)
                        enemies[curSelected - 1].mono.SetUnselected();
                    ++curSelected;
                    enemies[curSelected].mono.SetSelected(true);
                    if(curSelected + 1 < enemies.Count)
                        enemies[curSelected + 1].mono.SetSelected(false);
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
