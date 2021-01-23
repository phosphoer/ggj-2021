// The MIT License (MIT)
// Copyright (c) 2017 David Evans @phosphoer
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "new-sound-bank", menuName = "Sound Bank")]
public class SoundBank : ScriptableObject
{
  public AudioClip[] AudioClips;
  public bool IsSpatial;
  public float MaxDistance = 100.0f;
  public float MinDistance = 10.0f;
  public float VolumeScale = 1.0f;
  public float DopplerLevel = 0;
  public float PitchOffset = 0;
  public RangedFloat PitchOffsetRange = new RangedFloat(0, 0);
  public bool IsLooping;
  public AudioMixerGroup AudioMixerGroup;

  public AudioClip RandomClip
  {
    get
    {
      return AudioClips[Random.Range(0, AudioClips.Length)];
    }
  }
}