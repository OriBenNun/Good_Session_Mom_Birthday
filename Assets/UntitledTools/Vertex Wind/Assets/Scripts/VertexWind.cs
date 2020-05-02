using System.Collections.Generic;
using UnityEngine;

namespace UntitledTools
{
    namespace VertexWind
    {

        //The main vertex wind script
        public class VertexWind : MonoBehaviour
        {

            //Wind effector data type
            public struct WindEffector
            {
                public Vector3 pos;
                public Vector3 strength;
                public float radius;
            }

            //General variables
            public List<MeshFilter> objs = new List<MeshFilter>();
            public float speed = 10f;
            public float scale = 1f;
            public bool useMeshCombination = true;
            public Vector3 amount = Vector3.one * 0.5f;

            //Other mesh-related variables
            private Mesh[] instancedMeshes;
            private Vector3[][] objsOriginalVerts;
            private List<MeshFilter> newObjects = new List<MeshFilter>();

            //The effector variables
            private WindEffectorRadius[] effectorObjs;
            private WindEffector[] effectors;

            //The compute shader variables
            private ComputeShader windShader;
            private int doWindCalcId;

            //Editor-specific variables
            public string objectTag = string.Empty;
            public bool showAdvancedSelection = false;
            public bool showObjectsList = true;
            public MeshFilter selectedObj;

            private void Start()
            {
                effectorObjs = FindObjectsOfType<WindEffectorRadius>();
                effectors = new WindEffector[effectorObjs.Length];

                for (int i = 0; i < effectorObjs.Length; i++)
                    effectors[i] = new WindEffector();

                windShader = Resources.Load<ComputeShader>("WindShader");
                doWindCalcId = windShader.FindKernel("DoWindCalc");

                //Sets up objects for mass vertex calculations
                if (useMeshCombination)
                {
                    instancedMeshes = CombineMeshes(objs);
                    objsOriginalVerts = new Vector3[instancedMeshes.Length][];
                    for (int i = 0; i < instancedMeshes.Length; i++)
                    {
                        instancedMeshes[i].MarkDynamic();
                        objsOriginalVerts[i] = instancedMeshes[i].vertices;
                    }

                    for (int i = 0; i < objs.Count; i++)
                        objs[i].gameObject.SetActive(false);
                }
                else
                {
                    instancedMeshes = new Mesh[objs.Count];
                    objsOriginalVerts = new Vector3[objs.Count][];
                    for (int i = 0; i < objs.Count; i++)
                    {
                        instancedMeshes[i] = Instantiate(objs[i].mesh);
                        instancedMeshes[i].MarkDynamic();
                        objsOriginalVerts[i] = instancedMeshes[i].vertices;
                    }
                }
            }

            //Updates the wind effect
            private void Update()
            {
                //Refreshes the wind effectors
                for (int i = 0; i < effectorObjs.Length; i++)
                {
                    effectors[i].pos = effectorObjs[i].transform.position;
                    effectors[i].radius = effectorObjs[i].radius;
                    effectors[i].strength = effectorObjs[i].amount;
                }

                //Calculates the new vertex positions with mesh combination
                if (useMeshCombination)
                {
                    for (int i = 0; i < instancedMeshes.Length; i++)
                    {
                        instancedMeshes[i].vertices = CalcNewVerts(objsOriginalVerts[i], newObjects[i].transform.position);
                        newObjects[i].mesh = instancedMeshes[i];
                    }
                }
                //Calculates the new vertex positions without mesh combination
                else
                {
                    for (int i = 0; i < objs.Count; i++)
                    {
                        instancedMeshes[i].vertices = CalcNewVerts(objsOriginalVerts[i], objs[i].transform.position);
                        objs[i].mesh = instancedMeshes[i];
                    }
                }
            }

            //Using a compute shader for fast, gpgpu calculations, this method calculates the vertex positions for the wind effect
            private Vector3[] CalcNewVerts(Vector3[] verts, Vector3 objectPos)
            {
                ComputeBuffer vertsBuffer = new ComputeBuffer(verts.Length, 12);
                vertsBuffer.SetData(verts);

                //Sets all of the compute shader's variables
                windShader.SetFloat("time", Time.time * speed);
                windShader.SetFloat("scale", scale);
                windShader.SetVector("amount", amount);
                windShader.SetVector("objPos", objectPos);
                windShader.SetBuffer(doWindCalcId, "verts", vertsBuffer);

                //Runs if there is one or more wind effector in the scene
                if (effectors.Length > 0)
                {
                    ComputeBuffer effectorsBuffer = new ComputeBuffer(effectors.Length, 28);
                    effectorsBuffer.SetData(effectors);

                    windShader.SetBuffer(doWindCalcId, "effectors", effectorsBuffer);
                    windShader.SetBool("effectorsExist", true);

                    windShader.Dispatch(doWindCalcId, verts.Length, effectors.Length, 1);
                    effectorsBuffer.Release();
                }
                //Runs if there are no wind effectors in the scene
                else
                {
                    ComputeBuffer effectorsBuffer = new ComputeBuffer(1, 16);

                    windShader.SetBuffer(doWindCalcId, "effectors", effectorsBuffer);
                    windShader.SetBool("effectorsExist", false);

                    windShader.Dispatch(doWindCalcId, verts.Length, 1, 1);
                    effectorsBuffer.Release();
                }

                Vector3[] newVerts = new Vector3[verts.Length];
                vertsBuffer.GetData(newVerts);
                vertsBuffer.Release();

                return newVerts;
            }

            //Combines many meshes together to get high performance
            private Mesh[] CombineMeshes(List<MeshFilter> meshes)
            {
                int meshVertexCounter = 0;
                int objCounter = 0;
                List<Mesh> returnMeshes = new List<Mesh>();
                List<CombineInstance> instances = new List<CombineInstance>();

                for (int i = 0; i < meshes.Count; i++)
                {
                    Mesh curMesh = Instantiate(meshes[i].mesh);
                    meshVertexCounter += curMesh.vertexCount;
                    if (meshVertexCounter < 65535)
                    {
                        //Creates and adds a new combine instance to the current instances array
                        CombineInstance curInst = new CombineInstance();
                        Vector3[] newVerts = new Vector3[curMesh.vertexCount];

                        for (int v = 0; v < newVerts.Length; v++)
                            newVerts[v] = curMesh.vertices[v] + meshes[i].transform.position;

                        curInst.mesh = curMesh;
                        curInst.mesh.vertices = newVerts;
                        instances.Add(curInst);
                    }
                    else
                    {
                        //Finishes up mesh combination
                        Mesh newCombined = new Mesh
                        {
                            name = "Combined Mesh " + i
                        };
                        newCombined.CombineMeshes(instances.ToArray(), true, false, false);
                        instances = new List<CombineInstance>();
                        returnMeshes.Add(newCombined);

                        //Creates a new object for the combined mesh
                        GameObject newCombinedObj = Instantiate(meshes[objCounter].gameObject);
                        newCombinedObj.transform.position = Vector3.zero;
                        newCombinedObj.name = "Combined Meshes " + objCounter + " - " + i;
                        newCombinedObj.GetComponent<MeshFilter>().mesh = newCombined;
                        newCombinedObj.hideFlags = HideFlags.HideInHierarchy;

                        if (newCombinedObj.GetComponent<Collider>())
                        {
                            Destroy(newCombinedObj.GetComponent<Collider>());
                            newCombinedObj.AddComponent<MeshCollider>().sharedMesh = newCombined;
                        }

                        newObjects.Add(newCombinedObj.GetComponent<MeshFilter>());

                        objCounter = i;
                        meshVertexCounter = 0;
                        i--;
                    }
                }

                //Finishes up mesh combination
                Mesh newCombinedFinal = new Mesh
                {
                    name = "Combined Final Mesh"
                };
                newCombinedFinal.CombineMeshes(instances.ToArray(), true, false, false);
                returnMeshes.Add(newCombinedFinal);

                //Creates a new object for the combined mesh
                GameObject newCombinedObjFinal = Instantiate(meshes[objCounter].gameObject);
                newCombinedObjFinal.transform.position = Vector3.zero;
                newCombinedObjFinal.name = "Combined Meshes " + objCounter + " - " + meshes.Count;
                newCombinedObjFinal.GetComponent<MeshFilter>().mesh = newCombinedFinal;
                newCombinedObjFinal.hideFlags = HideFlags.HideInHierarchy;

                Destroy(newCombinedObjFinal.GetComponent<Collider>());
                newCombinedObjFinal.AddComponent<MeshCollider>().sharedMesh = newCombinedFinal;
                newObjects.Add(newCombinedObjFinal.GetComponent<MeshFilter>());

                return returnMeshes.ToArray();
            }

            //Sets all of the objects to be the children of this object
            public void ObjsAddChildren()
            {
                objs.AddRange(GetComponentsInChildren<MeshFilter>());
            }

            //Sets the objects to this current game object
            public void ObjsAddCurrent()
            {
                if (GetComponent<MeshFilter>() != null)
                    objs.Add(GetComponent<MeshFilter>());
            }

            //Sets the objects to the selected objects
            public void ObjsAddSelected()
            {
#if UNITY_EDITOR
                GameObject[] selectedObjs = UnityEditor.Selection.gameObjects;
                List<MeshFilter> selectedMeshes = new List<MeshFilter>();
                for (int i = 0; i < selectedObjs.Length; i++)
                    if (selectedObjs[i].GetComponent<MeshFilter>())
                        selectedMeshes.Add(selectedObjs[i].GetComponent<MeshFilter>());
                objs.AddRange(selectedMeshes);
#endif
            }

            //Sets the objects to ones with a specified tag
            public void ObjsAreTagged()
            {
                GameObject[] rawObjs = GameObject.FindGameObjectsWithTag(objectTag);
                List<MeshFilter> taggedMeshes = new List<MeshFilter>();
                for (int i = 0; i < rawObjs.Length; i++)
                    if (rawObjs[i].GetComponent<MeshFilter>())
                        taggedMeshes.Add(rawObjs[i].GetComponent<MeshFilter>());
                objs.AddRange(taggedMeshes);
            }

        }

    }
}
