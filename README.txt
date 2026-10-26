奥黛塔 OdetteCharacter 1.0.0
完整公开说明见 README.md；素材接口见 ASSET-CONTRACT.txt。
源码：https://github.com/ifgifg/OdetteCharacter

将发布的 OdetteCharacter.tmod 放入 tModLoader 的 Mods 文件夹并启用。
无需时装、饰品或指令。配置中开启“温柔表情”即可切换，默认严肃。
每种表情12姿势，含4个闲置芭蕾姿势；站稳5秒后展示，动作输入、
持物、入水、坐骑等立即取消。关闭本模组配置的启用选项恢复原外观。
纯外观，不改变属性、碰撞、重力、呼吸、装备效果、控制或存档外貌；
不增加自由游泳能力。

Vodyanitsa 模组与其 Enabled 配置都开启时，沃雅妮莎优先，
OdetteCharacter 主动让位并提示。使用奥黛塔时停用 Vodyanitsa 或关闭
其 Enabled 配置，不需要删除文件。旧 OdetteVanity 可继续保留启用，
已有物品仍在。本模组不会自动修改其他模组配置或启用列表。

构建：保持目录名为 OdetteCharacter，在PowerShell执行 .\build.ps1。
.\build.ps1 -CompileOnly 只编译；-TMLPath/-Dotnet 可指定本机路径。
需要 .NET 8 SDK 和 tModLoader，输出在相邻 OdetteCharacterBuild\release。
构建使用独立 runtime/save，不安装、不启用、不读取真实人物或世界。
已验证 Windows / tModLoader 2026.8.3.0 / Terraria 1.4.4.9，其他平台未验证。

二维切帧和前臂旋转不是完整骨骼，不含背面360度旋转或原角色技能。
原生持物和坐骑保留；其他模组的自定义人物效果可能被覆盖。
死亡/幽灵回退原版，睡眠使用坐姿，装备染料不逐项改变完整角色图集。
已检查双表情姿势、握点、透明、玩法一致性、双模组回退、服务器加载
和目标整合包启动；没有真实双客户端联机实测。

原创贡献采用 LICENSE 中的 MIT 许可。角色及其他既有第三方知识产权归
原权利人，详见 THIRD_PARTY_NOTICES.md。图集使用 AI 辅助生成。
PUBLIC-SOURCE-MANIFEST.txt 列明公开源码，不含参考图、游戏库、私人配置
或存档。二进制发布包与源码目录分别管理。
