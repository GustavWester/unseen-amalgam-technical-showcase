using System.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance;

    public Dictionary<string, ulong> ugsToClientId = new Dictionary<string, ulong>();
    private int clientsReady = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        DontDestroyOnLoad(gameObject); // Moved inside Awake for safety

    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            StartCoroutine(RegisterAfterConnected());
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        
        DontDestroyOnLoad(gameObject);
        
        Debug.Log($"NetworkGameManager spawned | IsClient={IsClient} IsServer={IsServer}");
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId) return;
        Debug.Log($"Client connected: {clientId}");
    }

    // ---------------------------
    // CLIENT SENDS UGS → SERVER
    // ---------------------------
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RegisterPlayerServerRpc(string ugsPlayerId, RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // FIXED: Handle reconnects. If ID exists, update it to the new ClientId
        if (ugsToClientId.ContainsKey(ugsPlayerId))
        {
            ugsToClientId[ugsPlayerId] = clientId;
            Debug.Log($"[SERVER] Updated Registration {ugsPlayerId} → ClientId {clientId}");
        }
        else
        {
            ugsToClientId.Add(ugsPlayerId, clientId);
            Debug.Log($"[SERVER] Registered {ugsPlayerId} → ClientId {clientId}");
        }
    }


    // ---------------------------
    // SERVER SENDS ROLE → CLIENT
    // ---------------------------
    [ClientRpc]
    public void ReceiveRoleAndStatusClientRpc(string ugsPlayerId, string role, int hp,
        ClientRpcParams rpcParams = default)
    {
        
        if (IsServer) return; // Hosten skal ikke køre dette!
        
        
        if (ClientGameManager.Instance != null)
        {
            ClientGameManager.Instance.SetMyRole(role, hp);
        }
        else
        {
            Debug.LogError("CRITICAL: ClientGameManager.Instance is null! Make sure the script is in the scene.");
        }
    }
    

    public void SendRolesToClients()
    {
        foreach (Player p in GameManager.Instance.players)
        {
            string role = p.GetAssignedRole();

            // Only send RPC if we find the client in the dictionary
            if (ugsToClientId.TryGetValue(p.UGSPlayerId, out ulong targetClient))
            {
                var sendParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { targetClient }
                    }
                };
                ReceiveRoleAndStatusClientRpc(p.UGSPlayerId, role, p.hp, sendParams);
            }
        }
    }



    public void ChangeSceneForClientsOnly(string sceneName)
    {
        if (!IsServer) return;

        // Send an RPC to clients only
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
                continue; // skip the host

            var sendParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
            };

            LoadClientSceneClientRpc(sceneName, sendParams);
        }
    }

// RPC to tell clients to load a scene
    [ClientRpc]
    private void LoadClientSceneClientRpc(string sceneName, ClientRpcParams sendParams = default)
    {
        Debug.Log($"[CLIENT] Received command to load scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
    
    
    [ClientRpc]
    private void ReceiveNightSnapshotClientRpc(NightSnapshot snapshot)
    {
        if (ClientGameManager.Instance == null)
        {
            Debug.LogError("[CLIENT] ClientGameManager not found!");
            return;
        }

        ClientGameManager.Instance.StoreNightSnapshot(snapshot);
    }


    public void SendNightSnapshotToClients()
{
    if (!IsServer) return;

    GameManager gm = GameManager.Instance;

    NightSnapshot snapshot = new NightSnapshot
    {
        worldFlags = new WorldFlags
        {
            radioTowerGeneratorFixed = gm.radioTowerGeneratorFixed,
            radioTowerControlPanelFixed = gm.radioTowerControlPanelFixed,
            radioTowerAntennaFixed = gm.radioTowerAntennaFixed,

            dogDenied = gm.dogDenied,
            musicianIsAmalgam = gm.musicianIsAmalgam,
            strangerIsAmalgam = gm.strangerIsAmalgam,
            fatherDaughterIsAmalgam = gm.fatherDaughterIsAmalgam,
            banditsIsAmalgam = gm.banditsIsAmalgam,
            womanIsAmalgam = gm.womanIsAmalgam,

            pharmacyWomanAnnoyed = gm.pharmacyWomanAnnoyed
        },

        baseState = new BaseState
        {
            morale = gm.morale,
            zombieAwarenessOfBase = gm.zombieAwarenessOfBase,
            banditAwarenessOfBase = gm.banditAwarenessOfBase,
            baseFortification = gm.baseFortification,
            groupSanity = gm.groupSanity,
            banditKarma = gm.banditKarma,
            banditGroupForce = gm.banditGroupForce,
            bigBadStrengthLevel = gm.bigBadStrengthLevel,
            fatherDaughter = gm. fatherDaughter
        },

        inventory = new InventoryState
        {
            foodRations = gm.foodRations,
            medicalKits = gm.medicalKits,
            hasRadio = gm.hasRadio,
            hasRifle = gm.hasRifle,
            hasHandgun = gm.hasHandgun,
            hasTools = gm.hasTools,
            hasDogCompanion = gm.hasDogCompanion,
            playingCards = gm.playingCards,
            rifleBullets = gm.rifleBullets
        },

        roundGains = new RoundGains
        {
            foodRationsGainedThisRound = gm.foodRationsGainedThisRound,
            bulletsGainedThisRound = gm.bulletsGainedThisRound,
            medkitsGainedThisRound = gm.medkitsGainedThisRound,
            radioGainedThisRound = gm.radioGainedThisRound,
            rifleGainedThisRound = gm.rifleGainedThisRound,
            toolsGainedThisRound = gm.toolsGainedThisRound
        },

        players = new List<PlayerInfo>()
    };
    
    

    foreach (var p in gm.players)
    {
        snapshot.players.Add(new PlayerInfo
        {
            ugsId = p.UGSPlayerId,
            role = p.GetAssignedRole(),
            alive = p.hp > 0,
            hp = p.hp,
            name = p.name,
            isScavenging = p.scavengingSlot > 0,
            isPraying = p.isPraying,
            
                //items
            hasRifle = p.playerRifle, // Bemærk: Brug din rigtige variabel fra Player-klassen!
            hasDog = p.playerDog,     // f.eks. p.hasDogCompanion eller lignende.
            hasTools = p.playerTools,    
            bullets = p.lootedBullets
    
        });
        
    }

    Debug.Log($"[HOST] Sending NightSnapshot");

    ReceiveNightSnapshotClientRpc(snapshot);
}
    
    
    
    
    private IEnumerator RegisterAfterConnected()
    {
        while (!NetworkManager.Singleton.IsConnectedClient)
            yield return null;

        string ugsId = AuthenticationService.Instance.PlayerId;
        RegisterPlayerServerRpc(ugsId);
    }

    private HashSet<ulong> readyClients = new HashSet<ulong>();

    [ServerRpc(RequireOwnership = false)]
    public void ClientReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        readyClients.Add(clientId);

        Debug.Log($"[SERVER] Client ready: {clientId}");
    }

    
    
    
    [Rpc(SendTo.Server)]
    public void RollAmalgamsServerRpc(int totalPlayers)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        int maxAmalgams = Mathf.FloorToInt(totalPlayers / 2f);
        GameManager.Instance.totalReptiles =
            Random.Range(1, maxAmalgams + 1);

        Debug.Log($"[SERVER] Rolled amalgams: {GameManager.Instance.totalReptiles}");
    }
    
    
    [Rpc(SendTo.Server)]
    public void ClientReadyForRolesServerRpc()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        Debug.Log($"Client {OwnerClientId} is ready for roles");

        clientsReady++;

        if (clientsReady >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            SendRolesToClients();
        }
    }


    public void LoadScavengeSceneForPlayer(string ugsPlayerId, string sceneName)
    {
        if (!IsServer) return;

        // FIXED LOGIC:
        // 1. Try to find the client in the dictionary
        if (ugsToClientId.TryGetValue(ugsPlayerId, out ulong clientId))
        {
            Debug.Log($"[SERVER] Sending Client {clientId} (UGS: {ugsPlayerId}) to scene {sceneName}");
            var sendParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            };
            LoadClientSceneClientRpc(sceneName, sendParams);
        }
        // 2. If not in dictionary, check if it's the Host (Server) player
        else if (AuthenticationService.Instance.PlayerId == ugsPlayerId) 
        {
            Debug.Log($"[SERVER] Loading local scene {sceneName} for Host");
            SceneManager.LoadScene(sceneName);
        }
        // 3. Otherwise, it's an error (Missing Client) - Do NOT load scene locally
        else
        {
            Debug.LogError($"[SERVER] ERROR: Could not find ClientID for UGS ID: {ugsPlayerId}. Scene {sceneName} was NOT loaded.");
        }
    }

    
    
    //Role functions serverpc
    [ServerRpc(RequireOwnership = false)]
    public void SendMedicProtectServerRpc(
        string medicUgsId,
        string targetUgsId)
    {
        var medic = GameManager.Instance.players
            .FirstOrDefault(p => p.UGSPlayerId == medicUgsId);

        if (medic == null || !medic.isMedic)
        {
            Debug.LogWarning("Invalid medic RPC.");
            return;
        }

        var target = GameManager.Instance.players
            .FirstOrDefault(p => p.UGSPlayerId == targetUgsId);

        if (target == null || target.isDeceased)
            return;

        target.isProtected = true;
        medic.scavengingSlot = 0;
    }

    
    //amalgam
    [ServerRpc(RequireOwnership = false)]
    public void SendAmalgamKillServerRpc(string killerUgsId, string victimUgsId)
    {
        var killer = GameManager.Instance.players
            .FirstOrDefault(p => p.UGSPlayerId == killerUgsId && p.isMad);

        var victim = GameManager.Instance.players
            .FirstOrDefault(p => p.UGSPlayerId == victimUgsId);

        if (killer == null || victim == null || victim.isDeceased)
            return;

        victim.isVictim = true;
        GameManager.Instance.pendingKills.Add((killer, victim));
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendAmalgamSabotageServerRpc(string ugsId, string type)
    {
        switch (type)
        {
            case "Antenna": GameManager.Instance.radioTowerAntennaFixed = false; break;
            case "Generator": GameManager.Instance.radioTowerGeneratorFixed = false; break;
            case "ControlPanel": GameManager.Instance.radioTowerControlPanelFixed = false; break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendAmalgamLootServerRpc(string ugsId, string loot)
    {
        var p = GameManager.Instance.players
            .FirstOrDefault(x => x.UGSPlayerId == ugsId);

        if (p == null) return;

        switch (loot)
        {
            case "BigFood": p.lootedFoodRations += 20; break;
            case "SmallFood": p.lootedFoodRations += 10; break;
            case "Ammo": p.lootedBullets += 2; break;
            case "Medkit": p.lootedMedkit = true; break;
            case "Tools": p.lootedTools = true; break;
            case "Rifle":
                p.lootedRifle = true;
                p.lootedBullets += 2;
                break;
        }
    }


    
    [ServerRpc(RequireOwnership = false)]
    public void ClientFinishedScavengingServerRpc(string ugsPlayerId)
    {
        var p = GameManager.Instance.players
            .FirstOrDefault(x => x.UGSPlayerId == ugsPlayerId);

        if (p == null)
        {
            Debug.LogWarning($"[SERVER] Unknown scavenger finished: {ugsPlayerId}");
            return;
        }

        p.scavengingSlot = 0;
        p.isScavenging = false; // if you use this flag

        Debug.Log($"[SERVER] {p.name} finished scavenging.");
    }

    
    //veteran
    [ServerRpc(RequireOwnership = false)]
    public void SendVeteranKillServerRpc(string killerUgsId, string victimUgsId)
    {
        var killer = GameManager.Instance.players
            .FirstOrDefault(p => p.UGSPlayerId == killerUgsId && p.isVeteran);

        var victim = GameManager.Instance.players
            .FirstOrDefault(p => p.UGSPlayerId == victimUgsId);

        if (killer == null || victim == null || victim.isDeceased)
            return;

        victim.isVictim = true;
        GameManager.Instance.pendingVeteranKills.Add((killer, victim));
    }
    
    //martyr
    [ServerRpc(RequireOwnership = false)]
    public void SendMartyrPrayServerRpc(string martyrUgsId)
    {
        var martyr = GameManager.Instance.players
            .FirstOrDefault(p => p.UGSPlayerId == martyrUgsId && p.isMartyr);

        if (martyrUgsId == null)
            return;

        martyr.isPraying = true;
    }
    
    
    
    
    [System.Serializable]
    public struct ScavengeOutcomeData : INetworkSerializable
    {
        public int foodChange;
        public int medkitChange;
        public int bulletChange;
        public int zombieAwarenessChange;
        public bool pharmacyWomanAnnoyed; 
        public int hpChange; 
        public bool isDead;
        public bool foundTools;
        public bool foundRifle;
        public bool fixedGenerator;
        public bool fixedControlPanel;
        public bool fixedAntenna;
        public int banditKarmaChange;       // NY
        public int banditAwarenessChange;   // NY
        public int banditGroupForceChange;
        public int dayNumber;
        public bool resetFatherDaughter;
        public bool fixedVentilation;
        
        public int sanityChange; // Ny

        // Nye Amalgam Identity Flags
        public bool isMusicianAmalgam;
        public bool isStrangerAmalgam;
        public bool isFatherDaughterAmalgam;
        public bool isBanditsAmalgam;
        public bool isWomanAmalgam;
        
        public int fortificationChange; // Ny
        public bool foundRadio;         // Ny
        public bool foundDog;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref foodChange);
            serializer.SerializeValue(ref medkitChange);
            serializer.SerializeValue(ref bulletChange);
            serializer.SerializeValue(ref zombieAwarenessChange);
            serializer.SerializeValue(ref pharmacyWomanAnnoyed);
            serializer.SerializeValue(ref hpChange);
            serializer.SerializeValue(ref isDead);
            serializer.SerializeValue(ref foundTools);
            serializer.SerializeValue(ref foundRifle);
            serializer.SerializeValue(ref fixedGenerator);
            serializer.SerializeValue(ref fixedControlPanel);
            serializer.SerializeValue(ref fixedAntenna);
            serializer.SerializeValue(ref banditKarmaChange);
            serializer.SerializeValue(ref banditAwarenessChange);
            serializer.SerializeValue(ref banditGroupForceChange); // TILFØJ DENNE!
            serializer.SerializeValue(ref dayNumber); // TILFØJ DENNE!
            
            serializer.SerializeValue(ref sanityChange);
            serializer.SerializeValue(ref isMusicianAmalgam);
            serializer.SerializeValue(ref isStrangerAmalgam);
            serializer.SerializeValue(ref isFatherDaughterAmalgam);
            serializer.SerializeValue(ref isBanditsAmalgam);
            serializer.SerializeValue(ref isWomanAmalgam);
            
            serializer.SerializeValue(ref fortificationChange);
            serializer.SerializeValue(ref foundRadio);
            serializer.SerializeValue(ref resetFatherDaughter);
            serializer.SerializeValue(ref fixedVentilation);
    
            serializer.SerializeValue(ref foundDog); // TILFØJ DENNE!
        }
    }
    
    // ---------------------------
    // SCAVENGE RESULTS HANDLER
    // ---------------------------
    
    [ServerRpc(RequireOwnership = false)]
    public void ReportScavengeOutcomeServerRpc(string ugsPlayerId, ScavengeOutcomeData data)
    {
        // 1. Find the authoritative player object in the Server's GameManager
        Player p = GameManager.Instance.players.FirstOrDefault(x => x.UGSPlayerId == ugsPlayerId);

        if (p == null)
        {
            Debug.LogError($"[SERVER] Could not find player {ugsPlayerId} to apply scavenge results.");
            return;
        }

        Debug.Log($"[SERVER] Applying results for {p.name}: Food {data.foodChange}, HP {data.hpChange}");

        // 2. Apply Personal Inventory Changes
        p.lootedFoodRations += data.foodChange;
        p.lootedBullets += data.bulletChange;
        
        if (data.medkitChange > 0) p.lootedMedkit = true;

        if (data.fixedVentilation)
        {
            GameManager.Instance.ventilationJammed = false;
        }
        
        if (data.resetFatherDaughter) GameManager.Instance.fatherDaughter = 0;
        
        // 3. Apply Health Changes
        p.hp += data.hpChange;
        
        // Ensure HP doesn't go below 0 purely by math
        if (p.hp < 0) p.hp = 0;

        if (data.foundRifle)
        {
            p.lootedRifle = true;  // For the end-of-round summary
            p.playerRifle = true; // PERMANENT flag for the player
            GameManager.Instance.hasRifle = true; // GLOBAL flag for the base
        }
        if (data.foundTools)
        {
            p.lootedTools = true;
            p.playerTools = true; // PERMANENT
        }
        
        if (data.foundDog) p.playerDog = true;
        
        // 4. Handle Death Flag
        if (data.isDead && p.hp > 0)
        {
            p.hp = 0; // Force kill if logic dictated it
            GameManager.Instance.HandlePlayerDeath(p);
        }
        else if (p.hp == 0 && !p.isDeceased)
        {
            // If they died just from HP damage
            GameManager.Instance.HandlePlayerDeath(p);
        }

        // 5. Apply Global World Changes
        // We use += so multiple players don't overwrite each other
        GameManager.Instance.zombieAwarenessOfBase += data.zombieAwarenessChange;
        
        // Inde i metoden, hvor du anvender data på serveren:
        GameManager.Instance.banditKarma += data.banditKarmaChange;
        GameManager.Instance.banditAwarenessOfBase += data.banditAwarenessChange;

        if (data.pharmacyWomanAnnoyed)
        {
            GameManager.Instance.pharmacyWomanAnnoyed = true;
        }
        
        // Inde i ServerRpc:
        GameManager.Instance.baseFortification += data.fortificationChange;
        if (data.foundRadio) p.lootedRadio = true; // eller GameManager.Instance.hasRadio = true;
        
        GameManager.Instance.banditGroupForce += data.banditGroupForceChange;
        // 6. Mark them as finished
        // This reuses your existing logic to clear the slot
        ClientFinishedScavengingServerRpc(ugsPlayerId);
    }
    
}

