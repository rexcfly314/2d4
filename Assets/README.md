# Unity 2D — 玩家與敵人 Slider 血量 UI（修正版）

適用於 Unity 6+、uGUI `Slider` 和 TextMeshPro。使用提供的 `PlayerStatus`、`EnemyStatus`、`PlayerUnit`、`EnemyUnit`、`EnemyPool`、`DeadModule`，不重新建立傷害或敵人物件池。

## 一、程式檔案與更新方式

| 檔案 | 操作 | 職責 |
| --- | --- | --- |
| `PlayerUnit.cs` | 取代原同名檔案 | 保留既有行為，增設 `HealthChanged` 事件 |
| `EnemyUnit.cs` | 取代原同名檔案 | 保留既有行為，增設 `HealthChanged` 事件 |
| `HealthBarView.cs` | 新增或取代前一版 | 使用 `Slider` 更新血條與可選的 TMP 數字 |
| `PlayerHealthPresenter.cs` | 新增或沿用上一版 | 監聽玩家血量變更並更新 HUD |
| `EnemyHealthPresenter.cs` | 新增或沿用上一版 | 監聽敵人血量變更，支援池化再生 |

> **從上一版 Image 血條教材升級時**：只需要替換 `HealthBarView.cs`，並在 Inspector 把 `Slider` 指派給 `Health Slider`。`PlayerUnit`、`EnemyUnit`、兩個 Presenter 的上一版若已正確安裝，可以保留，無須重複替換。

不要保留兩份 `PlayerUnit` 或 `EnemyUnit`。不修改 `PlayerStatus`、`EnemyStatus`、`Projectile`、`EnemyAttackModule`、`WeaponSlot`、`DeadModule`、`EnemyPool`。

## 二、玩家固定 HUD（Screen Space - Overlay）

用 Unity 選單 **GameObject > UI > Slider** 建立血條，Unity 會建立 Slider 內建的 Background / Fill Area / Fill / Handle Slide Area / Handle 階層。血量顯示不需要拖動，因此可以刪除 `Handle Slide Area`（連同 Handle）。

```text
Canvas [Screen Space - Overlay]
└── PlayerHealthBar [Slider, HealthBarView, PlayerHealthPresenter]
    ├── Background [Image]
    ├── Fill Area [RectTransform]
    │   └── Fill [Image]
    └── HealthText [TextMeshProUGUI] (可選)
```

1. `PlayerHealthBar` 的 `Slider`：`Direction = Left To Right`、`Interactable = Off`、`Whole Numbers = Off`。
2. 將 `Slider > Fill Rect` 指向 `Fill` 的 RectTransform。`Fill` 可以使用一般的 `Image Type = Simple`，**不需要切換成 Filled**。Slider 會依數值自動改變填滿區域。
3. `Min Value / Max Value / Value` 由 `HealthBarView` 於執行時更新，不需要在 Inspector 中手動同步玩家最大血量。
4. 在 `PlayerHealthBar` 上加入 `HealthBarView` 與 `PlayerHealthPresenter`。
5. `HealthBarView > Health Slider` 指向**同一物件的 Slider**；未指定時會自動抓取。`Health Text` 若需要顯示 `80 / 100`，則指向 `HealthText`；不需要就留空。
6. `PlayerHealthPresenter > Player` 指向場景內有 `PlayerUnit` 的玩家；`Health Bar View` 指向同物件的 `HealthBarView`。若未手動指定 Player，會嘗試透過 `GameManager.Instance.GetPlayer()` 取得。
7. `PlayerHealthBar` 的 RectTransform：左上 Anchor、Pivot `(0,1)`、參考位置 `X=24, Y=-24`、尺寸 `280 × 32`（可依遊戲調整）。
8. 若選擇 `Interactable = Off` 後顏色被 Unity 的 Disabled Tint 變暗，可將 Slider 的 `Transition = None`，並用 `Background` / `Fill` 自行指定顏色。

## 三、敵人頭頂血條（World Space）

**編輯 `EnemyPool` 使用的 Enemy Prefab 原始資產**，不要只調整場景中的某一個敵人實例。

```text
EnemyPrefab [EnemyStatus, EnemyUnit, EnemyHealthPresenter]
├── VisualRoot [原有 SpriteRenderer / Animator]
└── HealthCanvas [Canvas: World Space]
    └── EnemyHealthBar [Slider, HealthBarView]
        ├── Background [Image]
        └── Fill Area [RectTransform]
            └── Fill [Image]
```

1. 在 Enemy Prefab 根物件加入 `EnemyHealthPresenter`，把 `Health Bar View` 指向 `EnemyHealthBar` 的 `HealthBarView`。
2. 新增 `HealthCanvas`：`Render Mode = World Space`、RectTransform `160 × 22`、Local Scale 約 `(0.01,0.01,0.01)`、Local Position 約 `(0,0.85,0)`；依 Sprite 高度調整。可移除 `GraphicRaycaster`。
3. 在 World Space Canvas 內建立 `UI > Slider` 作為 `EnemyHealthBar`。刪除 Handle 子階層，保留 `Background` 與 `Fill Area/Fill`。
4. 設定 `Slider > Fill Rect = Fill`、`Direction = Left To Right`、`Interactable = Off`、`Transition = None`。`Fill` 使用一般 `Image` 即可，不需 `Image.Type.Filled`。
5. `HealthBarView` 的 `Health Slider` 指向同物件的 Slider。敵人血條通常不顯示數字，`Health Text` 留空。
6. 確認 Canvas 的 Sorting Layer/Order in Layer 能讓血條顯示在敵人視覺上方，並確認角色根物件不會隨移動旋轉血條（目前 `EnemyMovement > Rotate Toward Target = false`）。
7. 儲存並 Apply Enemy Prefab，確認 `EnemyPool > Enemy Prefab` 參考此 Prefab。

## 四、Slider 與數值的資料流

```text
Projectile / EnemyAttackModule
            │
            ▼
PlayerUnit.TakeDamage() / EnemyUnit.TakeDamage()
            │
            ▼
PlayerStatus / EnemyStatus (血量計算)
            │
            ▼
Unit.HealthChanged(currentHealth, maxHealth)
            │
            ▼
PlayerHealthPresenter / EnemyHealthPresenter
            │
            ▼
HealthBarView.SetHealth(currentHealth, maxHealth)
            │
            ├── slider.minValue = 0
            ├── slider.maxValue = maxHealth (至少 1)
            └── slider.SetValueWithoutNotify(currentHealth)
            │
            ▼
Slider 透過 Fill Rect 顯示填滿比例
```

`HealthBarView` 不使用 `Image.fillAmount`，亦不需要每一幀執行 `Update()`。

敵人池的 `EnemyPool.Spawn()` 在啟用敵人前呼叫 `RestoreFullHealth()`，`EnemyHealthPresenter.OnEnable()` 會重新讀取 `CurrentHealth` / `MaxHealth`，因此再次生成時會同步顯示滿血。

## 五、Play Mode 驗收

1. 玩家及敵人開始時顯示滿血；玩家 HUD 固定於畫面左上、敵人血條跟隨敵人。
2. 玩家受敵人接觸傷害後，Slider 從右往左縮短；玩家 TMP 數字同步更新（若有配置）。
3. `Projectile` 命中敵人後，敵人 Slider 正確減少。
4. 透過 Unit 的 `Heal()` / `SetMaxHealth()` / `RestoreFullHealth()` 更新血量時，Slider 正確同步。
5. 敵人死亡回到池子，再生後 Slider 恢復滿格。
6. 血條不可被滑鼠拖動；不顯示 Handle，且 Console 無缺少 Slider / Unit 的錯誤。

## 六、疑難排除

- **血條不會變動**：檢查 `Health Slider` 參考，以及 Slider 的 `Fill Rect` 是否已指派 `Fill`。
- **無法正常從左向右顯示**：檢查 Slider `Direction = Left To Right`，並避免將 Fill Rect 設到 `Background`。
- **血條灰掉**：檢查 `Transition = None`，避免 Slider Disabled Tint 改變顏色。
- **玩家 HUD 不見**：檢查 Canvas `Screen Space - Overlay`、Anchor、Canvas 啟用狀態。
- **只有一隻敵人有血條**：確認更改的是 Enemy Prefab，而非場景中某一實例。
- **再生後血量不正確**：確認使用 `EnemyPool.Spawn()` 與 `EnemyHealthPresenter.OnEnable()` 重刷。
- **重複定義錯誤**：確認 `PlayerUnit` / `EnemyUnit` 僅各有一個類別。

## 七、課堂練習

- A：改變玩家 Slider 的 Background / Fill 配色，保留左上固定 HUD。
- B：調整敵人 World Space Canvas 高度，讓血條顯示在 Sprite 頭頂。
- C：將敵人最大血量修改為 60，驗證 `CurrentHealth=30` 時 Slider 顯示 50%。

教學包不包含新圖片、美術素材或 Prefab。已檢查程式結構，但尚未在實際 Unity Editor 中執行編譯與 Play Mode。
