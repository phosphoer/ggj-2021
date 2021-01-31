using UnityEngine;
using System.Collections.Generic;

public class PlayerCharacterController : Singleton<PlayerCharacterController>
{
  public ObjectHolder ObjectHolder => _objectHolder;
  public CharacterMovementController Character => _characterMovement;
  public CameraControllerPlayer CameraRig => _cameraRig;
  public Transform AIVisibilityTarget => _aiVisibilityTarget;

  [SerializeField]
  private Transform _aiVisibilityTarget = null;

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

  [SerializeField]
  private ScreamDamageable _screamDamageable = null;

  private int _disabledStack = 0;
  private CameraControllerPlayer _cameraRig;
  private bool _isSneaking;

  private const int kRewiredPlayerId = 0;

  public float ScareSanityDamage = 10.0f;
  public float ScareDuration = 2.0f;
  private float _scaredTimer = 0;

  public void PushDisableControls()
  {
    _disabledStack += 1;
  }

  public void PopDisableControls()
  {
    _disabledStack -= 1;
  }

  public void StartMixingBottles(ScreamContainer heldBottle, ScreamContainer groundBottle)
  {
    Debug.Log($"Started mixing bottles {heldBottle.name} and {groundBottle.name}");
    ScreamInventoryComponent inventoryComponent = GameUI.Instance.ScreamComposerUI.ScreamInventory;

    inventoryComponent.StartMixingBottles(heldBottle, groundBottle);
    GameUI.Instance.ScreamComposerUI.Show();
  }

  private void Awake()
  {
    Instance = this;
    _objectHolder.HoldStart += OnHoldStart;
    _objectHolder.HoldEnd += OnHoldEnd;
    _screamDamageable.ScreamedAt += OnScreamedAt;

    _interactionController.PushEnabledInteraction("pickup");
  }

  private void Start()
  {
    _cameraRig = Instantiate(_cameraRigPrefab);
    CameraControllerStack.Instance.PushController(_cameraRig);
  }

  private void Update()
  {
    Rewired.Player rewiredPlayer = Rewired.ReInput.players.GetPlayer(kRewiredPlayerId);

    if (_scaredTimer > 0)
    {
      _scaredTimer -= Time.deltaTime;
      _characterMovement.MoveVector = Vector3.zero;
      _isSneaking = false;
      _characterMovement.MoveSpeedMultiplier = 0.0f;
      _playerAnimation.CurrentLocomotionState = PlayerAnimatorController.LocomotionState.Idle;
      _playerAnimation.CurrentLocomotionSpeed = 0.0f;
    }
    else
    {
      float horizontalAxis = rewiredPlayer.GetAxis(RewiredConsts.Action.MoveHorizontal);
      float verticalAxis = rewiredPlayer.GetAxis(RewiredConsts.Action.MoveVertical);

      Vector3 horizontalVector = Camera.main.transform.right.WithY(0).normalized * horizontalAxis;
      Vector3 verticalVector = Camera.main.transform.forward.WithY(0).normalized * verticalAxis;

      _characterMovement.MoveVector = horizontalVector + verticalVector;

      _isSneaking = rewiredPlayer.GetButton(RewiredConsts.Action.Sneak);
      _characterMovement.MoveSpeedMultiplier = _isSneaking ? 0.5f : 1.0f;

      if (_characterMovement.CurrentVelocity.magnitude > 0.01f)
      {
        if (_isSneaking)
        {
          if (_objectHolder.IsHoldingObject)
            _playerAnimation.CurrentLocomotionState = PlayerAnimatorController.LocomotionState.SneakCarry;
          else
            _playerAnimation.CurrentLocomotionState = PlayerAnimatorController.LocomotionState.Sneak;
        }
        else
        {
          if (_objectHolder.IsHoldingObject)
            _playerAnimation.CurrentLocomotionState = PlayerAnimatorController.LocomotionState.JogCarry;
          else
            _playerAnimation.CurrentLocomotionState = PlayerAnimatorController.LocomotionState.Jog;
        }

        _playerAnimation.CurrentLocomotionSpeed = _characterMovement.CurrentVelocity.magnitude;
      }
      else
      {
        if (_isSneaking)
        {
          _playerAnimation.CurrentLocomotionSpeed = 0;
          if (_objectHolder.IsHoldingObject)
            _playerAnimation.CurrentLocomotionState = PlayerAnimatorController.LocomotionState.SneakCarry;
          else
            _playerAnimation.CurrentLocomotionState = PlayerAnimatorController.LocomotionState.Sneak;
        }
        else
        {
          _playerAnimation.CurrentLocomotionSpeed = 1;
          if (_objectHolder.IsHoldingObject)
            _playerAnimation.CurrentLocomotionState = PlayerAnimatorController.LocomotionState.IdleCarry;
          else
            _playerAnimation.CurrentLocomotionState = PlayerAnimatorController.LocomotionState.Idle;
        }
      }

      // Contextual interact
      if (rewiredPlayer.GetButtonDown(RewiredConsts.Action.Interact))
      {
        // Interact with something
        if (_interactionController.ClosestInteractable != null)
        {
          Interactable interactable = _interactionController.ClosestInteractable;
          interactable.TriggerInteraction();
          InteractWithObject(interactable);
        }
        // Drop or throw a held object
        else if (_objectHolder.HeldObject != null)
        {
          HoldableObject heldObject = _objectHolder.HeldObject;
          _objectHolder.DropObject();
          if (_characterMovement.CurrentVelocity.magnitude > 0.5f)
          {
            Vector3 throwForce = (_characterMovement.CurrentVelocity + Vector3.up * 3) * 3;
            heldObject.Rigidbody.AddForce(throwForce, ForceMode.VelocityChange);
            heldObject.gameObject.AddComponent<SanityRestoreInWater>();
          }
        }
      }

      // Scream into or uncork a held bottle
      if (rewiredPlayer.GetButtonDown(RewiredConsts.Action.Scream))
      {
        if (_objectHolder.IsHoldingObject)
        {
          ReleaseBottleScream();
        }
      }
    }
  }

  private void InteractWithObject(Interactable interactable)
  {
    // Pick up holdable objects
    if (interactable.InteractionType == "pickup")
    {
      HoldableObject holdable = interactable.GetComponentInParent<HoldableObject>();
      if (holdable != null)
      {
        _objectHolder.HoldObject(holdable);
        return;
      }
    }
    else if (interactable.InteractionType == "mix")
    {
      // Mix bottles
      if (_objectHolder.IsHoldingObject)
      {
        ScreamContainer heldBottle = _objectHolder.HeldObject.GetComponent<ScreamContainer>();
        ScreamContainer groundBottle = interactable.GetComponentInParent<ScreamContainer>();
        if (heldBottle != null && groundBottle != null)
        {
          StartMixingBottles(heldBottle, groundBottle);
        }
      }
    }
    else if (interactable.InteractionType == "deposit")
    {
      // Deposit bottles into bank 
      if (_objectHolder.IsHoldingObject)
      {
        ScreamContainer heldBottle = _objectHolder.HeldObject.GetComponent<ScreamContainer>();
        GameStateManager.Instance.ScreamBank.DepositScream(heldBottle.ScreamSounds);
        ReleaseBottleScream();
      }
    }
  }

  private void ReleaseBottleScream()
  {
    _playerAnimation.PlayEmote(PlayerAnimatorController.EmoteState.OpenBottle);
    ScreamContainer bottle = _objectHolder.HeldObject.GetComponent<ScreamContainer>();
    if (bottle != null)
    {
      ScreamDamageable.DoScream(bottle.ScreamSounds, bottle.transform.position, transform.forward, _screamDamageable);
      bottle.ReleaseScream();
    }
  }

  private void OnScreamedAt(IReadOnlyList<ScreamSoundDefinition> screamSounds)
  {
    _playerAnimation.PlayEmote(PlayerAnimatorController.EmoteState.Scared);
    _scaredTimer = ScareDuration;
    GameStateManager.Instance.PlayerSanity.TakeSanityDamage(ScareSanityDamage);

    if (_objectHolder.HeldObject != null)
    {
      HoldableObject heldObject = _objectHolder.HeldObject;
      _objectHolder.DropObject();
    }
  }

  private void OnHoldStart()
  {
    _interactionController.PopEnabledInteraction("pickup");
    _interactionController.PushEnabledInteraction("mix");
    _interactionController.PushEnabledInteraction("deposit");
  }

  private void OnHoldEnd()
  {
    _interactionController.PushEnabledInteraction("pickup");
    _interactionController.PopEnabledInteraction("mix");
    _interactionController.PopEnabledInteraction("deposit");
  }
}