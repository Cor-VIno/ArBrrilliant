# Feedback 使用说明

## 1. Feedback 系统用途

`FeedbackData` 是后续技能、剑招、音效、特效的统一接入口。

- 美术在对应 Cue 中拖入 VFX prefab。
- 音乐 / 音效在对应 Cue 中拖入 AudioClip。
- 程序只维护事件和播放规则，不在战斗逻辑里写死表现资源。

Cue 为空是允许的。没有 VFX、没有 AudioClip、没有 Animator Trigger 时，技能仍然正常释放。

## 2. 文件位置

技能 FeedbackData：

`Assets/_Project/ScriptableObjects/Feedback/Skills/`

剑招 FeedbackData：

`Assets/_Project/ScriptableObjects/Feedback/SwordArts/`

测试 Feedback prefab：

`Assets/_Project/Prefabs/Feedback/Test/`

测试 Sprite：

`Assets/_Project/Art/Feedback/Test/TestFeedbackPixel.png`

## 3. 技能资源对应关系

- `Feedback_Basic_Horizontal`：轻击
- `Feedback_Basic_Vertical`：重击
- `Feedback_Basic_LeftFalling`：远程
- `Feedback_Basic_RightFalling`：突刺

这些资源中的 `skill` 字段已经绑定到对应 `SkillData`。

## 4. 剑招资源对应关系

- `Feedback_QingFengJue`：剑招：清风诀
- `Feedback_CangLangJue`：剑招：沧浪诀
- `Feedback_ChangShengJue`：剑招：长生诀

这些资源中的 `swordArt` 字段已经绑定到对应 `SwordArtData`。

## 5. 各 Cue 含义

技能 Cue：

- `castStartedCue`：技能前摇开始时播放。
- `executedCue`：技能真正执行时播放，例如 Hitbox、Projectile、Dash 开始产生判定的时机。
- `castFinishedCue`：技能结束时播放。
- `chargeStartedCue`：蓄力开始时播放。
- `chargeReleasedCue`：蓄力释放时播放。
- `dashStartedCue`：Dash / 突刺开始时播放。
- `dashFinishedCue`：Dash / 突刺结束时播放。
- `projectileSpawnedCue`：Projectile 实例生成后播放。

剑招 Cue：

- `executionStartedCue`：剑招执行开始时播放。
- `executionFinishedCue`：剑招执行结束时播放。

玩家完美闪避 Cue：

- `perfectDodgeCue`：配置在场景 Player 的 `PlayerSkillFeedbackBinder` 上，完美闪避触发时播放。

## 6. SpawnPoint 含义

- `CasterCenter`：释放者中心。常用于蓄力、爆发、完美闪避。
- `CasterFeet`：释放者脚下。常用于落地、步法、地面环形特效。
- `CasterForward`：释放者朝向前方。`localOffset.x` 表示沿攻击方向的偏移，`localOffset.y` 表示向上偏移。
- `Projectile`：Projectile 当前所在位置。常用于远程发射、弹体生成特效。
- `WorldPosition`：事件传入的世界坐标。当前主要用于完美闪避接触点等特殊事件。

## 7. 常用配置建议

- 轻击刀光：放在 `executedCue`，`spawnPoint = CasterForward`，开启 `rotateToDirection`。
- 重击蓄力光效：放在 `chargeStartedCue`，`spawnPoint = CasterCenter`，可开启 `parentToCaster`。
- 重击释放爆发：放在 `chargeReleasedCue`，`spawnPoint = CasterForward`。
- 远程发射特效：放在 `projectileSpawnedCue`，`spawnPoint = Projectile`。
- 突刺残影：放在 `dashStartedCue`，`spawnPoint = CasterCenter`，可开启 `parentToCaster`。
- 完美闪避特效：放在 Player 的 `perfectDodgeCue`，`spawnPoint = CasterCenter` 或 `WorldPosition`。
- 剑招大特效：放在剑招 FeedbackData 的 `executionStartedCue`。

## 8. 旧 Visual 系统说明

项目中仍然存在旧 `Assets/_Project/Scripts/Visuals/` 系统：

- `SkillVisualData`
- `VisualCueData`
- `PlayerSkillVisualBinder`

旧系统暂时保留，不删除，避免破坏已有引用。

从当前阶段开始，新接入的技能 / 剑招 VFX、SFX、Animator Trigger 建议优先走 `FeedbackData`。

不建议同一个技能同时在旧 `Visuals` 和新 `Feedback` 中配置同类 VFX，否则可能重复播放。例如轻击刀光不要同时填在 `SkillVisualData.ExecutedCue` 和 `SkillFeedbackData.executedCue`。

## 9. 注意事项

- 空 VFX / AudioClip 不会报错。
- 如果没有播放，先检查 Player 上的 `PlayerSkillFeedbackBinder` 或 `SwordArtFeedbackBinder` 是否绑定了对应 FeedbackData。
- 如果生成位置不对，检查 `spawnPoint` 和 `localOffset`。
- 如果方向不对，检查 `rotateToDirection`。
- 如果音效太大，调整 `volume`。
- 如果音高不对，调整 `pitch`。
- 如果特效需要跟随玩家，开启 `parentToCaster`。
- 不要直接修改战斗脚本来接特效或音效。

## 10. 当前测试配置

当前已经接入的测试 prefab 只用于验证链路，不是正式美术资源：

- `TestFeedback_CasterBurst`
- `TestFeedback_ForwardSlash`
- `TestFeedback_ProjectileSpawn`
- `TestFeedback_Dash`
- `TestFeedback_PerfectDodge`
- `TestFeedback_SwordArtBurst`

这些测试 prefab 都在：

`Assets/_Project/Prefabs/Feedback/Test/`

后续正式资源到位后，可以直接替换对应 Cue 的 `vfxPrefab`。
