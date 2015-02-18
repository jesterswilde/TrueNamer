//////////////////////////////////////////////////////////////////////////
// ShroudObjectManager.cs - Manager Component for the Shroud Unity Plugin
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


[AddComponentMenu("Physics/Shroud/Shroud Manager")]
public class ShroudObjectManager : MonoBehaviour
{
    // The number of threads that Shroud should NOT use for updating. 
    // Total number of threads used is (#cores - #reservedThreads)
    public int          m_reservedThreads = 1;
    public TextAsset[]  m_shroudFiles;

    private uint[]      m_shroudObjectIDs;
    private const uint  kInvalidShroudID = 0xFFFFFFFF;

#if (UNITY_IPHONE || UNITY_XBOX360) && !UNITY_EDITOR
    const string Shroud_Module_Name = "__Internal";
#else
    const string Shroud_Module_Name = "ShroudUnityPlugin";
#endif

    [DllImport(Shroud_Module_Name)]
    private static extern void InitializeShroud(uint numReservedThreads);

    [DllImport(Shroud_Module_Name)]
    private static extern void ShutdownShroud();

    [DllImport(Shroud_Module_Name, CharSet = CharSet.Ansi)]
    private static extern uint LoadObject(string data, uint size);

    [DllImport(Shroud_Module_Name)]
    private static extern void ReleaseObject(uint shroudObjectID);

    [DllImport(Shroud_Module_Name)]
    private static extern uint CreateInstance(uint shroudObjectID);

    [DllImport(Shroud_Module_Name)]
    private static extern uint GetNumErrors();

    [DllImport(Shroud_Module_Name)]
	private static extern IntPtr GetError(uint index);

    [DllImport(Shroud_Module_Name)]
	private static extern void ClearErrors();


    void Awake()
    {
        InitializeShroud((uint)m_reservedThreads);

        m_shroudObjectIDs = new uint[m_shroudFiles.Length];

        LoadFiles();
    }

    void OnDestroy()
    {
        for (uint i = 0; i < m_shroudObjectIDs.Length; ++i)
        {
            if (m_shroudObjectIDs[i] != kInvalidShroudID)
            {
                ReleaseObject(m_shroudObjectIDs[i]);
            }

            m_shroudObjectIDs[i] = kInvalidShroudID;
        }

        ShutdownShroud();
    }

    void LoadFiles()
    {
        for (uint i = 0; i < m_shroudFiles.Length; ++i)
        {
            m_shroudObjectIDs[i] = kInvalidShroudID;

            TextAsset file = m_shroudFiles[i];
            if (file)
            {
                m_shroudObjectIDs[i] = LoadObject(file.text, (uint)file.text.Length);
                if (m_shroudObjectIDs[i] == kInvalidShroudID)
                {
                    uint numErrorMsgs = GetNumErrors();
                    for (uint j = 0; j < numErrorMsgs; ++j)
                    {
                        string errorString = Marshal.PtrToStringAnsi(GetError(j));
                        Debug.LogError(errorString);
                    }

                    ClearErrors();

                    Debug.LogError("Failed to load Shroud file " + file.name);
                }
            }
        }
    }

    public uint CreateInstance(String filename)
    {
        for (uint i = 0; i < m_shroudFiles.Length; ++i)
        {
            TextAsset file = m_shroudFiles[i];
            if (file.name == filename)
            {
                return CreateInstance(m_shroudObjectIDs[i]);
            }
        }

        return kInvalidShroudID;
    }
}
