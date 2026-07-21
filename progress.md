# DungeonStory Progress

## 2026-07-22 - Responsive HUD and final verification closure

- Fixed the remaining portrait-HUD polish gap by clamping the upper-right control strip to the live canvas width and forcing top/bottom navigation buttons to share flexible width instead of preserving template widths.
- Extended `DungeonResolutionPlayModeVerifier` to cover actual `1600x900` and `900x1600` Game View sizes and to load `TitleScene` before title checks, removing the previous false failure when the verifier was launched from gameplay.
- Re-ran the resolution matrix in PlayMode. `Temp/resolution-matrix-report.txt` now ends `RESULT=PASS; failures=0` with `capturedErrors=0; capturedWarnings=0`; the 900x1600 gameplay capture keeps upper-right controls and bottom tabs in-bounds.

## 2026-07-21 - Physical item and hauling implementation started

- Added phases 27-34 to the active plan for the requested RimWorld-style physical item, hauling, pile UX, integration, save, and verification work.
- Confirmed the current Unity Editor Console reports `Error 0 / Warning 0`; standalone batchmode compile is blocked because the interactive Editor is already running.
- Audited the stock, warehouse, click-selection, grid-layer, AI action, and lifetime-scope integration points before editing.

## 2026-07-20 - Character growth and skill redesign

- Completed the final acceptance pass. All implemented EditMode suites plus progression, population, facility evolution, room system/environment, and AI-plan regressions passed together.
- Re-ran the P1/P2 UI surface verifier with real pointer input (`18/18`) and the exclusive character/building click verifier; both captured zero errors and warnings. Visual review confirmed character-only, building-only, and skill-alert states are distinct and readable.
- Recovered MCP `Camera_Capture` with a plain runtime camera copied from `Main Camera`. The resulting nonblank 1920x1080 world capture showed all three floors, room boundaries, doors, facilities, characters, and lighting without HUD interference.
- Final Unity Console state is `Error 0 / Warning 0`; character-growth phases 14-18 are complete.
- Replaced the placeholder mood-only handling for management skill modules with real domain integrations: production output, flat stock, research work, cleaning and repair duration, staffed shop revenue, positive relationship sentiment, and spawned-intruder damage for defense ultimates. Added deterministic progression scenarios for each numeric path and management day-use activation.
- Strengthened the Unified UI PlayMode verifier with a real `CharacterProgression.OnDraftReady` alert and Input System press/release events. The alert button opened its detail, the single choice closed it and selected Growth, the player copy hid all LLM/request terminology, the screenshot was nonblank, and the complete UI regression still passed with zero errors/warnings.
- Added and ran a repeatable skill-runtime PlayMode probe. Management fired once on day 7001, ignored a duplicate day event, reset on day 7002, and activated a real 1.3x output modifier; defense reduced a spawned intruder from 120 to 115.5 HP; a direct offense ultimate dealt damage, set cooldown 999, and rejected same-battle reuse. Console remained Error 0 / Warning 0.

- Diagnosed the start-party `staff=4`/inactive failure as two `CharacterActor`-derived components on each character prefab, not duplicate spawning. Removed the obsolete `Customer` component from both character prefabs and added GameObject-based actor canonicalization to start-party, offense, and world-save paths.
- The full real-LLM pointer verifier now passes rerolls, nine tabs, three candidate confirmations, passive readiness, party commit, same-species staff, and UI close with `errors=0; warnings=0`. Visual inspection rejected its mobile artifact because the Editor ignored `Screen.SetResolution` and wrote another 1920x1080 frame.
- Replaced the false mobile resize with the shared Editor Game View resolution controller. A second real-LLM pointer run passed at actual `1600x900` and `900x1600`, produced nonblank exact-size captures, kept all three cards inside the portrait viewport, committed exactly two active staff, and ended with zero errors/warnings.
- Updated the full-game progression save verifier for V3 growth/narrative payloads and explicit V2 rejection; the real game-save JSON round trip passed at Lv.4/XP 77 with active/passive skills intact. Unified UI then passed growth, mood, records, notices, staff, and building preview after replacing its stale section-label expectation.

- Accepted the finalized level-50 character growth, modular skill generation, three-character preparation, and persistent world-character specification.
- Restored the previous planning files and appended phases 14-18 without changing completed offense work.
- Confirmed Unity MCP tool availability; editor-state and compilation checks are next.
- Unity Editor state query passed while idle; Console baseline is Error 0 / Warning 0.
- Audited actor component ownership, final stat queries, combat snapshot creation, local LLM profiles, owner selection, VContainer registration, and character save capture/restore.
- Implemented the level-50 per-character growth records, constrained modular skill generation, hidden retry state, combat/management runtime effects, population profiles, save V3 payloads, and growth-tab presentation.
- Added `StartPartyPreparationService`: it prepares an owner plus two same-species staff, rolls identity/aptitude separately, preserves three partial reroll charges, pre-generates the next skill roll, requires a selected first active plus validated passive, and only creates world actors on final confirmation.
- Fresh world profiles now resume their missing level-one active/passive drafts after restore; replacing or restoring a prepared roll cancels stale generation callbacks.
- Unity rebuilt `Assembly-CSharp.dll` after the start-party service changes with no C# compiler errors. The MCP bridge revoked approval during that domain reload and must be retried before PlayMode verification.
- Replaced the owner-choice callback with a real three-card preparation UI. Each card has Identity/Aptitude/Skill tabs, unlimited full rerolls, three charged partial rerolls, double-click confirmation for the permanent first active, and a gated final start action.
- A PlayMode pointer event selected the Slime owner and produced all nine per-character tabs. Runtime inspection confirmed six isolated current/prefetch growth objects and two hidden pending drafts per object; no Console errors or warnings were emitted.
- Fixed a discovered rerender defect where bottom preparation buttons accumulated outside the preparation root.
- Prepared traits now drive the effective per-character runtime profile, so generated traits affect consumption, crowd sensitivity, work/facility preference, accidents, spending, movement, and combat modifiers in addition to final stats.
- Visitor profiles now use readable names/origins and are promoted to permanent staff profiles on recruitment, preventing the same hired person from returning as a guest.

## 2026-07-20 - Character progression follow-up started

- Began the per-character level and skill progression increment requested after the expedition loop was completed.
- Chose instance-owned progression with shared skill definitions so characters of the same species can level and build different loadouts.

## 2026-07-20 - Darkest-Dungeon-style offense completed

- Replaced launch-to-one-battle offense with deterministic branching expeditions containing battle, event, camp, cache, and boss nodes.
- Added expedition light, supplies, persistent health/stress, front/middle/rear formation, ability position rules, camping, loot, retreat, and ordinary-battle return to route choice.
- Connected formal dungeon rooms, modular facility support abilities, and real warehouse stock to preparation capacity, scouting, starting light, medicine, camp recovery, loadout withdrawal, rollback, and return deposits.
- Extended active-expedition saves with route node, phase, completed nodes, supplies, loot, formation, stress, damage, and preparation data plus legacy migration.
- Restyled route and battle surfaces with compact charcoal, burgundy, brass, deep-green states and removed the oversized mint action slabs.
- Updated reward, Product Shell, and P1/P2 regressions so they traverse the new route UI instead of assuming immediate combat.
- Pointer-driven Product Shell verification passed owner selection, recruitment, map/composition, journey start, first battle, and exact manual save/restore with `capturedErrors=0; capturedWarnings=0`.
- A PlayMode UI-event run selected the owner, clicked recon/target/party/route/node/action/target controls, completed all six regions through `truth_core`, and reported `truth=True; history=6`.
- Visually inspected route, battle, and final truth captures. Final domain pass and Unity Console report `Error 0 / Warning 0`.

## 2026-07-20 - Multi-node offense implementation

- Connected front/middle/rear formation to battle persistence, ability source/target positions, enemy AI, and survivor compaction.
- Stress now weakens combat performance without reducing maximum health; regular nodes use smaller encounters and the final node uses the boss formation.
- Removed the immediate boss battle and victory full-heal lifecycle. Expeditions now return to route choice after ordinary victories and finalize only on retreat, defeat, or boss victory.
- Added `BuildingExpeditionSupportAbility` and `DungeonOffensePreparationService`.
- Usable dungeon rooms and Meal/Rest/Research/Mana/Logistics/Hygiene facilities improve camp recovery, scouting, light, medicine, and supply capacity.
- Expedition supplies are withdrawn atomically from real warehouse inventory, rolled back on failure, and unused supplies plus carried loot are deposited on return.
- Reimported scripts through Unity MCP and confirmed Console Error 0 / Warning 0.

## 2026-07-20

- Replaced the active plan with the requested dungeon-linked multi-node offense redesign. The previous single-battle campaign and temporary campaign-order combat multiplier are explicitly superseded.
- Removed the temporary campaign-order combat stat multiplier and its misleading full-campaign balance regression.
- Added the first multi-node expedition domain: deterministic branching route graph, entrance/battle/event/camp/cache/boss nodes, supplies mapped to dungeon stock categories, light, persistent member stress, front/middle/rear formation, camp/event choices, carried stock loot, and retreat/defeat/completion phases.
- Extended `OffenseExpeditionRun` to own that journey state while retaining the legacy constructors needed for save migration. Unity recompiles with no Console errors.

- Read the approved offense-victory implementation plan.
- Re-audited offense, run flow, character persistence, save ordering, DI registrations, and title ownership.
- Confirmed the current runtime still uses timed automatic expeditions.
- Confirmed baseline runtime compilation succeeds.
- Reduced planning notes to the active turn-combat implementation only.
- Added `DungeonDifficulty` selection and launch propagation.
- Added the pure turn battle model, inline ability modules, six encounters, enemy AI, and command idempotence.
- Replaced product expedition completion with `OffenseBattleRuntime` and added the full-screen battle UI plus dungeon switching.
- Added stable character IDs and V2 active-battle save/restore with V1 migration.
- Replaced one-time final defense runtime with recurring 10-day `EndlessDefense` boss cycles.
- Updated old run-flow PlayMode expectations from `FinalChallenge/TruthHunt` to two recurring boss cycles.
- Editor compile attempt failed because `Library/Bee/artifacts/1900b0aE.dag/Assembly-CSharp.ref.dll` is missing; runtime compilation is the next recovery step.
- Removed remaining product/debug timer-completion and combat-power comparison paths from offense expeditions.
- Reworked reward, manual QA, P1/P2, and product-shell probes to submit real turn commands.
- Added explicit run difficulty persistence to start snapshots, save data, and run results.
- Fixed battle start ordering, stale restore clearing, first-turn migration, and exact command-wait restoration.
- Added product-shell pointer coverage for difficulty, battle actions, dungeon switching, and exact V2 manual save/load.
- Unity omitted the standalone battle factory source from Bee; merged factory/controller types into `OffenseBattlePanel.cs` and removed the omitted source file.
- Confirmed through Unity MCP that the Editor is idle, compilation completed, and Console reports `Error 0 / Warning 0`.
- Ran the current product-shell verifier. It failed at the first pointer callback (`StartupSettingsButton`) and all later synthetic Input System clicks; report and captures were inspected and rejected as completion evidence.
- Retried without forced `InputSystem.Update`; diagnostics proved the virtual mouse remained at `(0,0)` despite valid button screen coordinates. Switched to the existing queue-plus-`InputState.Change` fallback used by the modular-facility verifier.
- Pointer-state fallback succeeded through title, difficulty, owner selection, Settings, Save, return-to-title, Continue, and missing-save handoff with zero captured errors/warnings.
- Current failure is now the verifier's offense navigation order: it tries to click an off-screen target before opening the world map and composition surfaces.
- Added Close controls to the generated world-map and expedition overlays, and changed product verification to follow the visible map-target-close-composition-member-start path.
- Runtime inspection commands first failed because `OffenseBattleRuntime` is not a `UnityEngine.Object` and the dynamic command assembly does not reference VContainer; a panel-only diagnostic then showed the map remained active after target selection.
- Found and fixed the actual interruption: target selection re-rendered and destroyed its button, then the verifier dereferenced that destroyed button for pointer diagnostics. The interrupted request was removed and PlayMode stopped cleanly.
- Re-ran the visible offense flow. Map selection and composition passed, but the clean run had zero eligible staff; the capture and report were rejected as battle evidence.
- Added a recruitment activation service: recruitment now resolves or creates the live character, converts it to an active NPC employee with `AbilityWork`, and only then commits the recruited record.
- Extended product verification to accelerate four valid customer visits, pointer-click the recruitment card in Operations, assert live staff conversion/expedition eligibility, then continue through the visible offense composition path.
- Explicit verifier import exposed `CS0165` on a short-circuited `out` variable; initialized the recruitment record before the candidate predicate. The stale request file that was repeatedly retrying PlayMode was removed.
- The next product-shell run still found no recruitment candidate. Auditing the product scene exposed two real loop gaps: `RegularCustomerRuntime` was never present, and `CharacterSpawner` serialized only an NPC test asset while the actual customer existed only in the Resources catalog.
- Registered and eagerly created `RegularCustomerRuntime` under the gameplay lifetime scope, then made `CharacterSpawner` merge catalog customer definitions once after injection. Unity compiles these changes with `Error 0 / Warning 0`.
- Corrected the character catalog dependency to the existing `IRunCharacterCatalog`, and reordered the lifetime-scope build callback so scene injection always completes before the generated recruitment runtime is resolved.
- Bound each regular-customer record to the exact live visitor that produced the record. Recruitment now converts that actor first, falling back to an ID lookup only after save restoration or actor loss.
- The full product-shell verifier now passes through real pointer input: Hard difficulty, customer recruitment into an eligible NPC worker, map/composition, battle start, guard, dungeon switch, manual save, attack/target, exact V2 reload, title autosave, Continue, and missing-save handoff. Captured errors/warnings are both zero.
- Visually inspected `offense-turn-battle.png` and `offense-turn-battle-changed.png`: round, health, command log, and enemy health changed visibly without UI overlap.
- Fixed the AI macro-goal debug fixture so it creates its own grid instead of depending on unrelated scene state; all 29 implemented scenario suites now pass.
- Updated RunFlow verification to enter `SampleScene` from the title architecture and confirmed two recurring boss cycles, non-terminal defense, stages 1-5 without Victory, and `truth_core` truth-reveal Victory.
- Re-ran Save UI, Unified UI, P1/P2 feature surfaces, character click priority, and room inspection PlayMode verification; every report passes with zero captured errors and warnings.
- Updated P1/P2 offense coverage to use the visible map, target, composition, member, and launch controls, while accepting either an ongoing battle or a legitimate one-hit victory without allowing early truth reveal.
- Added a stable room-overlay MCP capture helper and verified 4 fill cells plus 10 outline segments. Pausing in the same command prevents ordinary hover polling from clearing the overlay between MCP calls.
- Captured and visually inspected the full dungeon room overlay, direct turn battle before/after state change, and final truth-result screen.
- Re-ran independent RoomSystem, RoomEnvironment, OffenseBattle, OffenseWorldMap, and OffenseReward scenarios; all passed and the final Console count is `Error 0 / Warning 0`.
- Rejected scenario-state completion as final evidence and began a clean, pointer-driven player run from `TitleScene`.
- Bought the visible commercial blueprint, waited at X5 for natural visitors, recruited the first real candidate from Operations, selected stage 1 on the world map, composed the party, and won by issuing visible barrier/attack/target commands.
- Fixed the visitor exit stall discovered during that run and added a focused AI scenario.
- Fixed duplicate expedition rows caused by legacy and canonical actor components sharing one GameObject.
- Fixed world-map/composition overlay overlap and added victory return treatment so surviving staff can continue the campaign.
- Added Orc and Vampire customer assets to make the natural recruitment pool large enough for the 2- and 3-member campaign gates.
- Re-ran customer, staff, world-map, battle, and reward regression suites; all five pass. Resource loading confirms three recruitable customer definitions.
- Direct replay exposed duplicate scene/generated recruitment runtimes; changed the lifetime scope to reuse the scene-authored runtime and verified the next clean run had exactly one.
- Direct replay also exposed seed-dependent recruitment starvation at the old 75 satisfaction threshold. Lowered the product/default threshold to 65 while retaining the separate visit-count gate.
- Recruited two employees through the visible Operations UI and directly won stages 1 and 2. Stage 3 then killed both employees despite using species abilities and focused targets, proving the current campaign was not naturally completable.
- Confirmed building training only affects mood and there is no persistent combat-stat growth path.
- Added stage-derived offense preparation, surfaced its bonus/effective power in the UI, and applied the same deterministic multiplier on battle start and exact save restoration.
- Added and passed a full six-stage, all-difficulty campaign-balance regression plus the existing battle, reward, and world-map suites.

## Next

- Tune content breadth: more authored route events, enemy intents, curios, diseases/quirks, and region-specific audiovisual treatment can now build on the completed expedition framework.

## 2026-07-20 - Character progression implementation

- Added actor-owned `CharacterProgression` with levels 1-20, rollover XP, learned skills, three equipped slots, exact loadout restore, and legacy-save defaults.
- Added species/trait skill tracks plus level 2/4/6 shared techniques; offense combat now receives equipped skills only.
- Character levels increase offense combat statistics by 4% per level and appear in expedition, battle, and character UI.
- Training use, completed work, battle outcomes, and successful expedition returns award XP at their single completion points.
- Character world saves now capture and restore level, XP, learned skill IDs, and equipped skill IDs.
- Added a generated `성장` tab with an XP meter and pointer-clickable skill loadout controls.
- Added and passed progression curve, actor isolation, unlock/loadout, persistence, legacy default, and training reward scenarios.
- Extended Unified UI PlayMode verification to click the Growth tab and toggle a skill through UI pointer events.
- Focused PlayMode save verification restored `Lv.6 / XP 77 / equipped 2` through the real game save service and JSON boundary.
- The pointer-driven six-region campaign still reaches `truth_core`; the three recurring party members finished at levels 6, 5, and 3 with 4, 3, and 2 learned skills.
- Final progression plus offense regression passed and Unity Console finished at `Error 0 / Warning 0`.
- Updated the stale full-save offense expectation and made orphan battle snapshots fall back to the valid saved journey; the complete game save round trip now passes too.

## 2026-07-20 - Weak-link audit

- Audited cross-system runtime paths and prioritized ten weak links across identity, progression pacing, LLM dependency, passive execution, room quality, offense preparation, formation, persistence, rerolls, and growth feedback. No gameplay code changed in this audit.
## 2026-07-20 Closed-loop integration implementation started

- Preserved the existing dirty worktree and scoped the approved follow-up to identity/save, skills/combat, room/work, equipment/recovery, UI, and direct-play verification.
- Added active phases 20-26 to `task_plan.md`; Phase 20 starts with the string identity registry and V4 persistence boundary.
- Converted regular-customer records, staff-discontent records, social rumors, and world profiles to string persistent IDs at their save/runtime boundaries.
- Added V4 save fields for per-actor social memory, global facility reputation, and staff discontent; staff discontent now captures/restores by persistent staff ID instead of shared templates.
- Strengthened focused EditMode coverage: regular customer, staff discontent, character progression/population restore, and duplicate world-profile ID rejection all pass. Unity Console remains `Error 0 / Warning 0`.
- Changed the level curve to the approved `20 + floor((level-1)/10)*5` formula; level 1->50 now requires 1,460 XP and is covered by progression scenarios.
- Connected offense node XP to real expedition resolution: event/camp/cache, normal battle, elite battle, boss battle, and successful return now award the approved stage-scaled XP values.
- Added offense pacing coverage showing the combat-heavy route reaches level 50 by stage 3 while the safer route reaches it by stage 4, with both avoiding earlier max-level drift.
- Generated active skills now store authored formation constraints. Damage skills convert to front/middle attacks, control skills to middle/rear, and support skills to middle/rear without LLM override.
- Added a guarded `CharacterSkillExecutionContext` path and connected battle-start, damage-taken, and enemy-defeated passive triggers through the real offense battle runtime. Focused progression/offense scenarios pass with Unity Console `Error 0 / Warning 0`.
- Fixed the equipment crafting UI path discovered by PlayMode: building info now becomes interactable immediately when opened, so visible craft buttons are not ignored during the fade-in frame.
- Strengthened `ExpeditionEquipmentPlayModeVerifier` to seed and verify the actual queried warehouse inventory, pointer-click the craft button, assert craft queue creation, material withdrawal, craft work completion, equipment equip, nonblank screen capture, and zero captured errors/warnings.
- The pointer-driven equipment loop now passes: `queue=0->1`, weapon stock `35->33`, Iron Edge inventory `0->1`, equip state visible in the expedition detail, and Unity Console `Error 0 / Warning 0`.
- Hardened offense save capture/restore against JsonUtility blank `activeBattle` objects: only nonempty battle snapshots are restored, and capture only keeps a battle that matches an active `InBattle` expedition.
- Extended the full-game V4 save round trip to include expedition equipment inventory, reserved loadout, craft queue, and expedition recovery stress. It now passes with `warnings=0`, one active expedition, one intruder, and exact equipment/recovery restoration.
- Re-ran focused closed-loop EditMode scenarios after the fixes: character progression, offense battle, modular facility, and room environment all pass. Final Unity Console check remains `Error 0 / Warning 0`.
- Added saved level-growth allocation records and a public stat-breakdown API so the growth UI can show base, species/trait, level, equipment, conditional passive, and final values without duplicating calculation logic.
- Rebuilt the generated character growth tab as a scrollable surface and added visible stat breakdowns, combat-only equipment notes, and recent growth-allocation reasons.
- Updated Unified UI PlayMode verification to open the Growth tab by pointer input and require the stat breakdown headings. It passes after fast-committing a real start party, with updated capture `Temp/phase67-character-growth-tab.png` and `capturedErrors=0; capturedWarnings=0`.
- Strengthened progression EditMode coverage so level 50 creates 59 allocation records, records match `levelGrowthStats`, and stat breakdowns match final stat calculation. Focused progression, offense battle, modular facility, and room environment scenarios pass with Unity Console `Error 0 / Warning 0`.
- Extended the V4 full-game save round trip again so the expedition test member levels up once and the new allocation record must survive JSON capture and restore. The PlayMode round trip passes with `jsonBytes=102875`, `warnings=0`, and Console `Error 0 / Warning 0`.
- Added direct-play recovery and recruitment gates to `NaturalRunPlayModeVerifier`: after each real offense stage it now waits for actual staff health/stress recovery and recruits through the visible Operations UI when the next region requires more members. No scenario-state injection is used for these gates.
- A clean commerce-strategy replay drove the UI through new run, blueprint purchase, research priority, day-7 defense placement, and stage-1 offense victory. Stage 2 then correctly blocked on readiness instead of launching, exposing that injured/stressed staff were not actually selecting recovery facilities (`ready=0/1`, hp `79%/41%`, stress `62/66`).
- Fixed that recovery connection gap: `Rest.asset` now uses the facility-aware `NeedRest` consideration, staff self-care facility considerations no longer require a guest visit count, and on-duty stressed staff can select Hygiene. The StaffDuty recovery scenario now verifies actual Rest/Hygiene action scores, destinations, and job-giver selection against P1 recovery facilities.
- Re-ran recovery-linked scenarios after the patch: staff duty, offense journey, and modular facility all pass with Unity Console `Error 0 / Warning 0`.
- Fixed a stale compile blocker in `NaturalRunRuntimeDebugProbe` (`IGameDataProvider.TryGetGameData` is the real API) so Unity no longer executed old assemblies while reporting outdated StaffDuty diagnostics.
- Corrected the StaffDuty recovery test contract: `TryFindBestScoredAction` is only a scored candidate and does not populate `AIAction.destination` until commit; the scenario now validates `TryResolveDestinationWithFailure` for Rest/Hygiene destinations. Focused closed-loop regressions pass again: StaffDuty, OffenseJourney, CharacterProgression, OffenseBattle, ModularFacility, and RoomEnvironment with Console `Error 0 / Warning 0`.
- Direct commerce replay after those fixes reached a real stage-1 expedition through natural UI/time flow, including day-7 defense placement. It then failed at the stage-1 boss: the two starting staff survived the first battle/cache/elite path, but one entered the boss route at roughly 6 HP and both died with the boss still at 50/82 HP. This is now an intra-expedition attrition/balance issue, not a recovery-facility selection issue.
- Updated the direct-play verifier to recruit and use the full three-person main party from stage 1, load actual available supplies from the preparation snapshot, spend medicine before dangerous route choices, and prefer camp nodes when health/stress justify it. This keeps the fix in the player automation path rather than weakening battle resolution.
- Fixed two headless listener regressions exposed by the focused suite: `EventAlertRuntime` now preserves records while skipping UI rendering when no presenter factory is injected, and `FacilityEvolutionRecordRuntime` ignores events until its recorder is injected. Focused closed-loop regressions pass again: OffenseJourney, OffenseBattle, OffenseReward, StaffDuty, CharacterProgression, ModularFacility, and RoomEnvironment with Console `Error 0 / Warning 0`.
- The next direct commerce replay passed the Day 7 pointer-driven defense build and Day 10 boss-defense trigger, then failed before stage 1 launch because the strongest candidate had 3 visits and 83.2 satisfaction but default recruitment still required a 4th visit. Lowered only the default recruit-candidate visit threshold to 3 and updated the recruitment regression to match the V4 persistent-person model where shared `CharacterSO` templates remain spawnable while the recruited persistent ID is promoted. Recruitment, StaffDuty, OffenseJourney, OffenseReward, and OffenseBattle regressions pass with Console `Error 0 / Warning 0`.
- A follow-up natural replay showed the code default was not enough because `SampleScene` had serialized the old recruitment threshold. Updated `SampleScene`'s `RegularCustomerRuntime` rule from 4 visits to 3 visits and re-ran recruitment plus offense journey/reward regressions with Console `Error 0 / Warning 0`.

## 2026-07-21 Closed-loop feature verification pass

- Switched from slow day-10/direct campaign play to focused feature tests as requested.
- Fixed stale QA contracts after the V4/string-identity and Craft-work additions: WorkPriority now expects `Craft`, staff rebellion/facility evolution/AI plan/P1P2 fixtures assign persistent IDs, and Save UI/ProductShell pointer helpers recreate or force the verification mouse when Unity input state sticks after scene changes.
- Re-ran focused EditMode failures (`WorkPriority`, `FacilityEvolution`, `StaffRebellionResponse`, `CharacterAiPlan`) and the broad implemented-scenario runner; all passed with no recent Console errors/warnings.
- PlayMode feature tests passed: camera movement stays unscaled by game speed, Save UI pointer save/load/delete works, Unified UI start/growth/event surfaces pass, ProductShell passes, and P1/P2 feature surface rows pass `18/18`.
- Added a file-request driven feature batch for MCP-independent PlayMode verification and fixed the QA-only `CS1626` compile error caused by yielding inside a `try/catch` coroutine.
- Final feature batch passed in one chain: BuildPlacement, CharacterClick, RoomInspection, ExpeditionEquipment, and SkillRuntime. Reports show `capturedErrors=0` and `capturedWarnings=0` where applicable.
- Current proof artifacts include `Temp/build-placement-ux-report.txt`, `Temp/phase47-exclusive-world-info-report.txt`, `Temp/room-inspection-playmode-report.txt`, `Artifacts/QA/expedition-equipment-playmode-report.txt`, and `Temp/character-skill-runtime-playmode-report.txt`.
- MCP direct Camera_Capture remains unavailable/revoked in this continuation, so Phase 26 direct-play/camera-capture evidence is intentionally still pending rather than claimed complete.

## 2026-07-21 Physical item and hauling implementation

- Added `DungeonItemCatalogSO`, `ItemHaulingSettingsSO`, `WorldItemStackRuntime`, `CharacterCarryInventory`, `AbilityHaul`, `AIHaul`, and `ItemPileInfoPanel`.
- Save data is now V5 and captures physical world stacks, hauling settings, and per-character carried items.
- Added `GridLayer.Item` and changed click priority to `Character > Item > Building`; `Alt` click forces item pile selection on a character-occupied cell.
- Delivery purchases and stock rewards now spawn loose world stacks at the entrance dropoff when the physical item runtime is active; gold remains abstract.
- Staff/owner hauling can reserve loose stacks, walk to the pickup cell, carry within character weight limits, suffer overburden speed penalties, and deposit stock categories into warehouse inventory.
- Shop restock uses the already-present physical restock route from warehouse to shop; successful purchases and shoplifting now add carried items to the customer.
- Options UI has a `최대 운반 배율` slider, and character info shows carried weight, base limit, max allowed weight, overburden penalty, and carried item names.
- The item pile panel opens as a singleton runtime listener, shows pile rows, switches the same panel to stack detail on row click, and returns to the list with a Back button.
- Created default `Assets/Resources/SO/Items/DungeonItemCatalog.asset` and `Assets/Resources/SO/Items/ItemHaulingSettings.asset` so item authoring and hauling tuning are visible in Unity.
- Added `PhysicalItemDebugScenarios`; the focused contract report passes for stock fallback, carry weight penalty, pile sorting/detail, stack delete fallback, warehouse aggregation, and V5 JSON save payloads.
- PlayMode MCP smoke verified `WorldItemStackRuntime.Active`, dropoff stack spawn, pile lookup, one-cell fallback marker bounds, item pile panel opening, row detail view, and Back navigation.
- Current verification state: Unity compile succeeds and Console is `Error 0 / Warning 0`.
- Expedition return supplies and loot now prefer entrance dropoff stacks when the physical item runtime is active, with direct warehouse deposit kept only as a fallback.
- Converted equipment craft materials/output and outbound expedition packing to physical stack flows, then added dedicated PlayMode coverage for actual `AIHaul` movement across loose-stack warehouse deposit, facility input delivery, craft material delivery, crafted-equipment output hauling, expedition supply packing/consume, and character carried-weight UI.
- Fixed two logistics bugs exposed by that verifier: `AbilityHaul.StartHauling()` no longer clears the freshly reserved job before starting its coroutine, and warehouse delivery now selects the nearest reachable warehouse delivery cell instead of whichever warehouse the scene query returns first.
- New proof artifact: `Artifacts/QA/physical-item-logistics-playmode-report.txt` reports `RESULT=PASS; failures=0`, with `capturedErrors=0` and `capturedWarnings=0`; carry UI capture is `Artifacts/QA/physical-item-carry-ui.png`.
- Re-ran the physical item pass for the current request. EditMode contracts pass in `Temp/physical-item-contracts.tsv` and the current Console is `Error 0 / Warning 0`.
- Fixed the item-pile PlayMode verifier entry path after it failed against the new start-party flow (`RUN_READY owner selection is still active`). It now uses the same fast start-party commit path as the logistics verifier and supports a request-file runner at `Temp/physical-item-pile-playmode.request`.
- Re-ran both physical PlayMode suites from the current assemblies: `Artifacts/QA/physical-item-pile-playmode-report.txt` and `Artifacts/QA/physical-item-logistics-playmode-report.txt` both report `RESULT=PASS; failures=0`, `capturedErrors=0`, and `capturedWarnings=0`.
- Added the final warehouse-storage polish: stored physical stacks mirror warehouse aggregate stock, stay hidden by default, and appear only while the new top-right `물품` toggle is enabled. The toggle turns itself off during build/grid modes.
- Restoring V5 physical item data now resynchronizes warehouse aggregate inventory from stored physical stacks when those stored stacks exist, preventing the old direct-inventory view from drifting away from the physical source of truth.
- Extended the V5 save contract so it explicitly round-trips a character `carryInventory` item. The updated `Temp/physical-item-contracts.tsv` reports `save_v5_contract PASS version=5; stacks=4; carried=3`.
- Re-ran the current Unity physical item verification after the save-contract change: EditMode physical contracts all pass, the UI feature batch reports `UI_REGRESSION_BATCH PASS` for `PhysicalItemPile` and `PhysicalItemLogistics`, both PlayMode reports end with `RESULT=PASS; failures=0`, and Unity Console is `Error 0 / Warning 0`.
- Resumed Phase 26 direct-play verification and found the stage-3 party was losing its recruited staff member because staff promotion was not authoritative across population binding/release. `WorldCharacterProfile.isStaff` now reasserts NPC runtime identity on bind/refresh/promote/release, and the spawner keeps staff profiles active instead of sending them back to the visitor pool.
- Added a regression to simulate a promoted staff actor whose type was reset to `Customer`; `CharacterPopulationDebugScenarios`, `StaffDutyDebugScenarios`, `CharacterProgressionDebugScenarios`, `OffenseJourneyDebugScenarios`, and `OffenseRewardDebugScenarios` all pass after the fix.
- Updated stale reward tests for physical rewards and the minimum-two recruit-candidate rule, then made `CharacterCarryInventory` single-instance and fixed physical-item fixtures to use `Ensure`. The refreshed `Temp/physical-item-contracts.tsv` reports every physical item case as `PASS`, and the current Console has `Error 0 / Warning 0`.

## 2026-07-22 Phase 26 direct-play completion

- Resumed the pending no-injection commerce direct-play campaign after the physical item work and reproduced the remaining failure at stage 6 `truth_core`: the verifier skipped usable camp recovery and entered the final boss with an under-recovered/under-leveled party, causing a real defeat.
- Fixed the verifier's natural-play strategy instead of weakening encounter stats: late-stage loadouts now reserve enough rations to enter and use camp, cautious camp selection accounts for the travel ration cost, enemy-directed non-damage combat modules are treated as usable attack/control skills, and the final stage waits for a Lv50 party with safe health/stress.
- Re-ran focused regressions after the patch: `CharacterPopulationDebugScenarios`, `StaffDutyDebugScenarios`, `CharacterProgressionDebugScenarios`, `OffenseBattleDebugScenarios`, `OffenseJourneyDebugScenarios`, `OffenseRewardDebugScenarios`, and `PhysicalItemDebugScenarios` pass.
- Clean `commerce` natural run now reports `NATURAL_RUN PASS strategy=commerce`. Stage 6 logs show `final:maxLevel=3/3`, camp `useSupply=True`, medicine before boss, `OFFENSE_STAGE_6_BATTLE_COMPLETE outcome=Victory`, and `OFFENSE_CAMPAIGN_FINISH completed=6/6; truth=True; outcome=Victory`.
- Evidence artifacts: `Temp/natural-run-commerce-report.txt`, final-result HUD `Temp/natural-run-commerce.png`, 1600x900 HUD `Temp/natural-run-commerce-1600x900.png`, 900x1600 HUD `Temp/natural-run-commerce-900x1600.png`, and MCP `Unity_Camera_Capture` from Main Camera instance `65154`.
- Final Unity Console check after captures and stopping PlayMode is `Error 0 / Warning 0`.
