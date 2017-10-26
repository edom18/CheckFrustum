using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckFrustum : MonoBehaviour
{
    enum Result
    {
        Outside, Inside, Intersect,
    }

    [SerializeField]
    private Collider[] _targets;

    [SerializeField]
    private Color _insideColor = Color.blue;

    [SerializeField]
    private Color _intersectColor = Color.green;

    [SerializeField]
    private Color _outsideColor = Color.red;

    private Material[] _materials;

    private void Update()
    {
        for (int i = 0; i < _targets.Length; i++)
        {
            Result result = Detect(_targets[i]);
            switch (result)
            {
                case Result.Outside:
                    _materials[i].color = _outsideColor;
                    break;
                case Result.Inside:
                    _materials[i].color = _insideColor;
                    break;
                case Result.Intersect:
                    _materials[i].color = _intersectColor;
                    break;
            }
        }
    }

    private void Start()
    {
        ShowPlanes();

        CalculateFrustumPlanes(Camera.main);

        _materials = new Material[_targets.Length];
        for (int i = 0; i < _targets.Length; i++)
        {
            _materials[i] = _targets[i].GetComponent<Renderer>().material;
        }

        Plane[] planes1 = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        Plane[] planes2 = CalculateFrustumPlanes(Camera.main);

        Debug.Log("hoge");
    }

    private void ShowPlanes()
    {
        //Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        Plane[] planes = CalculateFrustumPlanes(Camera.main);
        int i = 0;
        while (i < planes.Length)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Plane);
            p.name = "Plane " + i.ToString();
            p.transform.position = -planes[i].normal * planes[i].distance;
            p.transform.rotation = Quaternion.FromToRotation(Vector3.up, planes[i].normal);
            i++;

            p.transform.parent = Camera.main.transform;
        }
    }

    private Result Detect(Collider target)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        //Plane[] planes = CalculateFrustumPlanes(Camera.main);

        Result result = Result.Inside;

        for (int i = 0; i < planes.Length; i++)
        {
            Vector3 normal = planes[i].normal;
            Vector3 vp = GetPositivePoint(target, normal);
            Vector3 vn = GetNegativePoint(target, normal);

            // (vp - plane.pos)・normal
            float dp = planes[i].GetDistanceToPoint(vp);
            if (dp < 0)
            {
                return Result.Outside;
            }

            float dn = planes[i].GetDistanceToPoint(vn);
            if (dn < 0)
            {
                result = Result.Intersect;
            }
        }

        return result;
    }

    private Vector3 GetPositivePoint(Collider target, Vector3 normal)
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

    private Vector3 GetNegativePoint(Collider target, Vector3 normal)
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

    private Plane[] CalculateFrustumPlanes(Camera cam)
    {
        Plane[] result = new Plane[6];

        Matrix4x4 pmat = cam.projectionMatrix;

        for (int i = 0; i < 6; i++)
        {
            float a, b, c, d;
            int r = i / 2;
            if (i % 2 == 0)
            {
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

            result[i] = new Plane(normal, cam.transform.position);
        }

        //// for the left plane
        //{
        //    // 平面の方程式
        //    // ax + by + cz + d = 0
        //    float a = pmat[3, 0] - pmat[0, 0];
        //    float b = pmat[3, 1] - pmat[0, 1];
        //    float c = pmat[3, 2] - pmat[0, 2];
        //    float d = pmat[3, 3] - pmat[0, 3];

        //    Vector3 normal = -new Vector3(a, b, c).normalized;

        //    result[0] = new Plane(normal, cam.transform.position);
        //}

        //// for the right plane
        //{
        //    float a = pmat[3, 0] + pmat[0, 0];
        //    float b = pmat[3, 1] + pmat[0, 1];
        //    float c = pmat[3, 2] + pmat[0, 2];
        //    float d = pmat[3, 3] + pmat[0, 3];

        //    Vector3 normal = -new Vector3(a, b, c).normalized;

        //    result[1] = new Plane(normal, cam.transform.position);
        //}

        //// for the bottom plane
        //{
        //    float a = pmat[3, 0] - pmat[1, 0];
        //    float b = pmat[3, 1] - pmat[1, 1];
        //    float c = pmat[3, 2] - pmat[1, 2];
        //    float d = pmat[3, 3] - pmat[1, 3];

        //    Vector3 normal = -new Vector3(a, b, c).normalized;

        //    result[2] = new Plane(normal, cam.transform.position);
        //}

        //// for the top plane
        //{
        //    float a = pmat[3, 0] + pmat[1, 0];
        //    float b = pmat[3, 1] + pmat[1, 1];
        //    float c = pmat[3, 2] + pmat[1, 2];
        //    float d = pmat[3, 3] + pmat[1, 3];

        //    Vector3 normal = -new Vector3(a, b, c).normalized;

        //    result[3] = new Plane(normal, cam.transform.position);
        //}

        //// for the near plane
        //{
        //    float a = pmat[3, 0] - pmat[2, 0];
        //    float b = pmat[3, 1] - pmat[2, 1];
        //    float c = pmat[3, 2] - pmat[2, 2];
        //    float d = pmat[3, 3] - pmat[2, 3];

        //    Vector3 normal = -new Vector3(a, b, c).normalized;

        //    result[4] = new Plane(normal, cam.transform.position);
        //}

        //// for the far plane
        //{
        //    float a = pmat[3, 0] + pmat[2, 0];
        //    float b = pmat[3, 1] + pmat[2, 1];
        //    float c = pmat[3, 2] + pmat[2, 2];
        //    float d = pmat[3, 3] + pmat[2, 3];

        //    Vector3 normal = -new Vector3(a, b, c).normalized;

        //    result[5] = new Plane(normal, cam.transform.position);
        //}

        return result;
    }

    //private void CalculateFrustumPlanes(Camera cam)
    //{
    //    float halfFov = cam.fieldOfView * 0.5f;
    //    float near = cam.nearClipPlane;
    //    float far = cam.farClipPlane;
    //    float d = ((far - near) * 0.5f) + near;
    //    float h = d / Mathf.Cos(halfFov);

    //    // near plane
    //    GameObject np = GameObject.CreatePrimitive(PrimitiveType.Plane);
    //    np.name = "NearPlane";
    //    np.transform.position = cam.transform.position + (cam.transform.forward * near);
    //    np.transform.up = cam.transform.forward;

    //    // far plane
    //    GameObject fp = GameObject.CreatePrimitive(PrimitiveType.Plane);
    //    fp.name = "FarPlane";
    //    fp.transform.position = cam.transform.position + (cam.transform.forward * far);
    //    fp.transform.up = -cam.transform.forward;

    //    // left plane
    //    GameObject lp = GameObject.CreatePrimitive(PrimitiveType.Plane);
    //    lp.name = "LeftPlane";
    //    lp.transform.rotation = Quaternion.AngleAxis(-90f, lp.transform.forward) * Quaternion.AngleAxis(-halfFov, -lp.transform.right);// * cam.transform.rotation;
    //    //lp.transform.position = cam.transform.position + (lp.transform.right * h);
    //}
}
