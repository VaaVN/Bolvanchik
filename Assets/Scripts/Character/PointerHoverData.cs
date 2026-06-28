using UnityEngine;

public readonly struct PointerHoverData
{
    public RaycastHit Hit { get; }
    public PointerHoverData(RaycastHit hit)
    {
        Hit = hit;
    }
}
