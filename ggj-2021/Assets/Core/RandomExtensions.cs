using UnityEngine;

public static class RandomExtensions
{
  public static float NextFloat(this System.Random rand)
  {
    return (float)rand.NextDouble();
  }

  public static double NextDoubleRanged(this System.Random rand, double min, double max)
  {
    return min + rand.NextDouble() * (max - min);
  }

  public static float NextFloatRanged(this System.Random rand, float min, float max)
  {
    return min + (float)rand.NextDouble() * (max - min);
  }

  public static int NextIntRanged(this System.Random rand, int min, int max)
  {
    return min + Mathf.FloorToInt(rand.NextFloat() * (max - min));
  }

  public static Vector2 NextPointInsideCircle(this System.Random random)
  {
    // https://stackoverflow.com/questions/5837572/generate-a-random-point-within-a-circle-uniformly/50746409#50746409
    float r = Mathf.Sqrt(random.NextFloat());
    float theta = random.NextFloat() * 2 * Mathf.PI;
    float x = Mathf.Cos(theta) * r;
    float y = Mathf.Sin(theta) * r;
    return new Vector2(x, y);
  }
}