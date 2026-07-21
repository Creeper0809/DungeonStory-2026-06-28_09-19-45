# Progress: Remaining Runtime Design-Mistake Audit

## 2026-07-17
- Loaded the planning, Unity lifecycle, and ScriptableObject architecture skills.
- Restored root planning context and identified the prior audit plus ongoing remediation.
- Counted 335 runtime C# files under `Assets/Scripts`, excluding Editor paths.
- Created this isolated audit plan so the ongoing Phase 72 implementation plan remains untouched.
- Read the complete prior audit finding table and current remediation ledger; converted both into an explicit overlap filter.
- Runtime code changes: none.
- Logged and corrected one PowerShell quoting failure before any source scan executed.
- Completed broad enum/switch, concrete-type, service-lookup, magic-string, lifecycle, and file-size scans.
- Recorded six candidate clusters; none is confirmed until producer/consumer behavior is traced.
- Inventoried serializable containers, ScriptableObjects, exact global lookups, and semantic string comparisons.
- Added five high-signal data/routing candidates for end-to-end tracing.
- Confirmed the facility-shop union, LLM raw-validation dispatch, evolution status protocol, and species-name progression routing through their real consumers.
- Confirmed defense log-tag parsing end to end but flagged it for overlap review against the excluded prior P2 defense finding.
- Confirmed global GameData persistence, offense reward dispatch/state/text inference, and the full local-LLM request taxonomy.
- Distinguished container-owned `IDisposable` entry points from real caller/lifecycle defects; confirmed callback-less LLM abort and anonymous BackgroundManager subscriptions.
- Traced queue abort into permanently pending customer persona, facility evolution, and hidden character-log consumers.
- Found `AbilitySchedule` disable/reenable asymmetry, then rejected the initial pooled-actor claim after verifying the actual customer pool prefab lacks that component.
- Confirmed `GameData` duplicates derived clock values and engine time-scale authority in addition to its fixed save DTO.
- Confirmed the facility-evolution material provider is absent from production composition and replaced by a rejecting empty fallback.
- Confirmed role-specific persona state is mandatory in the CharacterActor aggregate and display-name facility matching affects actual scoring and AI targets.
- Confirmed typed AI failure kinds are reconstructed from localized diagnostic text and then alter cooldown/priority behavior.
- Verified the offense success path from `OffenseExpeditionRuntime` through `OffenseRewardRuntime` and the category grant service; no scene/prefab reference to the reward runtime script GUID was found, so the final report distinguishes the callable consumer path from current scene composition.
- Rechecked exact line anchors for all retained findings and removed lower-impact or overlapping candidates.
- Final report set contains nine findings: two high, one medium-high, and six medium.
- Runtime code changes: none. Only this isolated `.planning` audit record was updated.
