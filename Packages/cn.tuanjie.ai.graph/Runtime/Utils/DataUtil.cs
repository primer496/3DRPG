using System;

namespace UnityEngine.AIGraph
{
    public static class DataUtil
    {
        public static bool IsFloatZero(float value, float eps = 1e-6f)
        {
            return Math.Abs(value) < eps;
        }
    }
}