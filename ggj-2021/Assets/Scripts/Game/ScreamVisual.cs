using UnityEngine;

public class ScreamVisual : MonoBehaviour
{
  public ScreamSoundDefinition Scream
  {
    get { return _scream; }
    set
    {
      if (_scream != value)
      {
        _scream = value;
        _text.text = _scream.Letters.ToUpper();
        _text.color = _scream.Color;
      }
    }
  }

  [SerializeField]
  private TMPro.TMP_Text _text = null;

  [SerializeField]
  private AnimationCurve _scaleOverTime = null;

  [SerializeField]
  private float _lifeTime = 2;

  private ScreamSoundDefinition _scream;
  private float _lifeTimer = 0;

  private void OnEnable()
  {
    transform.localScale = Vector3.one * _scaleOverTime.Evaluate(0);
  }

  private void Update()
  {
    _lifeTimer += Time.deltaTime;
    float lifeT = Mathf.Clamp01(_lifeTimer / _lifeTime);

    float lifeScale = _scaleOverTime.Evaluate(lifeT);
    transform.localScale = Vector3.one * lifeScale;
    transform.position += Vector3.up * Time.deltaTime;

    if (lifeT == 1)
    {
      Destroy(gameObject);
    }
  }
}