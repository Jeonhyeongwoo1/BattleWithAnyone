using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FadingModel : MonoBehaviour, IEquatable<FadingModel>
{
    public List<Material> materials => m_Materials;
    public Vector3 position;

    List<Material> m_Materials = new List<Material>();

    public bool Equals(FadingModel fadingModel)
    {
        return position.Equals(fadingModel.position);
    }

    public override int GetHashCode()
    {
        return position.GetHashCode();
    }

    // Start is called before the first frame update
    void Start()
    {
        position = transform.position;
        List<MeshRenderer> renderers = new List<MeshRenderer>();
        renderers.AddRange(GetComponentsInChildren<MeshRenderer>());
        for (int i = 0; i < renderers.Count; i++)
        {
            materials.AddRange(renderers[i].materials);
            if (!renderers[i].gameObject.TryGetComponent<FadingModel>(out var model))
            {
                renderers[i].gameObject.AddComponent<FadingModel>();
            }
        }
    }

}
