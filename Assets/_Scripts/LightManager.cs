using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightManager : MonoBehaviour
{
    private Light _light;

    private void Awake()
    {
        _light = GetComponent<Light>();
        _light.intensity = 200f;
    }

    private void Start()
    {
        StartCoroutine(Dim());
    }

    private IEnumerator Dim()
    {
        while (_light.intensity > 175)
        {
            _light.intensity -= Time.deltaTime * 10f;
            yield return null;
        }
        while (_light.intensity > 0)
        {
            _light.intensity -= Time.deltaTime * 40;
            yield return null;
        }
    }
}
