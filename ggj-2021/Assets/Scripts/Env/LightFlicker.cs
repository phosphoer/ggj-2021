using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    public Vector2 IntensityFlickerAmount = new Vector2(0f,1f);
    public Vector2 FlickerTime = new Vector2(.1f,.25f);

    public bool flickerRange = true;
    public Vector2 RangeFlickerAmount = new Vector2(3f,10f);
    
    Light fLight;
    private float timer=0f;
    private float flickTime=0f;

    // Start is called before the first frame update
    void Start()
    {
        fLight = gameObject.GetComponent<Light>();
        FlickerLight();
    }

    // Update is called once per frame
    void Update()
    {
        timer+=Time.deltaTime;
        if(timer>=flickTime)
        {
            FlickerLight();
        }
    }

    void FlickerLight()
    {
        flickTime = Random.Range(FlickerTime.x,FlickerTime.y);
        timer = 0f;

        fLight.intensity = Random.Range(IntensityFlickerAmount.x, IntensityFlickerAmount.y);
        
        if(flickerRange) fLight.range = Random.Range(RangeFlickerAmount.x, RangeFlickerAmount.y);
                

    }
}
