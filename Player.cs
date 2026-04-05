using Animation;

public class Player
{
    public string name;
    public bool carriesItem;
    public int hp;
 
    public bool isBitten;
    public int daysBitten;
    public int daysScavenging;
    public int daysUntilTurn;
  

    //online
    public string UGSPlayerId;
    public bool isLocalPlayer = false;



    //roles
    public bool isKiller;
    public bool isIsolatedAmalgam;
    public bool isDoomsayer;
    public bool isScanner;
    public bool isMedic;
    public bool isMartyr;
    public bool isVeteran;
    public bool isAmalgamHost;
    public bool isBlocker;
    public bool isBaker;

    //status effects
    public bool isVictim;
    public bool isInjured;
    public bool isInsane;
    public bool isDetective;
    public bool isDeceased;
    public bool isProtected;
    public bool isMad;
    public bool isSick;
    public bool isPraying;

    // Internal Scavenging Slot (1-6, 0 = not scavenging)
    public int scavengingSlot;

    // Items
    public bool playerRifle;
    public bool playerDog;
    public bool playerTools;
    public bool playerCards;

    /// <summary>
    // player.lootedRifle=true; player.lootedRadio=true; player.lootedMedkit=true; player.lootedBullets; player.lootedFoodRations

    /// </summary>

    public DamageFlash damageFlash;
    public CameraShake cameraShake;
   

    //Resources the player is carrying
    public int lootedFoodRations;
    public int lootedBullets;
    public bool lootedMedkit;
    public bool lootedRadio;
    public bool lootedRifle;
    public bool lootedTools;
    public bool lootedDog;
   

    public Player(string name)
    {
        this.name = name;
        this.carriesItem = false;
        this.hp = 2; // Default HP
        this.isSick = false;
        this.isBitten = false;
        this.scavengingSlot = 0; // Not scavenging by default
        this.daysBitten = 0;
        this.daysScavenging = 0;
        this.daysUntilTurn = UnityEngine.Random.Range(3, 5);
        this.isMad = false;
        this.isKiller = false;
        this.isVictim = false;
        this.isInsane = false;
        this.isDetective = false;
    }

    // Compatibility with old scripts
    public bool isScavenging { get => scavengingSlot == 1; set => scavengingSlot = value ? 1 : 0; }
    public bool isScavenging2 { get => scavengingSlot == 2; set => scavengingSlot = value ? 2 : 0; }
    public bool isScavenging3 { get => scavengingSlot == 3; set => scavengingSlot = value ? 3 : 0; }
    public bool isScavenging4 { get => scavengingSlot == 4; set => scavengingSlot = value ? 4 : 0; }
    public bool isScavenging5 { get => scavengingSlot == 5; set => scavengingSlot = value ? 5 : 0; }
    public bool isScavenging6 { get => scavengingSlot == 6; set => scavengingSlot = value ? 6 : 0; }

    public bool IsScavenging()
    {
        return scavengingSlot > 0;
    }



    public void TakeDamage(int amount = 1)
    {
        hp -= amount;

        // Flash screen
        damageFlash?.TriggerFlash();

        // Shake camera
        cameraShake?.TriggerShake();

        // Optional: play damage sound
       

        // Handle death
        if (hp <= 0)
        {
            GameManager.Instance.HandlePlayerDeath(this);
        }
    }
    
    
    public string GetAssignedRole()
    {
        if (isScanner) return "Scanner";
        if (isMartyr) return "Martyr";
        if (isVeteran) return "Veteran";
        if (isMedic) return "Medic";
        if (isDoomsayer) return "Doomsayer";
        if (isMad) return "Amalgam";
        if (isAmalgamHost) return "Host";
        return "Civilian";  
    }

}
