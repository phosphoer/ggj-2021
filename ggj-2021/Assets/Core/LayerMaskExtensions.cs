using UnityEngine;
using System.Collections.Generic;

public static class LayerMaskExtensions
{
  public static bool ContainsLayer(this LayerMask layerMask, int layer)
  {
    return layerMask == (layerMask | (1 << layer));
  }
}