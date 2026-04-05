using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.Netcode;

public class ScavengeActionManagerSuburb : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI encounterText;
    public TextMeshProUGUI outcomeText;

    public Button choice2Button; // Car
    public Button choice3Button; // Corpse
    public Button choice4Button; // House
    public Button choice5Button; // Leave
    public Button endScavengeButton;

    // Host Only Reference
    private Player hostPlayerRef;
    
    private bool choiceMade = false;
    private bool isEnding = false;

    private enum RunMode
    {
        OfflineOrHost,
        ClientOnly
    }

    private RunMode runMode;

    private void Start()
    {
        // 1. Determine Mode
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            runMode = RunMode.ClientOnly;
        }
        else
        {
            runMode = RunMode.OfflineOrHost;
        }

        // 2. Setup
        if (runMode == RunMode.ClientOnly)
        {
            if (ClientGameManager.Instance != null)
                ClientGameManager.Instance.ResetScavengeLogic();

            SetupClient();
        }
        else
        {
            SetupHost();
        }
    }

    // =======================================================================================
    // SETUP
    // =======================================================================================

    private void SetupHost()
    {
        // === FIX START ===
        if (GameManager.Instance.online)
        {
            string hostId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
            hostPlayerRef = GameManager.Instance.players.FirstOrDefault(p => p.UGSPlayerId == hostId);
        }
        else
        {
            // Offline mode: just grab the next person in line
            hostPlayerRef = GameManager.Instance.players.FirstOrDefault(p => p.scavengingSlot > 0);
        }
        // === FIX END ===

        if (hostPlayerRef == null)
        {
            EndEncounter();
            return;
        }
        
        InitializeUI();
    }

    private void SetupClient()
    {
        if (!ClientGameManager.Instance.TryGetMyPlayer(out var me))
        {
            encounterText.text = "Waiting for server data...";
            return;
        }

        InitializeUI();
    }

    private void InitializeUI()
    {
        string[] streetDetails = {
            "Broken mailboxes lean into the street.",
            "A toppled streetlight blocks part of the road.",
            "A dog bowl filled with rain water sits on a porch."
        };

        string[] carDetails = {
            "There's a car with smashed windows sits at the curb.",
            "There's a car with a door hanging open.",
        };

        string[] corpseDetails = {
            "Looks like a corpse beside it.",
            "A body slumps up against it.",
        };

        string description = $"You reach a quiet suburban street. You can't tell if people are hiding from the Amalgams, or there simply is no one left. {streetDetails[Random.Range(0, streetDetails.Length)]} {carDetails[Random.Range(0, carDetails.Length)]} {corpseDetails[Random.Range(0, corpseDetails.Length)]}";

        encounterText.text = description;

        // === TILFØJET: TEKST PÅ KNAPPERNE ===
        // Her tildeler vi teksten til hver knaps TextMeshProUGUI-child.
        choice2Button.GetComponentInChildren<TextMeshProUGUI>().text = "Search the car";
        choice3Button.GetComponentInChildren<TextMeshProUGUI>().text = "Examine the corpse";
        choice4Button.GetComponentInChildren<TextMeshProUGUI>().text = "Scavenge the house";
        choice5Button.GetComponentInChildren<TextMeshProUGUI>().text = "Leave the area";
        // ====================================

        // Button Listeners
        choice2Button.onClick.AddListener(() => MakeChoice(2));
        choice3Button.onClick.AddListener(() => MakeChoice(3));
        choice4Button.onClick.AddListener(() => MakeChoice(4));
        choice5Button.onClick.AddListener(() => MakeChoice(5));

        outcomeText.gameObject.SetActive(false);
        endScavengeButton.gameObject.SetActive(false);
    }

    // =======================================================================================
    // DATA HELPERS (Bridge between Host and Client)
    // =======================================================================================

    // --- READERS ---
    private int GetAwareness()
    {
        if (runMode == RunMode.OfflineOrHost) return Mathf.Max(GameManager.Instance.zombieAwarenessOfBase, GameManager.Instance.banditAwarenessOfBase);
        return Mathf.Max(ClientGameManager.Instance.BaseState.zombieAwarenessOfBase, ClientGameManager.Instance.BaseState.banditAwarenessOfBase);
    }

    private bool HasRifle()
    {
        if (runMode == RunMode.OfflineOrHost) 
        {
            // On Host, find the local player in GameManager
            var localPlayer = GameManager.Instance.players.FirstOrDefault(p => p.isLocalPlayer);
            return localPlayer != null && localPlayer.playerRifle;
        }

        // On Client, find yourself in the last snapshot
        if (ClientGameManager.Instance.TryGetMyPlayer(out PlayerInfo me))
        {
            return me.hasRifle;
        }
    
        return false;
    }

    private int GetBulletCount()
    {
        if (runMode == RunMode.OfflineOrHost) return GameManager.Instance.rifleBullets;
        return ClientGameManager.Instance.Inventory.rifleBullets;
    }

    private bool HasDog()
    {
        if (runMode == RunMode.OfflineOrHost) return hostPlayerRef.playerDog;
        return ClientGameManager.Instance.Inventory.hasDogCompanion;
    }

    private bool HasTools()
    {
        if (runMode == RunMode.OfflineOrHost) return hostPlayerRef.playerTools;
        return ClientGameManager.Instance.Inventory.hasTools;
    }

    // --- WRITERS (Modifiers) ---
    private void ModHP(int amount)
    {
        if (runMode == RunMode.OfflineOrHost)
        {
            hostPlayerRef.hp += amount;
            if (amount < 0) GameManager.Instance.PlayPlayerDamageEffects();
        }
        else
        {
            ClientGameManager.Instance.AddPendingHpChange(amount);
            if (amount < 0) Debug.Log("[Client] Ouch!");
        }
    }

    private void ModFood(int amount)
    {
        if (runMode == RunMode.OfflineOrHost) hostPlayerRef.lootedFoodRations += amount;
        else ClientGameManager.Instance.AddPendingFood(amount);
    }

    private void ModBullets(int amount)
    {
        if (runMode == RunMode.OfflineOrHost)
        {
            if (amount > 0) hostPlayerRef.lootedBullets += amount; // Gaining loot
            else GameManager.Instance.rifleBullets += amount;      // Spending ammo
        }
        else ClientGameManager.Instance.AddPendingBullet(amount);
    }

    private void ModRifle()
    {
        if (runMode == RunMode.OfflineOrHost)
        {
            GameManager.Instance.hasRifle = true;
            if (hostPlayerRef != null) hostPlayerRef.playerRifle = true; // VIGTIGT!
        }
        else
        {
            ClientGameManager.Instance.MarkRifleFound();
        }
    }

    private void HandleDeath()
    {
        if (runMode == RunMode.OfflineOrHost) GameManager.Instance.HandlePlayerDeath(hostPlayerRef);
        else ClientGameManager.Instance.MarkPendingDeath();
    }

    private bool CheckIsDead()
    {
        if (runMode == RunMode.OfflineOrHost) return hostPlayerRef.hp <= 0;
        
        if (ClientGameManager.Instance.TryGetMyPlayer(out var me))
        {
            return (me.hp + ClientGameManager.Instance.PendingScavengeResults.hpChange) <= 0;
        }
        return false;
    }

    // =======================================================================================
    // LOGIC
    // =======================================================================================

    private void MakeChoice(int choice)
    {
        if (choiceMade) return;
        choiceMade = true;
        
        DisableButtons();

        switch (choice)
        {
            case 2: SearchCar(); break;
            case 3: SearchCorpse(); break;
            case 4: SearchHouse(); break;
            case 5: LeaveArea(); break;
        }
    }

    private bool ThreatCheck()
    {
        // Logic: Roll 0-9. If less than awareness, threat exists.
        return Random.Range(0, 10) < GetAwareness();
    }

    private void SearchCar()
    {
        if (ThreatCheck())
        {
            outcomeText.text = "An small amalgam stirs inside the car. It jumps you, biting into your shoulder. You fend it off, but not without injury";
            ModHP(-1);
            if(CheckIsDead()) HandleDeath();
        }
        else
        {
            outcomeText.text = "You find some canned food inside the car.";
            ModFood(10);
            
            if (!HasRifle())
            {
                ModRifle();
                outcomeText.text += " A rifle is tucked under the seat.";
            }
        }

        ShowOutcomeAndEnd();
    }

    private void SearchCorpse()
    {
        if (HasDog())
        {
            outcomeText.text = "Your dog growls. The corpse shifts but dies still. Its body is wrong — extra joints under the skin. You grab some loose ammo from it.";
            ModBullets(1);
        }
        else
        {
            if (ThreatCheck())
            {
                if (!HasRifle()) // HasWeapon check
                {
                    outcomeText.text = "The corpse lashes out. Up close, its bones don't match anything normal.. You are hurt fleeing from it.";
                    ModHP(-1);
                }
                else if (GetBulletCount() <= 0) // HasAmmo check
                {
                    outcomeText.text = "You raise your weapon but it clicks empty. You escape wounded. Up close, its bones don't match anything normal..";
                    ModHP(-1);
                }
                else
                {
                    outcomeText.text = "The creature moves. You fire and silence it. Strange tissue oozes from the body.";
                    ModBullets(-1); // Spend bullet
                    ModFood(5);     // Reward
                }
            }
            else
            {
                outcomeText.text = "The body is dead. Up close, its bones don't match anything normal. You find some rations nearby.";
                ModFood(5);
            }
        }
        
        if(CheckIsDead()) HandleDeath();

        ShowOutcomeAndEnd();
    }

    private void SearchHouse()
    {
        if (HasTools())
        {
            outcomeText.text = "You use your tools to open a locked basement. Inside is a supply of water and food for a couple days.";
            ModFood(10);
        }
        else
        {
            outcomeText.text = "You check the ground floor. In a closet, you find some leftover supplies.";
            ModFood(5);
        }

        if (!HasRifle())
        {
            ModRifle();
            outcomeText.text += " A rifle rests against the fireplace.";
        }

        ShowOutcomeAndEnd();
    }

    private void LeaveArea()
    {
        outcomeText.text = "You leave the neighborhood without searching.";
        ShowOutcomeAndEnd();
    }

    private void ShowOutcomeAndEnd()
    {
        outcomeText.gameObject.SetActive(true);
        Invoke(nameof(EnableEndButton), 1f);
    }
    
    private void EnableEndButton()
    {
        endScavengeButton.gameObject.SetActive(true);
        endScavengeButton.onClick.AddListener(EndEncounter);
    }

    private void DisableButtons()
    {
        choice2Button.interactable = false;
        choice3Button.interactable = false;
        choice4Button.interactable = false;
        choice5Button.interactable = false;
    }

    // =======================================================================================
    // FINISH
    // =======================================================================================

    public void EndEncounter()
    {
        if (isEnding) return;
        isEnding = true;
        
        endScavengeButton.interactable = false;
        // CLIENT
        if (runMode == RunMode.ClientOnly)
        {
            var report = ClientGameManager.Instance.PendingScavengeResults;

            NetworkGameManager.Instance.ReportScavengeOutcomeServerRpc(
                ClientGameManager.Instance.LocalPlayerId,
                report
            );
            
            if (HasRifle()) 
            {
                report.foundRifle = true;
            }

            SceneManager.LoadScene("ClientScene");
            return;
        }

        // HOST / OFFLINE
        if (hostPlayerRef != null)
        {
            hostPlayerRef.scavengingSlot = 0;
        }

        // === FIX START ===
        if (GameManager.Instance.online)
        {
            // In online mode, the Host goes straight to the waiting screen
            SceneManager.LoadScene("DayCounter");
        }
        else
        {
            // In offline mode, loop through the remaining players
            bool moreScavenging = GameManager.Instance.players.Exists(p => p.scavengingSlot > 0);
            SceneManager.LoadScene(moreScavenging ? "ScavengingEncounter" : "DayCounter");
        }
        // === FIX END ===
    }
}