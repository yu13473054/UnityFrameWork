using UnityEngine;
using System.Collections;
using LuaFramework;

public class StartUpCommand : ControllerCommand {

    public override void Execute(IMessage message) {
        if (!MyUtils.CheckEnvironment()) return;

        GameObject gameMgr = GameObject.Find("GlobalGenerator");
        if (gameMgr != null) {
            AppView appView = gameMgr.AddComponent<AppView>();
        }
        //-----------------关联命令-----------------------
        AppFacade.Instance.RegisterCommand(NotiConst.DISPATCH_MESSAGE, typeof(SocketCommand));

        //-----------------初始化管理器-----------------------
        AppFacade.Instance.AddManager<LuaMgr>(ManagerName.Lua);
        AppFacade.Instance.AddManager<UIMgr>(ManagerName.Panel);
        AppFacade.Instance.AddManager<SoundMgr>(ManagerName.Sound);
        AppFacade.Instance.AddManager<TimerMgr>(ManagerName.Timer);
        AppFacade.Instance.AddManager<NetworkMgr>(ManagerName.Network);
        AppFacade.Instance.AddManager<ResMgr>(ManagerName.Resource);
        AppFacade.Instance.AddManager<ThreadManager>(ManagerName.Thread);
        AppFacade.Instance.AddManager<PoolMgr>(ManagerName.ObjectPool);
        AppFacade.Instance.AddManager<GameMain>(ManagerName.Game);
    }
}