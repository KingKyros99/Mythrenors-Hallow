using UnityEngine;

public class BossGateSwing : MonoBehaviour
{
    [Header("Gate Doors")]
    public Transform leftGate;
    public Transform rightGate;

    [Header("Teammate's Door Collider")]
    public Collider doorCollider;

    [Header("Opening Settings")]
    public float openAngle = 90f;
    public float openTime = 1.5f;
    public bool reverseDirection = false;

    [Header("Pivot Adjustments")]
    public float leftPivotOffset = 0f;
    public float rightPivotOffset = 0f;

    private Vector3 leftPivot;
    private Vector3 rightPivot;
    private Vector3 leftStartPos;
    private Vector3 rightStartPos;
    private Quaternion leftStartRot;
    private Quaternion rightStartRot;
    private Vector3 swingAxis;

    private float timer;
    private bool opening;

    void Start()
    {
        if (!leftGate || !rightGate || !doorCollider)
        {
            Debug.LogError("BossGateSwing: Missing references!");
            enabled = false;
            return;
        }

        Renderer leftR = leftGate.GetComponent<Renderer>();
        Renderer rightR = rightGate.GetComponent<Renderer>();

        if (!leftR || !rightR)
        {
            Debug.LogError("BossGateSwing: Missing door renderers!");
            enabled = false;
            return;
        }

        leftStartPos = leftGate.position;
        rightStartPos = rightGate.position;
        leftStartRot = leftGate.rotation;
        rightStartRot = rightGate.rotation;

        // Gate frame's horizontal direction.
        Vector3 horizontal = Vector3.ProjectOnPlane(
            transform.right, Vector3.up
        ).normalized;

        if (horizontal.sqrMagnitude < 0.001f)
            horizontal = Vector3.right;

        // Determine which door is physically on each side.
        Vector3 leftCenter = leftR.bounds.center;
        Vector3 rightCenter = rightR.bounds.center;

        if (Vector3.Dot(leftCenter - rightCenter, horizontal) > 0f)
            horizontal = -horizontal;

        swingAxis = Vector3.up;

        leftPivot = GetOuterEdge(leftR.bounds, horizontal, false)
                    + horizontal * leftPivotOffset;

        rightPivot = GetOuterEdge(rightR.bounds, horizontal, true)
                     + horizontal * rightPivotOffset;

        Debug.Log("BossGateSwing: Gate pivots initialized.");
    }

    Vector3 GetOuterEdge(Bounds b, Vector3 axis, bool right)
    {
        float extent =
            Mathf.Abs(axis.x) * b.extents.x +
            Mathf.Abs(axis.y) * b.extents.y +
            Mathf.Abs(axis.z) * b.extents.z;

        return b.center + axis * extent * (right ? 1f : -1f);
    }

    void Update()
    {
        if (!opening && !doorCollider.enabled)
        {
            opening = true;
            timer = 0f;
            Debug.Log("BossGateSwing: Opening!");
        }

        if (!opening) return;

        timer += Time.deltaTime;

        float t = Mathf.SmoothStep(
            0f, 1f,
            Mathf.Clamp01(timer / Mathf.Max(openTime, 0.01f))
        );

        float direction = reverseDirection ? -1f : 1f;
        float angle = openAngle * t * direction;

        Swing(leftGate, leftPivot, leftStartPos,
              leftStartRot, angle);

        Swing(rightGate, rightPivot, rightStartPos,
              rightStartRot, -angle);
    }

    void Swing(
        Transform door,
        Vector3 pivot,
        Vector3 startPosition,
        Quaternion startRotation,
        float angle)
    {
        Quaternion rotation =
            Quaternion.AngleAxis(angle, swingAxis);

        door.position =
            pivot + rotation * (startPosition - pivot);

        door.rotation = rotation * startRotation;
    }
}