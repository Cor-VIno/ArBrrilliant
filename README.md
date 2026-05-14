[交接文档.md](https://github.com/user-attachments/files/27742802/default.md)
# 《惊鸿录》战斗系统交接说明

## 一、当前战斗系统完成度

当前战斗 Demo 已经完成了一个可运行的横板战斗闭环：

```text
玩家输入
  ↓
技能槽读取
  ↓
释放横 / 竖 / 撇 / 捺
  ↓
Hitbox / Projectile / Dash 执行
  ↓
造成伤害
  ↓
记录笔画
  ↓
匹配剑招
  ↓
释放剑招效果
  ↓
敌人反击
  ↓
玩家 / 敌人死亡
  ↓
胜利 / 失败判定
```

目前不是最终商业级战斗系统，但已经足够作为 GameJam / Demo 战斗核心。

------

# 二、基础技能系统

## 1. 输入层

输入层只负责把按键映射到：

```text
SkillSlot1
SkillSlot2
SkillSlot3
SkillSlot4
```

默认配置大致是：

```text
Slot1：横
Slot2：竖
Slot3：撇
Slot4：捺
```

也就是：

```text
左键：横
右键：竖
Q：撇
E：捺
```

注意：
输入层不直接绑定具体技能，具体技能由 `PlayerSkillLoadout` 决定。

------

## 2. 技能槽系统

`PlayerSkillLoadout` 决定四个技能槽分别装什么技能。

这样未来可以改成：

```text
Slot1 装横
Slot2 装撇
Slot3 装捺
Slot4 装竖
```

不需要改输入代码。

------

## 3. SkillData

`SkillData` 是基础技能配置。

每个技能可以配置：

```text
技能 ID
显示名
笔画类型
执行类型
冷却
前摇
后摇
伤害
是否暴击
是否击飞
Hitbox 形状
Hitbox 范围
ProjectileData
DashData
```

目前四个基础技能：

```text
横：轻击，InstantHitbox + Arc
竖：重击，InstantHitbox + Box，可击飞
撇：远程，Projectile
捺：突刺，Dash
```

------

# 三、Combat 基础链路

当前伤害系统核心组件：

```text
Health
Damageable
Hurtbox2D
Hitbox2D
DamageInfo
TeamId
```

职责如下：

```text
Health：
负责血量、回血、死亡事件。

Damageable：
负责接收 DamageInfo，扣血，处理击飞。

Hurtbox2D：
受击盒，通常放在子物体上，Collider2D 要勾 Is Trigger。

Hitbox2D：
攻击盒，负责扫描 Hurtbox 并造成伤害。

DamageInfo：
一次伤害的信息，包括伤害值、来源名、击飞参数等。

TeamId：
区分 Player / Enemy / Neutral，避免打到同队。
```

当前伤害来源已经支持：

```text
基础技能名
剑招名
敌人攻击名
```

所以日志不会再只显示 `unknown`。

------

# 四、Projectile 系统

当前 Projectile 系统用于玩家“撇”和敌人鱼叉。

核心组件：

```text
ProjectileData
ProjectileMotionType
ProjectileMover2D
ProjectileImpact2D
Hitbox2D
```

支持：

```text
Linear：直线飞行
Parabolic：抛物线飞行
```

`ProjectileImpact2D` 用于让投掷物撞到：

```text
Ground
Wall
```

后销毁。

------

# 五、Dash 系统

当前 Dash 系统用于“捺”。

核心组件：

```text
DashData
PlayerDashController2D
PlayerAirborneTargetFinder2D
```

捺目前支持：

```text
普通突刺
Dash 期间无敌
Dash Hitbox 跟随玩家
敌人浮空时追踪突刺
```

如果敌人处于 `AirborneTarget2D.IsAirborne == true`，并且 `DashData.EnableAirborneHoming` 开启，玩家按捺会优先朝浮空敌人突刺。

------

# 六、笔画记录系统

核心组件：

```text
StrokeRecord
StrokeRecorder
StrokeType
```

逻辑：

```text
玩家成功释放技能
  ↓
PlayerSkillController.OnSkillExecuted
  ↓
StrokeRecorder 记录 SkillData.StrokeType
```

每个笔画都有独立存在时间，例如 3 秒。

也就是说：

```text
横 3 秒后消失
竖 3 秒后消失
撇 3 秒后消失
捺 3 秒后消失
```

不是“整个连招窗口 3 秒”，而是“每个笔画独立过期”。

------

# 七、剑招系统

核心组件：

```text
SwordArtData
SwordArtEffectData
SwordArtMatcher
SwordArtExecutor
SwordArtEffectType
```

## SwordArtData

负责配置：

```text
剑招 ID
显示名
所需笔画序列
是否消耗匹配笔画
冷却时间
释放延迟
EffectData
```

## SwordArtMatcher

负责：

```text
监听新笔画
检查当前有效笔画序列末尾
匹配 SwordArtData
匹配成功后触发 OnSwordArtTriggered
```

注意：
`SwordArtMatcher` 只负责匹配，不负责生成伤害。

## SwordArtExecutor

负责：

```text
监听 OnSwordArtTriggered
读取 SwordArtEffectData
执行具体剑招效果
```

------

# 八、当前已完成剑招

## 1. 清风诀

输入：

```text
撇 → 横 → 撇 → 捺 → 捺
```

效果：

```text
大范围弧形伤害
可击飞敌人
```

当前是简化版，没有做策划案中的七段突进和斩杀。

------

## 2. 沧浪诀

输入：

```text
横 → 竖 → 撇 → 撇 → 捺
```

效果：

```text
从玩家到鼠标方向生成线形区域
持续造成伤害
结束时造成最终伤害并击飞
```

当前是简化版，没有做完整印记、僵直、三段触发伤害。

------

## 3. 长生诀

输入：

```text
撇 → 横 → 横 → 横 → 竖
```

效果：

```text
在释放位置生成圆形回血领域
玩家站在领域内每 0.5 秒回血
离开领域不回血
持续时间结束后领域销毁
```

------

# 九、敌人系统

当前敌人定位是“小怪水匪”。

策划案里水匪有两种攻击方式：

```text
近战普通攻击：挥、砍
远程投掷鱼叉：抛物线运动，落地后造成击飞
```

当前已经实现了简化版。

------

## 1. EnemyData

负责配置敌人：

```text
发现范围
脱战范围
移动速度
停止距离
近战攻击范围
近战攻击伤害
近战前摇
近战后摇
鱼叉攻击距离
鱼叉冷却
鱼叉 ProjectileData
后撤参数
危险技能反应参数
```

以后策划要改敌人强度，优先改 `EnemyData`。

------

## 2. EnemyBrain2D

负责敌人行为：

```text
发现玩家
追击玩家
近距离近战
中距离投鱼叉
攻击后停顿 / 后撤
玩家释放危险技能时概率后撤
Airborne 状态下停止 AI
```

当前敌人已经不会永远无脑贴脸追玩家，而是有一点距离控制和后撤行为。

------

## 3. AirborneTarget2D

负责敌人浮空 / 可追踪状态。

当前支持事件：

```text
OnAirborneStarted
OnAirborneRefreshed
OnAirborneEnded
```

这为未来接入动画、UI、音效预留了接口。

未来可以这样用：

```text
OnAirborneStarted：
播放击飞动画
显示可追踪突刺提示

OnAirborneEnded：
关闭提示
恢复普通动画
```

------

# 十、死亡和胜负闭环

当前已完成：

```text
EnemyDeathHandler
PlayerDeathHandler
CombatEncounterController
```

## EnemyDeathHandler

敌人死亡后：

```text
停止 AI
禁用 Collider / Hurtbox
停止 Rigidbody
延迟销毁
```

## PlayerDeathHandler

玩家死亡后：

```text
禁用输入
禁用移动
禁用技能
禁用 Dash
清空速度
打印死亡日志
```

## CombatEncounterController

负责判定：

```text
所有敌人死亡 → victory
玩家死亡 → defeat
```

目前只打印日志，没有 UI。

------

# 十一、场景配置注意事项

## 1. Player

Player 上应该有：

```text
PlayerInputReader
PlayerMotor2D
PlayerAim2D
PlayerDashController2D
PlayerSkillController
PlayerSkillLoadout 引用
StrokeRecorder
SwordArtMatcher
SwordArtExecutor
PlayerAirborneTargetFinder2D
Health
Damageable
PlayerDeathHandler
Rigidbody2D
Collider2D
```

Player 的 Hurtbox 建议放子物体：

```text
Player
  PlayerHurtbox
```

`PlayerHurtbox`：

```text
Layer = Hurtbox
Collider2D Is Trigger = true
Hurtbox2D 指向 Player 的 Damageable
```

------

## 2. Enemy

敌人根物体：

```text
Layer = Enemy
Rigidbody2D Dynamic
Gravity Scale > 0
Freeze Rotation Z = true
Collider2D Is Trigger = false
Health
Damageable
EnemyBrain2D
EnemyDeathHandler
AirborneTarget2D
```

敌人 Hurtbox 子物体：

```text
EnemyHurtbox
Layer = Hurtbox
Collider2D Is Trigger = true
Hurtbox2D 指向父物体 Damageable
```

------

## 3. Physics2D Layer Collision Matrix

关键设置：

```text
Enemy × Ground = true
Enemy × Wall = true
Enemy × Player = false

Player × Ground = true
Player × Wall = true
```

这样敌人会站在地面上，但不会和玩家实体挤压。

------

# 十二、资源配置入口

后续改数值，优先找这些资源：

```text
基础技能：
Assets/_Project/ScriptableObjects/Skills/SkillData/

Dash：
Assets/_Project/ScriptableObjects/Skills/DashData/

Projectile：
Assets/_Project/ScriptableObjects/Skills/ProjectileData/
或
Assets/_Project/ScriptableObjects/Enemies/Projectiles/

剑招：
Assets/_Project/ScriptableObjects/SwordArt/

剑招效果：
Assets/_Project/ScriptableObjects/SwordArt/Effects/

敌人：
Assets/_Project/ScriptableObjects/Enemies/
```

具体路径以当前 Unity 工程实际文件夹为准。

------

# 十三、已知简化 / 未完成项

这个很重要，交接时一定要说清楚。

## 1. 清风诀是简化版

当前只有：

```text
大范围伤害 + 击飞
```

未做：

```text
七段范围内突进
低血量斩杀
剑招强化版
```

------

## 2. 沧浪诀是简化版

当前只有：

```text
线形持续伤害
结束击飞
```

未做：

```text
印记系统
僵直系统
3 秒后对所有印记单位三段触发伤害
```

------

## 3. 长生诀是简化版

当前只有：

```text
固定位置回血领域
```

未做：

```text
特效
UI
领域强化
Buff 系统
```

------

## 4. 完美闪避未做

策划案里有：

```text
完美闪避
2 秒时缓
期间攻击暴击
剑招加强
```

当前未实现。

------

## 5. BOSS 未做

策划案里的 BOSS 悍爷未实现。

未做内容包括：

```text
快快慢慢快五连击
全场震击
定风波
50% 血量二阶段
```

------

## 6. UI / 动画 / 音效基本未接入

当前战斗系统偏代码和机制验证。

未来需要接：

```text
攻击动画
受击动画
击飞动画
剑招特效
回血特效
血条 UI
胜利失败 UI
音效
镜头震动
```

------

# 十四、后续别人继续开发，建议顺序

建议顺序：

```text
1. 给敌人和玩家接动画状态
2. 给三大剑招接 VFX
3. 补血条 UI / 剑招触发 UI
4. 补完美闪避
5. 优化水匪数值
6. 最后再做 BOSS
```

------

