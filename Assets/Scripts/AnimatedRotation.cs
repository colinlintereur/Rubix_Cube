using UnityEngine;

public class AnimatedRotation : MonoBehaviour
{
  public float Duration;

  public bool LockX;
  public bool LockY;
  public bool LockZ;

  [Header("Runtime Parameters")]
  [SerializeField] private Quaternion _fromDirection;
  [SerializeField] private Quaternion _targetDirection;
  [SerializeField] private float _timeRemaining;
  [SerializeField] private Vector3 _startingEulerAngles;

  void Start()
  {
    this._startingEulerAngles = this.transform.localEulerAngles;
  }

  void Update()
  {
    if (this._timeRemaining <= 0f)
    {
      return;
    }
    this._timeRemaining -= Time.deltaTime;

    // Interpolate towards the target direction
    this.transform.rotation = Quaternion.Lerp(
        this._fromDirection,
        this._targetDirection,
        (this.Duration - this._timeRemaining) / this.Duration
    );

    // Lock axes as needed
    Vector3 eulerAngles = this.transform.localEulerAngles;
    if (LockX)
    {
      eulerAngles.x = this._startingEulerAngles.x;
    }
    if (LockY)
    {
      eulerAngles.y = this._startingEulerAngles.y;
    }
    if (LockZ)
    {
      eulerAngles.z = this._startingEulerAngles.z;
    }
    this.transform.localEulerAngles = eulerAngles;
  }

  public void SetDirection(Vector3 direction)
  {
    this._timeRemaining = this.Duration;
    this._fromDirection = this.transform.rotation;
    this._targetDirection = Quaternion.FromToRotation(
        Vector3.forward,
        direction
    );
  }
}