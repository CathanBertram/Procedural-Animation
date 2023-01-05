using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Static
{
    public static float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }
}
