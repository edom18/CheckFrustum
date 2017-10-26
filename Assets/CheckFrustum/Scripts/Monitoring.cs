using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monitoring : MonoBehaviour
{
    [Header("---- 視点設定 ----")]
    [SerializeField]
    private float _fov = 60f;

    [SerializeField]
    private float _near = 0.03f;

    [SerializeField]
    private float _far = 1000f;

    [SerializeField]
    private Collider[] _targets;

    [SerializeField]
    private Color _insideColor = Color.blue;

    [SerializeField]
    private Color _intersectColor = Color.green;

    [SerializeField]
    private Color _outsideColor = Color.red;

    private Material[] _materials;

    private float Aspect
    {
        get { return (float)Screen.width / Screen.height; }
    }

    private void Start()
    {
        _materials = new Material[_targets.Length];
        for (int i = 0; i < _targets.Length; i++)
        {
            _materials[i] = _targets[i].GetComponent<Renderer>().material;
        }
    }

    private void Update()
    {
        Matrix4x4 pmat = Matrix4x4.Perspective(_fov, Aspect, _near, _far);
        for (int i = 0; i < _targets.Length; i++)
        {
            CheckFrustum.State result = CheckFrustum.Detect(_targets[i], pmat, transform, _near, _far);
            switch (result)
            {
                case CheckFrustum.State.Outside:
                    _materials[i].color = _outsideColor;
                    break;
                case CheckFrustum.State.Inside:
                    _materials[i].color = _insideColor;
                    break;
                case CheckFrustum.State.Intersect:
                    _materials[i].color = _intersectColor;
                    break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawFrustum(Vector3.zero, _fov, _far, _near, Aspect);
    }
}
