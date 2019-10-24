using System;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualTemplates;

[CustomEditor(typeof(ArchetypeManager))]
public class ArchetypeManagerEditor : Editor
{
    static ArchetypeManagerEditor()
    {
        VisualTemplateSettings.TemplateLoader = typeName =>
        {
            VisualTreeAsset vta = Resources.Load<VisualTreeAsset>($@"Templates/{typeName}");
            return vta;
        };
    }

    public override VisualElement CreateInspectorGUI()
    {
        AutoTemplate template = new AutoTemplate(this, typeof(ArchetypeManager));

        var entityModelsItemsControl = template.Q<ItemsControl>("entity-model-items-control");
        var addModelButton = template.Q<Button>("archetype-add-model");

        addModelButton.clickable = new Clickable(() => entityModelsItemsControl.AddItem(new EntityModel(), NewEntityData));

        return template;
    }

    void AssignData<T>(SerializedProperty sp, T d) => sp.managedReferenceValue = d;

    ///For creating a new element in the array and populating the data specifically rather than duplicating the previous element.
    ///Leave this empty if you don't want to override the default behaviour of arrays.
    void NewEntityData(SerializedProperty sp, EntityModel d)
    {
        var name = sp.FindPropertyRelative(nameof(EntityModel.Name));
        name.stringValue = "New Entity";

        var array = sp.FindPropertyRelative(nameof(EntityModel.ComponentData));
        array.ClearArray();
        array = sp.FindPropertyRelative(nameof(EntityModel.SharedComponentData));
        array.ClearArray();
        sp.serializedObject.ApplyModifiedProperties();
    }

    ///This is being used to setup the suggest box to populate different parts of the EntityModel when you select a result
    public void ConfigurerEntityModelElement(VisualElement element)
    {
        var sharedSystemStateDataComponentsItemsControl = element.Q<ItemsControl>("shared-system-data-components-items-control");
        var systemStateDataComponentsItemsControl = element.Q<ItemsControl>("system-data-components-items-control");
        var sharedDataComponentsItemsControl = element.Q<ItemsControl>("shared-data-components-items-control");
        var dataComponentsItemsControl = element.Q<ItemsControl>("data-components-items-control");

        var componentSearch = element.Q("component-search");
        if (componentSearch is SearchSuggest ss)
        {
            ss.OnSuggestedSelected += Ss_OnSuggestedSelected;
            void Ss_OnSuggestedSelected(SuggestOption pickedSuggestion)
            {
                var dataType = pickedSuggestion.data as Type;
                var instance = Activator.CreateInstance(dataType);
                switch (instance)
                {
                    case ISystemStateSharedComponentData cdi:
                        sharedSystemStateDataComponentsItemsControl.AddItem(cdi, AssignData);
                        break;
                    case ISystemStateComponentData cdi:
                        systemStateDataComponentsItemsControl.AddItem(cdi, AssignData);
                        break;
                    case ISharedComponentData cdi:
                        sharedDataComponentsItemsControl.AddItem(cdi, AssignData);
                        break;
                    case IComponentData cdi:
                        dataComponentsItemsControl.AddItem(cdi, AssignData);
                        break;
                }
            }
        }
    }

}
