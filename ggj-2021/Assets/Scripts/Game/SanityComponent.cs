﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SanityComponent : MonoBehaviour
{
  public bool SanityDecayActive = false;
  public float TotalSanity = 100;

  public float PursuitSanityDecayRate = 1;
  private int _enemyPursuitCount = 0;
  public int EnemyPursuitCount
  {
    get { return _enemyPursuitCount; }
  }

  public float SanityHalf = 50.0f;
  public float SanityWarning = 25.0f;
  public float SanityCritical = 10.0f;

  [SerializeField]
  private SoundBank _sanityDamageSound = null;

  private float _currentSanity;
  public float CurrentSanity
  {
    get { return _currentSanity; }
  }

  private float _aliveTimer;
  public float AliveTimer
  {
    get { return _aliveTimer; }
  }
  public bool HasSanityRemaining
  {
    get { return _currentSanity > 0.0f; }
  }

  public float SanityFraction
  {
    get { return _currentSanity / TotalSanity; }
  }

  private bool _hasDayStarted = false;

  public void OnStartedDay(int dayIndex)
  {
    _currentSanity = TotalSanity;
    _hasDayStarted = true;
  }

  public void OnCompletedDay()
  {
    _currentSanity = TotalSanity;
    _hasDayStarted = false;
  }

  public void OnPursuitStarted()
  {
    ++_enemyPursuitCount;
  }

  public void OnPursuitStopped()
  {
    _enemyPursuitCount = Mathf.Max(_enemyPursuitCount - 1, 0);
  }

  public void RestoreSanity(float amount)
  {
    _currentSanity = Mathf.Clamp(_currentSanity + amount, 0, TotalSanity);
  }

  public void TakeSanityDamage(float amount)
  {
    float previousSanity = _currentSanity;
    _currentSanity = Mathf.Max(_currentSanity - amount, 0.0f);

    AudioManager.Instance.PlaySound(_sanityDamageSound);
  }

  void Update()
  {
    if (!_hasDayStarted)
      return;

    if (SanityDecayActive && HasSanityRemaining)
    {
      float previousSanity = _currentSanity;
      float pursuitDecay = (float)_enemyPursuitCount * PursuitSanityDecayRate * Time.deltaTime;

      _currentSanity = Mathf.Max(_currentSanity - pursuitDecay, 0.0f);
    }

    if (HasSanityRemaining)
    {
      _aliveTimer += Time.deltaTime;
    }
  }
}
