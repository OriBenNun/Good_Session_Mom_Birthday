using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UntitledTools
{
    namespace VertexWind
    {

        //The editor for the vertex wind script
        [CustomEditor(typeof(VertexWind))]
        public class VertexWindEditor : Editor
        {

            //Creates the vertex wind inspector GUI
            public override void OnInspectorGUI()
            {

                //Finds the target script
                VertexWind script = (VertexWind)target;

                //Settings area for the vertex wind
                GUILayout.Space(20f);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(5f);

                EditorGUILayout.LabelField("Vertex Wind Settings", EditorStyles.boldLabel);
                GUILayout.Space(7f);
                GUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);

                GUIContent fieldContent = new GUIContent("Speed", "The speed of the wind");
                script.speed = EditorGUILayout.FloatField(fieldContent, script.speed);

                fieldContent = new GUIContent("Scale", "The scale factor for the wind");
                script.scale = EditorGUILayout.FloatField(fieldContent, script.scale);

                fieldContent = new GUIContent("Amount", "How far the wind will move the verticies");
                script.amount = EditorGUILayout.Vector3Field(fieldContent, script.amount);

                fieldContent = new GUIContent("Use Mesh Combination", "Combines meshes to increase performance,\nmay cause rendering issues!");
                script.useMeshCombination = EditorGUILayout.Toggle(fieldContent, script.useMeshCombination);

                GUILayout.EndVertical();
                GUILayout.Space(5f);
                GUILayout.EndVertical();
                GUILayout.Space(10f);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(5f);

                EditorGUILayout.LabelField("Object Selection", EditorStyles.boldLabel);
                GUILayout.Space(7f);
                GUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);

                fieldContent = new GUIContent("Advanced Selection", "Shows more options for selecting objects");
                script.showAdvancedSelection = EditorGUILayout.ToggleLeft(fieldContent, script.showAdvancedSelection);

                //All of the different advanced search options
                if (script.showAdvancedSelection)
                {
                    GUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);
                    fieldContent = new GUIContent("Add child objects", "Adds all child objects");
                    if (GUILayout.Button(fieldContent))
                        script.ObjsAddChildren();

                    fieldContent = new GUIContent("Add this object", "Adds \"" + script.gameObject.name + "\"");
                    if (GUILayout.Button(fieldContent))
                        script.ObjsAddCurrent();

                    fieldContent = new GUIContent("Add selected objects", "Adds all of the selected objects in the editor");
                    if (GUILayout.Button(fieldContent))
                        script.ObjsAddSelected();

                    if (script.objectTag != null && script.objectTag != string.Empty)
                        fieldContent = new GUIContent("Add objects with tag \"" + script.objectTag + "\"", "Adds all objects with the tag \"" + script.objectTag + "\"");
                    else
                        fieldContent = new GUIContent("Add objects by tag", "Adds all objects with a specific tag");
                    if (GUILayout.Button(fieldContent))
                    {
                        if (script.objectTag != null && script.objectTag != string.Empty)
                            script.ObjsAreTagged();
                        else
                            EditorUtility.DisplayDialog("Vertex Wind Editor", "Please set an object tag before searching for any objects with a tag.", "Okay");
                    }

                    fieldContent = new GUIContent("Search tag", "The tag used by the \"Objects are tagged\" button");
                    script.objectTag = EditorGUILayout.TextField(fieldContent, script.objectTag);
                    GUILayout.EndVertical();
                }

                GUILayout.Space(10f);

                //Area for adding an object to the list
                GUILayout.BeginHorizontal();
                fieldContent = new GUIContent("Add", "Adds the selected object to the objects list");
                if (GUILayout.Button(fieldContent))
                    script.objs.Add(script.selectedObj);
                script.selectedObj = (MeshFilter)EditorGUILayout.ObjectField(script.selectedObj, typeof(MeshFilter), true);
                GUILayout.EndHorizontal();

                //Area for displaying and clearing the object list
                GUILayout.BeginHorizontal();
                fieldContent = new GUIContent("Show Objects", "Shows the list of selected objects");
                script.showObjectsList = EditorGUILayout.ToggleLeft(fieldContent, script.showObjectsList);
                fieldContent = new GUIContent("Clear", "Clears all objects from the list");
                if (GUILayout.Button(fieldContent))
                    script.objs = new List<MeshFilter>();
                GUILayout.EndHorizontal();

                //Shows the object list
                if (script.showObjectsList)
                {
                    GUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                    fieldContent = new GUIContent("Object Count: " + script.objs.Count, "The number of objects being used by the script");
                    EditorGUILayout.LabelField(fieldContent);
                    for (int i = 0; i < script.objs.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        script.objs[i] = (MeshFilter)EditorGUILayout.ObjectField(script.objs[i], typeof(MeshFilter), true);
                        fieldContent = new GUIContent("Remove", "Removes " + script.objs[i] + " from the objects list");
                        if (GUILayout.Button(fieldContent))
                            script.objs.RemoveAt(i);
                        EditorGUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }

                GUILayout.EndVertical();
                GUILayout.Space(5f);
                GUILayout.EndVertical();
                GUILayout.Space(10f);

            }

        }

    }
}
