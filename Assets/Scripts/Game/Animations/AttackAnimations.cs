using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AttackAnimations : MonoBehaviour
{
    public MoveImage moveImageAnimation;                                //klasika od karty ku karte
    public EnlargeImage enlargeImageAnimation;
    public RandomImageSpawner randomImageSpawner;
    public DualImageAnimation dualImageAnimation;
    public RandomMoveImage randomMoveImageAnimation;
    public ShootAnimation shootAnimation;
    public BowShootAnimation bowShootAnimation;
    public DualMoveImageAnimation dualMoveImageAnimation;
    public WirelessChargerAnimation wirelessChargerAnimation;
    public MachineGunAnimation machineGunAnimation;
    public CorruptionAnimation corruptionAnimation;
    public EnvelopAnimation envelopAnimation;
    public BlocadeAnimation blocadeAnimation;
    public DepressionAnimation depressionAnimation;
    public BleedAnimation bleedAnimation;
    public AutoportraitAnimation autoportraitAnimation;
    public GravityPullAnimation gravityPullAnimation;
    public KamikazeAnimation kamikazeAnimation;
    public AirStrikeAnimation airStrikeAnimation;
    public RotatingEnlargeAnimation rotatingEnlargeAnimation;
    public RisingEnlargeImage risingEnlargeImageAnimation;
    public AmbushAnimation ambushAnimation;
    public RocketLaunchAnimation rocketLaunchAnimation;
    public SatelliteFlyoverAnimation satelliteFlyoverAnimation;
    public FlagWaveAnimation flagWaveAnimation;
    public MidpointRotatingEnlargeAnimation midpointRotatingEnlargeAnimation;


    //1
    public IEnumerator PlayPunchAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/punch");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/punch");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.7f,
                startSound: startSound,
                initialRotation: 0f, // napr. -15 stupňov na začiatku
                finalRotation: 0f     // napr. 15 stupňov na konci
            )
        );
    }

    //2
    public IEnumerator PlayKickAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/kick");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/kick");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.7f,
                startSound: startSound,
                initialRotation: 0f, // napr. -15 stupňov na začiatku
                finalRotation: 180f     // napr. 15 stupňov na konci
            )
        );
    }

    //3
    public IEnumerator PlayHealAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[1];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/heal");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/heal");

        Vector2 initialSize = new Vector2(50f, 50f); // Počiatočná veľkosť obrázka
        Vector2 finalSize = new Vector2(100f, 100f);   // Konečná veľkosť obrázka

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 3,
                totalImages: 20,
                startSize: initialSize,
                endSize: finalSize,
                soundEffect: effectSound
            )
        );
    }

    //4
    public IEnumerator PlayForgivenessAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/forgiveness");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/forgiveness");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                soundEffect: sound
            )
        );
    }

    //5
    public IEnumerator PlayCrusadeAnimation(Transform attacker, Transform receiver)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/crusade");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/crusade");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: sprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 1f,
                startSound: sound,
                initialRotation: 0f,
                finalRotation: 0f,
                rotateTowardsTarget: false
            )
        );
    }

    //6
    public IEnumerator PlayWaterToWineAnimation(Transform targetCard)
    {
        Sprite waterSprite = Resources.Load<Sprite>("Game/Animations/watertowine1");
        Sprite wineSprite = Resources.Load<Sprite>("Game/Animations/watertowine2");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/watertowine");

        yield return StartCoroutine(
            dualImageAnimation.StartAnimation(
                sprite1: waterSprite,
                sprite2: wineSprite,
                position: targetCard,
                imageSize: new Vector2(300f, 300f), // Predpokladaná veľkosť pohára
                duration: 1f, // Celková doba trvania animácie
                startSound: startSound,
                switchRatio: 0.5f // V polovici animácie sa zmení na fľašu vína
            )
        );
    }

    //7
    public IEnumerator PlayCarHitAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/carhit");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/carhit");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.7f,
                startSound: startSound,
                initialRotation: 0f, // napr. -15 stupňov na začiatku
                finalRotation: 90f     // napr. 15 stupňov na konci
            )
        );
    }

    //8
    public IEnumerator PlayMonkeyWrenchAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/monkeywrench");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/monkeywrench");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 100f,
                finalRotation: 0f
            )
        );
    }

    //9
    public IEnumerator PlayRadioactivityAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/radiation");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/radiation");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                soundEffect: effectSound
            )
        );
    }

    //10
    public IEnumerator PlayScratchAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/scratch");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/scratch");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                soundEffect: effectSound
            )
        );
    }

    //11
    public IEnumerator PlayScientificLectureAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[5];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/scientificlecture1");
        sprites[1] = Resources.Load<Sprite>("Game/Animations/scientificlecture2");
        sprites[2] = Resources.Load<Sprite>("Game/Animations/scientificlecture3");
        sprites[3] = Resources.Load<Sprite>("Game/Animations/scientificlecture4");
        sprites[4] = Resources.Load<Sprite>("Game/Animations/scientificlecture5");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/scientificlecture");

        Vector2 initialSize = new Vector2(50f, 50f); // Počiatočná veľkosť obrázka
        Vector2 finalSize = new Vector2(300f, 300f);   // Konečná veľkosť obrázka

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 1,
                totalImages: 3,
                startSize: initialSize,
                endSize: finalSize,
                imageLifetime: 0.5f,
                soundEffect: effectSound
            )
        );
    }

    //12
    public IEnumerator PlayChiSauAnimation(Transform sourceCard, Transform targetCard, int numberOfAttacks)
    {
        Sprite bowSprite = Resources.Load<Sprite>("Game/Animations/chisau");
        Sprite arrowSprite = Resources.Load<Sprite>("Game/Animations/chisau");
        AudioClip attackSound = Resources.Load<AudioClip>("Sounds/Game/Animations/chisau");

        //int numberOfAttacks = Random.Range(1, 6); // Generuje náhodné číslo medzi 1 a 5 vrátane.

        yield return StartCoroutine(
            bowShootAnimation.StartShootAnimation(
                bowSprite: bowSprite,
                arrowSprite: arrowSprite,
                shooterCard: sourceCard,
                targetCard: targetCard,
                imageSize: new Vector2(0f, 0f), // Veľkosť obrázka luku
                duration: 1f, // Trvanie animácie
                shootSound: attackSound,
                showHitImage: true, // Zobrazenie šípu
                rotateToTarget: false, // Otočenie smerom k cieľu
                arrowImageSize: new Vector2(250f, 250f), // Veľkosť obrázka šípu
                arrowCount: numberOfAttacks, // Počet šípov
                arrowInterval: 0.1f, // Interval medzi šípmi
                arrowFlightDuration: 0.3f // Čas letu šípu
            )
        );

        // yield return StartCoroutine(
        //     randomMoveImageAnimation.StartRandomMoveAnimation(
        //         sprite: attackSprite,
        //         sourceCard: sourceCard,
        //         targetCard: targetCard,
        //         cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty
        //         numberOfAttacks: numberOfAttacks,
        //         imageSize: new Vector2(250f, 250f),  // Veľkosť obrázka útoku
        //         duration: 1f,
        //         startSound: attackSound
        //     )
        // );
    }

    //13
    public IEnumerator PlayOneInchPunchAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/oneinchpunch");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/oneinchpunch");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250, 250),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 0f,
                finalRotation: 0f
            )
        );
    }

    //14
    public IEnumerator PlayUpInSmokeAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[3];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/upinsmoke1");
        sprites[1] = Resources.Load<Sprite>("Game/Animations/upinsmoke2");
        sprites[2] = Resources.Load<Sprite>("Game/Animations/upinsmoke3");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/upinsmoke");

        Vector2 initialSize = new Vector2(50f, 50f); // Počiatočná veľkosť obrázka
        Vector2 finalSize = new Vector2(300f, 300f);   // Konečná veľkosť obrázka

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 1,
                totalImages: 4,
                startSize: initialSize,
                endSize: finalSize,
                imageLifetime: 0.5f,
                soundEffect: effectSound
            )
        );
    }

    //15
    public IEnumerator PlaySingAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/sing");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/sing");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(400f, 400f),
                duration: 1f,
                soundEffect: effectSound
            )
        );
    }

    //16
    public IEnumerator PlayRevolverAnimation(Transform shooterCard, Transform targetCard, bool hit)
    {
        Sprite shooterSprite = Resources.Load<Sprite>("Game/Animations/revolver1");
        Sprite hitSprite = Resources.Load<Sprite>("Game/Animations/revolver2");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/revolver");

        yield return StartCoroutine(
            shootAnimation.StartShootAnimation(
                shooterSprite: shooterSprite,
                hitSprite: hitSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(300f, 300f),
                duration: 1f,
                shootRatio: 0.5f,
                shootSound: shootSound,
                recoilAngle: 10f,
                showHitImage: hit
            )
        );
    }

    //17
    public IEnumerator PlayArtilleryRegimentAnimation(Transform shooterCard, Transform targetCard, bool hit)
    {
        Sprite shooterSprite = Resources.Load<Sprite>("Game/Animations/artilleryregiment1");
        Sprite hitSprite = Resources.Load<Sprite>("Game/Animations/artilleryregiment2");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/artilleryregiment");

        yield return StartCoroutine(
            shootAnimation.StartShootAnimation(
                shooterSprite: shooterSprite,
                hitSprite: hitSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(400f, 400f),
                duration: 1f,
                shootRatio: 0.5f,
                shootSound: shootSound,
                recoilAngle: 10f,
                showHitImage: hit,
                rotateToTarget: false
            )
        );
    }

    //18
    public IEnumerator PlayBloodSuckingAnimation(Transform attacker, Transform receiver) //tu je receiver a attecker trochu naopak aby som to nemenil cele
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/bloodsucking");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/bloodsucking");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250, 250),
                duration: 1f,
                startSound: startSound,
                initialRotation: 0f, // napr. -15 stupňov na začiatku
                finalRotation: 0f     // napr. 15 stupňov na konci
            )
        );
    }

    //19
    public IEnumerator PlaySwordAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/sword");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/sword");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 100f, // napr. -15 stupňov na začiatku
                finalRotation: -15f     // napr. 15 stupňov na konci
            )
        );
    }

    //20
    public IEnumerator PlayPikeAnimation(Transform sourceCard, Transform targetCard)
    {
        Sprite attackSprite = Resources.Load<Sprite>("Game/Animations/pike");
        AudioClip attackSound = Resources.Load<AudioClip>("Sounds/Game/Animations/pike");

        int numberOfAttacks = 5; // Generuje náhodné číslo medzi 1 a 5 vrátane.

        yield return StartCoroutine(
            randomMoveImageAnimation.StartRandomMoveAnimation(
                sprite: attackSprite,
                sourceCard: sourceCard,
                targetCard: targetCard,
                cardSize: new Vector2(100f, 200f), // Predpokladaná veľkosť karty
                numberOfAttacks: numberOfAttacks,
                imageSize: new Vector2(350f, 350f),  // Veľkosť obrázka útoku
                duration: 1f,
                startSound: attackSound
            )
        );
    }

    //21
    public IEnumerator PlayTerrifyAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/terrify");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/terrify");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(400f, 400f),
                duration: 1f,
                soundEffect: effectSound
            )
        );
    }

    //21
    public IEnumerator PlayAnimationTerrified(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/terrified");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/terrified");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(400f, 400f),
                duration: 1f,
                soundEffect: effectSound
            )
        );
    }

    //22
    public IEnumerator PlayDrinkWineAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/drinkwine");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/drinkwine");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(400f, 400f),
                endSize: new Vector2(400f, 400f),
                duration: 1f,
                endRotation: 180f,
                soundEffect: effectSound
            )
        );
    }

    //23
    public IEnumerator PlayFlamingGunAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/flaminggun");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/flaminggun");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 1f,
                startSound: startSound,
                initialRotation: 0f, // napr. -15 stupňov na začiatku
                finalRotation: 0f,     // napr. 15 stupňov na konci
                rotateTowardsTarget: false
            )
        );
    }

    //24
    public IEnumerator PlayCleaverAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/cleaver");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/cleaver");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.7f,
                startSound: startSound,
                initialRotation: 90f, // napr. -15 stupňov na začiatku
                finalRotation: 0f     // napr. 15 stupňov na konci
            )
        );
    }

    //25
    public IEnumerator PlayPanAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/pan");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/pan");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 0f, // napr. -15 stupňov na začiatku
                finalRotation: 180f     // napr. 15 stupňov na konci
            )
        );
    }

    //26
    public IEnumerator PlayBoostAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[4];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/boost1");
        sprites[1] = Resources.Load<Sprite>("Game/Animations/boost2");
        sprites[2] = Resources.Load<Sprite>("Game/Animations/boost3");
        sprites[3] = Resources.Load<Sprite>("Game/Animations/boost4");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/boost");

        Vector2 initialSize = new Vector2(100f, 100f); // Počiatočná veľkosť obrázka
        Vector2 finalSize = new Vector2(150f, 150f);   // Konečná veľkosť obrázka

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(200f, 400f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 2,
                totalImages: 15,
                startSize: initialSize,
                endSize: finalSize,
                imageLifetime: 0.3f,
                soundEffect: effectSound
            )
        );
    }

    //27
    public IEnumerator PlayTemptationAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[5];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/temptation1");
        sprites[1] = Resources.Load<Sprite>("Game/Animations/temptation2");
        sprites[2] = Resources.Load<Sprite>("Game/Animations/temptation3");
        sprites[3] = Resources.Load<Sprite>("Game/Animations/temptation4");
        sprites[4] = Resources.Load<Sprite>("Game/Animations/temptation5");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/temptation");

        Vector2 initialSize = new Vector2(50f, 50f); // Počiatočná veľkosť obrázka
        Vector2 finalSize = new Vector2(150f, 150f);   // Konečná veľkosť obrázka

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 3,
                totalImages: 20,
                startSize: initialSize,
                endSize: finalSize,
                imageLifetime: 0.3f,
                soundEffect: effectSound
            )
        );
    }

    //28
    public IEnumerator PlayShamshirAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/shamshir");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/shamshir");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 100f, // napr. -15 stupňov na začiatku
                finalRotation: -15f     // napr. 15 stupňov na konci
            )
        );
    }

    //29
    public IEnumerator PlayDiplomacyAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/diplomacy");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/diplomacy");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 1f,
                startSound: startSound,
                initialRotation: 0f, // napr. -15 stupňov na začiatku
                finalRotation: 0f,     // napr. 15 stupňov na konci
                rotateTowardsTarget: false
            )
        );
    }

    //30
    public IEnumerator PlaySiegeAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/siegetower");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/siegetower");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(400f, 400f),
                duration: 0.7f,
                soundEffect: effectSound
            )
        );
    }

    //31
    public IEnumerator PlayTreeStratagemAnimation(Transform targetCard)
    {
        Sprite waterSprite = Resources.Load<Sprite>("Game/Animations/treestratagem1");
        Sprite wineSprite = Resources.Load<Sprite>("Game/Animations/treestratagem2");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/treestratagem");

        yield return StartCoroutine(
            dualImageAnimation.StartAnimation(
                sprite1: waterSprite,
                sprite2: wineSprite,
                position: targetCard,
                imageSize: new Vector2(300f, 300f), // Predpokladaná veľkosť pohára
                duration: 1f, // Celková doba trvania animácie
                startSound: startSound,
                switchRatio: 0.3f // V polovici animácie sa zmení na fľašu vína
            )
        );
    }

    //32
    public IEnumerator PlayTomahawkAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/tomahawk");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/tomahawk");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 100f, // napr. -15 stupňov na začiatku
                finalRotation: -15f     // napr. 15 stupňov na konci
            )
        );
    }

    //34
    public IEnumerator PlayRecurveBowAnimation(Transform shooterCard, Transform targetCard, bool hit)
    {
        Sprite bowSprite = Resources.Load<Sprite>("Game/Animations/recurvebow");
        Sprite arrowSprite = Resources.Load<Sprite>("Game/Animations/arrow");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/recurvebow");

        yield return StartCoroutine(
            bowShootAnimation.StartShootAnimation(
                bowSprite: bowSprite,
                arrowSprite: arrowSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(350f, 350f), // Veľkosť obrázka luku
                duration: 1f, // Trvanie animácie
                shootSound: shootSound,
                showHitImage: hit, // Zobrazenie šípu
                rotateToTarget: true, // Otočenie smerom k cieľu
                arrowImageSize: new Vector2(250f, 250f) // Veľkosť obrázka šípu
            )
        );
    }

    //35
    public IEnumerator PlayFuryAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/fury");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/fury");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                soundEffect: effectSound
            )
        );
    }

    //36
    public IEnumerator PlayGuerillaAnimation(Transform attacker, Transform receiver)
    {
        Sprite sprite1 = Resources.Load<Sprite>("Game/Animations/guerilla1");
        Sprite sprite2 = Resources.Load<Sprite>("Game/Animations/guerilla2");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/guerilla");

        yield return StartCoroutine(
            dualMoveImageAnimation.StartAnimation(
                sprite1: sprite1,
                sprite2: sprite2,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(305f, 350f),
                duration: 1f,
                startSound: startSound,
                switchRatio: 0.5f, // V 30% animácie sa zmení obrázok
                                   //switchEffectDuration: 0.2f, // Trvanie efektu zmeny
                initialRotation: 0f, // Začiatočná rotácia -15 stupňov
                finalRotation: 0f,   // Konečná rotácia 15 stupňov
                rotateTowardsTarget: true
            )
        );
    }

    //37
    public IEnumerator PlayFamineAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[5];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/famine1");
        sprites[1] = Resources.Load<Sprite>("Game/Animations/famine2");
        sprites[2] = Resources.Load<Sprite>("Game/Animations/famine3");
        sprites[3] = Resources.Load<Sprite>("Game/Animations/famine4");
        sprites[4] = Resources.Load<Sprite>("Game/Animations/famine5");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/famine");

        Vector2 initialSize = new Vector2(150f, 150f); // Počiatočná veľkosť obrázka
        Vector2 finalSize = new Vector2(250f, 250f);   // Konečná veľkosť obrázka

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 1,
                totalImages: 20,
                startSize: initialSize,
                endSize: finalSize,
                imageLifetime: 1f,
                soundEffect: effectSound
            )
        );
    }

    //38
    public IEnumerator PlayMarxismAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[5];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/marxism1");
        sprites[1] = Resources.Load<Sprite>("Game/Animations/marxism2");
        sprites[2] = Resources.Load<Sprite>("Game/Animations/marxism3");
        sprites[3] = Resources.Load<Sprite>("Game/Animations/marxism4");
        sprites[4] = Resources.Load<Sprite>("Game/Animations/marxism5");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/marxism");

        Vector2 initialSize = new Vector2(50f, 50f); // Počiatočná veľkosť obrázka
        Vector2 finalSize = new Vector2(300f, 300f);   // Konečná veľkosť obrázka

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(250f, 400), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 1,
                totalImages: 3,
                startSize: initialSize,
                endSize: finalSize,
                imageLifetime: 1f,
                soundEffect: effectSound
            )
        );
    }

    //39
    public IEnumerator PlayTeslaCoilAnimation(Transform shooterCard, Transform targetCard)
    {
        // Načítanie zdrojov pre animáciu
        Sprite bowSprite = Resources.Load<Sprite>("Game/Animations/teslacoil1");
        Sprite arrowSprite = Resources.Load<Sprite>("Game/Animations/teslacoil2");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/teslacoil");

        // Spustenie animácie výstrelu šípu
        yield return StartCoroutine(
            bowShootAnimation.StartShootAnimation(
                bowSprite: bowSprite,
                arrowSprite: arrowSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(350f, 350f), // Veľkosť obrázka luku
                duration: 1f, // Trvanie animácie
                shootSound: shootSound,
                showHitImage: true, // Zobrazenie šípu
                rotateToTarget: false, // Otočenie smerom k cieľu
                arrowImageSize: new Vector2(250f, 250f), // Veľkosť obrázka šípu
                arrowCount: 10, // Počet šípov
                arrowInterval: 0.1f, // Interval medzi šípmi
                arrowFlightDuration: 0.3f // Čas letu šípu
            )
        );
    }

    //40
    public IEnumerator PlayWirelessChargerAnimation(Transform position)
    {
        Sprite sprite1 = Resources.Load<Sprite>("Game/Animations/wirelessCharger");
        Sprite sprite2 = Resources.Load<Sprite>("Game/Animations/wirelessCharger");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/wirelessCharger");

        yield return StartCoroutine(
            wirelessChargerAnimation.StartAnimation(
                sprite1: sprite1,
                sprite2: sprite2,
                position: position,
                imageSize: new Vector2(300f, 300f),
                duration: 1f,
                startSound: startSound,
                switchRatio: 0.3f,
                fadeDurationRatio: 0.4f
            )
        );
    }

    //41
    public IEnumerator PlayExperimentAnimation(Transform targetCard)
    {
        Sprite sprite1 = Resources.Load<Sprite>("Game/Animations/experiment1");
        Sprite sprite2 = Resources.Load<Sprite>("Game/Animations/experiment2");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/experiment");

        yield return StartCoroutine(
            dualImageAnimation.StartAnimation(
                sprite1: sprite1,
                sprite2: sprite2,
                position: targetCard,
                imageSize: new Vector2(300f, 300f), // Predpokladaná veľkosť pohára
                duration: 1f, // Celková doba trvania animácie
                startSound: startSound,
                switchRatio: 0.5f // V polovici animácie sa zmení na fľašu vína
            )
        );
    }

    //42
    public IEnumerator PlayTommyGunAnimation(Transform shooterCard, Transform targetCard, int hits)
    {
        Sprite shooterSprite = Resources.Load<Sprite>("Game/Animations/tommygun1");
        Sprite hitSprite = Resources.Load<Sprite>("Game/Animations/tommygun2");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/tommygun");

        yield return StartCoroutine(
            machineGunAnimation.StartShootAnimation(
                shooterSprite: shooterSprite,
                hitSprite: hitSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(400f, 400f),
                duration: 1f,
                shootRatio: 0.2f,
                shootRatioEnd: 0.7f,
                shootSound: shootSound,
                recoilAngle: 5f,
                showHitImageCount: hits, // Napríklad 5 zásahov
                rotateToTarget: true,
                hitImageSize: new Vector2(150f, 150f)
            )
        );
    }

    //43
    public IEnumerator PlayTieUpAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/tieup");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/tieup");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 0f, // napr. -15 stupňov na začiatku
                finalRotation: 0f     // napr. 15 stupňov na konci
            )
        );
    }

    //43
    public IEnumerator PlayAnimationTiedUp(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/tiedup");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/tiedup");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.5f,
                soundEffect: effectSound
            )
        );
    }

    //44
    public IEnumerator PlayCorruptionAnimation(Transform targetCard, int mulitplier)
    {
        Sprite[] sprites = new Sprite[]
        {
            Resources.Load<Sprite>("Game/Animations/corruption1"),
            Resources.Load<Sprite>("Game/Animations/corruption2"),
            Resources.Load<Sprite>("Game/Animations/corruption3"),
            Resources.Load<Sprite>("Game/Animations/corruption4"),
            Resources.Load<Sprite>("Game/Animations/corruption5"),
        };
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/corruption");

        yield return StartCoroutine(
            corruptionAnimation.StartAnimation(
                sprites: sprites,
                soundEffect: soundEffect,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 300f), // Predpokladaná veľkosť karty
                duration: 1f, // Celková doba trvania animácie
                numberOfImages: mulitplier * 5, // Number of images you want to spawn
                imageSize: new Vector2(200f, 200f), // Size of the generated images
                scatterSpeed: 400f // Speed at which the images scatter
            )
        );
    }

    //45
    public IEnumerator PlayColt1911Animation(Transform shooterCard, Transform targetCard, bool hit)
    {
        Sprite shooterSprite = Resources.Load<Sprite>("Game/Animations/colt1911");
        Sprite hitSprite = Resources.Load<Sprite>("Game/Animations/colt1911hole");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/colt1911");

        yield return StartCoroutine(
            shootAnimation.StartShootAnimation(
                shooterSprite: shooterSprite,
                hitSprite: hitSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(300f, 300f),
                hitImageSize: new Vector2(100f, 100f),
                duration: 1f,
                shootRatio: 0.5f,
                shootSound: shootSound,
                recoilAngle: 10f,
                showHitImage: hit
            )
        );
    }

    //46
    public IEnumerator PlayMortarAnimation(Transform shooterCard, Transform targetCard, bool hit)
    {
        Sprite shooterSprite = Resources.Load<Sprite>("Game/Animations/mortar1");
        Sprite hitSprite = Resources.Load<Sprite>("Game/Animations/mortar2");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/mortar");

        yield return StartCoroutine(
            shootAnimation.StartShootAnimation(
                shooterSprite: shooterSprite,
                hitSprite: hitSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(300f, 300f),
                hitImageSize: new Vector2(200f, 200f),
                duration: 1f,
                shootRatio: 0.5f,
                shootSound: shootSound,
                recoilAngle: 10f,
                showHitImage: hit,
                rotateToTarget: false
            )
        );
    }

    //47
    public IEnumerator PlayGreatArmyAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[5];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/greatarmy1");
        sprites[1] = Resources.Load<Sprite>("Game/Animations/greatarmy2");
        sprites[2] = Resources.Load<Sprite>("Game/Animations/greatarmy3");
        sprites[3] = Resources.Load<Sprite>("Game/Animations/greatarmy4");
        sprites[4] = Resources.Load<Sprite>("Game/Animations/greatarmy5");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/greatarmy");

        Vector2 initialSize = new Vector2(200f, 200f); // Počiatočná veľkosť obrázka
        Vector2 finalSize = new Vector2(250f, 250f);   // Konečná veľkosť obrázka

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(200f, 400f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 1,
                totalImages: 10,
                startSize: initialSize,
                endSize: finalSize,
                imageLifetime: 1f,
                soundEffect: effectSound
            )
        );
    }

    //48
    public IEnumerator PlayDoubleEnvelopmentAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[]
        {
            Resources.Load<Sprite>("Game/Animations/doubleenvelop1"),
            Resources.Load<Sprite>("Game/Animations/doubleenvelop2"),
            Resources.Load<Sprite>("Game/Animations/doubleenvelop3"),
            Resources.Load<Sprite>("Game/Animations/doubleenvelop4"),
            Resources.Load<Sprite>("Game/Animations/doubleenvelop5"),
        };
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/doubleenvelop");

        yield return StartCoroutine(
            corruptionAnimation.StartAnimation(
                sprites: sprites,
                soundEffect: soundEffect,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 300f), // Predpokladaná veľkosť karty
                duration: 1f, // Celková doba trvania animácie
                numberOfImages: 5, // Number of images you want to spawn
                imageSize: new Vector2(200f, 200f), // Size of the generated images
                scatterSpeed: 250f // Speed at which the images scatter
            )
        );
    }

    //48
    public IEnumerator PlayDoubleEnvelopmentWaitAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/envelopwait");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/envelopwait");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                endRotation: -30f,
                duration: 0.7f,
                soundEffect: effectSound
            )
        );
    }

    //49
    public IEnumerator PlayDoubleEnvelopAttackAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[]
        {
            Resources.Load<Sprite>("Game/Animations/doubleenvelop1"),
            Resources.Load<Sprite>("Game/Animations/doubleenvelop2"),
            Resources.Load<Sprite>("Game/Animations/doubleenvelop3"),
            Resources.Load<Sprite>("Game/Animations/doubleenvelop4"),
            Resources.Load<Sprite>("Game/Animations/doubleenvelop5"),
        };
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/envelopattack");

        yield return StartCoroutine(
            envelopAnimation.StartAnimation(
                sprites: sprites,
                soundEffect: soundEffect,
                targetCard: targetCard,
                spawnDistance: 350f, // Predpokladaná vzdialenosť, kde sa obrázky objavia
                imageSize: new Vector2(200f, 200f), // Size of the generated images
                convergeDistance: 100f, // Vzdialenosť, kde sa obrázky zastavia od stredu karty
                duration: 1f, // Celková doba trvania animácie
                numberOfImages: 5 // Počet obrázkov, ktoré chcete vygenerovať
            )
        );
    }

    //50
    public IEnumerator PlayContinentalBlockadeAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/continentalblocade");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/continentalblocade");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.5f,
                startSound: startSound,
                rotateTowardsTarget: false,
                initialRotation: 0f, // napr. -15 stupňov na začiatku
                finalRotation: 0f,     // napr. 15 stupňov na konci
                moveDurationRatio: 0.5f
            )
        );
    }

    //51
    public IEnumerator PlayDepressionAnimation(Transform targetCard)
    {
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/depression");
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/depression");

        // Spustenie animácie
        yield return StartCoroutine(
            depressionAnimation.StartAnimation(
                sprite1: sprite,
                position: targetCard,
                imageSize: new Vector2(300f, 300f),
                duration: 1f,
                startSound: soundEffect
            )
        );
    }

    //?
    public IEnumerator PlayArtInspirationStartAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/artinspitationstart");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/artinspitationstart");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                soundEffect: soundEffect
            )
        );
    }


    //53
    public IEnumerator PlayKnifeAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/knife");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/knife");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(350f, 350f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 0f, // napr. -15 stupňov na začiatku
                finalRotation: 0f     // napr. 15 stupňov na konci
            )
        );
    }

    //54
    public IEnumerator PlayAutoportraitAnimation(Transform targetCard)
    {
        Sprite animationSprite = Resources.Load<Sprite>("Game/Animations/autoportrait");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/autoportrait");

        yield return StartCoroutine(
            autoportraitAnimation.StartAnimation(
                sprite: animationSprite,
                targetCard: targetCard,
                imageSize: new Vector2(250f, 250f),
                duration: 1f,
                moveWidth: 100f,
                speed: 300f,
                tiltAngle: -15f,
                soundEffect: startSound
            )
        );
    }

    //55
    public IEnumerator PlayGravityPullAnimation(Transform attacker, Transform receiver, int variantIndex)
    {
        Sprite[] sprites = new Sprite[]
        {
            Resources.Load<Sprite>("Game/Animations/gravitypull1"),
            Resources.Load<Sprite>("Game/Animations/gravitypull2"),
            Resources.Load<Sprite>("Game/Animations/gravitypull3"),
            Resources.Load<Sprite>("Game/Animations/gravitypull4"),
        };
        AudioClip[] sounds = new AudioClip[]
        {
            Resources.Load<AudioClip>("Sounds/Game/Animations/gravitypull1"),
            Resources.Load<AudioClip>("Sounds/Game/Animations/gravitypull2"),
            Resources.Load<AudioClip>("Sounds/Game/Animations/gravitypull3"),
            Resources.Load<AudioClip>("Sounds/Game/Animations/gravitypull4"),
        };

        // Spustenie animácie s konkrétnym obrázkom a zvukom
        yield return StartCoroutine(
            gravityPullAnimation.StartAnimation(
                sprite: sprites[variantIndex],
                sound: sounds[variantIndex],
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.7f,
                initialRotation: 0f,
                finalRotation: 0f,
                rotateTowardsTarget: false,
                moveDurationRatio: 1f
            )
        );
    }

    //56
    public IEnumerator PlayKamikazeAnimation(Transform attacker, Transform defender, bool isSuccessful)
    {
        // Načítanie obrázkov a zvukov
        Sprite planeSprite = Resources.Load<Sprite>("Game/Animations/kamikaze1");
        Sprite explosionSprite = Resources.Load<Sprite>("Game/Animations/kamikaze2");

        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/kamikaze");

        // Spustenie animácie
        yield return StartCoroutine(
            kamikazeAnimation.StartAnimation(
                planeSprite: planeSprite,
                explosionSprite: explosionSprite,
                sound: soundEffect,
                isSuccessful: isSuccessful,
                attacker: attacker,
                defender: defender,
                finalRotation: -90f,
                cardSize: new Vector2(300f, 300f), // Predpokladaná veľkosť karty
                imageSize: new Vector2(300f, 300f), // Veľkosť obrázkov
                duration: 1.5f, // Celková doba trvania animácie
                moveDurationRatio: 0.5f, // Pomer času pohybu lietadla
                explosionGrowDurationRatio: 0.2f // Pomer času rastu explózie
            )
        );
    }

    //57
    public IEnumerator PlayTakeOffAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/takeoff1");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/takeoff1");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                soundEffect: effectSound
            )
        );
    }

    //57
    public IEnumerator PlayTakeOffCrashAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/takeoff2");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/takeoff2");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                soundEffect: effectSound
            )
        );
    }

    //58
    public IEnumerator PlayAirStrikeAnimation(Transform attacker, Transform defender, int showHitImageCount)
    {
        Sprite planeSprite = Resources.Load<Sprite>("Game/Animations/airstrike1");
        Sprite explosionSprite = Resources.Load<Sprite>("Game/Animations/airstrike2");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/airstrike");
        AudioClip soundEffectHit = Resources.Load<AudioClip>("Sounds/Game/Animations/airstrike2");

        // Spustenie animácie
        yield return StartCoroutine(
            airStrikeAnimation.StartAnimation(
                planeSprite: planeSprite,
                explosionSprite: explosionSprite,
                planeSound: soundEffect,
                explosionSound: soundEffectHit,
                attacker: attacker,
                defender: defender,
                planeImageSize: new Vector2(300f, 300f),
                explosionImageSize: new Vector2(150f, 150f),
                cardSize: new Vector2(300f, 300f),
                duration: 1f,
                showHitImageCount: showHitImageCount, // Príklad hodnoty
                moveDurationRatio: 1f,
                explosionGrowDurationRatio: 0.2f,
                rotateTowardsTarget: true,
                initialRotation: 0f,
                finalRotation: 0f
            )
        );
    }


    //58
    public IEnumerator PlayAirStrikeCriticalAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/airstrikecritical");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/airstrikecritical");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(150f, 150f),
                endSize: new Vector2(250f, 250f),
                duration: 0.3f,
                soundEffect: effectSound
            )
        );
    }

    //59
    public IEnumerator PlayJusticeCrusadeAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[5];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/justicecrusade1");
        sprites[1] = Resources.Load<Sprite>("Game/Animations/justicecrusade2");
        sprites[2] = Resources.Load<Sprite>("Game/Animations/justicecrusade3");
        sprites[3] = Resources.Load<Sprite>("Game/Animations/justicecrusade4");
        sprites[4] = Resources.Load<Sprite>("Game/Animations/justicecrusade5");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/justicecrusade");

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(200f, 400f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 2,
                totalImages: 20,
                startSize: new Vector2(200f, 200f),
                endSize: new Vector2(250f, 250f),
                imageLifetime: 1f,
                soundEffect: effectSound
            )
        );
    }

    //60
    public IEnumerator PlayRapierAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/rapier");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/rapier");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 100f, // napr. -15 stupňov na začiatku
                finalRotation: -15f     // napr. 15 stupňov na konci
            )
        );
    }

    //61
    public IEnumerator PlayExpeditionaryAssaultAnimation(Transform attacker)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/expeditionaryAssault");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/expeditionaryAssault");

        yield return StartCoroutine(
            rotatingEnlargeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: attacker,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                rotationSpeed: 360f,
                soundEffect: startSound
            )
        );
    }

    //61
    public IEnumerator PlayExpeditionaryAssaultSuccessAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite1 = Resources.Load<Sprite>("Game/Animations/expeditionaryAssaultSuccess1");
        Sprite animationImageSprite2 = Resources.Load<Sprite>("Game/Animations/expeditionaryAssaultSuccess2");
        Sprite animationImageSprite3 = Resources.Load<Sprite>("Game/Animations/expeditionaryAssaultSuccess3");
        Sprite animationImageSprite4 = Resources.Load<Sprite>("Game/Animations/expeditionaryAssaultSuccess4");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/expeditionaryAssaultSuccess");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite1,
                startPoint: attacker,
                endPoint: receiver,
                rotateTowardsTarget: false,
                imageSize: new Vector2(250f, 250f),
                duration: 0.3f,
                startSound: startSound
            )
        );
        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite2,
                startPoint: receiver,
                endPoint: attacker,
                rotateTowardsTarget: false,
                imageSize: new Vector2(250f, 250f),
                duration: 0.3f,
                startSound: startSound
            )
        );
        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite3,
                startPoint: receiver,
                endPoint: attacker,
                rotateTowardsTarget: false,
                imageSize: new Vector2(250f, 250f),
                duration: 0.3f,
                startSound: startSound
            )
        );
        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite4,
                startPoint: receiver,
                endPoint: attacker,
                rotateTowardsTarget: false,
                imageSize: new Vector2(250f, 250f),
                duration: 0.3f,
                startSound: startSound
            )
        );
    }

    //61
    public IEnumerator PlayExpeditionaryAssaultFailAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/expeditionaryAssaultFail");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/expeditionaryAssaultFail");

        // Spustenie animácie
        yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                shakeDuration: 1f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: soundEffect
            )
        );
    }

    //62
    public IEnumerator PlayCulverinAnimation(Transform shooterCard, Transform targetCard, bool hit)
    {
        Sprite shooterSprite = Resources.Load<Sprite>("Game/Animations/culverin");
        Sprite hitSprite = Resources.Load<Sprite>("Game/Animations/mortar2");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/mortar");

        yield return StartCoroutine(
            shootAnimation.StartShootAnimation(
                shooterSprite: shooterSprite,
                hitSprite: hitSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(300f, 300f),
                hitImageSize: new Vector2(200f, 200f),
                duration: 1f,
                shootRatio: 0.5f,
                shootSound: shootSound,
                recoilAngle: 10f,
                showHitImage: hit,
                rotateToTarget: false
            )
        );
    }

    //63
    public IEnumerator PlayFireShipAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/fireship");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/fireship");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.7f,
                startSound: startSound,
                rotateTowardsTarget: false,
                initialRotation: 0f,
                finalRotation: 0f
            )
        );
    }

    //64
    public IEnumerator PlayHandcuffEscapeAnimation(Transform targetCard)
    {
        Sprite sprite1 = Resources.Load<Sprite>("Game/Animations/handcuffescape1");
        Sprite sprite2 = Resources.Load<Sprite>("Game/Animations/handcuffescape2");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/handcuffescape");

        yield return StartCoroutine(
            dualImageAnimation.StartAnimation(
                sprite1: sprite1,
                sprite2: sprite2,
                position: targetCard,
                imageSize: new Vector2(300f, 300f), // Predpokladaná veľkosť pohára
                duration: 1f, // Celková doba trvania animácie
                startSound: startSound,
                switchRatio: 0.5f // V polovici animácie sa zmení na fľašu vína
            )
        );
    }

    //65
    public IEnumerator PlayIllusionAnimation(Transform attacker, int variantIndex)
    {
        Sprite[] sprites = new Sprite[]
        {
            Resources.Load<Sprite>("Game/Animations/illusionbigfoot"),
            Resources.Load<Sprite>("Game/Animations/illusionnessie"),
            Resources.Load<Sprite>("Game/Animations/illusionufo"),
            Resources.Load<Sprite>("Game/Animations/illusion1"),
            Resources.Load<Sprite>("Game/Animations/illusion2"),
        };
        AudioClip[] sounds = new AudioClip[]
        {
            Resources.Load<AudioClip>("Sounds/Game/Animations/illusionbigfoot"),
            Resources.Load<AudioClip>("Sounds/Game/Animations/illusionnessie"),
            Resources.Load<AudioClip>("Sounds/Game/Animations/illusionufo"),
            Resources.Load<AudioClip>("Sounds/Game/Animations/illusion"),
            Resources.Load<AudioClip>("Sounds/Game/Animations/illusion"),
        };

        yield return StartCoroutine(
            wirelessChargerAnimation.StartAnimation(
                sprite1: sprites[variantIndex],
                sprite2: sprites[variantIndex],
                position: attacker,
                imageSize: new Vector2(300f, 300f),
                duration: 1f,
                startSound: sounds[variantIndex],
                switchRatio: 0.3f,
                fadeDurationRatio: 0.4f
            )
        );

    }

    //66
    public IEnumerator PlayCarcanoAnimation(Transform shooterCard, Transform targetCard, bool hit)
    {
        Sprite shooterSprite = Resources.Load<Sprite>("Game/Animations/carcano1");
        Sprite hitSprite = Resources.Load<Sprite>("Game/Animations/carcano2");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/carcano");

        yield return StartCoroutine(
            shootAnimation.StartShootAnimation(
                shooterSprite: shooterSprite,
                hitSprite: hitSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(350f, 350f),
                hitImageSize: new Vector2(100f, 100f),
                duration: 1f,
                shootRatio: 0.5f,
                shootSound: shootSound,
                recoilAngle: 10f,
                showHitImage: hit
            )
        );
    }

    //67
    public IEnumerator PlayWinchesterAnimation(Transform shooterCard, Transform targetCard, bool hit)
    {
        Sprite shooterSprite = Resources.Load<Sprite>("Game/Animations/winchester1");
        Sprite hitSprite = Resources.Load<Sprite>("Game/Animations/winchester2");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/winchester");

        yield return StartCoroutine(
            shootAnimation.StartShootAnimation(
                shooterSprite: shooterSprite,
                hitSprite: hitSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(350f, 350f),
                hitImageSize: new Vector2(100f, 100f),
                duration: 1f,
                shootRatio: 0.5f,
                shootSound: shootSound,
                recoilAngle: 10f,
                showHitImage: hit
            )
        );
    }

    //68
    public IEnumerator PlayAmbushAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[3];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/ambush1");
        sprites[1] = Resources.Load<Sprite>("Game/Animations/ambush2");
        sprites[2] = Resources.Load<Sprite>("Game/Animations/ambush3");
        AudioClip ambushSound = Resources.Load<AudioClip>("Sounds/Game/Animations/ambush");

        yield return StartCoroutine(
            ambushAnimation.StartAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                numberOfImages: 3,
                imageSize: new Vector2(200f, 200f),
                soundEffect: ambushSound,
                gatherSpeed: 300f,
                maxRotationAngle: 30f
            )
        );
    }

    //69
    public IEnumerator PlaySpaceRocketAnimation(Transform playerCard, bool isSuccessfulLaunch)
    {
        Sprite rocketSprite = Resources.Load<Sprite>("Game/Animations/spacerocket");
        Sprite explosionSprite = Resources.Load<Sprite>("Game/Animations/spacerocketfail");
        AudioClip successSound = Resources.Load<AudioClip>("Sounds/Game/Animations/spacerocket");
        AudioClip failureSound = Resources.Load<AudioClip>("Sounds/Game/Animations/spacerocketfail");

        yield return StartCoroutine(
            rocketLaunchAnimation.StartRocketAnimation(
                rocketSprite: rocketSprite,
                explosionSprite: explosionSprite,
                playerCard: playerCard,
                imageSize: new Vector2(300f, 300f),
                explosionStartSize: new Vector2(250f, 250f), // Pridané veľkosti pre explóziu
                explosionEndSize: new Vector2(350f, 350f),   // Pridané veľkosti pre explóziu
                successSound: successSound,
                failureSound: failureSound,
                isSuccessfulLaunch: isSuccessfulLaunch,
                duration: 1f
            )
        );
    }

    //70
    public IEnumerator PlayV2Animation(Transform attacker, Transform defender, bool isSuccessful)
    {
        // Načítanie obrázkov a zvukov
        Sprite planeSprite = Resources.Load<Sprite>("Game/Animations/v2");
        Sprite explosionSprite = Resources.Load<Sprite>("Game/Animations/v2explosion");

        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/v2");

        // Spustenie animácie
        yield return StartCoroutine(
            kamikazeAnimation.StartAnimation(
                planeSprite: planeSprite,
                explosionSprite: explosionSprite,
                sound: soundEffect,
                isSuccessful: isSuccessful,
                attacker: attacker,
                defender: defender,
                finalRotation: 0f,
                cardSize: new Vector2(300f, 300f), // Predpokladaná veľkosť karty
                imageSize: new Vector2(300f, 300f), // Veľkosť obrázkov
                duration: 1f, // Celková doba trvania animácie
                moveDurationRatio: 0.5f, // Pomer času pohybu lietadla
                explosionGrowDurationRatio: 0.2f // Pomer času rastu explózie
            )
        );
    }

    //71
    public IEnumerator PlayBattleCryAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/battlecry");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/battlecry");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                soundEffect: effectSound
            )
        );
    }

    //72
    public IEnumerator PlayRevelationAnimation(Transform targetCard, int variantIndex)
    {
        Sprite[] sprites = new Sprite[]
        {
            Resources.Load<Sprite>("Game/Animations/revelation"),
            Resources.Load<Sprite>("Game/Animations/revelationbush"),
            Resources.Load<Sprite>("Game/Animations/revelationlion"),
            Resources.Load<Sprite>("Game/Animations/revelationlotos"),
            Resources.Load<Sprite>("Game/Animations/revelationwolf"),
            Resources.Load<Sprite>("Game/Animations/revelationaztec"),
        };
        AudioClip[] sounds = new AudioClip[]
        {
            Resources.Load<AudioClip>("Sounds/Game/Animations/revelation"),
            Resources.Load<AudioClip>("Sounds/Game/Animations/revelationbush"),
            Resources.Load<AudioClip>("Sounds/Game/Animations/revelationlion"),
            Resources.Load<AudioClip>("Sounds/Game/Animations/revelationlotos"),
            Resources.Load<AudioClip>("Sounds/Game/Animations/revelationwolf"),
            Resources.Load<AudioClip>("Sounds/Game/Animations/revelationaztec"),
        };

        yield return StartCoroutine(
            risingEnlargeImageAnimation.StartRisingEnlargeAnimation(
                sprite: sprites[variantIndex],
                targetCard: targetCard,
                startSize: new Vector2(200f, 200f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                startVerticalOffset: 100f, // Počiatočný vertikálny posun
                endVerticalOffset: 0f, // Konečný vertikálny posun
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: sounds[variantIndex]
            )
        );
    }

    //73
    public IEnumerator PlayStandardAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/standard");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/standard");

        yield return StartCoroutine(
            flagWaveAnimation.StartFlagWaveAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                imageSize: new Vector2(300f, 300f),
                duration: 1f,
                minRotation: -30f,
                maxRotation: 30f,
                waveSpeed: 200f,
                soundEffect: startSound
            )
        );
    }

    //74
    public IEnumerator PlayPenAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/pen");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/pen");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.7f,
                startSound: startSound,
                initialRotation: 15f, // napr. -15 stupňov na začiatku
                finalRotation: 0f     // napr. 15 stupňov na konci
            )
        );
    }

    //75
    public IEnumerator PlayIambicPentameterAnimation(Transform shooterCard, Transform targetCard)
    {
        // Načítanie zdrojov pre animáciu
        Sprite bowSprite = Resources.Load<Sprite>("Game/Animations/pentameter1");
        Sprite arrowSprite = Resources.Load<Sprite>("Game/Animations/pentameter2");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/pentameter");

        // Spustenie animácie výstrelu šípu
        yield return StartCoroutine(
            bowShootAnimation.StartShootAnimation(
                bowSprite: bowSprite,
                arrowSprite: arrowSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(350f, 350f), // Veľkosť obrázka luku
                duration: 1f, // Trvanie animácie
                shootSound: shootSound,
                showHitImage: true, // Zobrazenie šípu
                rotateToTarget: true, // Otočenie smerom k cieľu
                arrowImageSize: new Vector2(250f, 250f), // Veľkosť obrázka šípu
                arrowCount: 10, // Počet šípov
                arrowInterval: 0.1f, // Interval medzi šípmi
                arrowFlightDuration: 0.3f // Čas letu šípu
            )
        );
    }

    //76
    public IEnumerator PlayGhostAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/ghost");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/ghost");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                rotateTowardsTarget: false,
                imageSize: new Vector2(300f, 300f),
                duration: 1f,
                startSound: startSound,
                initialRotation: 0f, // napr. -15 stupňov na začiatku
                finalRotation: 0f     // napr. 15 stupňov na konci
            )
        );
    }

    //77
    public IEnumerator PlayBuffaloHornsAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/buffalohorns");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/buffalohorns");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                soundEffect: effectSound
            )
        );
    }

    //78
    public IEnumerator PlayIklwaAnimation(Transform attacker, Transform receiver, bool isSuccessful)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/iklwa");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/iklwa");

        // Spustenie animácie
        yield return StartCoroutine(
            kamikazeAnimation.StartAnimation(
                planeSprite: sprite,
                explosionSprite: null,
                sound: sound,
                isSuccessful: isSuccessful,
                attacker: attacker,
                defender: receiver,
                finalRotation: 0f,
                cardSize: new Vector2(300f, 300f), // Predpokladaná veľkosť karty
                imageSize: new Vector2(300f, 300f), // Veľkosť obrázkov
                duration: 0.5f, // Celková doba trvania animácie
                moveDurationRatio: 1f, // Pomer času pohybu lietadla
                explosionGrowDurationRatio: 0f // Pomer času rastu explózie
            )
        );
    }

    //79
    public IEnumerator PlayIwisaAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/iwisa");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/iwisa");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.7f,
                startSound: startSound,
                initialRotation: 100f,
                finalRotation: 0f
            )
        );
    }

    //80
    public IEnumerator PlayNitenIchiRyuKatanaAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/katana");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/ichiryu");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(350f, 350f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 90f,
                finalRotation: 0f
            )
        );
    }

    //80
    public IEnumerator PlayNitenIchiRyuWakizashiAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/wakizashi");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/ichiryu");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: -90f,
                finalRotation: 0f
            )
        );
    }

    //81
    public IEnumerator PlayTessenjutsuAnimation(Transform attacker)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/tessenjutsu");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/tessenjutsu");

        yield return StartCoroutine(
            rotatingEnlargeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: attacker,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 0.7f,
                rotationSpeed: -180f,
                initialRotation: 90f,
                soundEffect: startSound
            )
        );
    }

    //82
    public IEnumerator PlayIaijutsuAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/wakizashi");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/iaijutsu");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.3f,
                startSound: startSound,
                initialRotation: -90f,
                finalRotation: 0f
            )
        );
    }

    //83
    public IEnumerator PlayKatanaAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/katana");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/katana");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 100f, // napr. -15 stupňov na začiatku
                finalRotation: -15f     // napr. 15 stupňov na konci
            )
        );
    }

    //84
    public IEnumerator PlayNodachiAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/nodachi");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/nodachi");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(350f, 350f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 100f, // napr. -15 stupňov na začiatku
                finalRotation: -15f     // napr. 15 stupňov na konci
            )
        );
    }

    //85
    public IEnumerator PlayYumiAnimation(Transform shooterCard, Transform targetCard, bool hit)
    {
        Sprite bowSprite = Resources.Load<Sprite>("Game/Animations/yumi");
        Sprite arrowSprite = Resources.Load<Sprite>("Game/Animations/yumiarrow");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/yumi");

        yield return StartCoroutine(
            bowShootAnimation.StartShootAnimation(
                bowSprite: bowSprite,
                arrowSprite: arrowSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(350f, 350f), // Veľkosť obrázka luku
                duration: 1f, // Trvanie animácie
                shootSound: shootSound,
                showHitImage: hit, // Zobrazenie šípu
                rotateToTarget: true, // Otočenie smerom k cieľu
                arrowImageSize: new Vector2(250f, 250f) // Veľkosť obrázka šípu
            )
        );
    }

    //86
    public IEnumerator PlayJujutsuAnimation(Transform attacker, Transform receiver)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/jujutsu");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/jujutsu");

        yield return StartCoroutine(
            midpointRotatingEnlargeAnimation.StartAnimation(
                sprite: sprite,
                attacker: attacker,
                defender: receiver,
                startSize: new Vector2(300f, 300f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                rotationSpeed: 90f,
                soundEffect: sound
            )
        );
    }

    //87
    public IEnumerator PlayEspionageAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite1 = Resources.Load<Sprite>("Game/Animations/espionage1");
        Sprite animationImageSprite2 = Resources.Load<Sprite>("Game/Animations/espionage2");
        Sprite animationImageSprite3 = Resources.Load<Sprite>("Game/Animations/espionage3");
        Sprite animationImageSprite4 = Resources.Load<Sprite>("Game/Animations/espionage1");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/espionage");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite1,
                startPoint: attacker,
                endPoint: receiver,
                rotateTowardsTarget: false,
                imageSize: new Vector2(300f, 300f),
                duration: 0.4f,
                startSound: startSound
            )
        );
        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite2,
                startPoint: receiver,
                endPoint: attacker,
                rotateTowardsTarget: false,
                imageSize: new Vector2(250f, 250f),
                duration: 0.2f
            )
        );
        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite3,
                startPoint: receiver,
                endPoint: attacker,
                rotateTowardsTarget: false,
                imageSize: new Vector2(250f, 250f),
                duration: 0.2f
            )
        );
        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite4,
                startPoint: receiver,
                endPoint: attacker,
                rotateTowardsTarget: false,
                imageSize: new Vector2(300f, 300f),
                duration: 0.4f
            )
        );
    }

    //88
    public IEnumerator PlaySabreAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/sabre");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/sabre");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 90f, // napr. -15 stupňov na začiatku
                finalRotation: 0f     // napr. 15 stupňov na konci
            )
        );
    }

    //89
    public IEnumerator PlayGambleAnimation(Transform attacker, Transform receiver, bool win)
    {
        Sprite animationImageSprite1 = Resources.Load<Sprite>("Game/Animations/gamblechips1");
        Sprite animationImageSprite2 = Resources.Load<Sprite>("Game/Animations/gamblechips2");
        Sprite animationImageSprite3 = Resources.Load<Sprite>("Game/Animations/gamblecoin1");
        Sprite animationImageSprite4 = Resources.Load<Sprite>("Game/Animations/gamblecoin2");
        AudioClip chipsSound = Resources.Load<AudioClip>("Sounds/Game/Animations/gamblechips");
        AudioClip coinSound = Resources.Load<AudioClip>("Sounds/Game/Animations/gamblecoin");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite1,
                startPoint: receiver,
                endPoint: attacker,
                rotateTowardsTarget: false,
                imageSize: new Vector2(300f, 300f),
                duration: 0.3f,
                startSound: chipsSound
            )
        );
        if (win)
        {
            yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: animationImageSprite3,
                targetCard: attacker,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 0.6f,
                shakeDuration: 0.3f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: coinSound
            )
        );
        }
        else
        {
            yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: animationImageSprite4,
                targetCard: attacker,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 0.6f,
                shakeDuration: 0.3f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: coinSound
            )
        );
            yield return StartCoroutine(
                moveImageAnimation.StartAnimation(
                    sprite: animationImageSprite2,
                    startPoint: attacker,
                    endPoint: receiver,
                    rotateTowardsTarget: false,
                    imageSize: new Vector2(300f, 300f),
                    duration: 0.3f,
                    startSound: chipsSound
                )
            );
        }
    }

    //90
    public IEnumerator PlayPhilosophyAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[5];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/philosophy1");
        sprites[1] = Resources.Load<Sprite>("Game/Animations/philosophy2");
        sprites[2] = Resources.Load<Sprite>("Game/Animations/philosophy3");
        sprites[3] = Resources.Load<Sprite>("Game/Animations/philosophy4");
        sprites[4] = Resources.Load<Sprite>("Game/Animations/philosophy5");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/philosophy");

        Vector2 initialSize = new Vector2(50f, 50f); // Počiatočná veľkosť obrázka
        Vector2 finalSize = new Vector2(300f, 300f);   // Konečná veľkosť obrázka

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 1,
                totalImages: 3,
                startSize: initialSize,
                endSize: finalSize,
                imageLifetime: 0.5f,
                soundEffect: effectSound
            )
        );
    }

    //91
    public IEnumerator PlayCalmAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/calm");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/calm");

        // Spustenie animácie
        yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                shakeDuration: 0f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: soundEffect
            )
        );
    }

    //92
    public IEnumerator PlayHonestyAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/honesty");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/honesty");

        // Spustenie animácie
        yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                startSize: new Vector2(300f, 300f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                shakeDuration: 0.5f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: soundEffect
            )
        );
    }

    //93
    public IEnumerator PlayValaskaAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/valaska");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/valaska");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 100f, // napr. -15 stupňov na začiatku
                finalRotation: -15f     // napr. 15 stupňov na konci
            )
        );
    }

    //94
    public IEnumerator PlayMoonshineAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/moonshine");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/moonshine");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(400f, 400f),
                endSize: new Vector2(400f, 400f),
                duration: 1f,
                endRotation: 90f,
                soundEffect: effectSound
            )
        );
    }

    //95
    public IEnumerator PlayOutlawBandAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[6];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/outlawband1");
        sprites[1] = Resources.Load<Sprite>("Game/Animations/outlawband2");
        sprites[2] = Resources.Load<Sprite>("Game/Animations/outlawband3");
        sprites[3] = Resources.Load<Sprite>("Game/Animations/outlawband4");
        sprites[4] = Resources.Load<Sprite>("Game/Animations/outlawband5");
        sprites[5] = Resources.Load<Sprite>("Game/Animations/outlawband6");
        AudioClip ambushSound = Resources.Load<AudioClip>("Sounds/Game/Animations/outlawband");

        yield return StartCoroutine(
            ambushAnimation.StartAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                numberOfImages: 6,
                imageSize: new Vector2(200f, 200f),
                soundEffect: ambushSound,
                gatherSpeed: 300f,
                maxRotationAngle: 30f
            )
        );
    }

    //95
    public IEnumerator PlayOutlawBandFailAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[6];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/outlawband1");
        sprites[1] = Resources.Load<Sprite>("Game/Animations/outlawband2");
        sprites[2] = Resources.Load<Sprite>("Game/Animations/outlawband3");
        sprites[3] = Resources.Load<Sprite>("Game/Animations/outlawband4");
        sprites[4] = Resources.Load<Sprite>("Game/Animations/outlawband5");
        sprites[5] = Resources.Load<Sprite>("Game/Animations/outlawband6");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/outlawbandfail");

        yield return StartCoroutine(
            corruptionAnimation.StartAnimation(
                sprites: sprites,
                soundEffect: soundEffect,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 300f), // Predpokladaná veľkosť karty
                duration: 0.7f, // Celková doba trvania animácie
                numberOfImages: 6, // Number of images you want to spawn
                imageSize: new Vector2(200f, 200f), // Size of the generated images
                scatterSpeed: 300f // Speed at which the images scatter
            )
        );
    }

    //96
    public IEnumerator PlayFlintlockPistolLoadingAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/flintlockloading");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/flintlockloading");

        // Spustenie animácie
        yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 0.7f,
                shakeDuration: 0.2f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: soundEffect
            )
        );
    }

    //96
    public IEnumerator PlayFlintlockPistolShotAnimation(Transform shooterCard, Transform targetCard, bool hit)
    {
        Sprite shooterSprite = Resources.Load<Sprite>("Game/Animations/flintlockshot");
        Sprite hitSprite = Resources.Load<Sprite>("Game/Animations/flintlockbullet");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/flintlockshot");

        yield return StartCoroutine(
            shootAnimation.StartShootAnimation(
                shooterSprite: shooterSprite,
                hitSprite: hitSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(350f, 350f),
                hitImageSize: new Vector2(100f, 100f),
                duration: 1f,
                shootRatio: 0.5f,
                shootSound: shootSound,
                recoilAngle: 10f,
                showHitImage: hit
            )
        );
    }

    //97
    public IEnumerator PlayPassiveResistanceAnimation(Transform targetCard)
    {
        Sprite waterSprite = Resources.Load<Sprite>("Game/Animations/passiveresistance1");
        Sprite wineSprite = Resources.Load<Sprite>("Game/Animations/passiveresistance2");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/passiveresistance");

        yield return StartCoroutine(
            dualImageAnimation.StartAnimation(
                sprite1: waterSprite,
                sprite2: wineSprite,
                position: targetCard,
                imageSize: new Vector2(300f, 300f), // Predpokladaná veľkosť pohára
                duration: 1f, // Celková doba trvania animácie
                startSound: startSound,
                switchRatio: 0.5f // V polovici animácie sa zmení na fľašu vína
            )
        );
    }

    //98
    public IEnumerator PlayHungerStrikeAnimation(Transform targetCard)
    {
        Sprite waterSprite = Resources.Load<Sprite>("Game/Animations/hungerstike1");
        Sprite wineSprite = Resources.Load<Sprite>("Game/Animations/hungerstike2");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/hungerstike");

        yield return StartCoroutine(
            dualImageAnimation.StartAnimation(
                sprite1: waterSprite,
                sprite2: wineSprite,
                position: targetCard,
                imageSize: new Vector2(300f, 300f), // Predpokladaná veľkosť pohára
                duration: 1f, // Celková doba trvania animácie
                startSound: startSound,
                switchRatio: 0.5f // V polovici animácie sa zmení na fľašu vína
            )
        );
    }

    //99
    public IEnumerator PlayGladiusAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/gladius");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/gladius");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 100f,
                finalRotation: 0f
            )
        );
    }


    //100
    public IEnumerator PlayShieldBashAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/shield");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/shield");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 15f,
                finalRotation: -15f
            )
        );
    }

    //101
    public IEnumerator PlayYperitSuccessAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/yperit");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/yperit");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 1f,
                rotateTowardsTarget: false,
                startSound: startSound,
                initialRotation: 0f,
                finalRotation: 0f
            )
        );
    }

    //101
    public IEnumerator PlayYperitFailAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/yperit");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/yperit");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(400f, 400f),
                duration: 1f,
                soundEffect: effectSound
            )
        );
    }

    //102
    public IEnumerator PlayBlitzkriegAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/blitzkrieg");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/blitzkrieg");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 1f,
                startSound: startSound,
                initialRotation: 0f,
                finalRotation: 0f
            )
        );
    }

    //103
    public IEnumerator PlayPropagandaAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite1 = Resources.Load<Sprite>("Game/Animations/propaganda1");
        Sprite animationImageSprite2 = Resources.Load<Sprite>("Game/Animations/propaganda2");
        Sprite animationImageSprite3 = Resources.Load<Sprite>("Game/Animations/propaganda3");
        Sprite animationImageSprite4 = Resources.Load<Sprite>("Game/Animations/propaganda4");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/propaganda");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite1,
                startPoint: attacker,
                endPoint: receiver,
                rotateTowardsTarget: false,
                imageSize: new Vector2(250f, 250f),
                duration: 0.3f,
                startSound: startSound
            )
        );
        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite2,
                startPoint: attacker,
                endPoint: receiver,
                rotateTowardsTarget: false,
                imageSize: new Vector2(250f, 250f),
                duration: 0.3f
            )
        );
        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite3,
                startPoint: attacker,
                endPoint: receiver,
                rotateTowardsTarget: false,
                imageSize: new Vector2(250f, 250f),
                duration: 0.3f
            )
        );
        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite4,
                startPoint: attacker,
                endPoint: receiver,
                rotateTowardsTarget: false,
                imageSize: new Vector2(250f, 250f),
                duration: 0.3f
            )
        );
    }

    //104
    public IEnumerator PlayRetiariusAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/retiarius");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/retiarius");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 100f, // napr. -15 stupňov na začiatku
                finalRotation: -180f     // napr. 15 stupňov na konci
            )
        );
    }

    //105
    public IEnumerator PlayShurikenAnimation(Transform attacker, Transform receiver, bool isSuccessful)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/shuriken");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/shuriken");

        // Spustenie animácie
        yield return StartCoroutine(
            kamikazeAnimation.StartAnimation(
                planeSprite: sprite,
                explosionSprite: null,
                sound: sound,
                isSuccessful: isSuccessful,
                attacker: attacker,
                defender: receiver,
                finalRotation: 450f,
                cardSize: new Vector2(300f, 300f), // Predpokladaná veľkosť karty
                imageSize: new Vector2(250f, 250f), // Veľkosť obrázkov
                duration: 0.7f, // Celková doba trvania animácie
                moveDurationRatio: 1f, // Pomer času pohybu lietadla
                explosionGrowDurationRatio: 0f // Pomer času rastu explózie
            )
        );
    }

    //106
    public IEnumerator PlayKusarigamaAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/kusarigama");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/kusarigama");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 90f,
                finalRotation: 0f
            )
        );
    }

    //108
    public IEnumerator PlayOrientalSpiceAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/orientalspices");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/orientalspices");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.7f,
                startSound: startSound,
                initialRotation: 0f,
                finalRotation: 0f,
                rotateTowardsTarget: false
            )
        );
    }

    //109
    public IEnumerator PlayArquebusExplosionAnimation(Transform targetCard)
    {
        Sprite sprite1 = Resources.Load<Sprite>("Game/Animations/arquebus");
        Sprite sprite2 = Resources.Load<Sprite>("Game/Animations/arquebusexpl");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/arquebus");

        yield return StartCoroutine(
            dualImageAnimation.StartAnimation(
                sprite1: sprite1,
                sprite2: sprite2,
                position: targetCard,
                imageSize: new Vector2(300f, 300f), // Predpokladaná veľkosť pohára
                duration: 1f, // Celková doba trvania animácie
                startSound: startSound,
                switchRatio: 0.5f // V polovici animácie sa zmení na fľašu vína
            )
        );
    }

    //109
    public IEnumerator PlayArquebusShotAnimation(Transform shooterCard, Transform targetCard, bool hit)
    {
        Sprite shooterSprite = Resources.Load<Sprite>("Game/Animations/arquebus");
        Sprite hitSprite = Resources.Load<Sprite>("Game/Animations/arquebusbullet");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/arquebus");

        yield return StartCoroutine(
            shootAnimation.StartShootAnimation(
                shooterSprite: shooterSprite,
                hitSprite: hitSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(350f, 350f),
                hitImageSize: new Vector2(100f, 100f),
                duration: 1f,
                shootRatio: 0.5f,
                shootSound: shootSound,
                recoilAngle: 10f,
                showHitImage: hit
            )
        );
    }

    //110
    public IEnumerator PlayPirateRaidAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/pirateraid");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/pirateraid");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 1f,
                startSound: startSound,
                rotateTowardsTarget: false,
                initialRotation: 0f,
                finalRotation: 0f,
                moveDurationRatio: 0.5f
            )
        );
    }

    //110
    public IEnumerator PlayPirateRaidFailAnimation(Transform attacker, Transform defender)
    {
        // Načítanie obrázkov a zvukov
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/pirateraid");
        Sprite explosionSprite = Resources.Load<Sprite>("Game/Animations/pirateraidfail");

        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/pirateraid");

        // Spustenie animácie
        yield return StartCoroutine(
            kamikazeAnimation.StartAnimation(
                planeSprite: sprite,
                explosionSprite: explosionSprite,
                sound: sound,
                isSuccessful: true,
                rotateTowardsTarget: false,
                attacker: attacker,
                defender: defender,
                finalRotation: 0f,
                cardSize: new Vector2(300f, 300f), // Predpokladaná veľkosť karty
                imageSize: new Vector2(300f, 300f), // Veľkosť obrázkov
                duration: 1.5f, // Celková doba trvania animácie
                moveDurationRatio: 0.5f, // Pomer času pohybu lietadla
                explosionGrowDurationRatio: 0.2f // Pomer času rastu explózie
            )
        );
    }

    //111
    public IEnumerator PlayAxeAnimation(Transform attacker, Transform receiver)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/axe");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/axe");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: sprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.5f,
                startSound: sound,
                initialRotation: 100f, // napr. -15 stupňov na začiatku
                finalRotation: -15f     // napr. 15 stupňov na konci
            )
        );
    }

    //112
    public IEnumerator PlayJaguarWarriorsAnimation(Transform attacker, Transform receiver)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/jaguarwarriors");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/jaguarwarriors");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: sprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 1f,
                startSound: sound,
                initialRotation: 0f,
                finalRotation: 0f,
                rotateTowardsTarget: false
            )
        );
    }

    //113
    public IEnumerator PlayAtlatlAnimation(Transform shooterCard, Transform targetCard, bool hit)
    {
        Sprite bowSprite = Resources.Load<Sprite>("Game/Animations/atlatl");
        Sprite arrowSprite = Resources.Load<Sprite>("Game/Animations/atlatlspear");
        AudioClip shootSound = Resources.Load<AudioClip>("Sounds/Game/Animations/atlatl");

        yield return StartCoroutine(
            bowShootAnimation.StartShootAnimation(
                bowSprite: bowSprite,
                arrowSprite: arrowSprite,
                shooterCard: shooterCard,
                targetCard: targetCard,
                imageSize: new Vector2(400f, 400f), // Veľkosť obrázka luku
                duration: 1f, // Trvanie animácie
                shootSound: shootSound,
                showHitImage: hit, // Zobrazenie šípu
                rotateToTarget: true, // Otočenie smerom k cieľu
                arrowImageSize: new Vector2(350f, 350f) // Veľkosť obrázka šípu
            )
        );
    }

    //114
    public IEnumerator PlayMacuahuitlAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/macuahuitl");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/macuahuitl");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 70f, // napr. -15 stupňov na začiatku
                finalRotation: 0f     // napr. 15 stupňov na konci
            )
        );
    }

    //115
    public IEnumerator PlayCubismAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[4];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/cubism1");
        sprites[1] = Resources.Load<Sprite>("Game/Animations/cubism2");
        sprites[2] = Resources.Load<Sprite>("Game/Animations/cubism3");
        sprites[3] = Resources.Load<Sprite>("Game/Animations/cubism4");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/cubism");

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(200f, 400f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 1,
                totalImages: 7,
                startSize: new Vector2(100f, 100f),
                endSize: new Vector2(300f, 300f),
                imageLifetime: 0.5f,
                soundEffect: effectSound
            )
        );
    }

    //116
    public IEnumerator PlayCosaNostraAnimation(Transform attacker, Transform receiver)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/cosanostra");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/cosanostra");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: sprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 1f,
                startSound: sound,
                initialRotation: 0f,
                finalRotation: 0f,
                rotateTowardsTarget: false
            )
        );
    }

    //117
    public IEnumerator PlayActAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/fool");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/fool");

        yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: sprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                shakeDuration: 0.5f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: soundEffect
            )
        );
    }

    //118
    public IEnumerator PlayFootballAnimation(Transform attacker, Transform receiver, bool isSuccessful)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/football");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/football");

        // Spustenie animácie
        yield return StartCoroutine(
            kamikazeAnimation.StartAnimation(
                planeSprite: sprite,
                explosionSprite: null,
                sound: sound,
                isSuccessful: isSuccessful,
                attacker: attacker,
                defender: receiver,
                finalRotation: 180f,
                cardSize: new Vector2(300f, 300f), // Predpokladaná veľkosť karty
                imageSize: new Vector2(300f, 300f), // Veľkosť obrázkov
                duration: 0.5f, // Celková doba trvania animácie
                moveDurationRatio: 1f, // Pomer času pohybu lietadla
                explosionGrowDurationRatio: 0f // Pomer času rastu explózie
            )
        );
    }

    //119
    public IEnumerator PlayBicycleKickAnimation(Transform attacker, Transform receiver)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/bicyclekick");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/bicyclekick");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: sprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.5f,
                startSound: sound,
                initialRotation: 0f,
                finalRotation: 90f
            )
        );
    }

    //119
    public IEnumerator PlayBicycleKickFailAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/bicyclekickfail");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/bicyclekickfail");

        yield return StartCoroutine(
            risingEnlargeImageAnimation.StartRisingEnlargeAnimation(
                sprite: sprite,
                targetCard: targetCard,
                startSize: new Vector2(200f, 200f),
                endSize: new Vector2(300f, 300f),
                duration: 0.5f,
                startVerticalOffset: 200f, // Počiatočný vertikálny posun
                endVerticalOffset: 0f, // Konečný vertikálny posun
                startRotation: 0f,
                endRotation: -90f,
                soundEffect: sound
            )
        );
    }

    //120
    public IEnumerator PlayWorldChampionAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/worldchampion");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/worldchampion");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                soundEffect: sound
            )
        );
    }

    //123
    public IEnumerator PlayCurseAnimation(Transform attacker, Transform defender)
    {
        // Načítanie obrázkov a zvukov
        Sprite planeSprite = Resources.Load<Sprite>("Game/Animations/cursecat");
        Sprite explosionSprite = Resources.Load<Sprite>("Game/Animations/cursemirror");

        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/curse");

        // Spustenie animácie
        yield return StartCoroutine(
            kamikazeAnimation.StartAnimation(
                planeSprite: planeSprite,
                explosionSprite: explosionSprite,
                sound: soundEffect,
                isSuccessful: true,
                attacker: attacker,
                defender: defender,
                finalRotation: 180f,
                cardSize: new Vector2(300f, 300f), // Predpokladaná veľkosť karty
                imageSize: new Vector2(300f, 300f), // Veľkosť obrázkov
                duration: 1f, // Celková doba trvania animácie
                moveDurationRatio: 0.5f, // Pomer času pohybu lietadla
                explosionGrowDurationRatio: 0.2f // Pomer času rastu explózie
            )
        );
    }

    ///////////////----------------------------------------------------------------------------------------------------------------------------BASIC-----------------------------------------------------------------------------------------------------------------------------------------------------

    public IEnumerator PlayAnimationNotImpressed(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/notimpressed");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/notimpressed");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(400f, 400f),
                duration: 1f,
                soundEffect: effectSound
            )
        );
    }

    public IEnumerator PlayAnimationImpressed(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/impressed");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/impressed");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(400f, 400f),
                duration: 1f,
                soundEffect: effectSound
            )
        );
    }

    public IEnumerator PlayAnimationNotEffective(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/pokerface");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/pokerface");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(300f, 300f),
                endSize: new Vector2(300f, 300f),
                duration: 0.7f,
                soundEffect: effectSound
            )
        );
    }

    ///////////////----------------------------------------------------------------------------------------------------------------------------EFECTS-----------------------------------------------------------------------------------------------------------------------------------------------------



    //1e
    public IEnumerator PlayBleedStartAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/bleedstart");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/bleedstart");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: targetCard,
                startSize: new Vector2(300f, 300f),
                endSize: new Vector2(300f, 300f),
                duration: 0.5f,
                soundEffect: soundEffect
            )
        );
    }

    //1e
    public IEnumerator PlayBleedContinueAnimation(Transform targetCard, int drops)
    {
        Sprite[] sprites = new Sprite[]
        {
            Resources.Load<Sprite>("Game/Animations/bleedcontinue"),
        };
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/bleedcontinue");

        yield return StartCoroutine(
            bleedAnimation.StartAnimation(
                sprites: sprites,
                soundEffect: soundEffect,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 300f), // Predpokladaná veľkosť karty
                duration: 0.5f, // Celková doba trvania animácie
                numberOfImages: drops, // Number of images you want to spawn
                imageSize: new Vector2(100f, 100f), // Size of the generated images
                scatterSpeed: 400f // Speed at which the images scatter
            )
        );
    }

    //2e
    public IEnumerator PlayAscetismStartAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/ascetismstart");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/ascetismstart");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 1f,
                soundEffect: effectSound
            )
        );
    }

    //2e
    public IEnumerator PlayAscetismEndAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/ascetismend");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/ascetismend");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 1f,
                soundEffect: effectSound
            )
        );
    }

    //3e
    public IEnumerator PlayDrunkAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[1];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/drunk");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/drunk");

        Vector2 initialSize = new Vector2(50f, 50f); // Počiatočná veľkosť obrázka
        Vector2 finalSize = new Vector2(300f, 300f);   // Konečná veľkosť obrázka

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 1,
                totalImages: 10,
                startSize: initialSize,
                endSize: finalSize,
                imageLifetime: 0.5f,
                soundEffect: effectSound
            )
        );
    }

    //3e
    public IEnumerator PlayKnockoutAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/knockout");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/knockout");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(0f, 0f),
                endSize: new Vector2(350f, 350f),
                duration: 1f,
                soundEffect: effectSound
            )
        );
    }

    //3e
    public IEnumerator PlayBoredomAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/boredom");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/boredom");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 1f,
                soundEffect: effectSound
            )
        );
    }

    //3e
    public IEnumerator PlaySleepAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/sleep");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/sleep");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 1f,
                soundEffect: effectSound
            )
        );
    }

    //3e
    public IEnumerator PlaySleepEndAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/sleepend");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/sleepend");

        // Spustenie animácie
        yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                shakeDuration: 0.5f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: soundEffect
            )
        );
    }

    //4e
    public IEnumerator PlayExposureStartAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/exposure");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/exposurestart");

        // Spustenie animácie
        yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                shakeDuration: 0.5f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: soundEffect
            )
        );
    }

    //4e
    public IEnumerator PlayExposureAnimation(Transform position)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/exposure");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/exposure");

        yield return StartCoroutine(
            wirelessChargerAnimation.StartAnimation(
                sprite1: sprite,
                sprite2: sprite,
                position: position,
                imageSize: new Vector2(300f, 300f),
                duration: 1f,
                startSound: startSound,
                switchRatio: 0.3f,
                fadeDurationRatio: 0.4f
            )
        );
    }

    //4e
    public IEnumerator PlayExposureEndAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/exposureend");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/exposureend");

        yield return StartCoroutine(
            risingEnlargeImageAnimation.StartRisingEnlargeAnimation(
                sprite: sprite,
                targetCard: targetCard,
                startSize: new Vector2(200f, 200f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                startVerticalOffset: -200f, // Počiatočný vertikálny posun
                endVerticalOffset: 200f, // Konečný vertikálny posun
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: sound
            )
        );
    }

    //5e
    public IEnumerator PlaySiegeContinueAnimation(Transform attacker, Transform receiver)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/siegeequipement");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/siegeequipement");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: sprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.7f,
                startSound: sound,
                initialRotation: 5f,
                finalRotation: -5f
            )
        );
    }

    //5e
    public IEnumerator PlaySiegeEndAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/siege");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/siege");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(400f, 400f),
                duration: 1f,
                soundEffect: effectSound
            )
        );
    }

    //6e
    public IEnumerator PlayFuryEndAnimation(Transform attacker)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/furyend");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/furyend");

        yield return StartCoroutine(
            rotatingEnlargeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: attacker,
                startSize: new Vector2(350f, 350f),
                endSize: new Vector2(100f, 100f),
                duration: 1f,
                rotationSpeed: 360f,
                soundEffect: startSound
            )
        );
    }

    //7e
    public IEnumerator PlayFamineContinueAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/faminecontinue");
        AudioClip sound = Resources.Load<AudioClip>("Sounds/Game/Animations/faminecontinue");

        yield return StartCoroutine(
            risingEnlargeImageAnimation.StartRisingEnlargeAnimation(
                sprite: sprite,
                targetCard: targetCard,
                startSize: new Vector2(200f, 200f),
                endSize: new Vector2(300f, 300f),
                duration: 0.5f,
                startVerticalOffset: -200f, // Počiatočný vertikálny posun
                endVerticalOffset: 0f, // Konečný vertikálny posun
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: sound
            )
        );
    }

    //7e
    public IEnumerator PlayFamineEndAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/famineend");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/famineend");

        // Spustenie animácie
        yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 0.5f,
                shakeDuration: 0f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: soundEffect
            )
        );
    }

    //8e
    public IEnumerator PlayElectricityStartAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/electricitystart");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/electricitystart");

        // Spustenie animácie
        yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                shakeDuration: 0.5f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: soundEffect
            )
        );
    }

    //8e
    public IEnumerator PlayElectricityAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[1];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/electricity");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/electricity");

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 3,
                totalImages: 20,
                startSize: new Vector2(50f, 50f),
                endSize: new Vector2(100f, 100f),
                soundEffect: effectSound
            )
        );
    }

    //8e
    public IEnumerator PlayElectricityEndAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[1];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/electricity");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/electricityend");

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty + 10%
                duration: 0.7f,
                spawnIntensity: 1,
                totalImages: 3,
                startSize: new Vector2(50f, 50f),
                endSize: new Vector2(100f, 100f),
                soundEffect: effectSound
            )
        );
    }

    //9e
    public IEnumerator PlayTetherAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/tether");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/tether");

        // Spustenie animácie
        yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                shakeDuration: 0.8f,
                startRotation: 0.3f,
                endRotation: 0f,
                soundEffect: soundEffect
            )
        );
    }

    //9e
    public IEnumerator PlayTetherEndAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/tetherend");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/tetherend");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: effectSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(400f, 400f),
                duration: 0.7f,
                soundEffect: effectSound
            )
        );
    }

    //12e
    public IEnumerator PlayBlocadeWaitAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/continentalblocade");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/blocadewait");

        // Spustenie animácie
        yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                shakeDuration: 0.1f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: soundEffect
            )
        );
    }

    //12e
    public IEnumerator PlayBlocadeEndAnimation(Transform targetCard)
    {
        Sprite sprite1 = Resources.Load<Sprite>("Game/Animations/continentalblocade");
        Sprite sprite2 = Resources.Load<Sprite>("Game/Animations/blocadeend");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/blocadeend");

        yield return StartCoroutine(
            dualImageAnimation.StartAnimation(
                sprite1: sprite1,
                sprite2: sprite2,
                position: targetCard,
                imageSize: new Vector2(300f, 300f), // Predpokladaná veľkosť pohára
                duration: 1f, // Celková doba trvania animácie
                startSound: startSound,
                switchRatio: 0.5f // V polovici animácie sa zmení na fľašu vína
            )
        );
    }

    //13e
    public IEnumerator PlayDepressionStartAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/depressionstart");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/depressionstart");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                soundEffect: soundEffect
            )
        );
    }

    //13e
    public IEnumerator PlayDepressionEndAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/depressionend");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/depressionend");

        yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: sprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                shakeDuration: 0.2f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: soundEffect
            )
        );
    }

    //14e
    public IEnumerator PlayArtInspirationWaitAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/artinspirationwait");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/artinspirationwait");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                soundEffect: soundEffect
            )
        );
    }

    //14e
    public IEnumerator PlayArtInspirationEndAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/artinspirationend");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/artinspirationend");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                soundEffect: soundEffect
            )
        );
    }

    //14e
    public IEnumerator PlayArtInspirationEndEnemyAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/artinspirationendenemy");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/artinspirationendenemy");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                soundEffect: soundEffect
            )
        );
    }

    //14e
    public IEnumerator PlayArtInspirationEndAttackAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/artinspitationendattack");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/artinspitationendattack");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(250f, 250f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 100f, // napr. -15 stupňov na začiatku
                finalRotation: -15f     // napr. 15 stupňov na konci
            )
        );
    }

    //14e
    public IEnumerator PlaySelfIsolationAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/selfisolation");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/selfisolation");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.5f,
                soundEffect: soundEffect
            )
        );
    }

    //15e
    public IEnumerator PlayAutoportraitFinishAnimation(Transform attacker)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/autoportraitfinish");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/autoportraitfinish");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: attacker,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 0.7f,
                soundEffect: soundEffect
            )
        );
    }

    //16e
    public IEnumerator PlayBurnStartAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/burnstart");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/burnstart");

        yield return StartCoroutine(
            risingEnlargeImageAnimation.StartRisingEnlargeAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                startSize: new Vector2(200f, 200f),
                endSize: new Vector2(300f, 300f),
                duration: 0.5f,
                startVerticalOffset: 0f, // Počiatočný vertikálny posun
                endVerticalOffset: 100f, // Konečný vertikálny posun
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: startSound
            )
        );
    }

    //16e
    public IEnumerator PlayBurnContinueAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[]
        {
            Resources.Load<Sprite>("Game/Animations/burn"),
        };
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/burn");

        yield return StartCoroutine(
            bleedAnimation.StartAnimation(
                sprites: sprites,
                soundEffect: soundEffect,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 300f), // Predpokladaná veľkosť karty
                duration: 0.5f, // Celková doba trvania animácie
                numberOfImages: 3, // Number of images you want to spawn
                imageSize: new Vector2(100f, 100f), // Size of the generated images
                scatterSpeed: 400f // Speed at which the images scatter
            )
        );
    }

    //17e
    public IEnumerator PlayConfusionStartAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[1];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/confusionstart");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/confusionstart");

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 1,
                totalImages: 7,
                startSize: new Vector2(150f, 150f),
                endSize: new Vector2(200f, 200f),
                imageLifetime: 0.3f,
                soundEffect: effectSound
            )
        );
    }

    //17e
    public IEnumerator PlayConfusionHurtItselfAnimation(Transform attacker)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/confusionhurtitself");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/confusionhurtitself");

        yield return StartCoroutine(
            rotatingEnlargeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: attacker,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(250f, 250f),
                duration: 0.4f,
                rotationSpeed: 180f,
                soundEffect: startSound
            )
        );
    }

    //17e
    public IEnumerator PlayConfusionForgetAnimation(Transform targetCard)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/confusionstart");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/confusionforget");

        yield return StartCoroutine(
            dualImageAnimation.StartAnimation(
                sprite1: sprite,
                sprite2: sprite,
                position: targetCard,
                imageSize: new Vector2(300f, 300f), // Predpokladaná veľkosť pohára
                duration: 1f, // Celková doba trvania animácie
                startSound: startSound,
                switchRatio: 0.5f // V polovici animácie sa zmení na fľašu vína
            )
        );
    }

    //17e
    public IEnumerator PlayConfusionEndAnimation(Transform targetCard)
    {
        Sprite sprite1 = Resources.Load<Sprite>("Game/Animations/confusionstart");
        Sprite sprite2 = Resources.Load<Sprite>("Game/Animations/confusionend");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/confusionend");

        yield return StartCoroutine(
            dualImageAnimation.StartAnimation(
                sprite1: sprite1,
                sprite2: sprite2,
                position: targetCard,
                imageSize: new Vector2(300f, 300f), // Predpokladaná veľkosť pohára
                duration: 1f, // Celková doba trvania animácie
                startSound: startSound,
                switchRatio: 0.5f // V polovici animácie sa zmení na fľašu vína
            )
        );
    }

    //18e
    public IEnumerator PlaySatelliteAnimation(Transform playerCard)
    {
        Sprite satelliteSprite = Resources.Load<Sprite>("Game/Animations/satellite");
        AudioClip flyoverSound = Resources.Load<AudioClip>("Sounds/Game/Animations/satellite");

        yield return StartCoroutine(
            satelliteFlyoverAnimation.StartAnimation(
                satelliteSprite: satelliteSprite,
                playerCard: playerCard,
                imageSize: new Vector2(200f, 200f),
                duration: 1f,
                flyoverSound: flyoverSound,
                distanceAboveCard: 250f,
                travelDistance: 200f
            )
        );
    }

    //20e
    public IEnumerator PlayBuffaloHornsContinueAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/buffalohorns");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/buffalocontinue");

        // Spustenie animácie
        yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(300f, 300f),
                duration: 1f,
                shakeDuration: 0.1f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: soundEffect
            )
        );
    }

    //20e
    public IEnumerator PlayBuffaloHornsEndAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/buffalostrike");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/buffalostrike");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(300f, 300f),
                duration: 0.7f,
                startSound: startSound,
                initialRotation: 0f,
                finalRotation: 0f
            )
        );
    }

    //21e
    public IEnumerator PlayCalmStartAnimation(Transform attacker)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/calmstart");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/calmstart");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: attacker,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 1f,
                soundEffect: soundEffect
            )
        );
    }

    //21e
    public IEnumerator PlayCalmEndAnimation(Transform attacker)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/calmend");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/calmend");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: attacker,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 1f,
                soundEffect: soundEffect
            )
        );
    }

    //23e
    public IEnumerator PlayTridentAimAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/trident");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/tridentaim");

        // Spustenie animácie
        yield return StartCoroutine(
            blocadeAnimation.StartAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                startSize: new Vector2(300f, 300f),
                endSize: new Vector2(300f, 300f),
                duration: 0.6f,
                shakeDuration: 0.3f,
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: soundEffect
            )
        );
    }

    //23e
    public IEnumerator PlayTridentHitAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/trident");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/tridenthit");

        yield return StartCoroutine(
            moveImageAnimation.StartAnimation(
                sprite: animationImageSprite,
                startPoint: attacker,
                endPoint: receiver,
                imageSize: new Vector2(350f, 350f),
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 0f, // napr. -15 stupňov na začiatku
                finalRotation: 0f     // napr. 15 stupňov na konci
            )
        );
    }

    //24e
    public IEnumerator PlayPoisonStartAnimation(Transform attacker)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/poison");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/poisonstart");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: attacker,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 1f,
                soundEffect: soundEffect
            )
        );
    }

    //24e
    public IEnumerator PlayPoisonContinueAnimation(Transform targetCard)
    {
        Sprite[] sprites = new Sprite[1];
        sprites[0] = Resources.Load<Sprite>("Game/Animations/poison");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/poisoncontinue");

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 1,
                totalImages: 7,
                startSize: new Vector2(150f, 150f),
                endSize: new Vector2(200f, 200f),
                imageLifetime: 0.3f,
                soundEffect: effectSound
            )
        );
    }

    //25e
    public IEnumerator PlayFearStartAnimation(Transform attacker)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/fear");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/fear");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: attacker,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 1f,
                soundEffect: soundEffect
            )
        );
    }

    //25e
    public IEnumerator PlayFearEndAnimation(Transform attacker)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/fearend");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/fearend");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: attacker,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 1f,
                soundEffect: soundEffect
            )
        );
    }

    //26e
    public IEnumerator PlayCurseStartAnimation(Transform attacker)
    {
        Sprite sprite = Resources.Load<Sprite>("Game/Animations/cursestorm");
        AudioClip soundEffect = Resources.Load<AudioClip>("Sounds/Game/Animations/cursestorm");

        yield return StartCoroutine(
            enlargeImageAnimation.StartEnlargeAnimation(
                sprite: sprite,
                targetCard: attacker,
                startSize: new Vector2(250f, 250f),
                endSize: new Vector2(350f, 350f),
                duration: 1f,
                soundEffect: soundEffect
            )
        );
    }

    //16e
    public IEnumerator PlayCurseEndAnimation(Transform targetCard)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/curseend");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/curseend");

        yield return StartCoroutine(
            risingEnlargeImageAnimation.StartRisingEnlargeAnimation(
                sprite: animationImageSprite,
                targetCard: targetCard,
                startSize: new Vector2(200f, 200f),
                endSize: new Vector2(300f, 300f),
                duration: 0.5f,
                startVerticalOffset: 200f, // Počiatočný vertikálny posun
                endVerticalOffset: -100f, // Konečný vertikálny posun
                startRotation: 0f,
                endRotation: 0f,
                soundEffect: startSound
            )
        );
    }


}
