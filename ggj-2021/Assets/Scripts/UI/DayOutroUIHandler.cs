using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayOutroUIHandler : MonoBehaviour
{
    public float ShowDuration = 3;
    public float _timer;

    public bool IsComplete()
    {
        return _timer <= 0;
    }

    void Start()
    {
        _timer = ShowDuration;
    }

    void Update()
    {
        _timer -= Time.deltaTime;
    }
}
