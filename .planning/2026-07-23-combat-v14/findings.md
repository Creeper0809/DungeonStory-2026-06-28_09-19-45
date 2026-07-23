# Findings

## Starting State

- The worktree contains the accumulated V13 gameplay implementation and many existing user changes. Do not revert or rewrite unrelated files.
- V13 already provides shared combat resolution, equipment instances, body health, defense engagements, ranged hunting, offense persistence, and command movement.
- The approved V14 scope is primarily missing runtime and UI integration around those existing foundations.
- `FacilityWorkType.Rescue` and work-priority UI support already existed, so rescue can be introduced as another continuous AI action without adding a duplicate work category.
- Existing body-health downing used one threshold in both directions; V14 needs hysteresis. Downing remains at consciousness 25% / mobility 20%, while recovery now requires consciousness 35%, mobility 30%, and blood loss below 70.
- Existing move/work penalties multiplied global injury by body-part capacity. V14 now takes the lower multiplier once, preventing accidental double penalties.
- Existing medical building abilities already expose `severityReduction` and `requiresMedicine`; the new medical runtime consumes those instead of introducing another medical settings object.
- Existing defense engagements treated only death as assignment loss. Downed actors now leave the line through the same release/switch flow.
- Combat affiliation needed to distinguish protected neutral visitors from allies and hostiles. Automatic fire now stops for allies/neutrals, while forced fire resolves real ordered interceptions.
- Existing combat stance and movement commands were reusable, but command ownership had to pause lifestyle AI and reserve firing/melee cells to prevent group overlap.
- The item/equipment runtime already had physical stacks and per-instance durability, so ammunition resupply and repair were implemented through real reservations, hauling, work amount, and output stacks.
- Runtime-only `ExteriorZoneMarker` and `WorldFilthWorkTarget` objects intentionally use negative IDs and have dedicated save data. They must never enter modular building persistence.
- Unity `Destroy` is deferred. Save restore paths that replace occupants in the same frame must synchronously detach the old occupant before creating the replacement.
- The gameplay QA start-party fallback and the product preparation scene intentionally use different confirm buttons; shared regression drivers must support both flows.
