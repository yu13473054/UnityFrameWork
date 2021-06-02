using LuaInterface;
using UnityEditor;
using UnityEngine;

public class MemoryTester
{
    [MenuItem("Tools/测试/Lua-Before快照")]
    static void Build_Upload()
    {
        if(!Application.isPlaying) return;
        string BeforeStr = @"
            function BeforeMyFunc(mri)
                mri.m_cConfig.m_bAllMemoryRefFileAddTime = false;
                collectgarbage() --内存回收
                mri.m_cMethods.DumpMemorySnapshot('./', 'Before', -1) --生成Before快照
            end
        ";
        object require = LuaMgr.Inst.lua.Require<object>(Application.dataPath + "/Editor/Tester/MemoryReferenceInfo");
        LuaMgr.Inst.lua.DoString(BeforeStr, "MemoryTester.cs");
        LuaMgr.Inst.Call("BeforeMyFunc", require);
    }
    [MenuItem("Tools/测试/Lua-After快照，并比较")]
    static void Build_NotUpload()
    {
        if (!Application.isPlaying) return;
        string AfterStr = @"
            function AfterMyFunc(mri)
                mri.m_cConfig.m_bAllMemoryRefFileAddTime = false;
                mri.m_cConfig.m_bComparedMemoryRefFileAddTime = false;
                collectgarbage()--内存回收
                --生成After快照
                mri.m_cMethods.DumpMemorySnapshot('./', 'After', -1) 
                --开始比较
                mri.m_cMethods.DumpMemorySnapshotComparedFile('./', 'Compared', -1, './LuaMemRefInfo-All-[Before].txt', './LuaMemRefInfo-All-[After].txt')
                -- 过滤比较文件中的指定字段
                mri.m_cBases.OutputFilteredResult('./LuaMemRefInfo-All-[Compared].txt', '.Data.', false, true)
            end
        ";

        object require = LuaMgr.Inst.lua.Require<object>(Application.dataPath + "/Editor/Tester/MemoryReferenceInfo");
        LuaMgr.Inst.lua.DoString(AfterStr, "MemoryTester.cs");
        LuaMgr.Inst.Call("AfterMyFunc", require);
    }
}