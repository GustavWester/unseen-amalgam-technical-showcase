using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class EncounterBanditBarter : MonoBehaviour
{
    public TextMeshProUGUI encounterText;
    public TextMeshProUGUI outcomeText;
    public Button choice1Button;
    public Button choice2Button;
    public Button choice3Button;
    public Button choice4Button;
    public Button endEncounterButton;

    private Player randomPlayer;
    private bool banditsHaveWeapons;
    private bool banditsHaveFood;
    private bool areAmalgams;

    private void Start()
    {
        randomPlayer = GameManager.Instance.players[Random.Range(0, GameManager.Instance.players.Count)];

        // Randomly determine bandit inventory
        banditsHaveWeapons = Random.value < 0.5f;
        banditsHaveFood = !banditsHaveWeapons;

        float amalgamChance = 0.1f;
        if (GameManager.Instance.zombieAwarenessOfBase > GameManager.Instance.banditAwarenessOfBase)
        {
            amalgamChance = 0.3f;
        }
        areAmalgams = Random.value < amalgamChance;

        string tradeOffer = banditsHaveFood ?
            "They say they’ll give you food if you give them two bullets." :
            (GameManager.Instance.hasRifle ?
                (GameManager.Instance.hasHandgun ? "They offer extra ammo in exchange for food." : "They offer you a handgun in exchange for food.") :
                "They offer you a rifle and a bullet in exchange for food.");

        encounterText.text = "Two ragged figures stand outside the bunker. They claim to have deserted a ruthless bandit gang and insist they mean no harm. They ask for a trade.\n" + tradeOffer;

        choice1Button.onClick.AddListener(() => MakeChoice(1));
        choice2Button.onClick.AddListener(() => MakeChoice(2));
        choice3Button.onClick.AddListener(() => MakeChoice(3));
        choice4Button.onClick.AddListener(() => MakeChoice(4));

        outcomeText.gameObject.SetActive(false);
    }

    private void MakeChoice(int choice)
    {
        choice1Button.interactable = false;
        choice2Button.interactable = false;
        choice3Button.interactable = false;
        choice4Button.interactable = false;

        switch (choice)
        {
            case 1: HandleTrade(); break;
            case 2: HandleAttack(); break;
            case 3: HandleSendAway(); break;
            case 4: HandleInterrogate(); break;
        }
    }

    private void HandleTrade()
    {
        if (banditsHaveFood)
        {
            outcomeText.text = "You trade two bullets for some food. The bandits nod and thank you before leaving quietly.";
            GameManager.Instance.rifleBullets -= 2;
            GameManager.Instance.foodRations += 10;
            GameManager.Instance.banditKarma += 1;
        }
        else
        {
            GameManager.Instance.foodRations -= 10;
            GameManager.Instance.banditKarma += 1;
            if (!GameManager.Instance.hasRifle)
            {
                outcomeText.text = "You trade food and receive a rifle. A worthwhile deal.";
                GameManager.Instance.hasRifle = true;

            }
            else if (!GameManager.Instance.hasHandgun)
            {
                outcomeText.text = "You trade food for a small handgun. Might come in handy.";
                GameManager.Instance.hasHandgun = true;
            }
            else
            {
                outcomeText.text = "They hand you two extra bullets in return for food. It’s something, at least.";
                GameManager.Instance.rifleBullets += 2;
            }
        }

        PossiblyTransformToAmalgams();
    }

    private void HandleAttack()
    {
        GameManager.Instance.banditKarma -= 2;
        if (banditsHaveWeapons)
        {
            outcomeText.text = $"You open fire. The bandits fire back before falling. {randomPlayer.name} is hit.";
            randomPlayer.hp -= 1;
            GameManager.Instance.rifleBullets -= 2;
            GameManager.Instance.PlayPlayerDamageEffects();
        }
        else
        {
            outcomeText.text = "You shoot them down easily.";
            randomPlayer.hp -= 1;
        }

        PossiblyTransformToAmalgams();
    }

    private void HandleSendAway()
    {
        outcomeText.text = "You refuse the trade and send them off. They leave, muttering under their breath.";
        GameManager.Instance.banditKarma -= 1;

        outcomeText.gameObject.SetActive(true);
        Invoke("EnableEndEncounterButton", 1f);
    }

    private void HandleInterrogate()
    {
        if (GameManager.Instance.hasRifle)
        {
            GameManager.Instance.banditKarma -= 1;
            if (GameManager.Instance.banditGroupForce > 10)
            {
                outcomeText.text = "You raise your weapon. The bandits crack. 'We were scouting. There’s a big group headed this way.'";
            }
            else
            {
                outcomeText.text = "You point the rifle at them. They break: 'Okay, okay—we’re scouts. But the group’s small, just a few left.'";
            }
        }
        else
        {
            outcomeText.text = "You try to pressure them, but without a weapon, they just laugh and leave.";
        }

        PossiblyTransformToAmalgams();
    }

    private void PossiblyTransformToAmalgams()
    {
        if (!areAmalgams)
        {
            outcomeText.gameObject.SetActive(true);
            Invoke("EnableEndEncounterButton", 1f);
            return;
        }

        outcomeText.text += $"\nSuddenly, their bodies twist and crack. They erupt into hideous shapes—amalgams! One lunges at {randomPlayer.name}, tearing flesh.";
        randomPlayer.hp -= 1;
        GameManager.Instance.PlayPlayerDamageEffects();

        outcomeText.gameObject.SetActive(true);
        Invoke("EnableEndEncounterButton", 1f);
    }

    private void EnableEndEncounterButton()
    {
        endEncounterButton.gameObject.SetActive(true);
        endEncounterButton.onClick.AddListener(EndEncounter);
    }

    private void EndEncounter()
    {
        GameManager.Instance.encounterCounter += 1;
        if (GameManager.Instance.encounterCounter < GameManager.Instance.encountersPerDay)
        {
            GameManager.Instance.LoadEncounterScene();
        }
        else
        {
            AudioManager.Instance.PlayScavengeMusic();
            GameManager.Instance.encounterCounter = 0;
            SceneManager.LoadScene("ScavengingEncounter");
        }
    }
}
