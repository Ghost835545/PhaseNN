using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utillites {

    public static float exDirSmth = 0.9f;
    public static float exVelSmth = 0.9f;
    public static float exStrafSmth = 0.9f;
    public static float exCrouchedSmth = 0.9f;
    public static float exGaitSmth = 0.1f;
    public static float exBodiesSmth = 0.5f;

    public struct WallPoints {
        public Vector2 wallStart;
        public Vector2 wallEnd;
    }

    public static Vector3 MixDir(Vector3 from, Vector3 to, float smthVal) {

        Quaternion xQuat = Quaternion.AngleAxis((Mathf.Atan2(from.x, from.z) * Mathf.Rad2Deg), new Vector3(0.0f, 1.0f, 0.0f));
        Quaternion yQuat = Quaternion.AngleAxis((Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg), new Vector3(0.0f, 1.0f, 0.0f));
        Quaternion zQuat = Quaternion.Slerp(xQuat, yQuat, smthVal);
        return zQuat * new Vector3(0.0f, 0.0f, 1.0f);
    }

    public static Quaternion QuaterExp(Vector3 vec) {

        float w = vec.magnitude;

        // maybe error (1016)
        Quaternion quat = w < 0.01f ? Quaternion.identity : new Quaternion(             
            vec.x * (Mathf.Sin(w) / w),
            vec.y * (Mathf.Sin(w) / w),
            vec.z * (Mathf.Sin(w) / w),
            Mathf.Cos(w));
        
        return Quaternion.Normalize(quat);
    }

    public static Vector2 SegNearest(Vector2 v, Vector2 w, Vector2 p) {

        float len = Vector2.Dot((v - w), (v - w));

        if (len == 0.0f) {
            return v;
        } else {
            float t = Mathf.Clamp01(Vector2.Dot((p - v), (w - v)) / len);
            return v + (t * (w - v));
        }        
    }


}
