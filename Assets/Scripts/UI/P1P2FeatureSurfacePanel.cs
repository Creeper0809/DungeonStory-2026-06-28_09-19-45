using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed partial class P0FeatureSurfacePanel
{
    private enum EconomyViewMode
    {
        Current,
        Latest,
        History
    }

    private enum ArchiveViewMode
    {
        Codex,
        Reports,
        Events
    }

    private EconomyViewMode economyViewMode;
    private ArchiveViewMode archiveViewMode;
    private CodexEntryCategory codexCategory = CodexEntryCategory.Monster;
    private EventAlertImportance? eventImportanceFilter;
    private string selectedCodexEntryId = string.Empty;
    private int selectedEventRecordId = -1;
    private int selectedShopId;
    private int selectedDefenseReportIndex;
    private int selectedExpeditionResultIndex;

    private sealed class RoomUiEntry
    {
        public Grid Grid;
        public RoomInstance Room;
        public RoomEnvironmentSnapshot Environment;
    }

    internal void BuildFacilitiesManagement()
    {
        BuildRoomIdentitySection();
        BuildSynthesisSection();
        BuildEvolutionSection();
    }

    private void BuildRoomIdentitySection()
    {
        FacilityEvolutionRuntime evolution = sceneQuery.First<FacilityEvolutionRuntime>(includeInactive: true);
        List<Grid> grids = sceneQuery.All<GridSystemManager>(includeInactive: true)
            .Where((manager) => manager != null && manager.grid != null)
            .Select((manager) => manager.grid)
            .Concat(FindPlacedFacilities().Select((facility) => facility.Grid).Where((grid) => grid != null))
            .Distinct()
            .ToList();
        List<RoomUiEntry> rooms = grids
            .SelectMany((grid) => roomLayoutCache.GetLayout(grid).Rooms
                .Where((room) => room != null && !room.IsSelfContained)
                .Select((room) => new RoomUiEntry
                {
                    Grid = grid,
                    Room = room,
                    Environment = roomEnvironmentEvaluator.Evaluate(grid, room)
                }))
            .OrderByDescending((entry) => entry.Room.Doors.Count > 0 && entry.Room.Walls.Count > 0)
            .ThenByDescending((entry) => entry.Room.IsUsable)
            .ThenBy((entry) => entry.Room.Id)
            .ToList();

        AddSection(
            "방 경계/배치 성향",
            $"정식 방 {rooms.Count}개 / 폐쇄 {rooms.Count((entry) => entry.Room.IsClosed)}개 / 사용 가능 {rooms.Count((entry) => entry.Room.IsUsable)}개");
        if (rooms.Count == 0)
        {
            AddLabel("벽과 문으로 구획된 방이 없습니다.", 18f, 44f);
            return;
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            int index = i;
            RoomUiEntry entry = rooms[i];
            RoomInstance room = entry.Room;
            RoomEnvironmentSnapshot environment = entry.Environment;
            BuildableObject representative = room.Furniture.FirstOrDefault((facility) => facility != null && !facility.isDestroy);
            RoomProfile profile = evolution != null && representative != null
                ? evolution.BuildContext(representative).Profile
                : null;
            bool selected = roomInspectionService.CurrentSnapshot != null
                && roomInspectionService.CurrentSnapshot.Grid == entry.Grid
                && ReferenceEquals(roomInspectionService.CurrentSnapshot.Room, room);
            string boundary = room.IsUsable
                ? "폐쇄 + 출입문"
                : room.IsClosed
                    ? "폐쇄 / 출입문 없음"
                    : "열린 경계";
            string identity = FormatRoomRoles(room.Roles);
            string detail = $"{boundary} / 면적 {room.Cells.Count} / 문 {room.Doors.Count} / 벽 {room.Walls.Count}\n"
                + $"내부 시설 {environment.Fixtures.Count} / 성향 {identity}\n"
                + $"넓이 {environment.Spaciousness:0} · 미관 {environment.Beauty:0} · 청결 {environment.Cleanliness:0} · 인상도 {environment.Impressiveness:0}";
            if (selected && profile != null)
            {
                detail += $"\n진화 압력 {FormatRoomIdentity(room, profile)}";
                detail += $"\n상위 점수 {FormatTopRoomScores(profile)} / 밀집도 {profile.GetMetric(FacilityEvolutionTerms.ClutterScore):0.##}";
            }

            CreateDataCard(
                $"P1Action_RoomInspect_{i}",
                $"방 {i + 1} / {FormatRoomRoles(room.Roles)}",
                detail,
                selected ? "선택됨" : "성향 확인",
                () =>
                {
                    bool shown = roomInspectionService.ShowRoom(entry.Grid, room);
                    SetFeedback(shown
                        ? $"방 {index + 1} 성향: {identity} / 인상도 {environment.Impressiveness:0}"
                        : $"방 {index + 1}은 현재 월드에 표시할 수 없습니다.");
                },
                selected ? 154f : 132f);
        }
    }

    private void BuildSynthesisSection()
    {
        FacilitySynthesisRuntime runtime = sceneQuery.First<FacilitySynthesisRuntime>(includeInactive: true);
        if (runtime == null)
        {
            AddSection("시설 합성 (별도 조합)", "합성 런타임이 현재 씬에 없습니다. 방 성향 판정과는 별도 기능입니다.");
            return;
        }

        List<BuildableObject> facilities = FindPlacedFacilities();
        AddSection(
            "시설 합성 (별도 조합)",
            $"방 성향과 별개 / 선택 재료 {runtime.SelectedMaterials.Count}개 / 배치 시설 {facilities.Count}개 / 공개 조합 {runtime.VisibleRecipes.Count}개");

        for (int i = 0; i < facilities.Count; i++)
        {
            BuildableObject facility = facilities[i];
            bool selected = runtime.SelectedMaterials.Contains(facility);
            string actionName = $"P1Action_SynthesisMaterial_{facility.GetInstanceID()}";
            CreateDataCard(
                actionName,
                GetBuildingName(facility),
                $"시설 ID {facility.BuildingData.id} / {(selected ? "합성 재료로 선택됨" : "선택 가능")}",
                selected ? "선택 해제" : "재료 선택",
                () =>
                {
                    runtime.ToggleMaterialSelection(facility);
                    SetFeedback($"합성 재료 변경: {GetBuildingName(facility)} / 선택 {runtime.SelectedMaterials.Count}개");
                },
                CompactCardHeight);
        }

        IReadOnlyList<FacilitySynthesisRecipeSO> recipes = runtime.VisibleRecipes;
        for (int i = 0; i < recipes.Count; i++)
        {
            FacilitySynthesisRecipeSO recipe = recipes[i];
            string materials = recipe.materialBuildings != null
                ? string.Join(" + ", recipe.materialBuildings
                    .Where((building) => building != null)
                    .Select(FacilityShopService.GetBuildingName))
                : "재료 없음";
            CreateDataCard(
                $"P1Action_SynthesisExecute_{i}",
                recipe.DisplayName,
                $"{materials} -> {FacilityShopService.GetBuildingName(recipe.resultBuilding)}\n{recipe.description}",
                "합성",
                () =>
                {
                    bool success = runtime.TrySynthesizeSelected(recipe, out FacilitySynthesisResult result);
                    SetFeedback($"합성 {(success ? "성공" : "실패")}: {result.Message}");
                },
                CardHeight);
        }
    }

    private void BuildEvolutionSection()
    {
        FacilityEvolutionRuntime runtime = sceneQuery.First<FacilityEvolutionRuntime>(includeInactive: true);
        if (runtime == null)
        {
            AddSection("시설 진화", "진화 런타임이 현재 씬에 없습니다.");
            return;
        }

        List<FacilityEvolutionUiCandidate> candidates = new List<FacilityEvolutionUiCandidate>();
        foreach (BuildableObject facility in FindPlacedFacilities())
        {
            foreach (FacilityEvolutionCandidate candidate in runtime.GetCandidates(
                facility,
                includeRejected: true,
                requestLlmProposal: false))
            {
                candidates.Add(new FacilityEvolutionUiCandidate(facility, candidate));
            }
        }

        AddSection(
            "시설 진화",
            $"공개 조합 {runtime.VisibleRecipes.Count}개 / 후보 {candidates.Count}개 / 승인 {candidates.Count((item) => item.Candidate.Approved)}개");
        if (candidates.Count == 0)
        {
            AddLabel("현재 배치 시설에서 확인할 수 있는 진화 후보가 없습니다.", 18f, 40f);
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            FacilityEvolutionUiCandidate item = candidates[i];
            FacilityEvolutionCandidate candidate = item.Candidate;
            string reason = candidate.Approved
                ? "조건 충족"
                : FirstText(candidate.RejectedHintText, candidate.Reason, "조건 미충족");
            CreateDataCard(
                $"P1Action_EvolutionExecute_{i}",
                $"{GetBuildingName(item.Facility)} -> {candidate.Recipe.DisplayName}",
                $"{reason}\n{candidate.Recipe.description}",
                candidate.Approved ? "진화" : "조건 확인",
                () =>
                {
                    bool success = runtime.TryEvolve(item.Facility, candidate.Recipe, out FacilityEvolutionResult result);
                    SetFeedback($"진화 {(success ? "성공" : "실패")}: {result.Message}");
                },
                CardHeight);
        }
    }

    private void BuildRunVariableSection()
    {
        RunVariableRuntime runtime = sceneQuery.First<RunVariableRuntime>(includeInactive: true);
        if (runtime == null)
        {
            AddSection("런/침공 변수", "런 변수 런타임이 현재 씬에 없습니다.");
            return;
        }

        IRunVariableStateView state = runtime.State;
        string startSummary = state.StartVariables != null
            ? state.StartVariables.ToSummaryText().Replace("\n", " / ")
            : "런 시작 변수 없음";
        AddSection(
            "런 시작 변수/침공 변수",
            $"시작 {(state.HasStarted ? "완료" : "대기")} / 운영 변수 {state.ActiveOperationVariables.Count}개 / 침공 {state.CurrentInvasionVariable?.title ?? "없음"}");
        CreateStatusCard(
            "P1State_RunStartVariables",
            state.HasStarted ? "이번 런" : "런 시작 대기",
            startSummary,
            CompactCardHeight);

        if (state.ActiveOperationVariables.Count == 0)
        {
            CreateStatusCard(
                "P1State_RunOperationVariables",
                "운영 변수 없음",
                GetCurrentDay() <= 1 ? "Day 2부터 하루 시작 시 운영 변수가 결정됩니다." : "오늘 적용 중인 운영 변수가 없습니다.",
                CompactCardHeight);
        }
        else
        {
            for (int i = 0; i < Mathf.Min(state.ActiveOperationVariables.Count, MaxVisibleCardsPerSection); i++)
            {
                ActiveRunVariable active = state.ActiveOperationVariables[i];
                CreateStatusCard(
                    $"P1State_RunOperationVariable_{i}",
                    active.Definition.title,
                    $"남은 {active.RemainingDays}일 / {active.Definition.detail}",
                    CompactCardHeight);
            }
        }

        RunVariableDefinition invasion = state.CurrentInvasionVariable;
        CreateStatusCard(
            "P1State_RunInvasionVariable",
            invasion != null ? invasion.title : "침공 변수 대기",
            invasion != null ? invasion.detail : "침공 후보가 발생하면 이번 침공 조건이 결정됩니다.",
            CompactCardHeight);
    }

    internal void BuildDefenseOperations()
    {
        InvasionThreatRuntime threat = sceneQuery.First<InvasionThreatRuntime>(includeInactive: true);
        InvasionDirectorRuntime director = sceneQuery.First<InvasionDirectorRuntime>(includeInactive: true);
        InvasionCombatReportRuntime reports = sceneQuery.First<InvasionCombatReportRuntime>(includeInactive: true);
        IReadOnlyList<InvasionIntruderRuntime> intruders = director != null
            ? director.ActiveIntruders
            : Array.Empty<InvasionIntruderRuntime>();

        if (threat != null)
        {
            InvasionThreatSnapshot snapshot = threat.LatestSnapshot;
            AddSection(
                "침공 위협도",
                $"위협 {threat.CurrentThreat:0.#} / 단계 {threat.CurrentStage} / 안전 {threat.SafetyRemaining:0.#}초 / 후보 {(threat.IsCandidatePending ? "대기" : "없음")}");
            CreateStatusCard(
                "P1State_Threat",
                $"{threat.CurrentStage} / {threat.CurrentThreat:0.#}",
                $"현재 요인: {snapshot.factors}",
                CompactCardHeight);
        }
        else
        {
            AddSection("침공 위협도", "위협 런타임이 현재 씬에 없습니다.");
        }

        AddSection("침입자 추적/상태", $"활성 침입자 {intruders.Count}명");
        if (intruders.Count == 0)
        {
            CreateStatusCard(
                "P1State_Intruders",
                "침입자 없음",
                threat != null && threat.IsCandidatePending ? "침입 후보 접근 중" : "현재 던전 내부는 안전합니다.",
                CompactCardHeight);
        }

        for (int i = 0; i < Mathf.Min(intruders.Count, MaxVisibleCardsPerSection); i++)
        {
            InvasionIntruderRuntime intruder = intruders[i];
            CharacterActor actor = intruder != null ? intruder.IntruderActor : null;
            DefenseStatusRuntime status = actor != null ? actor.GetComponent<DefenseStatusRuntime>() : null;
            string statusText = status != null && status.ActiveStatuses.Count > 0
                ? string.Join(", ", status.ActiveStatuses.Select(FormatDefenseStatus))
                : "방어 상태 효과 없음";
            InvasionIntruderPatternDefinition pattern = intruder.Pattern;
            string targetText = intruder.CurrentPriorityTarget != null
                ? GetBuildingName(intruder.CurrentPriorityTarget)
                : FormatPatternTarget(pattern.targetPreference);
            CreateDataCard(
                $"P1Action_IntruderTrack_{i}",
                $"{pattern.title} · {(actor != null ? actor.name : "침입자")}",
                $"상태 {FormatIntruderState(intruder.State)} / 집중 {intruder.Focus:0.#} / 목표 {targetText}\n{statusText}",
                "추적",
                () => SetFeedback($"{pattern.title}: {pattern.detail}"),
                CardHeight);
        }

        BuildDefenseFacilitySection(intruders);
        BuildInvasionReportSection(reports);
    }

    private void BuildDefenseFacilitySection(IReadOnlyList<InvasionIntruderRuntime> intruders)
    {
        List<DefenseFacility> defenses = sceneQuery.All<DefenseFacility>()
            .Where((facility) => facility != null && !facility.isDestroy && facility.Defense != null)
            .ToList();
        AddSection(
            "방어 시설 효과/쿨다운",
            $"방어 시설 {defenses.Count}개 / 활성 침입자 {intruders.Count}명");

        for (int i = 0; i < Mathf.Min(defenses.Count, MaxVisibleCardsPerSection); i++)
        {
            DefenseFacility facility = defenses[i];
            string effects = FormatDefenseEffects(facility.Defense);
            string condition = CodexTextFormatter.FormatTriggerTimings(facility.Defense.triggerTimings);
            string status = facility.IsDamaged
                ? "파손"
                : facility.CooldownRemaining > 0f
                    ? $"재사용 {facility.CooldownRemaining:0.0}초"
                    : "대기";
            CreateStatusCard(
                $"P1State_DefenseFacility_{i}",
                GetBuildingName(facility),
                $"{facility.Defense.concept} / {condition} / {status} / {effects}",
                CardHeight);
        }
    }

    private void BuildInvasionReportSection(InvasionCombatReportRuntime runtime)
    {
        IReadOnlyList<InvasionCombatReportSnapshot> history = runtime != null
            ? runtime.ReportHistory
            : Array.Empty<InvasionCombatReportSnapshot>();
        AddSection("침공 전투 리포트", $"완료 기록 {history.Count}건 / 현재 보고서 {(runtime?.CurrentReport != null ? "있음" : "없음")}");
        if (history.Count == 0 && runtime?.CurrentReport != null)
        {
            AddLabel(runtime.CurrentReport.ToDetailText(), 16f, 150f);
        }

        for (int i = 0; i < Mathf.Min(history.Count, MaxVisibleCardsPerSection); i++)
        {
            int index = i;
            InvasionCombatReportSnapshot report = history[i];
            string title = report.Defended ? "방어 성공" : "방어 실패";
            CreateDataCard(
                $"P1Action_CombatReport_{i}",
                $"{title} / 위협 {report.ThreatSnapshot.threat:0.#}",
                selectedDefenseReportIndex == i
                    ? report.ToDetailText()
                    : $"잔여 위험 {report.ResidualRisk:0.#} / 방어 기여 {report.DefenseContributions.Count}개 / 파손 {report.DamagedFacilities.Count}개",
                selectedDefenseReportIndex == i ? "선택됨" : "상세",
                () =>
                {
                    selectedDefenseReportIndex = index;
                    SetFeedback($"침공 보고서 선택: {title}");
                },
                selectedDefenseReportIndex == i ? 170f : CardHeight);
        }
    }

    internal void BuildOffenseOperations()
    {
        OffenseWorldMapRuntime worldMap = sceneQuery.First<OffenseWorldMapRuntime>(includeInactive: true);
        OffenseExpeditionRuntime expeditions = sceneQuery.First<OffenseExpeditionRuntime>(includeInactive: true);
        OffenseRewardRuntime rewards = sceneQuery.First<OffenseRewardRuntime>(includeInactive: true);
        if (worldMap == null || expeditions == null)
        {
            AddEmptyState("공격 월드맵 또는 원정 런타임이 현재 씬에 없습니다.");
            return;
        }

        int completedTargets = worldMap.State.CompletedTargetCount;
        int totalTargets = worldMap.CampaignTargetCount;
        string campaignState = worldMap.State.TruthRevealed
            ? "진실 공개 완료"
            : $"진실 추적 {completedTargets}/{totalTargets}";
        AddSection(
            "오펜스 승리 경로",
            $"{campaignState} / 정찰 Lv.{worldMap.State.ReconLevel} / 출정 중 {expeditions.ActiveExpeditions.Count}");
        AddLabel(
            worldMap.State.TruthRevealed
                ? OffenseWorldMapService.TruthRevealText
                : "승리 조건: 오펜스 목표를 순서대로 완료하고 마지막 심장부에서 던전의 진실을 밝히세요.",
            18f,
            worldMap.State.TruthRevealed ? 86f : 52f);
        CreateButtonRow(
            "P1Action_OffenseOpenMap",
            "월드맵 열기",
            $"선택 대상: {worldMap.State.SelectedTargetId}",
            () =>
            {
                OffenseWorldMapPanel panel = worldMap.ShowWorldMap();
                SetFeedback(panel != null ? "공격 월드맵을 열었습니다." : "월드맵을 열지 못했습니다.");
            });
        CreateButtonRow(
            "P1Action_OffenseOpenExpedition",
            "원정 편성 열기",
            $"참가 가능 직원 {expeditions.GetAvailableMemberActors().Count}명",
            () =>
            {
                OffenseExpeditionPanel panel = expeditions.ShowExpeditionPanel();
                SetFeedback(panel != null ? "원정 편성 화면을 열었습니다." : "원정 편성 화면을 열지 못했습니다.");
            });
        CreateButtonRow(
            "P1Action_OffenseRecon",
            "정찰 강화",
            "정찰 범위를 넓혀 새 원정 대상을 발견합니다.",
            () =>
            {
                bool success = worldMap.TryUpgradeRecon(out string message);
                Refresh();
                SetFeedback($"정찰 {(success ? "성공" : "실패")}: {message}");
            });

        IReadOnlyList<OffenseTargetSnapshot> targets = worldMap.VisibleTargets;
        IReadOnlyList<OffenseTargetSnapshot> displayTargets = targets
            .Where(target => !target.isCompleted)
            .OrderBy(target => target.campaignOrder)
            .Take(MaxVisibleCardsPerSection)
            .ToList();
        if (displayTargets.Count == 0)
        {
            displayTargets = targets
                .OrderByDescending(target => target.campaignOrder)
                .Take(MaxVisibleCardsPerSection)
                .ToList();
        }

        for (int i = 0; i < displayTargets.Count; i++)
        {
            OffenseTargetSnapshot target = displayTargets[i];
            bool selected = worldMap.State.SelectedTargetId == target.id;
            CreateDataCard(
                $"P1Action_OffenseTarget_{i}",
                $"{target.campaignOrder}. {target.title}{(target.revealsTruth ? " [최종]" : string.Empty)}",
                $"{target.statusMessage} / 위험 {target.danger:0.#} / 인원 {target.requiredMembers} / 적 {OffenseEncounterCatalog.GetEnemySummary(target.campaignOrder)}",
                target.isCompleted ? "완료" : selected ? "선택됨" : target.isAvailable ? "대상 선택" : "잠김",
                () =>
                {
                    bool success = worldMap.TrySelectTarget(target.id, out _, out string message);
                    Refresh();
                    SetFeedback($"대상 선택 {(success ? "성공" : "실패")}: {message}");
                },
                CompactCardHeight);
        }

        OffenseTargetSnapshot selectedTarget = targets.FirstOrDefault((target) => target.id == worldMap.State.SelectedTargetId);
        if (selectedTarget != null && selectedTarget.isAvailable)
        {
            CreateButtonRow(
                "P1Action_OffenseStart",
                "선택 대상 전투 시작",
                $"{selectedTarget.title} / 필요 {selectedTarget.requiredMembers}명",
                () =>
                {
                    CharacterActor[] party = expeditions.GetAvailableMemberActors()
                        .Take(selectedTarget.requiredMembers)
                        .ToArray();
                    bool success = expeditions.TryStartExpedition(selectedTarget.id, party, out _, out string message);
                    Refresh();
                    SetFeedback($"원정 출발 {(success ? "성공" : "실패")}: {message}");
                });
        }

        BuildOffenseRewardSection(expeditions, rewards);
    }

    private void BuildOffenseRewardSection(OffenseExpeditionRuntime expeditions, OffenseRewardRuntime rewards)
    {
        IOffenseRewardStateView state = rewards != null ? rewards.State : null;
        AddSection(
            "원정 보상",
            state != null
                ? $"누적 자금 {state.MoneyEarned} / 영입 후보 {state.RecruitCandidateCount} / 포로 {state.PrisonerCount} / 특별 몬스터 {state.SpecialMonsterCount}"
                : "보상 런타임 없음");

        IReadOnlyList<OffenseExpeditionResult> history = expeditions.ResultHistory;
        if (history.Count == 0)
        {
            AddLabel("완료된 전투가 없습니다. 승리 보상은 전투 종료 시 지급됩니다.", 18f, 44f);
        }

        for (int i = 0; i < Mathf.Min(history.Count, MaxVisibleCardsPerSection); i++)
        {
            int index = i;
            OffenseExpeditionResult result = history[i];
            bool selected = selectedExpeditionResultIndex == i;
            string rewardText = result.rewardSummaries.Count > 0
                ? string.Join(", ", result.rewardSummaries)
                : "지급 보상 없음";
            CreateDataCard(
                $"P1Action_ExpeditionReward_{i}",
                $"{result.targetTitle} / {(result.success ? "성공" : "실패")}",
                selected ? result.ToDetailText() : rewardText,
                selected ? "선택됨" : "결과 상세",
                () =>
                {
                    selectedExpeditionResultIndex = index;
                    SetFeedback($"원정 결과 선택: {result.targetTitle} / {rewardText}");
                },
                selected ? 190f : CardHeight);
        }
    }

    private void BuildShopOperationsDetail()
    {
        List<BuildableObject> shops = FindRetailFacilities();
        AddSection("상점 상품/가격/계산대", $"운영 상점 {shops.Count}개");
        for (int i = 0; i < Mathf.Min(shops.Count, MaxVisibleCardsPerSection); i++)
        {
            BuildableObject building = shops[i];
            IRetailFacility shop = (IRetailFacility)building;
            bool selected = selectedShopId == building.GetInstanceID();
            string checkout = shop.RequiresStaffedCheckout
                ? shop.HasServingWorker ? "직원 계산대 운영" : "직원 필요"
                : "셀프 계산대";
            CreateDataCard(
                $"P2Action_ShopSelect_{i}",
                GetBuildingName(shop),
                $"재고 {shop.CurrentStock}/{shop.MaxInternalStock} / 대기 {shop.WaitingCheckoutCount}명 / {checkout} / 가격 x{shop.CurrentPriceMultiplier:0.0} / 절도 위험 {shop.GetCheckoutCrimeChance(1) * 100f:0.#}%",
                selected ? "선택됨" : "상품 보기",
                () =>
                {
                    selectedShopId = building.GetInstanceID();
                    SetFeedback($"상점 선택: {GetBuildingName(shop)} / 상품 {shop.ProductSnapshots.Count}개");
                },
                CardHeight);

            if (!selected)
            {
                continue;
            }

            IReadOnlyList<RetailProductSnapshot> products = shop.ProductSnapshots;
            for (int productIndex = 0; productIndex < Mathf.Min(products.Count, MaxVisibleCardsPerSection); productIndex++)
            {
                RetailProductSnapshot product = products[productIndex];
                CreateDataCard(
                    $"P2Action_ShopProduct_{i}_{productIndex}",
                    string.IsNullOrWhiteSpace(product.Name) ? $"상품 {product.Id}" : product.Name,
                    $"판매가 {product.Price} / 수량 {product.Quantity}",
                    "가격 확인",
                    () => SetFeedback($"{product.Name}: 판매가 {product.Price}, 재고 {product.Quantity}"),
                    CompactCardHeight);
            }
        }
    }

    private void BuildEconomyDetailSection()
    {
        OperatingDaySettlementRuntime settlement = sceneQuery.First<OperatingDaySettlementRuntime>(includeInactive: true);
        if (settlement == null)
        {
            AddSection("경제/HUD 세부 효과", "정산 런타임이 현재 씬에 없습니다.");
            return;
        }

        AddSection(
            "경제/HUD 세부 효과",
            $"현재 Day {settlement.CurrentDay} / 최근 정산 {(settlement.LatestReport != null ? $"Day {settlement.LatestReport.day}" : "없음")} / 보관 {settlement.ReportHistory.Count}건");
        AddEconomyModeButton(EconomyViewMode.Current, "현재 장부", "P2Action_EconomyCurrent");
        AddEconomyModeButton(EconomyViewMode.Latest, "최근 정산", "P2Action_EconomyLatest");
        AddEconomyModeButton(EconomyViewMode.History, "정산 이력", "P2Action_EconomyHistory");

        if (economyViewMode == EconomyViewMode.Current)
        {
            OperatingCostForecast forecast = settlement.CurrentOperatingCostForecast;
            CreateDataCard(
                "P2Action_EconomyCurrentDetail",
                $"Day {settlement.CurrentDay} 진행 중",
                $"매출 {settlement.CurrentRevenue} / 방문 {settlement.CurrentVisits} / 만족 {settlement.CurrentAverageSatisfaction:0.#} / 유지비 {forecast.MaintenanceCost} / 급여 {forecast.PayrollCost} / 미납 {forecast.OutstandingDebt} / 총 예정 {forecast.TotalDue} / 부족 {forecast.ExpectedShortfall} / 재고 소비 {settlement.CurrentConsumedStock} / 사건 {settlement.CurrentIncidentCount}",
                "새로고침",
                () => SetFeedback("현재 운영 장부를 갱신했습니다."),
                CardHeight);
            return;
        }

        if (economyViewMode == EconomyViewMode.Latest)
        {
            OperatingDayReport latest = settlement.LatestReport;
            if (latest == null)
            {
                AddLabel("아직 완료된 정산이 없습니다.", 18f, 40f);
                return;
            }

            AddLabel(latest.ToDetailText(), 15f, 260f);
            return;
        }

        foreach (OperatingDayReport report in settlement.ReportHistory.Take(MaxVisibleCardsPerSection))
        {
            CreateDataCard(
                $"P2Action_EconomyReport_{report.day}",
                $"Day {report.day}",
                $"매출 {report.totalRevenue} / 운영비 {report.paidOperatingCost}/{report.totalOperatingCost} / 미납 {report.unpaidOperatingCost} / 마감 {report.closingBalance} / 방문 {report.totalVisits} / 만족 {report.averageSatisfaction:0.#} / 사건 {report.incidents.Count}",
                "상세",
                () => SetFeedback(report.ToDetailText().Replace("\n", " / ")),
                CardHeight);
        }
    }

    internal void BuildCodexAndHistory()
    {
        AddSection("도감/기록", "도감, 전투·운영 보고서, 이벤트 히스토리를 조회합니다.");
        AddArchiveModeButton(ArchiveViewMode.Codex, "도감", "P2Action_ArchiveCodex");
        AddArchiveModeButton(ArchiveViewMode.Reports, "보고서", "P2Action_ArchiveReports");
        AddArchiveModeButton(ArchiveViewMode.Events, "이벤트", "P2Action_ArchiveEvents");

        switch (archiveViewMode)
        {
            case ArchiveViewMode.Codex:
                BuildCodexEntries();
                break;
            case ArchiveViewMode.Reports:
                BuildArchiveReports();
                break;
            case ArchiveViewMode.Events:
                BuildEventHistory();
                break;
        }
    }

    private void BuildCodexEntries()
    {
        CodexRuntime runtime = sceneQuery.First<CodexRuntime>(includeInactive: true);
        if (runtime == null)
        {
            AddEmptyState("도감 런타임이 현재 씬에 없습니다.");
            return;
        }

        AddSection("도감 분류", $"전체 기록 {runtime.State.Entries.Count}개 / 현재 {codexCategory}");
        AddCodexCategoryButton(CodexEntryCategory.Monster, "몬스터", 0);
        AddCodexCategoryButton(CodexEntryCategory.Invasion, "침공", 1);
        AddCodexCategoryButton(CodexEntryCategory.Facility, "시설", 2);

        IReadOnlyList<CodexEntrySnapshot> entries = runtime.GetEntries(codexCategory);
        if (entries.Count == 0)
        {
            AddLabel("이 분류에서 발견한 기록이 없습니다.", 18f, 40f);
        }

        for (int i = 0; i < Mathf.Min(entries.Count, MaxVisibleCardsPerSection); i++)
        {
            CodexEntrySnapshot entry = entries[i];
            bool selected = selectedCodexEntryId == entry.entryId;
            string detail = selected
                ? entry.ToDisplayText()
                : $"정보 {entry.lines?.Length ?? 0}개 / {(entry.discovered ? "발견" : "미발견")}";
            CreateDataCard(
                $"P2Action_CodexEntry_{i}",
                entry.title,
                detail,
                selected ? "선택됨" : "상세",
                () =>
                {
                    selectedCodexEntryId = entry.entryId;
                    SetFeedback($"도감 선택: {entry.title}");
                },
                selected ? 150f : CompactCardHeight);
        }
    }

    private void BuildArchiveReports()
    {
        InvasionCombatReportRuntime invasion = sceneQuery.First<InvasionCombatReportRuntime>(includeInactive: true);
        OffenseExpeditionRuntime offense = sceneQuery.First<OffenseExpeditionRuntime>(includeInactive: true);
        OperatingDaySettlementRuntime operation = sceneQuery.First<OperatingDaySettlementRuntime>(includeInactive: true);
        int invasionCount = invasion?.ReportHistory.Count ?? 0;
        int offenseCount = offense?.ResultHistory.Count ?? 0;
        int operationCount = operation?.ReportHistory.Count ?? 0;
        AddSection("보고서 아카이브", $"침공 {invasionCount} / 원정 {offenseCount} / 운영 {operationCount}");

        if (invasion?.ReportHistory.FirstOrDefault() is InvasionCombatReportSnapshot invasionReport)
        {
            CreateDataCard(
                "P2Action_ArchiveInvasionReport",
                "최근 침공 보고서",
                invasionReport.ToDetailText(),
                "확인",
                () => SetFeedback("최근 침공 보고서를 확인했습니다."),
                180f);
        }

        if (offense?.ResultHistory.FirstOrDefault() is OffenseExpeditionResult offenseResult)
        {
            CreateDataCard(
                "P2Action_ArchiveExpeditionReport",
                "최근 원정 보고서",
                offenseResult.ToDetailText(),
                "확인",
                () => SetFeedback("최근 원정 보고서를 확인했습니다."),
                180f);
        }

        if (operation?.ReportHistory.FirstOrDefault() is OperatingDayReport operationReport)
        {
            CreateDataCard(
                "P2Action_ArchiveOperationReport",
                "최근 운영 보고서",
                operationReport.ToDetailText(),
                "확인",
                () => SetFeedback("최근 운영 보고서를 확인했습니다."),
                180f);
        }
    }

    private void BuildEventHistory()
    {
        EventAlertRuntime runtime = sceneQuery.First<EventAlertRuntime>(includeInactive: true);
        if (runtime == null)
        {
            AddEmptyState("이벤트 알림 런타임이 현재 씬에 없습니다.");
            return;
        }

        AddSection(
            "알림/이벤트 히스토리",
            $"전체 {runtime.EventLog.Count}건 / 필터 {(eventImportanceFilter?.ToString() ?? "전체")}");
        AddEventFilterButton(null, "전체", 0);
        AddEventFilterButton(EventAlertImportance.High, "높음", 1);
        AddEventFilterButton(EventAlertImportance.Medium, "중간", 2);
        AddEventFilterButton(EventAlertImportance.Low, "낮음", 3);

        IReadOnlyList<EventAlertRecord> records = runtime.EventLog
            .Where((record) => record != null
                && (!eventImportanceFilter.HasValue || record.Importance == eventImportanceFilter.Value))
            .Reverse()
            .Take(MaxVisibleCardsPerSection)
            .ToArray();
        if (records.Count == 0)
        {
            AddLabel("현재 필터에 해당하는 이벤트 기록이 없습니다.", 18f, 40f);
        }

        for (int i = 0; i < records.Count; i++)
        {
            EventAlertRecord record = records[i];
            bool selected = selectedEventRecordId == record.Id;
            CreateDataCard(
                $"P2Action_EventRecord_{i}",
                record.ButtonText,
                selected ? record.ToDetailText() : $"{record.Importance} / {record.Category} / 선택지 {record.Choices.Count}개",
                selected ? "선택됨" : "상세",
                () =>
                {
                    selectedEventRecordId = record.Id;
                    runtime.Open(record);
                    SetFeedback($"이벤트 선택: {record.Title}");
                },
                selected ? 170f : CompactCardHeight);
        }
    }

    private void AddEconomyModeButton(EconomyViewMode mode, string label, string actionName)
    {
        CreateButtonRow(
            actionName,
            economyViewMode == mode ? $"{label} 선택됨" : label,
            "표시할 경제 상세 범위를 전환합니다.",
            () =>
            {
                economyViewMode = mode;
                SetFeedback($"경제 상세 전환: {label}");
            });
    }

    private void AddArchiveModeButton(ArchiveViewMode mode, string label, string actionName)
    {
        CreateButtonRow(
            actionName,
            archiveViewMode == mode ? $"{label} 선택됨" : label,
            "기록 화면을 전환합니다.",
            () =>
            {
                archiveViewMode = mode;
                SetFeedback($"기록 화면 전환: {label}");
            });
    }

    private void AddCodexCategoryButton(CodexEntryCategory category, string label, int index)
    {
        CreateButtonRow(
            $"P2Action_CodexCategory_{index}",
            codexCategory == category ? $"{label} 선택됨" : label,
            "도감 분류를 전환합니다.",
            () =>
            {
                codexCategory = category;
                selectedCodexEntryId = string.Empty;
                SetFeedback($"도감 분류: {label}");
            });
    }

    private void AddEventFilterButton(EventAlertImportance? importance, string label, int index)
    {
        CreateButtonRow(
            $"P2Action_EventFilter_{index}",
            eventImportanceFilter == importance ? $"{label} 선택됨" : label,
            "이벤트 중요도 필터를 전환합니다.",
            () =>
            {
                eventImportanceFilter = importance;
                selectedEventRecordId = -1;
                SetFeedback($"이벤트 필터: {label}");
            });
    }

    private List<BuildableObject> FindPlacedFacilities()
    {
        return sceneQuery.All<BuildableObject>()
            .Where((building) => building != null && !building.isDestroy && building.BuildingData != null)
            .OrderBy((building) => GetBuildingName(building))
            .ToList();
    }

    private static string FormatDefenseEffects(DefenseFacilityData defense)
    {
        if (defense == null)
        {
            return "효과 없음";
        }

        int assetCount = defense.effectAssets?.Count((effect) => effect != null) ?? 0;
        return $"효과 {assetCount}개";
    }

    private static string FormatDefenseStatus(DefenseStatusSnapshot status)
    {
        return $"{status.Kind} x{status.Stacks} ({status.RemainingSeconds:0.0}s)";
    }

    private static string FormatPatternTarget(InvasionIntruderTargetPreference preference)
    {
        return preference switch
        {
            InvasionIntruderTargetPreference.DefenseFacility => "방어 시설 탐색",
            InvasionIntruderTargetPreference.ValuableFacility => "고가 운영 시설 탐색",
            _ => "사장"
        };
    }

    private static string FormatIntruderState(InvasionIntruderState state)
    {
        return state switch
        {
            InvasionIntruderState.Entering => "진입 중",
            InvasionIntruderState.Searching => "탐색 중",
            InvasionIntruderState.MovingToOwner => "사장 추적",
            InvasionIntruderState.MovingToFacility => "시설 추적",
            InvasionIntruderState.DamagingFacility => "시설 파괴",
            InvasionIntruderState.FinalCombat => "최종 교전",
            InvasionIntruderState.Finished => "종료",
            _ => "대기"
        };
    }

    private static string FormatRoomIdentity(RoomInstance room, RoomProfile profile)
    {
        if (profile != null && profile.IdentityPressures.Count > 0)
        {
            KeyValuePair<string, float> dominant = profile.IdentityPressures
                .OrderByDescending((pair) => pair.Value)
                .First();
            if (!string.IsNullOrWhiteSpace(dominant.Key) && dominant.Value > 0f)
            {
                return $"{dominant.Key} {dominant.Value:P0}";
            }
        }

        return FormatRoomRoles(room != null ? room.Roles : FacilityRole.None);
    }

    private static string FormatTopRoomScores(RoomProfile profile)
    {
        if (profile == null || profile.Scores.Count == 0)
        {
            return "없음";
        }

        return string.Join(", ", profile.Scores
            .OrderByDescending((pair) => pair.Value)
            .Take(3)
            .Select((pair) => $"{pair.Key} {pair.Value:0.#}"));
    }

    private static string FormatRoomRoles(FacilityRole roles)
    {
        if (roles == FacilityRole.None)
        {
            return "미정";
        }

        List<string> labels = FacilityRoleCatalog
            .Enumerate(roles)
            .Select((definition) => definition.RoomLabel)
            .ToList();
        return labels.Count > 0 ? string.Join(" + ", labels) : "미정";
    }

    private static string FirstText(params string[] values)
    {
        return values?.FirstOrDefault((value) => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }

    private readonly struct FacilityEvolutionUiCandidate
    {
        public FacilityEvolutionUiCandidate(BuildableObject facility, FacilityEvolutionCandidate candidate)
        {
            Facility = facility;
            Candidate = candidate;
        }

        public BuildableObject Facility { get; }
        public FacilityEvolutionCandidate Candidate { get; }
    }
}
