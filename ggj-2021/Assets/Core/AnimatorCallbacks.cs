using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// Provides an interface to register callbacks for Animation Events, given they are named 'OnAnimEvent'
// with a string parameter defining the actual event name. 
[RequireComponent(typeof(Animator))]
public class AnimatorCallbacks : MonoBehaviour
{
  private Dictionary<string, System.Action> _callbacks = new Dictionary<string, System.Action>();

  // Add a callback to the given event name
  public void AddCallback(string eventName, System.Action callback)
  {
    System.Action eventCallback;
    if (_callbacks.TryGetValue(eventName, out eventCallback))
    {
      eventCallback += callback;
      _callbacks[eventName] = eventCallback;
    }
    else
    {
      _callbacks.Add(eventName, new System.Action(callback));
    }
  }

  // Remove a callback from the given event name
  public void RemoveCallback(string eventName, System.Action callback)
  {
    System.Action eventCallback;
    if (_callbacks.TryGetValue(eventName, out eventCallback))
    {
      eventCallback -= callback;
      _callbacks[eventName] = eventCallback;
    }
  }

  public void ClearCallbacks(string eventName)
  {
    _callbacks.Remove(eventName);
  }

  public IEnumerator WaitForEvent(string eventName)
  {
    bool triggered = false;
    System.Action eventCallback = () =>
    {
      triggered = true;
    };

    AddCallback(eventName, eventCallback);

    while (!triggered)
      yield return null;

    RemoveCallback(eventName, eventCallback);
  }

  // Called by sibling Animator
  private void OnAnimEvent(string eventName)
  {
    System.Action eventCallback;
    if (_callbacks.TryGetValue(eventName, out eventCallback))
    {
      eventCallback?.Invoke();
    }
  }
}