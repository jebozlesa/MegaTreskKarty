using System;
using System.Collections.Generic;

[Serializable]
public class LeaderboardStateResponse
{
    public bool success;
    public string mode;
    public List<string> availableModes;
    public string statisticName;
    public string currentPlayerId;
    public List<LeaderboardEntryDto> entries;
    public string error;
    public string stage;
}

[Serializable]
public class LeaderboardEntryDto
{
    public int rank;
    public string playerId;
    public string displayName;
    public int score;
}
