using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEditor.XR.Interaction.Toolkit;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class FixedEditor : Editor
{
    protected string[] propertiesInBaseClass;
    protected System.Type editorType;
    protected Editor baseEditor;

    protected void OnEnable()
    {
        editorType = typeof(XRBaseInteractableEditor);// this is the editor for the base class
        EnableProps();
        // Debug.Log("enabled");
        if (baseEditor != null)
        {
            DestroyImmediate(baseEditor);
            baseEditor = null;
        }
        if (target != null && editorType != null && serializedObject != null)
        {
            baseEditor = CreateEditor(target, editorType);
        }
    }
    private void OnDisable()
    {
        if (baseEditor)
        {
            DestroyImmediate(baseEditor);
            baseEditor = null;
        }
    }
    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();
        // if (baseEditor == null && serializedObject != null && editorType != null)
        // {
            // baseEditor = CreateEditor(target, editorType);
        // }
        if (baseEditor != null && serializedObject != null)
        {
            serializedObject.Update();
            baseEditor.OnInspectorGUI();
            DrawPropertiesExcluding(serializedObject, propertiesInBaseClass);
            serializedObject.ApplyModifiedProperties();
        }
    }
    public virtual void EnableProps()
    {
    }
    public void SetProps(System.Type mytype)
    {
        propertiesInBaseClass = GetPropertiesFor(mytype.BaseType);
    }
    public static string[] GetPropertiesFor(params System.Type[] classes)
    {
        List<string> fieldNames = new List<string>();
        List<System.Type> allTypes = new List<System.Type>(classes);
        // get all base classes
        for (int i = 0; i < allTypes.Count; i++)
        {
            System.Type checkType = (System.Type)allTypes[i];
            System.Type baseType = checkType.BaseType;

            if (baseType != typeof(MonoBehaviour))
            {
                if (!allTypes.Contains(baseType))
                {
                    allTypes.Add(baseType);
                }
            }
        }
        // get all properties in the class
        foreach (var checkType in allTypes)
        {
            FieldInfo[] fields = (checkType).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < fields.Length; i++)
            {
                fieldNames.Add(fields[i].Name);
            }
        }
        fieldNames.Add("m_Script");
        return fieldNames.ToArray();
    }
}

[CustomEditor(typeof(BowString))]
public class BowStringInspector : FixedEditor
{
    public override void EnableProps()
    {
        SetProps(typeof(BowString));
        editorType = typeof(XRBaseInteractableEditor);
    }
}
[CustomEditor(typeof(BowNotch))]
public class BowNotchInspector : FixedEditor
{
    public override void EnableProps()
    {
        SetProps(typeof(BowNotch));
        editorType = typeof(XRSocketInteractorEditor);
    }
}
[CustomEditor(typeof(ArrowQuiver))]
public class ArrowQuiverInspector : FixedEditor
{
    public override void EnableProps()
    {
        SetProps(typeof(ArrowQuiver));
        editorType = typeof(XRBaseInteractableEditor);
    }
}
[CustomEditor(typeof(Bow))]
public class BowInspector : FixedEditor
{
    public override void EnableProps()
    {
        SetProps(typeof(Bow));
        editorType = typeof(XRGrabInteractableEditor);
    }
}
[CustomEditor(typeof(BaseArrow), true)]
public class ArrowInspector : FixedEditor
{
    public override void EnableProps()
    {
        SetProps(typeof(BaseArrow));
        editorType = typeof(XRGrabInteractableEditor);
    }
}

