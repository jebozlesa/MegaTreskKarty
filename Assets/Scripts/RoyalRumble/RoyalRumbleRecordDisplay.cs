using System;

public static class RoyalRumbleRecordDisplay
{
    public static int ResolveVisibleRecord(
        int currentVisibleRecord,
        RoyalRumbleSessionDto session,
        RoyalRumbleRecordSyncDto recordSync = null)
    {
        int visibleRecord = Math.Max(0, currentVisibleRecord);
        RoyalRumbleProgressDto progress = session?.progress;

        if (progress != null)
        {
            visibleRecord = Math.Max(visibleRecord, progress.defeatedEnemyCount);
            visibleRecord = Math.Max(visibleRecord, progress.bestSubmittedScore);
            visibleRecord = Math.Max(visibleRecord, progress.pendingRecordScore ?? 0);
        }

        if (recordSync != null)
        {
            visibleRecord = Math.Max(visibleRecord, recordSync.score);
        }

        return visibleRecord;
    }
}
