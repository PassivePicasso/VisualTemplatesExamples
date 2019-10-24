using System;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public struct EntityModel
{
    public string Name;

    [SerializeReference]
    public IComponentData[] ComponentData;

    [SerializeReference]
    public ISharedComponentData[] SharedComponentData;
}
