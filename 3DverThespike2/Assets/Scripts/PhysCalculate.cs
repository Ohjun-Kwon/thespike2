using UnityEngine;
using static Constants;
public static class PhysCalculate
{
    public static float getMaxHeightBySpeed( float verticalSpeed ) {
        return Mathf.Pow(verticalSpeed,2) / (2 * gravityScale); // 최대 높이
    }
    public static float getFlightTimeBySpeed(float verticalSpeed){
        return ( verticalSpeed/gravityScale );
    }
    public static float getTimeByHeight(float H){
        if (H > 0)
            return Mathf.Sqrt(2 * H / gravityScale);
        else
            return -Mathf.Sqrt(2 * -H / gravityScale);
    }

    public static float getXFlippedDirection(float _dir) {
        float dir = 180f - _dir;
        dir = (dir + 360f) % 360f;
        return dir;
    }
    public static float getYFlippedDirection(float _dir) {
        float dir = 360f - _dir;
        dir = (dir + 360f) % 360f;
        return dir;
    }
}    
