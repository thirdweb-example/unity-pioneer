using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class SpotlightController : MonoBehaviour
{
    [SerializeField]
    private float _startAngle = 0f;

    [SerializeField]
    private float _endAngle = 15f;

    [SerializeField]
    private float _duration = 1f;

    private Light _spotlight;
    private Coroutine _spotlightCoroutine;

    private void Awake()
    {
        _spotlight = GetComponent<Light>();
        _spotlight.enabled = false;
    }

    private void Start()
    {
        ToggleSpotlight(true);
    }

    public void ToggleSpotlight(bool on)
    {
        if (_spotlightCoroutine != null)
            StopCoroutine(_spotlightCoroutine);

        if (on)
        {
            _spotlight.enabled = true;
            _spotlightCoroutine = StartCoroutine(
                ChangeSpotlightAngle(_startAngle, _endAngle, _duration)
            );
        }
        else
        {
            _spotlight.enabled = false;
        }
    }

    IEnumerator ChangeSpotlightAngle(float startAngle, float endAngle, float duration)
    {
        float angle = startAngle;
        _spotlight.spotAngle = angle;

        while (angle < endAngle)
        {
            angle = Mathf.Lerp(angle, endAngle, Time.deltaTime / duration);
            _spotlight.spotAngle = angle;
            yield return null;
        }
    }
}
