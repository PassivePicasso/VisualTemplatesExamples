using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

//[ExecuteAlways]
public class ArchetypeManager : MonoBehaviour
{
    public EntityModel[] EntityModels;

    private EntityManager entityManager;
    public EntityArchetype[] entityArchetypes;

    public Dictionary<string, EntityArchetype> ArchetypeDictionary { get; set; }
    public Dictionary<string, EntityModel> ModelDictionary { get; set; }
    private void Start()
    {
        entityManager = World.Active.EntityManager;
        ArchetypeDictionary = new Dictionary<string, EntityArchetype>();
        ModelDictionary = EntityModels.ToDictionary(kvp => kvp.Name);

        foreach (var model in EntityModels)
        {
            if (!ArchetypeDictionary.ContainsKey(model.Name))
            {
                var componentTypes = model.ComponentData.Select(cd => (ComponentType)cd.GetType())
                   .Union(model.SharedComponentData.Select(cd => (ComponentType)cd.GetType()))
                   .ToArray();

                var archetype = entityManager.CreateArchetype(componentTypes);

                ArchetypeDictionary[model.Name] = archetype;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var model = EntityModels[0];
            CreateEntity(model);
        }
    }

    public void CreateEntity(string name, int count)
    {
        if (!ArchetypeDictionary.ContainsKey(name)) return;

        for (int i = 0; i < count; i++)
            CreateEntity(ModelDictionary[name]);
    }

    private void CreateEntity(EntityModel model)
    {
        EntityArchetype archetype = ArchetypeDictionary[model.Name];

        var entity = entityManager.CreateEntity(archetype);

        var dataArray = new object[2];
        dataArray[0] = entity;

        InjectData(model, dataArray, model.ComponentData);
        InjectData(model, dataArray, model.SharedComponentData);
    }

    private void InjectData<T>(EntityModel model, object[] dataArray, IEnumerable<T> componentDatas)
    {
        foreach (var componentData in componentDatas)
        {
            MethodInfo method = null;
            switch (componentData)
            {
                case IComponentData icd:
                    method = typeof(EntityManager).GetMethod("SetComponentData");
                    break;
                case ISharedComponentData iscd:
                    Type[] methodParamTypes = new Type[] { typeof(Entity) };
                    method = typeof(EntityManager).GetMethods()
                        .Where(mi => mi.Name == "SetSharedComponentData")
                        .First(mi => mi.GetParameters()[0].ParameterType == typeof(Entity));
                    break;
                default:
                    continue;
            }
            dataArray[1] = componentData;
            var setTypedComponentData = method.MakeGenericMethod(componentData.GetType());
            setTypedComponentData.Invoke(entityManager, dataArray);
        }
    }
}
