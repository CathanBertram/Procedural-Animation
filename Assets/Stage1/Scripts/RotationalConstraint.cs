using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RotationalConstraint
{
    public Vector2 PitchConstraint;
    public Vector2 YawConstraint;
    public Vector2 RollConstraint;

    public void SetConstraints(Vector2 p, Vector2 y, Vector2 r)
    {
        PitchConstraint = p;
        YawConstraint = y;
        RollConstraint = r;
    }
}
