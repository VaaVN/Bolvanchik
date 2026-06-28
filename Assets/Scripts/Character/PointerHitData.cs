using UnityEngine;
public readonly struct PointerHitData
{
    public RaycastHit Hit { get; }
    public float DragDistance { get; }
    public PointerHitData(RaycastHit hit, float dragDistance)
    {
        Hit = hit;
        DragDistance = dragDistance;
    }
}
