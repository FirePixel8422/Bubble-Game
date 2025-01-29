using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DayAndNightCycle : MonoBehaviour
{
    private int _time;
    [SerializeField] private Material dayMat;
    [SerializeField] private Light sun;
    private bool _isNight;
    private float _currentExposure;

    private void Start()
    {
        StartCoroutine(Cycle());
    }

    private IEnumerator Cycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            _time++;
            switch (_time)
            {
                case < 180:
                    _currentExposure = RenderSettings.skybox.GetFloat("_Exposure");
                    RenderSettings.skybox.SetFloat("_Exposure", _currentExposure - 0.0047f);
                    sun.color = Color.Lerp(sun.color, Color.blue, 0.01f);
                    sun.intensity -= 0.005f;
                    break;
                case < 360:
                    sun.color = Color.Lerp(sun.color, Color.white, 0.01f);
                    _currentExposure = RenderSettings.skybox.GetFloat("_Exposure");
                    RenderSettings.skybox.SetFloat("_Exposure", _currentExposure + 0.0047f);
                    sun.intensity += 0.005f;
                    break;
                case 360:
                    _time = 0;
                    break;
            }
        }
    }

    public void OnApplicationQuit()
    {
        dayMat.SetFloat("_Exposure", 0.85f);
    }
}
