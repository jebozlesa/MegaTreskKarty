using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AttackAnimations : MonoBehaviour
{
    public MoveImage moveImageAnimation;
    public EnlargeImage enlargeImageAnimation;
    public RandomImageSpawner randomImageSpawner;
    public DualImageAnimation dualImageAnimation;
    public RandomMoveImage randomMoveImageAnimation;

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
                duration: 0.5f,
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
                duration: 0.5f,
                startSound: startSound,
                initialRotation: 180f, // napr. -15 stupňov na začiatku
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

        Vector2 initialSize = new Vector2(10f, 10f); // Počiatočná veľkosť obrázka
        Vector2 finalSize = new Vector2(50f, 50f);   // Konečná veľkosť obrázka

        yield return StartCoroutine(
            randomImageSpawner.StartRandomSpawnAnimation(
                sprites: sprites,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty + 10%
                duration: 1f,
                spawnIntensity: 3,
                spawnInterval: 0.1f,
                startSize: initialSize,
                endSize: finalSize,
                soundEffect: effectSound
            )
        );
    }



    //4
    public IEnumerator PlayForgivenessAnimation(Transform targetCard)
    {
        Sprite effectSprite = Resources.Load<Sprite>("Game/Animations/forgiveness");
        AudioClip effectSound = Resources.Load<AudioClip>("Sounds/Game/Animations/forgiveness");

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

    //5
    public IEnumerator PlayCrusadeAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/crusade");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/crusade");

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
                duration: 1f,
                startSound: startSound,
                initialRotation: 0f, // napr. -15 stupňov na začiatku
                finalRotation: 90f     // napr. 15 stupňov na konci
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
                spawnInterval: 0.4f,
                startSize: initialSize,
                endSize: finalSize,
                imageLifetime: 0.5f,
                soundEffect: effectSound
            )
        );
    }

    public IEnumerator PlayChiSauAnimation(Transform sourceCard, Transform targetCard, int numberOfAttacks)
    {
        Sprite attackSprite = Resources.Load<Sprite>("Game/Animations/chisau");
        AudioClip attackSound = Resources.Load<AudioClip>("Sounds/Game/Animations/chisau");
        
        //int numberOfAttacks = Random.Range(1, 6); // Generuje náhodné číslo medzi 1 a 5 vrátane.

        yield return StartCoroutine(
            randomMoveImageAnimation.StartRandomMoveAnimation(
                sprite: attackSprite,
                sourceCard: sourceCard,
                targetCard: targetCard,
                cardSize: new Vector2(300f, 500f), // Predpokladaná veľkosť karty
                numberOfAttacks: numberOfAttacks,
                imageSize: new Vector2(250f, 250f),  // Veľkosť obrázka útoku
                duration: 1f,
                startSound: attackSound
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
                initialRotation: 15f, // napr. -15 stupňov na začiatku
                finalRotation: -15f     // napr. 15 stupňov na konci
            )
        );
    }

    //100
    public IEnumerator PlayAxeAnimation(Transform attacker, Transform receiver)
    {
        Sprite animationImageSprite = Resources.Load<Sprite>("Game/Animations/axe");
        AudioClip startSound = Resources.Load<AudioClip>("Sounds/Game/Animations/axe");

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

    public IEnumerator PlaySleepAnimation(Transform targetCard)
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

}
