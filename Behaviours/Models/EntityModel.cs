using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct EntityModel
{
    public string Name;

    [SerializeReference]
    public IComponentData[] ComponentData;

    [SerializeReference]
    public ISharedComponentData[] SharedComponentData;
}
