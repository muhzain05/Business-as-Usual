---
type: "query"
date: "2026-10-10T16:36:05.001457+00:00"
question: "Inspect HUD, interaction, receipt, task and shift-report dependencies for the approved Phase 4.2 integration"
contributor: "graphify"
outcome: "useful"
source_nodes: ["ShopHUD", "ShopUIView", "ShopUITheme", "ShopShift", "TransactionBook"]
---

# Q: Inspect HUD, interaction, receipt, task and shift-report dependencies for the approved Phase 4.2 integration

## Answer

Expanded tokens: ShopHUD ShopUIView ShopUITheme ShopShift TransactionBook. ShopHUD uses the read-only ShopUIView snapshot and shared ShopUITheme drawing helpers. Shift and transaction logic remain in ShopShift and ShopDomain. Source locations: Assets/Shopkeeper/Scripts/ShopHUD.cs, ShopUIView.cs, ShopUITheme.cs, ShopShift.cs and ShopDomain.cs. Unity confirmed the original SunnyPlayer HUD shift/player references. Scene and gameplay files are protected by hash comparison. Tab retains the receipt-book binding; released-cursor GUI clicks browse pages without producing transactions. Refresh presentation caches in OnEnable to survive editor assembly reloads. verify_phase3.py has an empty-index assertion incompatible with preserving staged changes.

## Outcome

- Signal: useful

## Source Nodes

- ShopHUD
- ShopUIView
- ShopUITheme
- ShopShift
- TransactionBook