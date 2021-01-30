using UnityEngine;

public class PlayerCharacterController : Singleton<PlayerCharacterController>
{
  public ObjectHolder ObjectHolder => _objectHolder;

  [SerializeField]
  private CharacterMovementController _characterMovement = null;

  [SerializeField]
  private InteractionController _interactionController = null;

  [SerializeField]
  private ObjectHolder _objectHolder = null;

  private int _disabledStack = 0;

  private const int kRewiredPlayerId = 0;

  public void PushDisableControls()
  {
    _disabledStack += 1;
  }

  public void PopDisableControls()
  {
    _disabledStack -= 1;
  }

  private void Awake()
  {
    Instance = this;
  }

  private void Update()
  {
    Rewired.Player rewiredPlayer = Rewired.ReInput.players.GetPlayer(kRewiredPlayerId);

    float horizontalAxis = rewiredPlayer.GetAxis(RewiredConsts.Action.MoveHorizontal);
    float verticalAxis = rewiredPlayer.GetAxis(RewiredConsts.Action.MoveVertical);

    Vector3 horizontalVector = Camera.main.transform.right.WithY(0).normalized * horizontalAxis;
    Vector3 verticalVector = Camera.main.transform.forward.WithY(0).normalized * verticalAxis;

    _characterMovement.MoveVector = horizontalVector + verticalVector;


    // Interact with an interactable 
    if (rewiredPlayer.GetButtonDown(RewiredConsts.Action.Interact))
    {
      if (_objectHolder.HeldObject != null)
      {
        _objectHolder.DropObject();
      }
      else if (_interactionController.ClosestInteractable != null)
      {
        _interactionController.ClosestInteractable.TriggerInteraction();
      }
    }

    // Scream into or uncork a held bottle
    if (rewiredPlayer.GetButtonDown(RewiredConsts.Action.Scream))
    {
      if (_objectHolder.HeldObject != null)
      {
        ScreamContainer bottle = _objectHolder.HeldObject.GetComponent<ScreamContainer>();
        if (bottle != null)
        {
          bottle.ReleaseScream();
        }
      }
    }
  }
}