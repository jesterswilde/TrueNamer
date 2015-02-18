//////////////////////////////////////////////////////////////////////////
// ShroudInstance.cs - Instance Component for the Shroud Unity Plugin
//
// Copyright (C) 2009 - 2012 CloakWorks Inc.  All rights reserved.
//
// The coded instructions, statements, computer programs, and/or related material 
// (collectively the "Software") in these files contain information 
// proprietary to CloakWorks Inc., which is protected by 
// United States of America federal copyright law and by international 
// treaties. 
//
// The Software may not be disclosed or distributed to third parties, in whole or in
// part, without the prior written consent of CloakWorks Inc. ("CloakWorks").
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. WITHOUT 
// LIMITING THE FOREGOING, CLOAKWORKS DOES NOT WARRANT THAT THE OPERATION
// OF THE SOFTWARE WILL BE UNINTERRUPTED OR ERROR FREE. IN NO EVENT
// SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY DAMAGES OR OTHER 
// LIABILITY, WHETHER IN CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR 
// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;


class ShroudMesh : MonoBehaviour
{
#if (UNITY_IPHONE || UNITY_XBOX360) && !UNITY_EDITOR
    const string Shroud_Module_Name = "__Internal";
#else
    const string Shroud_Module_Name = "ShroudUnityPlugin";
#endif

    public uint m_shroudInstanceID;
    public uint m_meshIndex;

    private Vector3[] m_vertices;
    private Vector3[] m_normals;
    private Vector4[] m_tangents;
    private GCHandle m_posBufferHandle;
    private GCHandle m_normBufferHandle;
    private GCHandle m_tanBufferHandle;

    [DllImport(Shroud_Module_Name)]
    private static extern uint GetNumMeshVerts(uint meshIndex, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern uint GetNumMeshIndices(uint meshIndex, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern bool FillPositionsArray(IntPtr positionData, uint numVerts, uint meshIndex, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern bool FillNormalsArray(IntPtr normalData, uint numVerts, uint meshIndex, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern bool FillTangentsArray(IntPtr tangentData, uint numVerts, uint meshIndex, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern bool FillTexCoordsArray([In, Out] Vector2[] texCoordData, uint numTexCoords, uint meshIndex, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern bool FillIndexArray([In, Out] int[] indexData, uint numIndices, uint meshIndex, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern void GetBounds([In, Out] ref Vector3 min, [In, Out] ref Vector3 max, uint meshIndex, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    public static extern void EndUpdate(uint shroudInstanceID);

    void Start()
    {
        uint numVerts = GetNumMeshVerts(m_meshIndex, m_shroudInstanceID);
        uint numIndices = GetNumMeshIndices(m_meshIndex, m_shroudInstanceID);

        m_vertices = new Vector3[numVerts];
        m_normals = new Vector3[numVerts];
        m_tangents = new Vector4[numVerts];

        Vector2[] newUV = new Vector2[numVerts];
        int[] newTriangles = new int[numIndices];

        // "pin" the array in memory, so we can pass direct pointer to it's data to the plugin,
        // without costly marshaling of the memory.
        m_posBufferHandle = GCHandle.Alloc(m_vertices, GCHandleType.Pinned);
        m_normBufferHandle = GCHandle.Alloc(m_normals, GCHandleType.Pinned);
        m_tanBufferHandle = GCHandle.Alloc(m_tangents, GCHandleType.Pinned);

        FillPositionsArray(m_posBufferHandle.AddrOfPinnedObject(), numVerts, m_meshIndex, m_shroudInstanceID);
        FillNormalsArray(m_normBufferHandle.AddrOfPinnedObject(), numVerts, m_meshIndex, m_shroudInstanceID);
        FillTangentsArray(m_tanBufferHandle.AddrOfPinnedObject(), numVerts, m_meshIndex, m_shroudInstanceID);
        FillTexCoordsArray(newUV, numVerts, m_meshIndex, m_shroudInstanceID);
        FillIndexArray(newTriangles, numIndices, m_meshIndex, m_shroudInstanceID);

        Mesh mesh = new Mesh();
        mesh.vertices = m_vertices;
        mesh.normals = m_normals;
        mesh.tangents = m_tangents;
        mesh.uv = newUV;
        mesh.triangles = newTriangles;

        Vector3 min = new Vector3();
        Vector3 max = new Vector3();
        GetBounds(ref min, ref max, m_meshIndex, m_shroudInstanceID);

        // Note: when this is first called, the transform has not been updated to reflect
        // the translation and rotation of its parent
        min = transform.parent.TransformPoint(min);
        max = transform.parent.TransformPoint(max);

        Bounds newBounds = new Bounds();
        newBounds.SetMinMax(min, max);
        mesh.bounds = newBounds;

        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    void OnDestroy()
    {
        m_posBufferHandle.Free();
        m_normBufferHandle.Free();
        m_tanBufferHandle.Free();
    }

    void OnWillRenderObject()
    {
        // Because there are multiple meshes, EndUpdate() will get called multiple times 
        // for this instance ID which is ok
        EndUpdate(m_shroudInstanceID);

        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

        uint numVerts = GetNumMeshVerts(m_meshIndex, m_shroudInstanceID);

        FillPositionsArray(m_posBufferHandle.AddrOfPinnedObject(), numVerts, m_meshIndex, m_shroudInstanceID);
        FillNormalsArray(m_normBufferHandle.AddrOfPinnedObject(), numVerts, m_meshIndex, m_shroudInstanceID);
        FillTangentsArray(m_tanBufferHandle.AddrOfPinnedObject(), numVerts, m_meshIndex, m_shroudInstanceID);

        Mesh mesh = meshFilter.mesh;
        mesh.vertices = m_vertices;
        mesh.normals = m_normals;
        mesh.tangents = m_tangents;

        UpdateBounds();
    }

    public void UpdateBounds()
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

        Mesh mesh = meshFilter.mesh;

        Vector3 min = new Vector3();
        Vector3 max = new Vector3();
        GetBounds(ref min, ref max, m_meshIndex, m_shroudInstanceID);

        min = transform.InverseTransformPoint(min);
        max = transform.InverseTransformPoint(max);

        Bounds newBounds = new Bounds();
        newBounds.SetMinMax(min, max);

        mesh.bounds = newBounds;
    }
}

[AddComponentMenu("Physics/Shroud/Shroud Instance")]
public class ShroudInstance : MonoBehaviour
{
#if (UNITY_IPHONE || UNITY_XBOX360) && !UNITY_EDITOR
    const string Shroud_Module_Name = "__Internal";
#else
    const string Shroud_Module_Name = "ShroudUnityPlugin";
#endif

    public String m_shroudFileName;

    public SkinnedMeshRenderer[] m_skinnedMesh;

	public Material[] m_materials;
    public float m_blendStartDistance = 30;
    public float m_blendEndDistance = 35;

    private Transform[] m_boneTransforms;
    private Transform[] m_colliderTransforms;

    private GameObject[] m_shroudGameObjects;

    private uint m_shroudInstanceID;

    private const uint kInvalidShroudID = 0xFFFFFFFF;


    [DllImport(Shroud_Module_Name)]
    private static extern void ReleaseInstance(uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern uint GetNumMeshes(uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern void SetBlend(float blendValue, uint meshIndex, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern void SetRootMatrix(IntPtr mtx, uint meshIndex, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern uint GetNumTransforms(uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern IntPtr GetTransformBoneName(uint index, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern void SetTransformMatrix(uint index, IntPtr mtx, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern uint GetNumColliders(uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern IntPtr GetColliderBoneName(uint index, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern void SetColliderMatrix(uint index, IntPtr mtx, uint shroudInstanceID);
	
	[DllImport(Shroud_Module_Name)]
    private static extern void SetWind(ref Vector3 direction, float strength, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern void SetCameraDirection(ref Vector3 direction, uint shroudInstanceID);

    [DllImport(Shroud_Module_Name)]
    private static extern void BeginUpdate(float timestep, uint shroudInstanceID);

    void Start()
    {
        GameObject shroudMgr = GameObject.FindWithTag("Shroud Manager");
        if (shroudMgr)
        {
            ShroudObjectManager shroudMgrCmp = shroudMgr.GetComponent<ShroudObjectManager>();
            if (shroudMgrCmp)
            {
                m_shroudInstanceID = shroudMgrCmp.CreateInstance(m_shroudFileName);
            }
        }
        

        if (m_shroudInstanceID != kInvalidShroudID)
        {
            uint numMeshes = GetNumMeshes(m_shroudInstanceID);
            m_shroudGameObjects = new GameObject[numMeshes];

            for (uint meshIndex = 0; meshIndex < numMeshes; ++meshIndex)
            {
                m_shroudGameObjects[meshIndex] = new GameObject("Shroud Mesh");
                m_shroudGameObjects[meshIndex].layer = gameObject.layer;
                m_shroudGameObjects[meshIndex].transform.parent = transform;

                m_shroudGameObjects[meshIndex].AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = m_shroudGameObjects[meshIndex].AddComponent<MeshRenderer>();
                ShroudMesh shroudMeshCmp = m_shroudGameObjects[meshIndex].AddComponent<ShroudMesh>();

                shroudMeshCmp.m_shroudInstanceID = m_shroudInstanceID;
                shroudMeshCmp.m_meshIndex = meshIndex;

                if(meshIndex < m_materials.Length && m_materials[meshIndex])
				{
					meshRenderer.material = m_materials[meshIndex];
				}
				else if (meshIndex < m_skinnedMesh.Length && m_skinnedMesh[meshIndex])
                {
                    // Hide the existing skeletal mesh
                    m_skinnedMesh[meshIndex].enabled = false;
                    meshRenderer.material = m_skinnedMesh[meshIndex].material;
                }
            }

            SetupTransforms();
            SetupColliders();
        }
        else
        {
            Debug.LogError("Shroud: Failed to create Shroud Instance for " + m_shroudFileName);
        }
    }


    void OnDestroy()
    {
        ReleaseInstance(m_shroudInstanceID);
    }


    void LateUpdate()
    {
        uint numMeshes = GetNumMeshes(m_shroudInstanceID);
        if (numMeshes > 0)
        {
            // Ensure that the previous update is finished.  There is no harm in calling this
            // when it is not necessary
            ShroudMesh.EndUpdate(m_shroudInstanceID);

            // Make sure that the bounds for the meshes gets updated so that the culling system has recent information
            // to work with
            for (uint meshIndex = 0; meshIndex < numMeshes; ++meshIndex)
            {
                ShroudMesh shroudMeshCmp = m_shroudGameObjects[meshIndex].GetComponent<ShroudMesh>();
                if (shroudMeshCmp)
                {
                    shroudMeshCmp.UpdateBounds();
                }
            }

            bool continueUpdate = UpdateBlendValue();
            if (continueUpdate)
            {
                UpdateTransforms();
                UpdateColliders();

                for (uint meshIndex = 0; meshIndex < numMeshes; ++meshIndex)
                {
                    IntPtr mtxPtr = MarshalToPointer(m_shroudGameObjects[meshIndex].transform.localToWorldMatrix);
                    SetRootMatrix(mtxPtr, meshIndex, m_shroudInstanceID);
                    Marshal.FreeHGlobal(mtxPtr);
                }

                BeginUpdate(Time.deltaTime, m_shroudInstanceID);
            }
        }
    }


    private void SetupTransforms()
    {
        uint numTransforms = GetNumTransforms(m_shroudInstanceID);

        m_boneTransforms = new Transform[numTransforms];

        for(uint transformIndex = 0; transformIndex < numTransforms; ++transformIndex)
        {
            string boneName = Marshal.PtrToStringAnsi(GetTransformBoneName(transformIndex, m_shroudInstanceID));

            if (boneName.Length > 0)
            {
                Transform boneTransform = FindBone(gameObject.transform, boneName);

                if (boneTransform)
                {
                    m_boneTransforms[transformIndex] = boneTransform;
                }
                else
                {
                    Debug.LogWarning("Shroud: Could not find bone defined in " + m_shroudFileName + ": " + boneName);
                }
            }
			else if(transformIndex == 0)
			{
				// If this transform is the first, and it is not bound to a bone, then bind it to the root
				// of this game object
				m_boneTransforms[transformIndex] = transform;
			}
        }
    }


    private void SetupColliders()
    {
        uint numColliders = GetNumColliders(m_shroudInstanceID);

        m_colliderTransforms = new Transform[numColliders];

        for (uint colliderIndex = 0; colliderIndex < numColliders; ++colliderIndex)
        {
            string boneName = Marshal.PtrToStringAnsi(GetColliderBoneName(colliderIndex, m_shroudInstanceID));

            if (boneName.Length > 0)
            {
                Transform boneTransform = FindBone(gameObject.transform, boneName);

                if (boneTransform)
                {
                    m_colliderTransforms[colliderIndex] = boneTransform;
                }
                else
                {
                    Debug.LogWarning("Shroud: Could not find bone defined in " + m_shroudFileName + ": " + boneName);
                }
            }
        }
    }


    void UpdateTransforms()
    {
        uint numTransforms = GetNumTransforms(m_shroudInstanceID);

        for (uint transformIndex = 0; transformIndex < numTransforms; ++transformIndex)
        {
            if (m_boneTransforms[transformIndex])
            {
                IntPtr mtxPtr = MarshalToPointer(m_boneTransforms[transformIndex].localToWorldMatrix);
                SetTransformMatrix(transformIndex, mtxPtr, m_shroudInstanceID);
                Marshal.FreeHGlobal(mtxPtr);
            }
        }
    }


    bool UpdateBlendValue()
    {
        Camera[] cameras = Camera.allCameras;

		if(cameras.Length > 0 && cameras[0])
		{
            Camera currentCamera = cameras[0];

            Vector3 cameralookDir = currentCamera.transform.forward;
            SetCameraDirection(ref cameralookDir, m_shroudInstanceID);

            Vector3 myPos = transform.position;

			// Calculate the LOD blend value
            Vector3 cameraDelta = myPos - currentCamera.transform.position;
            float cameraDist = cameraDelta.magnitude;

            float blendWeight = (cameraDist - m_blendStartDistance) / (m_blendEndDistance - m_blendStartDistance);
			blendWeight = (blendWeight < 0) ? 0 : blendWeight;
			blendWeight = (blendWeight > 1) ? 1 : blendWeight;
			blendWeight = 1 - blendWeight;

            uint numMeshes = GetNumMeshes(m_shroudInstanceID);
            for (uint meshIndex = 0; meshIndex < numMeshes; ++meshIndex)
			{
                SetBlend(blendWeight, meshIndex, m_shroudInstanceID);

                if (meshIndex < m_skinnedMesh.Length && m_skinnedMesh[meshIndex])
                {
                    MeshRenderer meshRenderer = m_shroudGameObjects[meshIndex].GetComponent<MeshRenderer>();

                    if (blendWeight == 0)
                    {
                        m_skinnedMesh[meshIndex].enabled = true;
                        meshRenderer.enabled = false;
                    }
                    else
                    {
                        m_skinnedMesh[meshIndex].enabled = false;
                        meshRenderer.enabled = true;
                    }
                }
			}

            return (blendWeight > 0);
		}

        return true;
    }


    void UpdateColliders()
    {
        uint numColliders = GetNumColliders(m_shroudInstanceID);
        for (uint colliderIndex = 0; colliderIndex < numColliders; ++colliderIndex)
        {
            if (m_colliderTransforms[colliderIndex])
            {
                IntPtr mtxPtr = MarshalToPointer(m_colliderTransforms[colliderIndex].localToWorldMatrix);
                SetColliderMatrix(colliderIndex, mtxPtr, m_shroudInstanceID);
                Marshal.FreeHGlobal(mtxPtr);
            }
        }
    }


    private Transform FindBone(Transform current, string name)
    {
        // check if the current bone is the bone we're looking for, if so return it
        if (current.name == name)
            return current;

        // search through child bones for the bone we're looking for
        for (int i = 0; i < current.GetChildCount(); ++i)
        {
            // the recursive step; repeat the search one step deeper in the hierarchy
            Transform found = FindBone(current.GetChild(i), name);

            // a transform was returned by the search above that is not null,
            // it must be the bone we're looking for
            if (found != null)
                return found;
        }

        // bone with name was not found
        return null;
    }


    private IntPtr MarshalToPointer(object data)
    {
        IntPtr buf = Marshal.AllocHGlobal(Marshal.SizeOf(data));
        Marshal.StructureToPtr(data, buf, false);
        return buf;
    }
}

