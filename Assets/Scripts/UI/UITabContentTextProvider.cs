using System;
using System.Text;
using UnityEngine;

public interface IUITabContentTextProvider
{
    string Build(int id);
}

public sealed class UITabContentTextProvider : IUITabContentTextProvider
{
    private readonly IBuildingManagementSummaryService buildingManagementSummaryService;
    private readonly IStaffWorkforceQueryService staffWorkforceQueryService;
    private readonly IInvasionDefenseSummaryService invasionDefenseSummaryService;
    private readonly IOffenseTabSummaryService offenseTabSummaryService;
    private readonly IOperationTabSummaryService operationTabSummaryService;
    private readonly IResearchCraftingSummaryService researchCraftingSummaryService;
    private readonly ICodexRecordSummaryService codexRecordSummaryService;

    public UITabContentTextProvider(
        IBuildingManagementSummaryService buildingManagementSummaryService,
        IStaffWorkforceQueryService staffWorkforceQueryService,
        IInvasionDefenseSummaryService invasionDefenseSummaryService,
        IOffenseTabSummaryService offenseTabSummaryService,
        IOperationTabSummaryService operationTabSummaryService,
        IResearchCraftingSummaryService researchCraftingSummaryService,
        ICodexRecordSummaryService codexRecordSummaryService)
    {
        this.buildingManagementSummaryService = buildingManagementSummaryService
            ?? throw new ArgumentNullException(nameof(buildingManagementSummaryService));
        this.staffWorkforceQueryService = staffWorkforceQueryService
            ?? throw new ArgumentNullException(nameof(staffWorkforceQueryService));
        this.invasionDefenseSummaryService = invasionDefenseSummaryService
            ?? throw new ArgumentNullException(nameof(invasionDefenseSummaryService));
        this.offenseTabSummaryService = offenseTabSummaryService
            ?? throw new ArgumentNullException(nameof(offenseTabSummaryService));
        this.operationTabSummaryService = operationTabSummaryService
            ?? throw new ArgumentNullException(nameof(operationTabSummaryService));
        this.researchCraftingSummaryService = researchCraftingSummaryService
            ?? throw new ArgumentNullException(nameof(researchCraftingSummaryService));
        this.codexRecordSummaryService = codexRecordSummaryService
            ?? throw new ArgumentNullException(nameof(codexRecordSummaryService));
    }

    public string Build(int id)
    {
        return id switch
        {
            1 => BuildBuildingManagementText(),
            2 => BuildStaffManagementText(),
            3 => BuildShopText(),
            4 => BuildWarehouseText(),
            5 => BuildOperationText(),
            6 => BuildInvasionDefenseText(),
            7 => BuildOffenseText(),
            8 => BuildResearchCraftingText(),
            9 => BuildCodexRecordText(),
            _ => string.Empty
        };
    }

    private string BuildBuildingManagementText()
    {
        BuildingManagementSummary summary = buildingManagementSummaryService.CaptureBuildings();

        return string.Join("\n", new[]
        {
            $"총 건물: {summary.TotalBuildings}",
            $"방문 가능 시설: {summary.VisitorFacilities}",
            $"직원 작업 가능 시설: {summary.WorkableFacilities}",
            $"수리 필요: {summary.DamagedBuildings}",
            string.Empty,
            "다음 UI 연결 후보",
            "- 선택 건물 상세: 레벨, 내구/손상, 수용 인원, 담당 직원",
            "- 일괄 수리/업그레이드/철거",
            "- 보충 필요 시설과 연구 가능 시설 필터"
        });
    }

    private string BuildStaffManagementText()
    {
        int staffCount = 0;
        int offDutyCount = 0;
        int workingCount = 0;
        int expeditionCount = 0;

        foreach (CharacterActor character in staffWorkforceQueryService.FindActiveWorkers())
        {
            if (!staffWorkforceQueryService.IsActiveWorker(character)
                || !CharacterWorkRoleUtility.TryGetWork(character, out AbilityWork work))
            {
                continue;
            }

            staffCount++;
            if (work.IsOffDuty) offDutyCount++;
            if (work.isWorking) workingCount++;
            if (character.IsOnExpedition) expeditionCount++;
        }

        return string.Join("\n", new[]
        {
            $"직원 수: {staffCount}",
            $"근무 중: {workingCount}",
            $"비번/휴식 보호: {offDutyCount}",
            $"원정 중: {expeditionCount}",
            string.Empty,
            "다음 UI 연결 후보",
            "- 직원별 업무 우선순위",
            "- 불만도/피로도/기분 위험 목록",
            "- 특정 시설 우선 배치와 침입자 제압 명령"
        });
    }

    private string BuildShopText()
    {
        ShopManagementSummary summary = buildingManagementSummaryService.CaptureShops();

        return string.Join("\n", new[]
        {
            $"상점 수: {summary.TotalShops}",
            $"판매 가능: {summary.StockedShops}",
            $"품절/보충 필요: {summary.EmptyShops}",
            string.Empty,
            "다음 UI 연결 후보",
            "- 오늘의 시설 상점 상품",
            "- 판매 재고/가격/품절 위험",
            "- 창고에서 수동 보충 요청",
            "- 구매한 설계도와 연구 잠금 상태"
        });
    }

    private string BuildWarehouseText()
    {
        WarehouseManagementSummary summary = buildingManagementSummaryService.CaptureWarehouses();

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"창고 시설: {summary.WarehouseCount}");
        builder.AppendLine(summary.HasCapacityLimit
            ? $"총 재고: {summary.TotalStock} / {summary.TotalCapacity}"
            : $"총 재고: {summary.TotalStock}");
        builder.AppendLine($"식재료: {summary.FoodStock}");
        builder.AppendLine($"잡화: {summary.GeneralStock}");
        builder.AppendLine($"무기: {summary.WeaponStock}");
        builder.AppendLine($"마력: {summary.ManaStock}");
        builder.AppendLine();
        builder.AppendLine("창고 탭에 들어가야 할 핵심");
        builder.AppendLine("- 카테고리별 보유량과 남은 용량");
        builder.AppendLine("- 상점 보충 예약/수동 보충");
        builder.AppendLine("- 원정 보상 입고와 시설 생산 입고 로그");
        builder.AppendLine("- 품절 위험 품목과 하루 예상 소모량");
        builder.AppendLine("- 창고별 우선순위: 식재료/무기/마력 전용화");
        return builder.ToString();
    }

    private string BuildOperationText()
    {
        OperationTabSummary summary = operationTabSummaryService.Capture();
        StringBuilder builder = new StringBuilder();
        if (summary.HasGameData)
        {
            builder.AppendLine($"날짜: Day {summary.Day} / {summary.Hour}:00");
            builder.AppendLine($"보유 자금: {summary.HoldingMoney}");
            builder.AppendLine($"게임 속도: x{summary.GameSpeed}");
        }
        else
        {
            builder.AppendLine("날짜/자금 데이터: 연결 필요");
        }

        builder.AppendLine($"이벤트 알림: {summary.EventAlertCount}");
        builder.AppendLine($"최근 정산 보고서: {(summary.HasLatestReport ? $"Day {summary.LatestReportDay}" : "없음")}");
        builder.AppendLine($"런 변수: {(summary.HasRunVariables ? "활성" : "없음")}");
        builder.AppendLine();
        builder.AppendLine("운영 탭에 들어갈 핵심");
        builder.AppendLine("- 일일 정산, 매출, 유지비, 순이익");
        builder.AppendLine("- 방문자 수, 평균 만족도, 단골 변화");
        builder.AppendLine("- 당일 사건/알림 로그와 선택지");
        builder.AppendLine("- 다음 운영일 진행 버튼");
        return builder.ToString();
    }

    private string BuildInvasionDefenseText()
    {
        InvasionDefenseSummary summary = invasionDefenseSummaryService.Capture();
        StringBuilder builder = new StringBuilder();
        if (summary.HasThreatRuntime)
        {
            builder.AppendLine($"위협도: {summary.CurrentThreat:0.#}");
            builder.AppendLine($"단계: {summary.CurrentStage}");
            builder.AppendLine($"안전 시간: {summary.SafetyRemaining:0.#}초");
            builder.AppendLine($"침공 후보 대기: {(summary.IsCandidatePending ? "예" : "아니오")}");
        }
        else
        {
            builder.AppendLine("위협도 런타임: 없음");
        }

        builder.AppendLine($"활성 침입자: {summary.ActiveIntruders}");
        builder.AppendLine($"방어 시설: {summary.DefenseFacilities}");
        builder.AppendLine($"손상 시설: {summary.DamagedFacilities}");
        builder.AppendLine($"최근 전투 보고서: {(summary.HasCurrentCombatReport ? "있음" : "없음")}");
        builder.AppendLine();
        builder.AppendLine("침공/방어 탭에 들어갈 핵심");
        builder.AppendLine("- 위협도 게이지와 침공 예고");
        builder.AppendLine("- 활성 침입자 위치/상태");
        builder.AppendLine("- 방어 시설 발동 로그와 파손 목록");
        builder.AppendLine("- 전투 결과 리포트");
        return builder.ToString();
    }

    private string BuildOffenseText()
    {
        OffenseTabSummary summary = offenseTabSummaryService.Capture();
        StringBuilder builder = new StringBuilder();
        if (summary.HasWorldMap)
        {
            builder.AppendLine($"정찰 레벨: {summary.ReconLevel}");
            builder.AppendLine($"정찰 범위: {summary.ScanRange:0.#}");
            builder.AppendLine($"발견 목표: {summary.VisibleTargets}");
            builder.AppendLine($"선택 목표: {(summary.HasSelectedTarget ? summary.SelectedTargetId : "없음")}");
        }
        else
        {
            builder.AppendLine("월드맵 런타임: 없음");
        }

        builder.AppendLine($"진행 중 원정: {summary.ActiveExpeditions}");
        builder.AppendLine($"누적 원정 자금: {summary.MoneyEarned}");
        builder.AppendLine($"포로/후보: {summary.PrisonerCount}/{summary.RecruitCandidateCount}");
        builder.AppendLine();
        builder.AppendLine("원정 탭에 들어갈 핵심");
        builder.AppendLine("- 월드맵 목표 선택");
        builder.AppendLine("- 파티 편성, 요구 전투력, 성공 확률");
        builder.AppendLine("- 진행 중 원정 타이머");
        builder.AppendLine("- 보상 수령과 창고 입고 내역");
        return builder.ToString();
    }

    private string BuildResearchCraftingText()
    {
        ResearchCraftingSummary summary = researchCraftingSummaryService.Capture();
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"연구 작업 수: {summary.ResearchTaskCount}");
        builder.AppendLine($"완료 설계도: {summary.CompletedBlueprintCount}");
        builder.AppendLine(summary.HasActiveTask
            ? $"진행 중 연구: {summary.ActiveBlueprintName} {summary.ActiveProgressRatio:P0}"
            : "진행 중 연구: 없음");
        builder.AppendLine($"선택 합성 재료: {summary.SelectedSynthesisMaterials}");
        builder.AppendLine($"보이는 합성식: {summary.VisibleSynthesisRecipes}");
        builder.AppendLine();
        builder.AppendLine("연구/제작 탭에 들어갈 핵심");
        builder.AppendLine("- 설계도 연구 큐와 진행률");
        builder.AppendLine("- 연구 완료 보상/해금 시설");
        builder.AppendLine("- 시설 합성 재료 선택");
        builder.AppendLine("- 결과 시설 미리보기와 합성 확정");
        return builder.ToString();
    }

    private string BuildCodexRecordText()
    {
        CodexRecordSummary summary = codexRecordSummaryService.Capture();
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"몬스터 도감: {summary.MonsterEntries}");
        builder.AppendLine($"침공 도감: {summary.InvasionEntries}");
        builder.AppendLine($"시설 도감: {summary.FacilityEntries}");
        builder.AppendLine($"이벤트 기록: {summary.EventLogCount}");
        builder.AppendLine($"최근 운영 보고서: {(summary.HasLatestReport ? $"Day {summary.LatestReportDay}" : "없음")}");
        builder.AppendLine();
        builder.AppendLine("도감/기록 탭에 들어갈 핵심");
        builder.AppendLine("- 몬스터/침입자/시설 도감");
        builder.AppendLine("- 발견 조건과 새 정보 표시");
        builder.AppendLine("- 운영일 보고서 히스토리");
        builder.AppendLine("- 침공 전투 리포트 아카이브");
        return builder.ToString();
    }
}
