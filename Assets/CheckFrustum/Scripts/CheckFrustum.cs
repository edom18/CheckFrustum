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

        _materials = new Material[_targets.Length];
        for (int i = 0; i < _targets.Length; i++)
        {
            _materials[i] = _targets[i].GetComponent<Renderer>().material;
        }
    }

    private void ShowPlanes()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
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
}
