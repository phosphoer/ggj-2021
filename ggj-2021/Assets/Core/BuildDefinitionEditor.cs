// The MIT License (MIT)
// Copyright (c) 2017 David Evans @phosphoer
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildDefinition))]
[CanEditMultipleObjects]
public class BuildDefinitionEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    // This creates an error in the inspector after build about the GUI stack
    // being in an invalid state, presumably Unity doesn't appreciate being told to 
    // do a build mid-inspector rendering. Not sure how to fix but seems relatively harmless.
    if (GUILayout.Button("Build"))
    {
      long buildNumber = System.DateTimeOffset.Now.ToUnixTimeSeconds();
      foreach (Object obj in targets)
      {
        BuildDefinition buildDef = obj as BuildDefinition;
        if (buildDef != null)
        {
          buildDef.Build(buildNumber);
        }
      }
    }

    if (GUILayout.Button("Apply Settings"))
    {
      long buildNumber = System.DateTimeOffset.Now.ToUnixTimeSeconds();
      foreach (Object obj in targets)
      {
        BuildDefinition buildDef = obj as BuildDefinition;
        if (buildDef != null)
        {
          buildDef.ApplyBuildSettings(buildNumber);
        }
      }
    }
  }
}

#endif