using UnityEngine;

public class PlayerCharacterController : Singleton<PlayerCharacterController>
{
  public ObjectHolder ObjectHolder => _objectHolder;
  public CharacterMovementController Character => _characterMovement;
  public CameraControllerPlayer CameraRig => _cameraRig;

  [SerializeField]
  private CharacterMovementController _characterMovement = null;

  [SerializeField]
  private PlayerAnimatorController _playerAnimation = null;

  [SerializeField]
  private InteractionController _interactionController = null;

  [SerializeField]
  private ObjectHolder _objectHolder = null;

  [SerializeField]
  private CameraControllerPlayer _cameraRigPrefab = null;

  private int _disabledStack = 0;
  private CameraControllerPlayer _cameraRig;

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

  private void Start()
  {
    _cameraRig = Instantiate(_cameraRigPrefab);
    CameraControllerStack.Instance.PushController(_cameraRig);
  }

  private void OnDestroy()
  {
    if (_cameraRig != null)
    {
      if (CameraControllerStack.Instance != null)
        CameraControllerStack.Instance.PopController(_cameraRig);

      Destroy(_cameraRig.gameObject);
    }
  }

  private void Update()
  {
    Rewired.Player rewiredPlayer = Rewired.ReInput.players.GetPlayer(kRewiredPlayerId);

    float horizontalAxis = rewiredPlayer.GetAxis(RewiredConsts.Action.MoveHorizontal);
    float verticalAxis = rewiredPlayer.GetAxis(RewiredConsts.Action.MoveVertical);

    Vector3 horizontalVector = Camera.main.transform.right.WithY(0).normalized * horizontalAxis;
    Vector3 verticalVector = Camera.main.transform.forward.WithY(0).normalized * verticalAxis;

    _characterMovement.MoveVector = horizontalVector + verticalVector;

    if (_characterMovement.CurrentVelocity.magnitude > 0.01f)
    {
      if (_objectHolder.IsHoldingObject)
        _playerAnimation.CurrentLocomotionState = PlayerAnimatorController.LocomotionState.JogCarry;
      else
        _playerAnimation.CurrentLocomotionState = PlayerAnimatorController.LocomotionState.Jog;

      _playerAnimation.CurrentLocomotionSpeed = _characterMovement.CurrentVelocity.magnitude;
    }
    else
    {
      if (_objectHolder.IsHoldingObject)
        _playerAnimation.CurrentLocomotionState = PlayerAnimatorController.LocomotionState.IdleCarry;
      else
        _playerAnimation.CurrentLocomotionState = PlayerAnimatorController.LocomotionState.Idle;

      _playerAnimation.CurrentLocomotionSpeed = 1;
    }

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
        _playerAnimation.PlayEmote(PlayerAnimatorController.EmoteState.OpenBottle);
        ScreamContainer bottle = _objectHolder.HeldObject.GetComponent<ScreamContainer>();
        if (bottle != null)
        {
          bottle.ReleaseScream();
        }
      }
    }
  }
}