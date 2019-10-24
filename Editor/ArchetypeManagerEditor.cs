using System;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualTemplates;

[CustomEditor(typeof(ArchetypeManager))]
public class ArchetypeManagerEditor : Editor
{
    /// <summary>
    /// This is how we setup how we want to access uxml files, I decided to use Resources, but you could probably replace this with AssetReferences or use the AssetDatabase.
    /// The premise has been on using types to define templates, and idea taken from WPF's DataTemplates.
    /// </summary>
    static ArchetypeManagerEditor()
    {
        VisualTemplateSettings.TemplateLoader = typeName =>
        {
            VisualTreeAsset vta = Resources.Load<VisualTreeAsset>($@"Templates/{typeName}");
            return vta;
        };
    }

    /// <summary>
    /// The only standard Unity call, here we are just going to create an AutoTemplate, which is the base for the entire system.
    /// It should be assumed that all controls in VisualTemplates require that AutoTemplate is an ancestor in the VisualTree
    /// </summary>
    public override VisualElement CreateInspectorGUI()
    {
        AutoTemplate template = new AutoTemplate(this, typeof(ArchetypeManager));

        var entityModelsItemsControl = template.Q<ItemsControl>("entity-model-items-control");
        
        //adding a way to add elements to an the EntityModel items control, we use a button to add a new element.
        
        var addModelButton = template.Q<Button>("archetype-add-model");
        //pass a method into AddItem to override default unity array behavior and set values to your custom defaults.
        addModelButton.clickable = new Clickable(() => entityModelsItemsControl.AddItem(new EntityModel(), NewEntityData));

        return template;
    }



    /// <summary>
    /// We have to assign data when we call ItemsControl.AddItem, we do this by passing in a method with this signature.
    /// In this case we don't need to assign any data as we are just increasing the number of elements in an array for a struct.
    /// With Unity this copies the previous element automatically, however I wanted to clear out all the data when adding this item however
    /// </summary>
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

    /// <summary>
    /// This determines how data is pushed into a serialized property for an items control.  this is being passed to ItemsControl.AddItem
    /// We are saving [SerializedReference] values for the EntityModel.ComponentData and EntityModel.SharedComponentData fields.
    /// Because of this we need to override the default value saving behavior to saved the object into the managedReferenceValue field of the SerializedProperty
    /// </summary>
    void AssignData<T>(SerializedProperty sp, T d) => sp.managedReferenceValue = d;

    /// <summary>
    ///This is being used to setup the suggest box to populate different parts of the EntityModel when you select a result
    ///This is being called from \Editor\Resources\Templates\ArchetypeManager.uxml using the ItemsControl config-method attribute.
    ///Setting up this configuration callback isn't required for using ItemsControl as you can see when you start drilling down in the UXML template files under Editor\Resources
    /// </summary>
    public void ConfigurerEntityModelItemsControl(VisualElement element)
    {
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
