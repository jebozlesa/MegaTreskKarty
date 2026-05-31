using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CampaignOnlineGameBootstrap
{
    private const string BattleSceneName = "Game";
    private const string DefaultCampaignId = "bushido";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryBootstrap(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBootstrap(scene);
    }

    private static void TryBootstrap(Scene scene)
    {
        if (!scene.IsValid()
            || scene.name != BattleSceneName
            || GameParameters.MissionID <= 0)
        {
            return;
        }

        CampaignOnlineShellController existingController = Object.FindFirstObjectByType<CampaignOnlineShellController>();
        if (existingController != null)
        {
            ConfigureController(existingController);
            return;
        }

        RoyalRumbleShellController royalRumbleShell = Object.FindFirstObjectByType<RoyalRumbleShellController>();
        RoyalRumbleBattleCoordinator royalRumbleCoordinator = Object.FindObjectsByType<RoyalRumbleBattleCoordinator>(FindObjectsSortMode.None)
            .FirstOrDefault();
        FightSystem fightSystem = Object.FindObjectsByType<FightSystem>(FindObjectsSortMode.None).FirstOrDefault();
        FightSystemCampaign fightSystemCampaign = Object.FindObjectsByType<FightSystemCampaign>(FindObjectsSortMode.None)
            .FirstOrDefault();

        DisableLegacyBattleControllers();

        GameObject controllerObject = new GameObject("CampaignOnlineSmokeController");
        ServerFunctionsManager serverFunctionsManager = Object.FindFirstObjectByType<ServerFunctionsManager>()
            ?? controllerObject.AddComponent<ServerFunctionsManager>();

        CampaignOnlineService campaignService = controllerObject.AddComponent<CampaignOnlineService>();
        campaignService.serverFunctionsManager = serverFunctionsManager;

        CampaignOnlineBattlePlayback battlePlayback = controllerObject.AddComponent<CampaignOnlineBattlePlayback>();
        CampaignOnlineShellController controller = controllerObject.AddComponent<CampaignOnlineShellController>();
        controller.campaignService = campaignService;
        controller.battlePlayback = battlePlayback;

        CopySceneReferences(controller, battlePlayback, royalRumbleShell, royalRumbleCoordinator, fightSystem, fightSystemCampaign);
        ConfigureController(controller);

        Debug.LogWarning(
            $"[CampaignOnlineGameBootstrap] Routed Game scene to online Campaign smoke: campaign={controller.campaignId}, mission={controller.missionId}."
        );
    }

    private static void ConfigureController(CampaignOnlineShellController controller)
    {
        if (controller == null)
        {
            return;
        }

        controller.campaignId = ResolveCampaignId();
        controller.missionId = GameParameters.MissionID;
        controller.autoStart = true;
        controller.ReplaceSceneButtonListenersForOnlineCampaign();
    }

    private static string ResolveCampaignId()
    {
        if (GameParameters.CampaignID <= 0 || GameParameters.CampaignID == 1)
        {
            return DefaultCampaignId;
        }

        Debug.LogWarning(
            $"[CampaignOnlineGameBootstrap] Unknown CampaignID={GameParameters.CampaignID}; using {DefaultCampaignId} for online smoke."
        );
        return DefaultCampaignId;
    }

    private static void DisableLegacyBattleControllers()
    {
        foreach (RoyalRumbleShellController controller in Object.FindObjectsByType<RoyalRumbleShellController>(FindObjectsSortMode.None))
        {
            controller.enabled = false;
        }

        foreach (RoyalRumbleBattleCoordinator coordinator in Object.FindObjectsByType<RoyalRumbleBattleCoordinator>(FindObjectsSortMode.None))
        {
            coordinator.enabled = false;
        }

        foreach (FightSystem fightSystem in Object.FindObjectsByType<FightSystem>(FindObjectsSortMode.None))
        {
            fightSystem.enabled = false;
        }

        foreach (FightSystemCampaign fightSystemCampaign in Object.FindObjectsByType<FightSystemCampaign>(FindObjectsSortMode.None))
        {
            fightSystemCampaign.enabled = false;
        }
    }

    private static void CopySceneReferences(
        CampaignOnlineShellController controller,
        CampaignOnlineBattlePlayback battlePlayback,
        RoyalRumbleShellController royalRumbleShell,
        RoyalRumbleBattleCoordinator royalRumbleCoordinator,
        FightSystem fightSystem,
        FightSystemCampaign fightSystemCampaign)
    {
        if (controller == null)
        {
            return;
        }

        controller.player = royalRumbleShell != null ? royalRumbleShell.player : controller.player;
        controller.enemy = royalRumbleShell != null ? royalRumbleShell.enemy : controller.enemy;
        controller.playerLifeBar = royalRumbleShell != null ? royalRumbleShell.playerLifeBar : controller.playerLifeBar;
        controller.enemyLifeBar = royalRumbleShell != null ? royalRumbleShell.enemyLifeBar : controller.enemyLifeBar;
        controller.dialogText = royalRumbleShell != null ? royalRumbleShell.dialogText : controller.dialogText;
        controller.attackDescriptions = royalRumbleShell != null ? royalRumbleShell.attackDescriptions : controller.attackDescriptions;
        controller.cardPrefab = royalRumbleShell != null ? royalRumbleShell.cardPrefab : controller.cardPrefab;
        controller.playerBoard = royalRumbleShell != null ? royalRumbleShell.playerBoard : controller.playerBoard;
        controller.enemyBoard = royalRumbleShell != null ? royalRumbleShell.enemyBoard : controller.enemyBoard;

        if (royalRumbleCoordinator != null)
        {
            controller.player ??= royalRumbleCoordinator.player;
            controller.enemy ??= royalRumbleCoordinator.enemy;
            controller.playerLifeBar ??= royalRumbleCoordinator.playerLifeBar;
            controller.enemyLifeBar ??= royalRumbleCoordinator.enemyLifeBar;
            controller.dialogText ??= royalRumbleCoordinator.dialogText;
            controller.attackDescriptions ??= royalRumbleCoordinator.attackDescriptions;
            controller.cardPrefab ??= royalRumbleCoordinator.cardPrefab;
            controller.playerBoard ??= royalRumbleCoordinator.playerBoard;
            controller.enemyBoard ??= royalRumbleCoordinator.enemyBoard;
            controller.attackButton1 ??= royalRumbleCoordinator.attackButton1;
            controller.attackButton2 ??= royalRumbleCoordinator.attackButton2;
            controller.attackButton3 ??= royalRumbleCoordinator.attackButton3;
            controller.attackButton4 ??= royalRumbleCoordinator.attackButton4;
            controller.confirmButton ??= royalRumbleCoordinator.confirmButton;
            controller.attackButton1Text ??= royalRumbleCoordinator.attackButton1Text;
            controller.attackButton2Text ??= royalRumbleCoordinator.attackButton2Text;
            controller.attackButton3Text ??= royalRumbleCoordinator.attackButton3Text;
            controller.attackButton4Text ??= royalRumbleCoordinator.attackButton4Text;
            controller.attackButton1CountText ??= royalRumbleCoordinator.attackButton1CountText;
            controller.attackButton2CountText ??= royalRumbleCoordinator.attackButton2CountText;
            controller.attackButton3CountText ??= royalRumbleCoordinator.attackButton3CountText;
            controller.attackButton4CountText ??= royalRumbleCoordinator.attackButton4CountText;
        }

        if (fightSystem != null)
        {
            controller.player ??= fightSystem.player;
            controller.enemy ??= fightSystem.enemy;
            controller.playerLifeBar ??= fightSystem.playerLifeBar;
            controller.enemyLifeBar ??= fightSystem.enemyLifeBar;
            controller.dialogText ??= fightSystem.dialogText;
            controller.attackDescriptions ??= fightSystem.attackDescriptions;
            controller.cardPrefab ??= fightSystem.kartaPrefab;
            controller.playerBoard ??= fightSystem.playerBoard;
            controller.enemyBoard ??= fightSystem.enemyBoard;
            controller.attackButton1 ??= fightSystem.button1;
            controller.attackButton2 ??= fightSystem.button2;
            controller.attackButton3 ??= fightSystem.button3;
            controller.attackButton4 ??= fightSystem.button4;
            controller.attackButton1Text ??= fightSystem.button1Text;
            controller.attackButton2Text ??= fightSystem.button2Text;
            controller.attackButton3Text ??= fightSystem.button3Text;
            controller.attackButton4Text ??= fightSystem.button4Text;
            controller.attackButton1CountText ??= fightSystem.button1CountText;
            controller.attackButton2CountText ??= fightSystem.button2CountText;
            controller.attackButton3CountText ??= fightSystem.button3CountText;
            controller.attackButton4CountText ??= fightSystem.button4CountText;
        }

        if (fightSystemCampaign != null)
        {
            controller.player ??= fightSystemCampaign.player;
            controller.enemy ??= fightSystemCampaign.enemy;
            controller.playerLifeBar ??= fightSystemCampaign.playerLifeBar;
            controller.enemyLifeBar ??= fightSystemCampaign.enemyLifeBar;
            controller.dialogText ??= fightSystemCampaign.dialogText;
            controller.attackDescriptions ??= fightSystemCampaign.attackDescriptions;
            controller.cardPrefab ??= fightSystemCampaign.kartaPrefab;
            controller.playerBoard ??= fightSystemCampaign.playerBoard;
            controller.enemyBoard ??= fightSystemCampaign.enemyBoard;
            controller.attackButton1 ??= fightSystemCampaign.button1;
            controller.attackButton2 ??= fightSystemCampaign.button2;
            controller.attackButton3 ??= fightSystemCampaign.button3;
            controller.attackButton4 ??= fightSystemCampaign.button4;
            controller.attackButton1Text ??= fightSystemCampaign.button1Text;
            controller.attackButton2Text ??= fightSystemCampaign.button2Text;
            controller.attackButton3Text ??= fightSystemCampaign.button3Text;
            controller.attackButton4Text ??= fightSystemCampaign.button4Text;
        }

        if (battlePlayback != null)
        {
            battlePlayback.playerLifeBar = controller.playerLifeBar;
            battlePlayback.enemyLifeBar = controller.enemyLifeBar;
            battlePlayback.dialogText = controller.dialogText;
            battlePlayback.attackComponent = Object.FindFirstObjectByType<Attack>();
            battlePlayback.cardAnimator = Object.FindFirstObjectByType<MultiplayerCardAnimator>();
        }
    }
}
