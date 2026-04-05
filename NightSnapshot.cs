using Unity.Netcode;
using System.Collections.Generic;
using Unity.Services.Authentication;



[System.Serializable]
public struct PlayerInfo : INetworkSerializable
{
    public string ugsId;
    public string role;
    public bool alive;
    public int hp;
    public bool isScavenging;
    public string name;
    
    public bool hasRifle;
    public bool hasDog;
    public bool hasTools;
    public int bullets;
    public bool isPraying;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref ugsId);
        serializer.SerializeValue(ref role);
        serializer.SerializeValue(ref alive);
        serializer.SerializeValue(ref hp);
        serializer.SerializeValue(ref isScavenging);
        serializer.SerializeValue(ref name);
        
        // --- TILFØJET: Nu pakkes dine items rent faktisk ned og sendes! ---
        serializer.SerializeValue(ref hasRifle);
        serializer.SerializeValue(ref hasDog);
        serializer.SerializeValue(ref hasTools);
        serializer.SerializeValue(ref bullets);
    }
}



[System.Serializable]
public struct WorldFlags : INetworkSerializable
{
    public bool radioTowerGeneratorFixed;
    public bool radioTowerControlPanelFixed;
    public bool radioTowerAntennaFixed;

    public bool dogDenied;
    public bool musicianIsAmalgam;
    public bool strangerIsAmalgam;
    public bool fatherDaughterIsAmalgam;
    public bool banditsIsAmalgam;
    public bool womanIsAmalgam;
    public bool dayNumber;
    
    
    public bool pharmacyWomanAnnoyed;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref radioTowerGeneratorFixed);
        serializer.SerializeValue(ref radioTowerControlPanelFixed);
        serializer.SerializeValue(ref radioTowerAntennaFixed);

        serializer.SerializeValue(ref dogDenied);
        serializer.SerializeValue(ref musicianIsAmalgam);
        serializer.SerializeValue(ref strangerIsAmalgam);
        serializer.SerializeValue(ref fatherDaughterIsAmalgam);
        serializer.SerializeValue(ref banditsIsAmalgam);
        serializer.SerializeValue(ref womanIsAmalgam);
        serializer.SerializeValue(ref dayNumber);

        serializer.SerializeValue(ref pharmacyWomanAnnoyed);
        
    }
}

[System.Serializable]
public struct BaseState : INetworkSerializable
{
    public int zombieAwarenessOfBase;
    public int banditAwarenessOfBase;
    public int baseFortification;
    public int groupSanity;
    public int banditKarma;
    public int banditGroupForce;
    public int morale;
    public int bigBadStrengthLevel;
    public int dayNumber;
    public int fatherDaughter;
    

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref zombieAwarenessOfBase);
        serializer.SerializeValue(ref banditAwarenessOfBase);
        serializer.SerializeValue(ref baseFortification);
        serializer.SerializeValue(ref groupSanity);
        serializer.SerializeValue(ref banditKarma);
        serializer.SerializeValue(ref banditGroupForce);
        serializer.SerializeValue(ref morale);
        serializer.SerializeValue(ref bigBadStrengthLevel);
        serializer.SerializeValue(ref fatherDaughter);
    }
}

[System.Serializable]
public struct InventoryState : INetworkSerializable
{
    public int foodRations;
    public int medicalKits;

    public bool hasRadio;
    public bool hasRifle;
    public bool hasHandgun;
    public bool hasTools;
    public bool hasDogCompanion;
    public bool playingCards;

    public int rifleBullets;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref foodRations);
        serializer.SerializeValue(ref medicalKits);

        serializer.SerializeValue(ref hasRadio);
        serializer.SerializeValue(ref hasRifle);
        serializer.SerializeValue(ref hasHandgun);
        serializer.SerializeValue(ref hasTools);
        serializer.SerializeValue(ref hasDogCompanion);
        serializer.SerializeValue(ref playingCards);

        serializer.SerializeValue(ref rifleBullets);
    }
}

[System.Serializable]
public struct RoundGains : INetworkSerializable
{
    public int foodRationsGainedThisRound;
    public int bulletsGainedThisRound;
    public int medkitsGainedThisRound;

    public bool radioGainedThisRound;
    public bool rifleGainedThisRound;
    public bool toolsGainedThisRound;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref foodRationsGainedThisRound);
        serializer.SerializeValue(ref bulletsGainedThisRound);
        serializer.SerializeValue(ref medkitsGainedThisRound);

        serializer.SerializeValue(ref radioGainedThisRound);
        serializer.SerializeValue(ref rifleGainedThisRound);
        serializer.SerializeValue(ref toolsGainedThisRound);
    }
}



[System.Serializable]
public struct NightSnapshot : INetworkSerializable
{
    // --- Global game state ---
    public WorldFlags worldFlags;
    public BaseState baseState;
    public InventoryState inventory;
    public RoundGains roundGains;

    // --- Per-player state ---
    public List<PlayerInfo> players;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        // Global state
        serializer.SerializeValue(ref worldFlags);
        serializer.SerializeValue(ref baseState);
        serializer.SerializeValue(ref inventory);
        serializer.SerializeValue(ref roundGains);

        // Player list
        int count = players == null ? 0 : players.Count;
        serializer.SerializeValue(ref count);

        if (serializer.IsWriter)
        {
            for (int i = 0; i < count; i++)
            {
                PlayerInfo info = players[i];
                serializer.SerializeValue(ref info);
            }
        }
        else
        {
            players = new List<PlayerInfo>(count);
            for (int i = 0; i < count; i++)
            {
                PlayerInfo info = default;
                serializer.SerializeValue(ref info);
                players.Add(info);
            }
        }
    }
}