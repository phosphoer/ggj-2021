using UnityEngine;

public class PlayerCharacterController : Singleton<PlayerCharacterController>
{
  [SerializeField]
  private CharacterMovementController _characterMovement = null;

  [SerializeField]
  private InteractionController _interactionController = null;

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

    if (_interactionController.ClosestInteractable != null)
    {
      // Interact with an interactable 
      if (rewiredPlayer.GetButtonDown(RewiredConsts.Action.Interact))
      {
        _interactionController.ClosestInteractable.TriggerInteraction();
      }
    }

    // Scream into or uncork a held bottle
    if (rewiredPlayer.GetButtonDown(RewiredConsts.Action.Scream))
    {
    }
  }
}