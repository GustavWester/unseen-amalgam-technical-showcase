using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Animation;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    public enum Language
    {
        English,
        Danish,

        
        
    }

    public Language currentLanguage = Language.English;

   //online game
   public bool online = true;

    public int daysPassed = 1;
    
    public bool hasGenerator = false;
    public bool hasMap = false;
    
    public bool militaryCalled = false;
    public int livePlayers;
    public bool playerDeadAlert = false;
    
    public Player LocalPlayer { get; private set; }

   
    public bool Scanner = false;
    public bool Martyr = false;
    public bool Veteran = false;
    public bool Doomsayer = false;
    public bool Medic = false;
    public bool RandomRoles = false;

    
    //MessageTriggers
    public bool martyrTriggered = false;
    public bool medicSaveTriggered = false;
    public List<Player> martyrKillers = new List<Player>();


    public List<(Player killer, Player victim)> pendingKills = new List<(Player, Player)>();
    public List<(Player veteran, Player target)> pendingVeteranKills = new List<(Player, Player)>();

    public GameObject damageFlashPrefab;
    

    //Metrics that determine encounter outcomes.
    public int zombieAwarenessOfBase = 0;
    public int banditAwarenessOfBase = 0;
    public int baseFortification = 10;
    public int groupSanity = 10;
    public int banditKarma = 5; // 
    public int banditGroupForce = 10; // Hodw strong is is the bandit group
    public int morale;
    public int bigBadStrengthLevel = 10;

    //Items
    public int rifleBullets = 99;
    public bool hasRifle = true;
    public bool hasDogCompanion = false;
    public bool hasTools = false;
    public bool playingCards = false;
    public bool hasHandgun = false;
    public int foodRations = 100;
    public bool hasRadio = false;
    public int medicalKits;


    //Items gaiend by scavenging players.
    public int foodRationsGainedThisRound;
    public int bulletsGainedThisRound;
    public int medkitsGainedThisRound;
    public bool radioGainedThisRound;
    public bool rifleGainedThisRound;
    public bool toolsGainedThisRound;


    public bool dogWhistle = false;

   
   
    public bool kidAlly = false;
    public int ventilation = 5; // how good is the ventilation inside the bunker.
    public bool ventilationJammed = false;

    public bool deerShot = false;
    public bool hasMagnifyingGlass = false;
    public bool hallucinatingPlayer = false;
 
    //status of scavengeencounter
    public int fatherDaughter = 0;
    public bool pharmacyWomanAnnoyed = false;
    public bool playerStuck;
    public int totalRiflesFound;

    //status of encounters
    public bool dogDenied = false;
    public bool musicianIsAmalgam = false;
    public bool strangerIsAmalgam = false;
    public bool fatherDaughterIsAmalgam = false;
    public bool banditsIsAmalgam = false;
    public bool womanIsAmalgam = false;
    public bool bakerIsAmalgam = false;
    public bool farmerIsmalgam = false;
    public bool littleBoyIsAmalgam = false;
    public bool characterAmalgamStatusSet = false;
    
     
    //Radio tower maintenance status.
    public bool radioTowerGeneratorFixed = false;
    public bool radioTowerControlPanelFixed = false;
    public bool radioTowerAntennaFixed = false;



    public bool tutorialDone = true;
    public bool isDetectiveTutorialDone = false;
    public int researchProgress = 0;


    public bool mapFound = false;
    public bool radioTowerEncounterTriggered = false;
    public bool radioTowerFirstFix = false;
    public bool radioTowerSecondFix = false;
    public bool radioTowerThirdFix = false;
    public int totalReptiles=1;
    public int roundsSinceLastPlotEncounter = 0;
    public int plotEncounterThreshold = 4;
    public int encounterCounter = 0;
    public int encountersPerDay = 2;


    public bool killActivated = false;
    public bool radioTowerSabotaged = false;

    public int totalPlayers;
    public List<Player> players = new List<Player>();
    public List<Player> scavengingPlayers = new List<Player>();
    public List<Player> deadPlayers = new List<Player>();
    public List<Player> missingPlayers = new List<Player>();
    public Player myPlayer;
    // Track read-aloud queue
    public List<Player> readAloudQueue = new List<Player>();
    
    private Dictionary<string, int> inventory = new Dictionary<string, int>();
    public bool playerWentMad = false; // Track if a player has gone mad
    public string lastKilledPlayerName;
    public Player activeScavenger;
    public Player chosenVictim;
    public int maxScavengers = 3;

    private HashSet<string> completedEncounters = new HashSet<string>();
    private HashSet<string> completedScavengingEncounters = new HashSet<string>();

    private List<string> orderedScavengingScenes = new List<string>();
    private int scavengeIndex = 0;
    private int seed;

    private List<string> orderedEncounterScenes = new List<string>();
   
    private int encounterIndex = 0; // Add a field to track index
    public int encountersLeft;


    public bool scavengeMusic = false;
    public int playerRoleCounter = 0;
    
    
    //online
    
    public Dictionary<string, ScavengingEncounterResult> preparedEncounters
        = new Dictionary<string, ScavengingEncounterResult>();


    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make sure this object persists across scenes
        }
        else
        {
            Destroy(gameObject);
        }


       
    }
    private void Start()
    {
        StartCoroutine(LogScavengingPlayers());
        ResetDontDestroyOnLoadObjects();
        seed = new System.Random().Next(); // This can be saved for reproducibility if needed
        UnityEngine.Random.InitState(seed);
        Debug.Log(seed);
        

        // Initialize and shuffle the ordered scavenging scenes
        orderedScavengingScenes = new List<string>
    {
        
        "ScavengeAction3",
        "ScavengeAction3",
        "ScavengeAction3",
        "ScavengeAction1",
        "ScavengeAction1",
        "ScavengeAction1",
        "ScavengeAction7",
        "ScavengeAction7",
        "ScavengeAction7",
        "ScavengeAction7",
       "ScavengeActionSuperMarket",
       "ScavengeActionSuperMarket",
       "ScavengeActionSuperMarket",
       "ScavengeActionSuperMarket",
       "ScavengeActionPoliceStation",
       "ScavengeActionPoliceStation",
       "ScavengeActionPoliceStation",
       "ScavengeActionBodyEncounter",
       "ScavengeActionBodyEncounter",
       "ScavengeActionStrangeBunker",
       "ScavengeActionStrangeBunker",
       "ScavengeActionStrangeBunker",
       "ScavengeActionDeerHunt",
       "ScavengeActionDeerHunt",
       "ScavengeActionPharmacy",
       "ScavengeActionPharmacy",
       "ScavengeActionNothing",
       "ScavengeActionNothing",
       "ScavengeActionNothing",
       "ScavengeActionBanditFight",
       "ScavengeActionBanditFight",
       "ScavengeActionBanditFight",
       "ScavengeActionBanditFight",
       "ScavengeActionSuburb",
       "ScavengeActionSuburb",
       "ScavengeActionSuburb",
       "ScavengeActionSuburb",
       
      
       
       /*


        "ScavengeActionStrangeBunker",
        "ScavengeAction1",
        // "ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction7",
        "ScavengeAction8",
        //"ScavengeAction12",
        //"ScavengeAction4",
        ScavengeActionSuperMarket,
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeActionNothing",
        //"ScavengeActionNothing",
        "ScavengeAction1",
        //"ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction8",
       // "ScavengeAction12",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeAction12",
        "ScavengeActionBanditFight",
        "ScavengeActionSuperMarket",
        "ScavengeActionBodyEncounter",
        "ScavengeActionBodyEncounter",
        "ScavengeActionStrangeBunker",
        "ScavengeAction1",
        // "ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction7",
        "ScavengeAction8",
        //"ScavengeAction12",
        //"ScavengeAction4",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeActionNothing",
        //"ScavengeActionNothing",
        "ScavengeAction1",
        //"ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction8",
        // "ScavengeAction12",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeAction12",
        "ScavengeActionBanditFight",
        "ScavengeActionSuperMarket",
        "ScavengeActionBodyEncounter",
        "ScavengeActionBodyEncounter",
        "ScavengeActionStrangeBunker",
        "ScavengeAction1",
        // "ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction7",
        "ScavengeAction8",
        //"ScavengeAction12",
        //"ScavengeAction4",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeActionNothing",
        //"ScavengeActionNothing",
        "ScavengeAction1",
        //"ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction8",
        // "ScavengeAction12",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeAction12",
        "ScavengeActionBanditFight",
        "ScavengeActionSuperMarket",
        "ScavengeActionBodyEncounter",
        "ScavengeActionBodyEncounter",
        "ScavengeActionStrangeBunker",
        "ScavengeAction1",
        // "ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction7",
        "ScavengeAction8",
        //"ScavengeAction12",
        //"ScavengeAction4",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeActionNothing",
        //"ScavengeActionNothing",
        "ScavengeAction1",
        //"ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction8",
        // "ScavengeAction12",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeAction12",
        "ScavengeActionBanditFight",
        "ScavengeActionSuperMarket",
        "ScavengeActionBodyEncounter",
        "ScavengeActionBodyEncounter",
           "ScavengeActionStrangeBunker",
        "ScavengeAction1",
        // "ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction7",
        "ScavengeAction8",
        //"ScavengeAction12",
        //"ScavengeAction4",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeActionNothing",
        //"ScavengeActionNothing",
        "ScavengeAction1",
        //"ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction8",
       // "ScavengeAction12",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeAction12",
        "ScavengeActionBanditFight",
        "ScavengeActionSuperMarket",
        "ScavengeActionBodyEncounter",
        "ScavengeActionBodyEncounter",
        "ScavengeActionStrangeBunker",
        "ScavengeAction1",
        // "ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction7",
        "ScavengeAction8",
        //"ScavengeAction12",
        //"ScavengeAction4",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeActionNothing",
        //"ScavengeActionNothing",
        "ScavengeAction1",
        //"ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction8",
        // "ScavengeAction12",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeAction12",
        "ScavengeActionBanditFight",
        "ScavengeActionSuperMarket",
        "ScavengeActionBodyEncounter",
        "ScavengeActionBodyEncounter",
        "ScavengeActionStrangeBunker",
        "ScavengeAction1",
        // "ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction7",
        "ScavengeAction8",
        //"ScavengeAction12",
        //"ScavengeAction4",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeActionNothing",
        //"ScavengeActionNothing",
        "ScavengeAction1",
        //"ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction8",
        // "ScavengeAction12",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeAction12",
        "ScavengeActionBanditFight",
        "ScavengeActionSuperMarket",
        "ScavengeActionBodyEncounter",
        "ScavengeActionBodyEncounter",
        "ScavengeActionStrangeBunker",
        "ScavengeAction1",
        // "ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction7",
        "ScavengeAction8",
        //"ScavengeAction12",
        //"ScavengeAction4",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeActionNothing",
        //"ScavengeActionNothing",
        "ScavengeAction1",
        //"ScavengeAction3",
        "ScavengeAction6",
        "ScavengeAction8",
        // "ScavengeAction12",
        "ScavengeActionSuperMarket",
        "ScavengeActionPoliceStation",
        "ScavengeActionStrangeBunker",
        //"ScavengeAction12",
        "ScavengeActionBanditFight",
        "ScavengeActionSuperMarket",
        "ScavengeActionBodyEncounter",
        "ScavengeActionBodyEncounter",*/
    };

        // Shuffle scavenging scenes
        ShuffleList(orderedScavengingScenes);

        // Act-based encounter ordering
        List<string> act1Encounters = new List<string>
    {
        
        "Encounter2", // HandGunVote
         "Encounter15",  // Amalgam Infiltration
        "Encounter3", // Trash Can
        "Encounter14", // Shedding
        "Encounter13", // BurstSteamPipe
       
        
          "EncounterBanditBarter", // Bandit Barter
                  "Encounter1", // Musician
                  //"EncounterBigbad1",
                  "EncounterStrangeWoman",
                  "Encounter9",//Strange Woman
        "Encounter7", // Father
        "Encounter14", // Shedding
        "Encounter5", // Birthday
           
           "Encounter10", // Medical Kit
        
    };

        List<string> act2Encounters = new List<string>
    {
        
        "Encounter10", // Medical Kit
        "Encounter13", // BurstSteamPipe
        "EncounterBanditBarter", // Bandit Barter
        "Encounter1", // Musician
        "EncounterBigBad2"
    };

        List<string> act3Encounters = new List<string>
    {
        
      
        "EncounterStrangeWoman",
        "Encounter9",//Strange Woman
        
         "Encounter7", // Father
        "Encounter14", // Shedding
        "Encounter15"  // Amalgam Infiltration
        // "EncounterBigBad3" is excluded from the shuffle
    };

        // Shuffle each act independently
        ShuffleList(act1Encounters);
        ShuffleList(act2Encounters);
        ShuffleList(act3Encounters);

        // Combine all acts
        orderedEncounterScenes = new List<string>();
        orderedEncounterScenes.AddRange(act1Encounters);
        orderedEncounterScenes.AddRange(act2Encounters);
        orderedEncounterScenes.AddRange(act3Encounters);

        // Ensure EncounterBigBad3 is always the final encounter
        orderedEncounterScenes.Add("Encounter14");
        orderedEncounterScenes.Add("Encounter14");
        orderedEncounterScenes.Add("Encounter14");
        orderedEncounterScenes.Add("Encounter14");
        orderedEncounterScenes.Add("Encounter14");
        orderedEncounterScenes.Add("Encounter14");
        orderedEncounterScenes.Add("Encounter14");
        orderedEncounterScenes.Add("Encounter14");
        orderedEncounterScenes.Add("Encounter14");
        orderedEncounterScenes.Add("Encounter14");
        orderedEncounterScenes.Add("Encounter14");
        orderedEncounterScenes.Add("Encounter14");
        
        orderedEncounterScenes.Add("EncounterBigBad3");
    }
    private void ShuffleList(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            string temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
    public void SetTotalPlayers(int count, int amalgamSetting)
    {
        if (count < playerRoleCounter)
        {
            Debug.LogWarning($"Cannot set totalPlayers to {count}: roles already assigned!");
            return;
        }

        totalPlayers = count;

        if (!online)
        {
            players.Clear();
        }

        if (amalgamSetting == 0)
        {
            if (online)
            {
                NetworkGameManager.Instance.RollAmalgamsServerRpc(totalPlayers);
            }
            else
            {
                int maxAmalgams = Mathf.FloorToInt(totalPlayers / 2f);
                totalReptiles = Random.Range(1, maxAmalgams + 1);
            }
        }
        else
        {
            totalReptiles = amalgamSetting;
        }

        maxScavengers = totalPlayers - 1;
        foodRations = totalPlayers * 8 + 30;

        Debug.Log($"Total players: {totalPlayers}, Reptiles: {totalReptiles}");
    }
    
    public void AddPlayerName(string name)
    {
        if (players.Count < totalPlayers)
        {
            players.Add(new Player(name));
        }
    }
    public void ClearPlayers(){
        
    }
    //ONLINE ADDPLAYER
    public void AddPlayer(string ugsId, string name)
    {
        if (players.Exists(p => p.UGSPlayerId == ugsId))
        {
            Debug.LogWarning($"Duplicate player prevented: {name}");
            return;
        }

        players.Add(new Player(name)
        {
            UGSPlayerId = ugsId,
            name = name
        });
    }



    public void SendPlayerScavenging(Player player, int scavengerIndex)
    {
        switch (scavengerIndex)
        {
            case 0:
                player.isScavenging = true;
                break;
            case 1:
                player.isScavenging2 = true;
                break;
            case 2:
                player.isScavenging3 = true;
                break;
            case 3:
                player.isScavenging4 = true;
                break;
            case 4:
                player.isScavenging5 = true;
                break;
            case 5:
                player.isScavenging6 = true;
                break;
            default:
                Debug.LogError("Scavenger index out of range!");
                return;
        }

        player.daysScavenging = 1; // They will be out for 1 day

        //  Only add THIS player to the scavengingPlayers list
        if (!scavengingPlayers.Contains(player))
        {
            scavengingPlayers.Add(player);
        }

        // (Optional) Debug log
        Debug.Log($"Added {player.name} to scavengingPlayers.");
    }
    public void ExilePlayer(Player player)
    {
        players.Remove(player);
        Debug.Log($"{player.name} has been exiled from the bunker.");
    }
    public void NextDay()
    {
        daysPassed++;

        foreach (Player player in players)
        {
            player.isVictim = false;
            
            scavengingPlayers.Add(player);
        }
        
        
        
       // ResolveAllPendingKills(); // <-- new
        ReturnItemsToPool();

        if (daysPassed >=2 ) { 
        bool hasInfectedPlayer = players.Exists(p => p.isSick);
        float baseInfectionChance = 0.02f; // Base daily infection risk (1%)

        if (hasInfectedPlayer)
        {
            // Increase infection chance if there is already an infected player
            baseInfectionChance += 0.1f; // Additional 10% chance if someone is sick
        }

        // Amplify infection risk based on ventilation conditions
        if (ventilationJammed)
        {
            baseInfectionChance += 0.2f; // Extra 20% if ventilation is jammed
        }


        else
        {
            // Increase risk based on ventilation level, inversely proportional
            baseInfectionChance += Mathf.Clamp01(0.2f - (ventilation * 0.02f));
        }

        // Randomly determine if infection will spread
        if (Random.value < baseInfectionChance)
        {
            // Select a random healthy player to infect
            List<Player> healthyPlayers = players.FindAll(p => !p.isSick);
            if (healthyPlayers.Count > 0)
            {
                Player target = healthyPlayers[Random.Range(0, healthyPlayers.Count)];
                target.isSick = true;
                Debug.Log($"{target.name} has become sick due to infection spread in the bunker.");
            }
        }

        }

       
        // Calculate daily resource consumption
        int dailyRations = 0;
        foreach (Player player in players)
        {

            
            if (player.hp > 0)
            {
                if (player.isSick)
                {
                    dailyRations += 3; // Sick or bitten players require 3 rations

                   
                }
                else
                {
                    dailyRations += 1; // Healthy players require 1 ration
                }
            }
        }

        if (!players.Any(p => p.isMad) && players.Any(p2 => p2.isDoomsayer))
        {
            Debug.Log("The Doomsayer sacrifices themself to the Amalgam");
            foreach (Player player in players)
            {
                if (player.isDoomsayer)
                {
                    player.isMad = true;
                }
            }   
            return;
        }

        // Deduct daily rations from the total food rations
        foodRations -= dailyRations;

        // Check for scavenging players and other conditions


        // Check if a mad player has killed their target


    }

    public void SetActiveScavenger(Player player)
    {
        activeScavenger = player;
    }


    public void ResolveKillingEvent()
    {
        // TEMPORARY NO-OP
        Debug.Log("ResolveKillingEvent() called, but killing is now delayed to end of scavenging phase.");
    }

    public void ResolveAllPendingKills()
    {
        Debug.Log("Resolving all pending kills...");

        // Track all victims that actually died this night
        var actuallyKilledVictims = new List<Player>();

        // First pass: resolve Amalgam kills
        foreach (var (killer, victim) in pendingKills)
        {
            if (victim.isProtected)
            {
                Debug.Log($"{victim.name} was protected and survived!");
                
                medicSaveTriggered = true;
                continue;
            }

            Debug.Log($"{victim.name} was killed by {killer.name}.");
            HandlePlayerDeath(victim);
            actuallyKilledVictims.Add(victim);
        }

        // Martyr revenge pass
        foreach (var martyr in actuallyKilledVictims.Where(v => v.isMartyr))
        {
            Debug.Log($"{martyr.name} was a Martyr. Retaliating!");

            if (martyr.isPraying)
            {

                foreach (var (killer, victim) in pendingKills)
                {
                    if (victim == martyr && !killer.isDeceased)
                    {
                        Debug.Log($"Martyr kills {killer.name} in revenge!");
                        HandlePlayerDeath(killer);
                        martyrTriggered = true;
                        if (!martyrKillers.Contains(killer))
                            martyrKillers.Add(killer);
                    }
                }
            
            }
            else
            {
                Debug.Log($"Martyr was not praying"); 
            }
            
           martyr.isPraying = false;
           Debug.Log($"Martyr forced to isPraying=false");
        }

        // Veteran kills resolution
        foreach (var (veteran, target) in pendingVeteranKills)
        {
            if (target.isProtected)
            {
                Debug.Log($"Veteran's target {target.name} was protected. No one dies.");
                
                medicSaveTriggered = true;
                continue;
            }

            Debug.Log($"Veteran kills target {target.name}.");
            HandlePlayerDeath(target);
            actuallyKilledVictims.Add(target);

            // Check if target was an amalgam
            if (target.isMad)
            {
                Debug.Log($"Veteran killed an amalgam. Veteran survives.");
            }
            else
            {
                Debug.Log($"Veteran killed an innocent! Veteran dies from guilt.");
                HandlePlayerDeath(veteran);
            }
        }

        // Clean up queues
        pendingKills.Clear();
        pendingVeteranKills.Clear();
        killActivated = false;
        chosenVictim = null;
        
        foreach (var player in players)
        {
            player.isProtected = false;
        }

    }

    

public void HandlePlayerDeath(Player player)
{
    if (player.isProtected)
    {
        Debug.Log($"{player.name} was protected and survived!");
        player.isProtected = false;
        medicSaveTriggered = true;

        return;
    }
        else
        {
            Debug.Log($"{player.name} has died.");
            player.hp = 0;
            player.isDeceased = true;
            playerDeadAlert = true;

            if (!deadPlayers.Contains(player))
            {
                deadPlayers.Add(player);
            }
        }

   
}

public void NextDayFromMainMenu()
    {
        if (foodRations <= 0)
        {
            // Handle game over condition
            Debug.Log("Game Over: The group has run out of essential resources.");

        }
        else
        {
            // Load the next encounter scene or main bunker scene
            SceneManager.LoadScene("ChapterScene");
            
        }
    }
    
public void RemoveItem(string itemName)
    {
        if (inventory.ContainsKey(itemName) && inventory[itemName] > 0)
        {
            inventory[itemName]--;
        }
    }

 public void LoadEncounterScene()
    {
        // Increment the counter for rounds since the last plot encounter
        roundsSinceLastPlotEncounter++;
        
        // Check for "mad" players or empty player list
        if (!players.Any(p => p.isMad) && !players.Any(p2 => p2.isDoomsayer))
        {
            Debug.Log("No mad players and doomsayers found. Loading 'SurvivorsWin' scene.");
            SceneManager.LoadScene("SurvivorsWin");
            return;
        }
       else if (players.Count == 0 || players.All(p => p.isMad) || players.All(p => p.isDoomsayer))
        {
            Debug.Log("All players are either mad or no players left. Loading 'MonstersWin' scene.");
            SceneManager.LoadScene("MonstersWin");
            return;
        }


        // Tutorial check
       /* if (!tutorialDone)
        {
            Debug.Log("Tutorial is not complete, forcing 'Encounter6' scene.");
            //SceneManager.LoadScene("Encounter6");
            tutorialDone = true;
            return;
        }*/

        // Force plot encounters if threshold is reached
        bool forcePlotEncounter = roundsSinceLastPlotEncounter >= plotEncounterThreshold;
        List<string> plotScenes = new List<string>();
        if (!mapFound) plotScenes.Add("Encounter4");
        if (radioTowerGeneratorFixed && radioTowerControlPanelFixed && radioTowerAntennaFixed && hasRadio && !militaryCalled) plotScenes.Add("EncounterContactMilitary");

        if (forcePlotEncounter && plotScenes.Count > 0 && Random.value < 0.5f)
        {
            string plotScene = plotScenes[UnityEngine.Random.Range(0, plotScenes.Count)];
            Debug.Log("Forcing plot encounter: " + plotScene);
            roundsSinceLastPlotEncounter = 0; // Reset the counter after a plot encounter
            SceneManager.LoadScene(plotScene);
            return;
        }

        // Build a list of conditional encounters
        List<string> conditionalScenes = new List<string>();


        if (rifleBullets >= 4 && medicalKits >= 1 && playingCards) conditionalScenes.Add("Encounter11");
        if (!mapFound) conditionalScenes.Add("Encounter4");
        if (radioTowerThirdFix && hasRadio && !militaryCalled) conditionalScenes.Add("EncounterContactMilitary");
        if (militaryCalled) conditionalScenes.Add("EncounterEnding");
        if (banditAwarenessOfBase >= 4) conditionalScenes.Add("EncounterBanditAttack");
        if (dogWhistle==true && hasDogCompanion==false && dogDenied==false) conditionalScenes.Add("Encounter12");
       


        // Check for available conditional scenes
        if (conditionalScenes.Count > 0)
        {
            string conditionalScene = conditionalScenes[UnityEngine.Random.Range(0, conditionalScenes.Count)];
            Debug.Log("Loading conditional encounter: " + conditionalScene);
            SceneManager.LoadScene(conditionalScene);
            return;
        }

        // Load from randomized order if conditions aren't met
        if (encounterIndex < orderedEncounterScenes.Count)
        {
            string sceneName = orderedEncounterScenes[encounterIndex];
            encounterIndex++;
            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("No more ordered encounter scenes. Reshuffling.");
            ReshuffleEncounterScenes();
            encounterIndex = 0; // Reset index
            SceneManager.LoadScene(orderedEncounterScenes[encounterIndex]);
            encounterIndex++; // Prepare for the next scene
        }

        // Fallback: No scenes available
        if (orderedEncounterScenes.Count == 0)
        {
            Debug.Log("No more ordered encounter scenes. Reshuffling.");
            ReshuffleEncounterScenes();
            if (orderedEncounterScenes.Count > 0)
            {
                string sceneName = orderedEncounterScenes[0];
                orderedEncounterScenes.RemoveAt(0); // Ensure the scene is removed after loading
                Debug.Log("Loading reshuffled encounter: " + sceneName);
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError("No scenes available even after reshuffling.");
            }
        }

    }
    
    // Method to load the scavenging encounter scene
    public void LoadScavengingEncounterScene()
    {
        
        if (online)
        {
            PrepareMultiplayerScavengingEncounters();
            
            
            Player hostPlayer = LocalPlayer;

            if (hostPlayer == null || !hostPlayer.IsScavenging())
            {
                // Host is NOT scavenging → go straight to DayCounter
                SceneManager.LoadScene("DayCounter");
                
            }
            return;
        }
        
        
        
        // Find the next scavenging player
        Player nextScavenger = players.FirstOrDefault(p => p.IsScavenging());

        if (nextScavenger != null)
        {
            LoadPlayerSpecificEncounter(nextScavenger);
        }
        else
        {
            Debug.LogError("No scavenging players found! Returning to main scene.");
            SceneManager.LoadScene("MainGame");
        }
    }


    // Helper method to handle player-specific encounters
    private void LoadPlayerSpecificEncounter(Player scavenger)
    {

        if (hasRifle==false && Random.value < 0.13f)
        {
            Debug.Log("No rifle, randomzing outcome for maybe rifle scene.");
            SceneManager.LoadScene("ScavengeAction3");
        }

        else if (ventilationJammed && scavenger.playerTools)
        {
            Debug.Log("Ventilation is jammed. Forcing 'ScavengeVentilationJammed'.");
            SceneManager.LoadScene("ScavengeActionVentilationJammed");
        }
        else if (ventilationJammed && Random.value<0.20f)
        {
            Debug.Log("Ventilation is jammed. Forcing 'ScavengeVentilationJammed'.");
            SceneManager.LoadScene("ScavengeActionVentilationJammed");
        }

        else
        {
            
            
            Debug.Log($"{scavenger.name} has no special conditions. Loading a normal scavenging scene.");
            // Check for radio tower progress
            bool radioTowerIncomplete = !radioTowerGeneratorFixed || !radioTowerControlPanelFixed || !radioTowerAntennaFixed;

            if (hasMap && radioTowerIncomplete && roundsSinceLastPlotEncounter >= plotEncounterThreshold && Random.value < 0.40f)
            {
                Debug.Log("Triggering Radio Tower Encounter (Unified).");
                SceneManager.LoadScene("ScavengeActionRadioTower");
                roundsSinceLastPlotEncounter = 0;
                return;
            }



            // Handle group-wide forced conditions
           

            if (foodRations <= 40 && !deerShot )
            {
                Debug.Log("Low food rations, forcing 'ScavengeAction8'.");
                SceneManager.LoadScene("ScavengeActionDeerHunt");
                return;
            }

            if (fatherDaughter != 0 && Random.value < 0.13f)
            {
                Debug.Log("Father-daughter condition met, forcing 'ScavengeAction11'.");
                SceneManager.LoadScene("ScavengeAction11");
                return;
            }
            LoadNextOrderedScene();
        }
    }
    
    
    // Helper method to load the next scene from the ordered list
    private void LoadNextOrderedScene()
    {
        if (scavengeIndex < orderedScavengingScenes.Count)
        {
            string sceneName = orderedScavengingScenes[scavengeIndex];
            scavengeIndex++;
            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("No more ordered scavenging scenes. Reshuffling.");
            ReshuffleScenes();
            SceneManager.LoadScene(orderedScavengingScenes[0]);
            scavengeIndex = 1; // Set to 1 as the first scene is already loaded
        }
    }

    // Helper method to reshuffle scenes
    private void ReshuffleScenes()
    {
        for (int i = orderedScavengingScenes.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            string temp = orderedScavengingScenes[i];
            orderedScavengingScenes[i] = orderedScavengingScenes[j];
            orderedScavengingScenes[j] = temp;
        }
        Debug.Log("Scenes reshuffled.");
    }

    private void ReshuffleEncounterScenes()
    {
        for (int i = orderedEncounterScenes.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            string temp = orderedEncounterScenes[i];
            orderedEncounterScenes[i] = orderedEncounterScenes[j];
            orderedEncounterScenes[j] = temp;
        }
        Debug.Log("Scenes reshuffled.");
    }
    
    public void MarkEncounterCompleted(string encounterName)
    {
        completedEncounters.Add(encounterName);
        Debug.Log("Marked encounter completed: " + encounterName);
    }
    
    // Method to add resources to the inventory
    public void AddResources(int food)
    {
        foodRations += food;
    }

    // Method to add items to the inventory
    public void AddItem(string itemName)
    {
        switch (itemName)
        {
            case "Generator":
                hasGenerator = true;
                break;
            case "Map":
                hasMap = true;
                mapFound = true;
                MarkEncounterCompleted("Encounter4"); // Mark the map encounter as completed
                break;
            case "Radio":
                hasRadio = true;
                break;
            case "Medicine":
                AddItemToInventory(itemName);
                break;
            case "Rusty Saw":
                AddItemToInventory(itemName);
                break;
            case "Gun":
                AddItemToInventory(itemName);
                break;
            case "Flashlight":
                AddItemToInventory(itemName);
                break;
            default:
                Debug.LogWarning("Unknown item: " + itemName);
                break;
        }
    }

    private void AddItemToInventory(string itemName)
    {
        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName]++;
        }
        else
        {
            inventory[itemName] = 1;
        }
    }
    

    // Method to check the status of an item
    public bool HasItem(string itemName)
    {
        switch (itemName)
        {
            case "Generator":
                return hasGenerator;
            case "Map":
                return hasMap;
            case "Radio":
                return hasRadio;
            case "Medicine":
            case "Rusty Saw":
            case "Gun":
            case "Flashlight":
                return inventory.ContainsKey(itemName) && inventory[itemName] > 0;
            default:
                Debug.LogWarning("Unknown item: " + itemName);
                return false;
        }
    }

    private void ResetDontDestroyOnLoadObjects()
    {
        GameObject[] dontDestroyOnLoadObjects =
            FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in dontDestroyOnLoadObjects)
        {
            if (obj.scene.name == null) // Objects with DontDestroyOnLoad are not part of any specific scene
            {
                Destroy(obj);
            }
        }
    }
    
    public void ReturnItemsToPool()
    {
        int totalFoodGained = 0;
        int totalBulletsGained = 0;
        int totalMedkitsGained = 0;
        bool radioGained = false;
        bool rifleGained = false;
        bool toolsGained = false;

        foreach (Player player in players)
        {
            // Forbrugsvarer (Mad, Patroner, Medkits) ryger stadig i den fælles pulje
            totalFoodGained += player.lootedFoodRations;
            totalBulletsGained += player.lootedBullets;
            player.lootedFoodRations = 0;
            player.lootedBullets = 0;

            if (player.lootedMedkit)
            {
                totalMedkitsGained++;
                player.lootedMedkit = false;
            }

            if (player.lootedRadio && !hasRadio)
            {
                hasRadio = true;
                radioGained = true;
                player.lootedRadio = false;
            }

            // ==========================================
            // ONLINE VS OFFLINE: VÅBEN OG VÆRKTØJ
            // ==========================================
            if (online)
            {
                // ONLINE: Spilleren beholder sine egne ting.
                
                if (player.lootedRifle)
                {
                    player.playerRifle = true;
                    player.lootedRifle = true;
                    
                    // ÆNDRING HER: Vi sætter IKKE rifleGained = true, 
                    // fordi riflen ikke ryger i bunkerens fælles kasse!
                    rifleGained = false; 
                }

                if (player.lootedTools)
                {
                    player.playerTools = true;
                    player.lootedTools = true;

                    // Samme her: Værktøjet deles ikke med bunkeren
                    toolsGained = false;
                }
                if (player.lootedDog)
                {
                    player.playerDog = true;
                    player.lootedDog = true;

                    // Samme her: Værktøjet deles ikke med bunkeren
                    
                }
            }
            else
            {
                // OFFLINE: Tingene afleveres til bunkeren og slettes fra spilleren
                
                if (player.lootedRifle)
                {
                    if (!hasRifle)
                    {
                        hasRifle = true;
                        rifleGained = true;
                    }
                    player.lootedRifle = false; // Slettes altid fra spilleren i offline
                }

                if (player.lootedTools)
                {
                    if (!hasTools)
                    {
                        hasTools = true;
                        toolsGained = true;
                    }
                    player.lootedTools = false; // Slettes altid fra spilleren i offline
                }
            }
        }

        // Føj ressourcerne til bunkerens totals
        foodRations += totalFoodGained;
        rifleBullets += totalBulletsGained;
        medicalKits += totalMedkitsGained;

        Debug.Log($"Gained: {totalFoodGained} Food Rations, {totalBulletsGained} Bullets, {totalMedkitsGained} Medkits" +
                  (radioGained ? ", 1 Radio" : "") +
                  (rifleGained ? ", 1 Rifle" : "") +
                  (toolsGained ? ", 1 set of tools" : ""));

        // Opdater stat variabler for den daglige rapport (DayCounter)
        foodRationsGainedThisRound = totalFoodGained;
        bulletsGainedThisRound = totalBulletsGained;
        medkitsGainedThisRound = totalMedkitsGained;
        radioGainedThisRound = radioGained;
        rifleGainedThisRound = rifleGained;
        toolsGainedThisRound = toolsGained;
    }



    public void PlayPlayerDamageEffects()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        GameObject flashInstance = Instantiate(damageFlashPrefab, canvas.transform);
        DamageFlash flash = flashInstance.GetComponent<DamageFlash>();
        flash?.TriggerFlash();
        
    }


    private IEnumerator LogScavengingPlayers()
    {
        while (true)
        {
            Debug.Log("--- Scavenging Players ---");

            foreach (Player player in scavengingPlayers)
            {
                if (player != null)
                {
                    Debug.Log($"Player: {player.name} | Alive: {!player.isDeceased} | Mad: {player.isMad}");
                }
            }

            yield return new WaitForSeconds(10f); // Wait 10 seconds
        }

    }
    
    public static string L(params string[] translations)
    {
        int langIndex = (int)Instance.currentLanguage;

        // Hvis den valgte sprog-indeks findes i arrayet
        if (langIndex < translations.Length && !string.IsNullOrEmpty(translations[langIndex]))
        {
            return translations[langIndex];
        }

        // Hvis oversættelsen mangler, brug engelsk som fallback (index 0)
        return translations.Length > 0 ? translations[0] : "";
    }

    public static event System.Action OnLanguageChanged;

    public void SetLanguage(Language newLanguage)
    {
        currentLanguage = newLanguage;
        OnLanguageChanged?.Invoke();
    }

    public void UpdateMaxScavengers()
    {
        maxScavengers = players.Count - 1;
    }

    
    /* public delegate void OnRoleChanged(string role);
          public event OnRoleChanged RoleChanged;

          public void AssignMyPlayerRole(string role)
          {
              myPlayerRole = role;
              RoleChanged?.Invoke(role);
          }*/


    
    //online scavengeencounter distribution
    
    [System.Serializable]
    public class ScavengingEncounterResult
    {
        public string playerId;
        public string sceneName;
    }
    
   
    private string DetermineScavengingEncounter(Player scavenger)
    {
        if (!hasRifle && Random.value < 0.13f)
            return "ScavengeAction3";

        if (ventilationJammed && scavenger.playerTools)
            return "ScavengeActionVentilationJammed";

        if (ventilationJammed && Random.value < 0.20f)
            return "ScavengeActionVentilationJammed";

        bool radioTowerIncomplete =
            !radioTowerGeneratorFixed ||
            !radioTowerControlPanelFixed ||
            !radioTowerAntennaFixed;

        if (hasMap && radioTowerIncomplete &&
            roundsSinceLastPlotEncounter >= plotEncounterThreshold &&
            Random.value < 0.40f)
        {
            roundsSinceLastPlotEncounter = 0;
            return "ScavengeActionRadioTower";
        }

        if (groupSanity <= 5 && !hallucinatingPlayer)
            return "ScavengeAction10";

        if (foodRations <= 40 && !deerShot)
            return "ScavengeActionDeerHunt";

        if (fatherDaughter != 0 && Random.value < 0.13f)
            return "ScavengeAction11";

        return GetNextOrderedScene();
    }
    
    public void PrepareMultiplayerScavengingEncounters()
    {
        // 1. Tøm ordbogen fra i går
        preparedEncounters.Clear();
        Debug.Log("<b>[GAMEMANAGER]</b> Preparing encounters... cleared dictionary.");

        // 2. Find alle der skal scavenge
        foreach (Player scavenger in players.Where(p => p.IsScavenging()))  
        {
            // 3. Vælg scene
            string scene = DetermineScavengingEncounter(scavenger);

            // 4. Gem i ordbog under deres UGS ID
            preparedEncounters[scavenger.UGSPlayerId] = new ScavengingEncounterResult
            {
                playerId = scavenger.UGSPlayerId,
                sceneName = scene
            };

            Debug.Log($"<b>[GAMEMANAGER]</b> Prepared encounter for {scavenger.name} (UGS: {scavenger.UGSPlayerId}): {scene}");
        
            // VIGTIGT: Vi har slettet NetworkGameManager.Instance.LoadScavengeSceneForPlayer herfra!
            // Nu venter vi pænt på, at Hosten trykker på "Proceed" knappen i næste skærmbillede.
        }
    }


    
    private string GetNextOrderedScene()
    {
        if (scavengeIndex < orderedScavengingScenes.Count)
            return orderedScavengingScenes[scavengeIndex++];

        ReshuffleScenes();
        scavengeIndex = 1;
        return orderedScavengingScenes[0];
    }

    public void ResolveLocalPlayer()
    {
        string localUgsId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;

        LocalPlayer = players.FirstOrDefault(p => p.UGSPlayerId == localUgsId);

        if (LocalPlayer == null)
        {
            Debug.LogWarning($"[GameManager] LocalPlayer not found for UGS ID {localUgsId}");
        }
        else
        {
            Debug.Log($"[GameManager] LocalPlayer resolved: {LocalPlayer.name}");
        }
    }

    
    // Add this to GameManager.cs
    public void SetOnlineGameRules(int amalgamCount)
    {
        // 1. Set the difficulty/monster count
        this.totalReptiles = amalgamCount;

        // 2. Note: We DO NOT touch 'totalPlayers' or the 'players' list here.
        // That is handled dynamically by NetworkManager as people join.
        
        Debug.Log($"[GameManager] Online Rules Updated. Amalgams set to: {totalReptiles}");
    }



    public void SetCharacterAmalgamStatus()
    {
        if (Random.Range(1, 0) == 0)
        {
            musicianIsAmalgam = true;
            Debug.Log("Musician is Amalgam");
        }
        if (Random.Range(1, 0) == 0)
        {
            strangerIsAmalgam = true;
            Debug.Log("Stranger is Amalgam");
        }
        if (Random.Range(1, 0) == 0)
        {
            fatherDaughterIsAmalgam = true;
            Debug.Log("Father and daughter is Amalgam");
        }
        if (Random.Range(1, 0) == 0)
        {
            banditsIsAmalgam = true;
            Debug.Log("Bandits are Amalgam");
        }
        if (Random.Range(1, 0) == 0)
        {
            womanIsAmalgam = true;
            Debug.Log("Woman is Amalgam");
        }
        if (Random.Range(1, 0) == 0)
        {
            bakerIsAmalgam = true;
            Debug.Log("Baker is Amalgam");
        }
        if (Random.Range(1, 0) == 0)
        {
            farmerIsmalgam = true;
            Debug.Log("Farmer is Amalgam");
        }
        if (Random.Range(1, 0) == 0)
        {
            littleBoyIsAmalgam = true;
            Debug.Log("Little boy is Amalgam");
        }

        characterAmalgamStatusSet = true;

    }
    
}
