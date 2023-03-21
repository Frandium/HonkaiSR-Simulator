# HonkaiSR Simulator
《崩坏：星穹铁道》战斗模拟器。
力图还原游戏的战斗机制，从而为数据分析提供参考。
使用中遇到 bug，欢迎提 issue，或在 Bilibili 等平台反馈。

## 使用方法
### Android
在 Release 中下载对应版本的 HonkaiSR-Simulator.apk，安装运行。
### Windows
在 Release 中下载对应版本的 HonkaiSR-Simulator.zip，解压后运行 HonkaiSR-Simulator.exe。
### Unity Editor
可以直接克隆代码到本地打开。创建新的类实现 ACharacterTalent 和 AEnemyTalent 即可设计新的角色和敌人的战斗天赋。在 StreamingAssets 中配置角色、敌人和战斗关卡来测试代码（注意更新配置后需要清理 PersitentDataPath 下的数据文件，更新了的 StreamingAssets 才会被 copy 到 PersistentDataPath 下）。更多介绍将在后续文档中。
### iOS & Mac OS
预计不会支持，因为没有苹果开发者账号，也没有苹果开发设备。
### 网页
还不会把 Unity 工程导到网页呜呜呜，可以去学但是还没有时间表。

## v0.4 2023/3/21 更新日志：
- 机制
  + 遗器的配装和套装效果实现
  + 统一了所有有持续时间/次数限制的状态运行方式
  + 新增了造成/受到伤害前/后的事件接口
  + 统一了希儿星魂6、停云天赋、布洛妮娅星魂4的协同攻击机制。
- 遗器：三测公开的所有遗器效果都已实现
- 操作系统：现在支持 Android 系统了

## v0.3 2023/3/6 更新日志：
- 角色：布洛妮娅、杰帕德、停云、希儿
- 光锥：但战斗还未结束、于夜色中、制胜的瞬间
- 界面：角色配置界面，包括等级、光锥、天赋、遗器、星魂
- 界面：战斗中可以实时查看角色属性和buff列表
- 机制：遗器套装效果尚未实现

## v0.2 2023/2/26 更新日志：
- 基于《崩坏：星穹铁道》，复核了战斗结构
- 放弃了角色光锥等的可配置，拿 C# 都还写不明白呢 QAQ
- 实现了布洛妮娅、杰帕德、停云、希儿的普攻、战技、终结技、天赋、秘技；额外能力在后续实现
- 战斗中可以实时查看角色属性了
- 实现了选择关卡战斗的页面

## v0.1 2023/2/7 更新日志：
- 第一次更新
- 基于《原神》的元素反应体系和《崩坏：星穹铁道》构建了当前战斗结构
- 角色的属性可以配置了，后续会有文档解释。
- 角色的技能可以配置了，目前支持造成伤害、造成治疗、附着元素和上 buff 等 4 种操作。
- 敌人的属性可以配置了，后续会有文档解释。
- 战斗关卡可以配置了，后续会有文档解释。
- 目前仅支持扩散和冻结两种元素反应，其中扩散反应不会为周为单位附着元素。后续计划在《崩坏：星穹铁道》上线后，讲元素反应体系更新为《崩坏：星穹铁道》的体系。

## 免责声明
项目中部分美术素材和设计来源米哈游。
个人兴趣作品，请勿商用。
