using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreamComposerUIHandler : UIPageBase
{
  [SerializeField]
  private Button[] _screamTrack;

  [SerializeField]
  private RMF_RadialMenu _radialMenu = null;

  protected override void Awake()
  {
    base.Awake();
    Shown += OnShown;
  }

  // Start is called before the first frame update
  private void OnShown()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }
}
