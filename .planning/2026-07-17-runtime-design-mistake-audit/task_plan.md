# Task Plan: Remaining Runtime Design-Mistake Audit

## Goal
Audit `Assets/Scripts` runtime code for remaining extensibility and ownership defects after the completed P0/P1/P2 remediation, trace each candidate through real consumers, and report evidence without changing runtime code.

## Scope Exclusions
- Static event payloads
- Grid removal identity
- BehaviorSource owner handling
- Camera fallback
- Backdrop scene-name protocol
- CharacterSO basicBehaviorPatterns
- Every finding already listed in `docs/qa/extensibility-architecture-gap-audit.md`
- Test-only and Editor-only code, except as corroborating evidence

## Phases

### Phase 1: Baseline And Inventory
- [x] Restore existing planning context and identify the active remediation state.
- [x] Read the prior audit and map each resolved/excluded finding to current symbols.
- [x] Inventory current runtime declarations and consumers relevant to the seven requested smell classes.
- **Status:** completed

### Phase 2: Candidate Discovery
- [x] Search fixed optional bags, enum/concrete switches, duplicated authority, caller cleanup, hidden fallback/service lookup, magic routing, and broad aggregates.
- [x] Record candidates with declaration and consumer evidence.
- **Status:** completed

### Phase 3: Consumer Tracing And Triage
- [x] Trace each candidate end to end through mutation, dispatch, persistence, UI, or lifecycle behavior.
- [x] Reject overlap, compatibility-only code, benign closed-world switches, and unused declarations.
- [x] Assign severity based on observable extension cost or runtime failure mode.
- **Status:** completed

### Phase 4: Report
- [x] Verify exact current line numbers and paths.
- [x] Report severity, files/lines, evidence, and minimum improvement direction.
- [x] Confirm no runtime code was changed.
- **Status:** completed

## Errors Encountered
| Error | Attempt | Resolution |
|---|---:|---|
| Initial parallel discovery batch stopped when one child search returned exit code 1. | 1 | Read the required existing planning files first, then use bounded searches whose no-match results do not suppress unrelated evidence. |
| The first seven-pattern scan used double-quoted regex text containing escaped quotes; PowerShell parsed part of it as a pipeline and aborted the batch. | 1 | Use single-quoted regex arguments and collect each parallel result independently with `Promise.allSettled`. |
| A defense-source search passed a Windows wildcard path directly to `rg`, which rejected the path syntax. | 1 | Search the defense directory with `-g '*.cs'` or read exact files; the other independent evidence reads still completed. |
| A planning patch mixed a `progress.md` line into the `findings.md` update block, so context verification rejected the whole patch. | 1 | Read all three isolated planning-file tails and reapply each update under the correct file marker. |
| A broad magic-text scan again hit PowerShell quote parsing, then a too-specific regex returned no matches. | 1 | Use a single-quoted alternation over literal method prefixes and keep no-match searches separate from evidence reads. |
