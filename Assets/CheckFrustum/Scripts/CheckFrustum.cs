using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class CheckFrustum
{
    public enum State
    {
        Outside, Inside, Intersect,
    }

    /// <summary>
    /// 対象AABBとProjection Matrixから視錐台内に入っているかの検知を行う
    /// </summary>
    /// <param name="target">AABB対象</param>
    /// <param name="pmat">Projection Matrix</param>
    /// <param name="eyeTrans">カメラ位置</param>
    /// <param name="near">カメラのNear</param>
    /// <param name="far">カメラのFar</param>
    /// <returns></returns>
    static public State Detect(Collider target, Matrix4x4 pmat, Transform eyeTrans, float near, float far)
    {
        Plane[] planes = CalculateFrustumPlanes(pmat, eyeTrans, near, far);

        State result = State.Inside;

        for (int i = 0; i < planes.Length; i++)
        {
            Vector3 normal = planes[i].normal;
            Vector3 vp = GetPositivePoint(target, normal);
            Vector3 vn = GetNegativePoint(target, normal);

            // (vp - plane.pos)・normal
            float dp = planes[i].GetDistanceToPoint(vp);
            if (dp < 0)
            {
                return State.Outside;
            }

            float dn = planes[i].GetDistanceToPoint(vn);
            if (dn < 0)
            {
                result = State.Intersect;
            }
        }

        return result;
    }

    /// <summary>
    /// 法線から一番近い点を算出する
    /// </summary>
    /// <param name="target">ターゲットとなるAABB</param>
    /// <param name="normal">算出する法線</param>
    /// <returns></returns>
    static private Vector3 GetPositivePoint(Collider target, Vector3 normal)
    {
        Bounds bounds = target.bounds;
        Vector3 result = bounds.min;

        if (normal.x > 0)
        {
            result.x += bounds.size.x;
        }
        if (normal.y > 0)
        {
            result.y += bounds.size.y;
        }
        if (normal.z > 0)
        {
            result.z += bounds.size.z;
        }

        return result;
    }

    /// <summary>
    /// 法線から一番遠い点を算出する
    /// </summary>
    /// <param name="target">ターゲットとなるAABB</param>
    /// <param name="normal">算出する法線</param>
    /// <returns></returns>
    static private Vector3 GetNegativePoint(Collider target, Vector3 normal)
    {
        Bounds bounds = target.bounds;
        Vector3 result = bounds.min;

        if (normal.x < 0)
        {
            result.x += bounds.size.x;
        }
        if (normal.y < 0)
        {
            result.y += bounds.size.y;
        }
        if (normal.z < 0)
        {
            result.z += bounds.size.z;
        }

        return result;
    }

    /// <summary>
    /// 指定されたProjection Matricsから視錐台の6面の平面を求める
    /// </summary>
    /// <param name="pmat">Projection Matrix</param>
    /// <param name="eyeTrans">カメラ位置</param>
    /// <param name="near">カメラのNear</param>
    /// <param name="far">カメラのFar</param>
    /// <returns></returns>
    static public Plane[] CalculateFrustumPlanes(Matrix4x4 pmat, Transform eyeTrans, float near, float far)
    {
        Plane[] result = new Plane[6];

        // 0: Left, 1: Right, 2: Bottm, 3: Top
        for (int i = 0; i < 4; i++)
        {
            float a, b, c, d;
            int r = i / 2;
            if (i % 2 == 0)
            {
                // 平面の方程式
                // ax + by + cz + d = 0
                a = pmat[3, 0] - pmat[r, 0];
                b = pmat[3, 1] - pmat[r, 1];
                c = pmat[3, 2] - pmat[r, 2];
                d = pmat[3, 3] - pmat[r, 3];
            }
            else
            {
                a = pmat[3, 0] + pmat[r, 0];
                b = pmat[3, 1] + pmat[r, 1];
                c = pmat[3, 2] + pmat[r, 2];
                d = pmat[3, 3] + pmat[r, 3];
            }

            Vector3 normal = -new Vector3(a, b, c).normalized;
            normal = eyeTrans.rotation * normal;

            result[i] = new Plane(normal, eyeTrans.position);
        }

        // for the near plane
        {
            float a = pmat[3, 0] + pmat[2, 0];
            float b = pmat[3, 1] + pmat[2, 1];
            float c = pmat[3, 2] + pmat[2, 2];
            float d = pmat[3, 3] + pmat[2, 3];

            Vector3 normal = -new Vector3(a, b, c).normalized;
            normal = eyeTrans.rotation * normal;

            Vector3 pos = eyeTrans.position + (eyeTrans.forward * near);
            result[4] = new Plane(normal, pos);
        }

        // for the far plane
        {
            float a = pmat[3, 0] - pmat[2, 0];
            float b = pmat[3, 1] - pmat[2, 1];
            float c = pmat[3, 2] - pmat[2, 2];
            float d = pmat[3, 3] - pmat[2, 3];

            Vector3 normal = -new Vector3(a, b, c).normalized;
            normal = eyeTrans.rotation * normal;

            Vector3 pos = eyeTrans.position + (eyeTrans.forward * near) + (eyeTrans.forward * far);
            result[5] = new Plane(normal, pos);
        }

        return result;
    }
}
