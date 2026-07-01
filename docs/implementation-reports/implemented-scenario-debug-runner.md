# 통합 디버그 시나리오 러너 구현 보고

## 구현 파일

- `Assets/Scripts/Editor/ImplementedScenarioDebugRunner.cs`

## 목표

P0부터 P3 11.3까지 흩어져 있는 디버그 시나리오를 한 번에 실행할 수 있게 했다.

Unity MCP나 수동 에디터 검증을 할 때 개별 메뉴를 하나씩 누르지 않아도 전체 구현 범위를 빠르게 확인하기 위한 장치다.

## 실행 메뉴

```text
DungeonStory/Debug/Run All Implemented Scenarios
DungeonStory/Debug/Open Last Implemented Scenario Report
```

batchmode 자동 검증 진입점:

```text
ImplementedScenarioDebugRunner.RunForBatchMode
```

이 진입점은 모든 시나리오가 통과하면 exit code `0`, 하나라도 실패하거나 예외가 나면 exit code `1`로 Unity Editor를 종료한다.

## 실행 흐름

```text
P1 방어/상점/합성 에셋 보정
-> P0 Grid foundation 시나리오
-> P1 경영/AI/침입/연구/합성/도감 시나리오
-> P2 영입/내부 위협/런 변수/메타 진행 시나리오
-> P3 오펜스 월드맵/원정/보상 시나리오
-> 전체 성공/실패 요약 출력
```

## 결과 파일

메뉴 실행 시 콘솔 로그와 함께 다음 파일에 같은 요약을 저장한다.

```text
Temp/DungeonStoryImplementedScenarioReport.txt
Temp/DungeonStoryImplementedScenarioReport.json
```

실패가 섞여 있어도 전체 묶음을 끝까지 실행한 뒤 `[PASS]`, `[FAIL]` 목록을 남긴다.

저장된 결과는 `DungeonStory/Debug/Open Last Implemented Scenario Report` 메뉴로 바로 열 수 있다.

각 묶음에는 실행 시간과 실패 상세도 함께 남긴다.

```text
[PASS] P1 Customer AI (12ms)
[FAIL] P1 Facility synthesis (4ms) / InvalidOperationException: ...
```

JSON 리포트는 자동 검증에서 `success`, `passed`, `failed`, `results[]`를 기계적으로 확인하기 위한 파일이다.

## 설계 의도

실패가 나도 즉시 중단하지 않고 다음 시나리오를 계속 실행한다.

이렇게 하면 한 시스템이 깨져도 나머지 시스템 상태를 같이 볼 수 있고, 마지막 요약에서 어떤 묶음이 실패했는지 확인할 수 있다.

## 현재 한계

이 러너는 Unity Editor 안에서 실행해야 한다.

현재 Codex 쪽 Unity MCP 연결은 Editor 승인 문제로 막혀 있으므로, 자동 실행 검증은 승인 복구 후 다시 수행해야 한다.

batchmode 실행도 시도했지만, 현재 같은 프로젝트가 Unity Editor에 열려 있어 동시 실행 제한에 걸렸다.

복사본 프로젝트와 `-noUpm` 옵션으로도 시도했지만, 현재 Codex 실행 환경에서는 Unity Package Manager 또는 Bee/IL Post Processor의 IPC 생성이 막혀 시나리오 실행까지 도달하지 못했다.

따라서 통합 시나리오의 실제 통과 여부는 현재 열려 있는 Editor에서 직접 메뉴를 실행하거나, Editor/MCP 승인 상태가 정상인 환경에서 batchmode로 다시 확인해야 한다.
