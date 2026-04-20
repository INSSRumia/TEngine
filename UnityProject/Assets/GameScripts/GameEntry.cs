using GameLogic;
using TEngine;
using UnityEngine;

public class GameEntry : MonoBehaviour
{
    void Awake()
    {
        ModuleSystem.GetModule<IUpdateDriver>();
        ModuleSystem.GetModule<IResourceModule>();
        ModuleSystem.GetModule<IDebuggerModule>();
        ModuleSystem.GetModule<IFsmModule>();
        var uiModule = UIModule.Instance;
        Settings.ProcedureSetting.StartProcedure().Forget();
        DontDestroyOnLoad(this);
    }
}