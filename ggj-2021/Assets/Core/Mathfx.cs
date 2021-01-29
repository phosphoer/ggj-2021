// The MIT License (MIT)
// Copyright (c) 2016 David Evans @phosphoer
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// This is based off the Mathfx from Unity Wiki but I added my own stuff that I find useful and removed stuff that was redundant with Unity and added some docs

using UnityEngine;

public static class Mathfx
{
  // Interpolation functions 
  public static float Hermite(float start, float end, float value)
  {
    return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
  }

  public static float Sinerp(float start, float end, float value)
  {
    return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
  }

  public static float SmoothSin(float start, float end, float value)
  {
    return Mathf.Lerp(start, end, Mathf.Sin(value * 2 * Mathf.PI + Mathf.PI * 0.5f));
  }

  public static float Coserp(float start, float end, float value)
  {
    return Mathf.Lerp(start, end, 1.0f - Mathf.Cos(value * Mathf.PI * 0.5f));
  }

  public static float Berp(float start, float end, float value)
  {
    value = Mathf.Clamp01(value);
    value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
    return start + (end - start) * value;
  }

  public static float SmoothStep(float x, float min, float max)
  {
    x = Mathf.Clamp(x, min, max);
    float v1 = (x - min) / (max - min);
    float v2 = (x - min) / (max - min);
    return -2 * v1 * v1 * v1 + 3 * v2 * v2;
  }

  // Project a point onto a plane, not necessarily at origin
  // Vector3.ProjectOnPlane assumes origin for the plane
  public static Vector3 ProjectPointOnPlane(Vector3 point, Plane plane)
  {
    return Vector3.ProjectOnPlane(point, plane.normal) + plane.normal * plane.distance;
  }

  // Nearest point on an infinite line to a point
  public static Vector3 NearestPointLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
  {
    Vector3 lineDirection = Vector3.Normalize(lineEnd - lineStart);
    float closestPoint = Vector3.Dot((point - lineStart), lineDirection) / Vector3.Dot(lineDirection, lineDirection);
    return lineStart + (closestPoint * lineDirection);
  }

  // Nearest point on a line, clamped to the endpoints of the line segment
  public static Vector3 NearestPointSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
  {
    Vector3 fullDirection = lineEnd - lineStart;
    Vector3 lineDirection = Vector3.Normalize(fullDirection);
    float closestPoint = Vector3.Dot((point - lineStart), lineDirection) / Vector3.Dot(lineDirection, lineDirection);
    return lineStart + (Mathf.Clamp(closestPoint, 0.0f, Vector3.Magnitude(fullDirection)) * lineDirection);
  }

  public static float Bounce(float x)
  {
    return Mathf.Abs(Mathf.Sin(6.28f * (x + 1f) * (x + 1f)) * (1f - x));
  }

  // test for value that is near specified float (due to floating point inprecision)
  // all thanks to Opless for this!
  public static bool Approx(float val, float about, float range)
  {
    return ((Mathf.Abs(val - about) < range));
  }

  public static float CeilNormal(float val)
  {
    if (val > 0)
      return 1.0f;
    else if (val < 0)
      return -1.0f;
    return 0.0f;
  }

  // test if a Vector3 is close to another Vector3 (due to floating point inprecision)
  // compares the square of the distance to the square of the range as this 
  // avoids calculating a square root which is much slower than squaring the range
  public static bool Approx(Vector3 val, Vector3 about, float range)
  {
    return ((val - about).sqrMagnitude < range * range);
  }

  // Dampening functions, stateless-ly and frame independently interpolate towards a value
  public static float Damp(float source, float target, float smoothing, float dt, float snapEpsilon = 0.01f)
  {
    float val = Mathf.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
    if (Mathf.Abs(val - target) < snapEpsilon)
      val = target;

    return val;
  }

  public static Vector4 Damp(Vector4 source, Vector4 target, float smoothing, float dt)
  {
    return Vector4.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
  }

  public static Vector3 Damp(Vector3 source, Vector3 target, float smoothing, float dt)
  {
    return Vector3.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
  }

  public static Vector2 Damp(Vector2 source, Vector2 target, float smoothing, float dt)
  {
    return Vector2.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
  }

  public static Color Damp(Color source, Color target, float smoothing, float dt)
  {
    return Color.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
  }

  public static Quaternion Damp(Quaternion source, Quaternion target, float smoothing, float dt)
  {
    return Quaternion.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
  }

  /*
     * CLerp - Circular Lerp - is like lerp but handles the wraparound from 0 to 360.
     * This is useful when interpolating eulerAngles and the object
     * crosses the 0/360 boundary.  The standard Lerp function causes the object
     * to rotate in the wrong direction and looks stupid. Clerp fixes that.
  */
  public static float Clerp(float start, float end, float value)
  {
    float min = 0.0f;
    float max = 360.0f;
    float half = Mathf.Abs((max - min) / 2.0f);//half the distance between min and max
    float retval = 0.0f;
    float diff = 0.0f;

    if ((end - start) < -half)
    {
      diff = ((max - start) + end) * value;
      retval = start + diff;
    }
    else if ((end - start) > half)
    {
      diff = -((max - end) + start) * value;
      retval = start + diff;
    }
    else retval = start + (end - start) * value;

    return retval;
  }

  public static Vector3 ViewportToCanvasPosition(RectTransform canvas, Vector3 viewportPos)
  {
    viewportPos.x *= canvas.rect.size.x;
    viewportPos.y *= canvas.rect.size.y;
    viewportPos.x -= canvas.rect.size.x * canvas.pivot.x;
    viewportPos.y -= canvas.rect.size.y * canvas.pivot.y;
    return viewportPos;
  }

  public static Vector3 WorldToCanvasPosition(RectTransform canvas, Camera camera, Vector3 worldPos)
  {
    Vector3 pos = camera.WorldToViewportPoint(worldPos);
    pos = ViewportToCanvasPosition(canvas, pos);
    return pos;
  }

  public static Vector3 CanvasToWorldPosition(RectTransform canvas, Camera camera, Vector2 canvasLocalPos)
  {
    Vector2 viewportPoint = canvasLocalPos + canvas.rect.size * canvas.pivot;
    viewportPoint /= canvas.rect.size;
    Vector3 worldPos = camera.ViewportToWorldPoint(viewportPoint, Camera.MonoOrStereoscopicEye.Mono);
    return worldPos;
  }

  public static bool IsPointInViewport(Vector3 worldPosition, Camera camera)
  {
    Vector3 viewportPos = camera.WorldToViewportPoint(worldPosition);
    return viewportPos.x > 0 && viewportPos.x < 1 &&
           viewportPos.y > 0 && viewportPos.y < 1;
  }

  // https://answers.unity.com/questions/283192/how-to-convert-decibel-number-to-audio-source-volu.html
  public static float LinearToDecibel(float linear)
  {
    float dB;

    if (linear != 0)
      dB = 20.0f * Mathf.Log10(linear);
    else
      dB = -144.0f;

    return dB;
  }

  public static float DecibelToLinear(float dB)
  {
    float linear = Mathf.Pow(10.0f, dB / 20.0f);

    return linear;
  }

  public static Bounds NegativeBounds()
  {
    Bounds negativeBounds = new Bounds();
    negativeBounds.min = Vector3.positiveInfinity;
    negativeBounds.max = Vector3.negativeInfinity;
    return negativeBounds;
  }
}

public static class VectorExtensions
{
  // Get a copy of this vector with a given value for x/y/z
  // My primary use case is getting a forward vector whose y is 0 
  public static Vector2 WithX(this Vector2 v, float x)
  {
    v.x = x;
    return v;
  }

  public static Vector2 WithY(this Vector2 v, float y)
  {
    v.y = y;
    return v;
  }

  public static Vector3 WithX(this Vector3 v, float x)
  {
    v.x = x;
    return v;
  }

  public static Vector3 WithY(this Vector3 v, float y)
  {
    v.y = y;
    return v;
  }

  public static Vector3 WithZ(this Vector3 v, float z)
  {
    v.z = z;
    return v;
  }
}