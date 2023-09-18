using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AttackAnimations : MonoBehaviour
{
    public MoveImage moveImageAnimation;
    public EnlargeImage enlargeImageAnimation;

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

    //1
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

    // Môžete pridať ďalšie metódy pre iné animácie podobne ako PlayPunchAnimation
}
