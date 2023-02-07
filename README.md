# HonkaiSR Simulator
《崩坏：星穹铁道》战斗模拟器。
力图还原游戏的战斗机制，从而为数据分析提供参考。
后续计划添加编辑器、伤害占比可视化等功能。
使用中遇到 bug，欢迎提 issue。

## 使用方法
### 如果您不使用 Unity Editor
可以在 Release 文件夹中下载对应的版本，解压后运行 HonkaiSRSimulator.exe
### 如果您安装了 Unity Editor
可以直接克隆代码到本地打开。创建新的类实现 ACharacterTalent 和 AEnemyTalent 即可设计新的角色和敌人的战斗天赋。在 StreamingAssets 中配置角色、敌人和战斗关卡来测试代码。更多介绍将在后续文档中。
注意编辑器版本的不同可能产生意料之外的错误。

## v0.1 2023/2/7 更新日志：
- 第一次更新
- 基于《原神》的元素反应体系和《崩坏：星穹铁道》构建了当前战斗结构
- 角色的属性可以配置了，后续会有文档解释。
- 角色的技能可以配置了，目前支持造成伤害、造成治疗、附着元素和上 buff 等 4 种操作。
- 敌人的属性可以配置了，后续会有文档解释。
- 战斗关卡可以配置了，后续会有文档解释。
- 目前仅支持扩散和冻结两种元素反应，其中扩散反应不会为周为单位附着元素。后续计划在《崩坏：星穹铁道》上线后，讲元素反应体系更新为《崩坏：星穹铁道》的体系。


## 免责声明
项目中部分美术素材和玩法设计来源米哈游。
个人兴趣作品，请勿商用。
