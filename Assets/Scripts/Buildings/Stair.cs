using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stair : BuildableObject, IInteractable, IGridMovementHandler
{
    private const float EnterSpeedMultiplier = 0.9f;
    private const float DefaultHiddenTravelDelay = 2f;
    private const float ReappearDelay = 0.06f;

    public override GridMoveType GridMoveType => GridMoveType.Stair;

    public IEnumerator Traverse(Character character, GridMoveStep step)
    {
        if (character == null || step == null) yield break;

        AbilityMove moveable = character.GetAbility<AbilityMove>();
        if (moveable == null || grid == null) yield break;

        Vector3 fromAnchor = GetFloorCenterAnchor(step.From);
        Vector3 toAnchor = GetFloorCenterAnchor(step.To);
        fromAnchor.z = character.transform.position.z;
        toAnchor.z = character.transform.position.z;

        if ((character.transform.position - fromAnchor).sqrMagnitude > 0.01f)
        {
            yield return moveable.Move2PosBySpeed(fromAnchor, EnterSpeedMultiplier);
        }

        List<RendererVisibilityState> rendererStates = CaptureRendererVisibility(character);
        List<CanvasVisibilityState> canvasStates = CaptureCanvasVisibility(character);
        SetCharacterTraversalVisible(rendererStates, canvasStates, false);

        yield return new WaitForSeconds(GetHiddenTravelDelay());
        character.transform.position = toAnchor;
        yield return new WaitForSeconds(ReappearDelay);

        RestoreCharacterTraversalVisible(rendererStates, canvasStates);
    }

    public IEnumerator Interact(Character character)
    {
        if (character == null || grid == null) yield break;

        Vector2Int from = grid.GetXY(character.transform.position);
        Vector2Int to = from;
        GridCell cell = grid.GetGridCell(from);
        if (cell != null)
        {
            foreach (GridTraversalLink link in cell.TraversalLinks)
            {
                if (ReferenceEquals(link.Through, this))
                {
                    to = link.To;
                    break;
                }
            }
        }

        if (to == from)
        {
            to = new Vector2Int(from.x, Mathf.Clamp(from.y + 1, 0, grid.height - 1));
        }

        yield return Traverse(character, new GridMoveStep(from, to, this, this, GridMoveType.Stair));
    }

    private float GetHiddenTravelDelay()
    {
        return BuildingData != null
            ? Mathf.Max(0f, BuildingData.movementTravelTime)
            : DefaultHiddenTravelDelay;
    }

    private Vector3 GetFloorCenterAnchor(Vector2Int fallbackPosition)
    {
        if (grid == null)
        {
            return transform.position;
        }

        if (buildPoses == null || buildPoses.Count == 0)
        {
            return GetMovementWorldPosition(fallbackPosition);
        }

        bool found = false;
        int minX = int.MaxValue;
        int maxX = int.MinValue;
        foreach (Vector2Int position in buildPoses)
        {
            if (position.y != fallbackPosition.y) continue;

            found = true;
            minX = Mathf.Min(minX, position.x);
            maxX = Mathf.Max(maxX, position.x);
        }

        if (!found)
        {
            return GetMovementWorldPosition(fallbackPosition);
        }

        Vector3 anchor = grid.GetWorldPos(new Vector2((minX + maxX) * 0.5f, fallbackPosition.y));
        if (BuildingData != null)
        {
            anchor += (Vector3)BuildingData.movementAnchorOffset;
        }

        return anchor;
    }

    private static List<RendererVisibilityState> CaptureRendererVisibility(Character character)
    {
        List<RendererVisibilityState> states = new List<RendererVisibilityState>();
        if (character == null) return states;

        foreach (Renderer renderer in character.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer != null)
            {
                states.Add(new RendererVisibilityState(renderer, renderer.enabled));
            }
        }

        return states;
    }

    private static List<CanvasVisibilityState> CaptureCanvasVisibility(Character character)
    {
        List<CanvasVisibilityState> states = new List<CanvasVisibilityState>();
        if (character == null) return states;

        foreach (Canvas canvas in character.GetComponentsInChildren<Canvas>(true))
        {
            if (canvas != null)
            {
                states.Add(new CanvasVisibilityState(canvas, canvas.enabled));
            }
        }

        return states;
    }

    private static void SetCharacterTraversalVisible(
        List<RendererVisibilityState> rendererStates,
        List<CanvasVisibilityState> canvasStates,
        bool visible)
    {
        foreach (RendererVisibilityState state in rendererStates)
        {
            if (state.Renderer != null)
            {
                state.Renderer.enabled = visible;
            }
        }

        foreach (CanvasVisibilityState state in canvasStates)
        {
            if (state.Canvas != null)
            {
                state.Canvas.enabled = visible;
            }
        }
    }

    private static void RestoreCharacterTraversalVisible(
        List<RendererVisibilityState> rendererStates,
        List<CanvasVisibilityState> canvasStates)
    {
        foreach (RendererVisibilityState state in rendererStates)
        {
            if (state.Renderer != null)
            {
                state.Renderer.enabled = state.Enabled;
            }
        }

        foreach (CanvasVisibilityState state in canvasStates)
        {
            if (state.Canvas != null)
            {
                state.Canvas.enabled = state.Enabled;
            }
        }
    }

    private readonly struct RendererVisibilityState
    {
        public RendererVisibilityState(Renderer renderer, bool enabled)
        {
            Renderer = renderer;
            Enabled = enabled;
        }

        public Renderer Renderer { get; }
        public bool Enabled { get; }
    }

    private readonly struct CanvasVisibilityState
    {
        public CanvasVisibilityState(Canvas canvas, bool enabled)
        {
            Canvas = canvas;
            Enabled = enabled;
        }

        public Canvas Canvas { get; }
        public bool Enabled { get; }
    }
}
