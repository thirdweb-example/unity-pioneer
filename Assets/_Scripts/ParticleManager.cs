using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ParticleAnimation
{
    public string identifier;
    public GameObject particle;
    public Vector3 finalPosition;
    public float delay;
    public float duration;
    public float waitDuration;
    public bool waitForever;
    public SoundName playOnStart;
    public UnityEvent onAnimationEnd;
}

public class ParticleManager : MonoBehaviour
{
    public List<ParticleAnimation> particleAnimations;

    private void Start()
    {
        foreach (var particleAnimation in particleAnimations)
        {
            StartCoroutine(AnimateParticle(particleAnimation));
        }
    }

    public void SkipAnimations()
    {
        StopAllCoroutines();
        SoundManager.Instance.StopAllSounds();
        for (int i = 0; i < particleAnimations.Count; i++)
            particleAnimations[i].onAnimationEnd?.Invoke();
    }

    private IEnumerator AnimateParticle(ParticleAnimation particleAnimation)
    {
        var initialPos = particleAnimation.particle.transform.position;

        yield return new WaitForSeconds(particleAnimation.delay);

        if (particleAnimation.playOnStart != SoundName.None)
            SoundManager.Instance.PlaySound(particleAnimation.playOnStart);

        float elapsedTime = 0f;
        while (elapsedTime < particleAnimation.duration)
        {
            particleAnimation.particle.transform.position = Vector3.Lerp(
                initialPos,
                particleAnimation.finalPosition,
                elapsedTime / particleAnimation.duration
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (particleAnimation.waitForever)
        {
            particleAnimation.onAnimationEnd?.Invoke();
            yield break;
        }

        yield return new WaitForSeconds(particleAnimation.waitDuration);

        elapsedTime = 0f;
        while (elapsedTime < particleAnimation.duration)
        {
            particleAnimation.particle.transform.position = Vector3.Lerp(
                particleAnimation.finalPosition,
                initialPos,
                elapsedTime / particleAnimation.duration
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        particleAnimation.onAnimationEnd?.Invoke();
    }
}
