using UnityEngine;


// from https://kylewbanks.com/blog/animating-rotations-through-code-in-unity
public class AnimatedRotation : MonoBehaviour
{

    public float Duration;

    public bool LockX;
    public bool LockY;
    public bool LockZ;

    [SerializeField] public Vector3 LastDirection { get; private set; } = Vector3.up;

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

    public bool SetDirection(Vector3 direction)
    {
        this._fromDirection = this.transform.rotation;
        this._targetDirection = Quaternion.FromToRotation(Vector3.up, direction);
        //this._targetDirection = Quaternion.AngleAxis(Vector3.Angle(this.transform.up, direction), Vector3.Cross(this.transform.up, direction));
        //this._targetDirection = Quaternion.AngleAxis(Vector3.Angle(Vector3.up, direction), Vector3.Cross(Vector3.up, direction));

        if (this._fromDirection == this._targetDirection && this.transform.up == direction)
        {
            return false;
        }

        this._timeRemaining = this.Duration;
        this.LastDirection = direction;

        return true;
    }
}