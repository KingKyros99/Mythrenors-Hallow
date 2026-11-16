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

    private void Start()
    {
        if (!leftGate || !rightGate || !doorCollider)
        {
            enabled = false;
            return;
        }

        Renderer leftRenderer = leftGate.GetComponent<Renderer>();
        Renderer rightRenderer = rightGate.GetComponent<Renderer>();

        if (!leftRenderer || !rightRenderer)
        {
            enabled = false;
            return;
        }

        leftStartPos = leftGate.position;
        rightStartPos = rightGate.position;
        leftStartRot = leftGate.rotation;
        rightStartRot = rightGate.rotation;

        Vector3 horizontal = Vector3.ProjectOnPlane(
            transform.right,
            Vector3.up
        ).normalized;

        if (horizontal.sqrMagnitude < 0.001f)
            horizontal = Vector3.right;

        Vector3 leftCenter = leftRenderer.bounds.center;
        Vector3 rightCenter = rightRenderer.bounds.center;

        if (Vector3.Dot(leftCenter - rightCenter, horizontal) > 0f)
            horizontal = -horizontal;

        swingAxis = Vector3.up;

        leftPivot =
            GetOuterEdge(leftRenderer.bounds, horizontal, false) +
            horizontal * leftPivotOffset;

        rightPivot =
            GetOuterEdge(rightRenderer.bounds, horizontal, true) +
            horizontal * rightPivotOffset;
    }

    private Vector3 GetOuterEdge(
        Bounds bounds,
        Vector3 axis,
        bool right)
    {
        float extent =
            Mathf.Abs(axis.x) * bounds.extents.x +
            Mathf.Abs(axis.y) * bounds.extents.y +
            Mathf.Abs(axis.z) * bounds.extents.z;

        return bounds.center + axis * extent * (right ? 1f : -1f);
    }

    private void Update()
    {
        if (!opening && !doorCollider.enabled)
        {
            opening = true;
            timer = 0f;
        }

        if (!opening) return;

        timer += Time.deltaTime;

        float progress = Mathf.SmoothStep(
            0f,
            1f,
            Mathf.Clamp01(timer / Mathf.Max(openTime, 0.01f))
        );

        float direction = reverseDirection ? -1f : 1f;
        float angle = openAngle * progress * direction;

        Swing(
            leftGate,
            leftPivot,
            leftStartPos,
            leftStartRot,
            angle
        );

        Swing(
            rightGate,
            rightPivot,
            rightStartPos,
            rightStartRot,
            -angle
        );
    }

    private void Swing(
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