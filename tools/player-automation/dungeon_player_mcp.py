#!/usr/bin/env python3
"""Dependency-free MCP adapter for DungeonStory's standalone player bridge."""

from __future__ import annotations

import argparse
import base64
import json
import os
import socket
import sys
import time
import uuid
from pathlib import Path
from typing import Any


DEFAULT_CONNECTION = (
    Path.home()
    / "AppData"
    / "LocalLow"
    / "DungeonStory"
    / "DungeonStoryPlaytest"
    / "Automation"
    / "bridge.json"
)


TOOLS = [
    {
        "name": "dungeon_player_status",
        "description": "Read the running standalone player's scene, run, objective, economy, screen, and camera state.",
        "inputSchema": {"type": "object", "properties": {}, "additionalProperties": False},
    },
    {
        "name": "dungeon_player_ui_list",
        "description": "List active standalone-player UI controls with names, labels, interactability, and screen bounds.",
        "inputSchema": {"type": "object", "properties": {}, "additionalProperties": False},
    },
    {
        "name": "dungeon_player_ui_click",
        "description": "Click one active Unity UI Button by exact GameObject name through pointer event handlers.",
        "inputSchema": {
            "type": "object",
            "properties": {"target": {"type": "string"}},
            "required": ["target"],
            "additionalProperties": False,
        },
    },
    {
        "name": "dungeon_player_key",
        "description": "Hold a Unity KeyCode such as W, A, S, D, LeftArrow, or Escape for a bounded duration.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "key": {"type": "string"},
                "duration": {"type": "number", "minimum": 0.05, "maximum": 30},
            },
            "required": ["key"],
            "additionalProperties": False,
        },
    },
    {
        "name": "dungeon_player_pointer",
        "description": "Move the standalone player's virtual pointer and optionally schedule a raw click.",
        "inputSchema": {
            "type": "object",
            "properties": {
                "x": {"type": "number"},
                "y": {"type": "number"},
                "click": {"type": "boolean"},
                "button": {"type": "integer", "minimum": 0, "maximum": 2},
            },
            "required": ["x", "y"],
            "additionalProperties": False,
        },
    },
    {
        "name": "dungeon_player_screenshot",
        "description": "Capture the standalone player's full rendered frame and return the PNG as MCP image content.",
        "inputSchema": {
            "type": "object",
            "properties": {"name": {"type": "string"}},
            "additionalProperties": False,
        },
    },
]


class PlayerBridge:
    def __init__(self, connection_path: Path) -> None:
        self.connection_path = connection_path

    def command(self, command: str, **arguments: Any) -> dict[str, Any]:
        info = json.loads(self.connection_path.read_text(encoding="utf-8-sig"))
        request = {
            "id": uuid.uuid4().hex,
            "token": info["token"],
            "command": command,
            "target": arguments.get("target", ""),
            "key": arguments.get("key", ""),
            "path": arguments.get("path", ""),
            "x": float(arguments.get("x", 0)),
            "y": float(arguments.get("y", 0)),
            "duration": float(arguments.get("duration", 0)),
            "button": int(arguments.get("button", 0)),
        }
        payload = (json.dumps(request, separators=(",", ":")) + "\n").encode("utf-8")
        with socket.create_connection((info["host"], int(info["port"])), timeout=5) as client:
            client.sendall(payload)
            reader = client.makefile("r", encoding="utf-8")
            line = reader.readline()
        if not line:
            raise RuntimeError("The player automation bridge returned an empty response")
        response = json.loads(line)
        if not response.get("ok"):
            raise RuntimeError(response.get("error") or "Player automation command failed")
        data = response.get("data")
        if isinstance(data, str) and data:
            try:
                response["parsedData"] = json.loads(data)
            except json.JSONDecodeError:
                pass
        return response


def tool_result(bridge: PlayerBridge, name: str, arguments: dict[str, Any]) -> dict[str, Any]:
    if name == "dungeon_player_status":
        response = bridge.command("game.status")
    elif name == "dungeon_player_ui_list":
        response = bridge.command("ui.list")
    elif name == "dungeon_player_ui_click":
        response = bridge.command("ui.click", target=arguments["target"])
    elif name == "dungeon_player_key":
        response = bridge.command(
            "input.key_down",
            key=arguments["key"],
            duration=arguments.get("duration", 0.25),
        )
    elif name == "dungeon_player_pointer":
        command = "input.pointer_click" if arguments.get("click") else "input.pointer_move"
        response = bridge.command(
            command,
            x=arguments["x"],
            y=arguments["y"],
            button=arguments.get("button", 0),
        )
    elif name == "dungeon_player_screenshot":
        response = bridge.command("capture.screen", path=arguments.get("name", ""))
        capture_path = Path(response["parsedData"]["path"])
        deadline = time.monotonic() + 8
        while time.monotonic() < deadline and not capture_path.exists():
            time.sleep(0.05)
        if not capture_path.exists():
            raise RuntimeError(f"Screenshot was queued but not written: {capture_path}")
        return {
            "content": [
                {"type": "text", "text": str(capture_path)},
                {
                    "type": "image",
                    "data": base64.b64encode(capture_path.read_bytes()).decode("ascii"),
                    "mimeType": "image/png",
                },
            ]
        }
    else:
        raise ValueError(f"Unknown tool: {name}")

    return {"content": [{"type": "text", "text": json.dumps(response, ensure_ascii=False, indent=2)}]}


def send(message: dict[str, Any]) -> None:
    sys.stdout.write(json.dumps(message, ensure_ascii=False, separators=(",", ":")) + "\n")
    sys.stdout.flush()


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--connection",
        type=Path,
        default=Path(os.environ.get("DUNGEON_PLAYER_AUTOMATION_CONNECTION", DEFAULT_CONNECTION)),
    )
    args = parser.parse_args()
    bridge = PlayerBridge(args.connection)

    for line in sys.stdin:
        if not line.strip():
            continue
        request: dict[str, Any] | None = None
        try:
            json_start = line.find("{")
            request = json.loads(line[json_start:] if json_start >= 0 else line)
            method = request.get("method", "")
            request_id = request.get("id")
            if method == "initialize":
                requested_protocol = request.get("params", {}).get("protocolVersion", "2025-03-26")
                result = {
                    "protocolVersion": requested_protocol,
                    "capabilities": {"tools": {"listChanged": False}},
                    "serverInfo": {"name": "dungeon-player", "version": "1.0.0"},
                }
            elif method == "tools/list":
                result = {"tools": TOOLS}
            elif method == "tools/call":
                params = request.get("params", {})
                result = tool_result(bridge, params.get("name", ""), params.get("arguments") or {})
            elif method == "ping":
                result = {}
            elif method.startswith("notifications/"):
                continue
            else:
                raise ValueError(f"Unsupported MCP method: {method}")
            send({"jsonrpc": "2.0", "id": request_id, "result": result})
        except Exception as exception:
            request_id = request.get("id") if isinstance(request, dict) else None
            send(
                {
                    "jsonrpc": "2.0",
                    "id": request_id,
                    "error": {"code": -32000, "message": f"{type(exception).__name__}: {exception}"},
                }
            )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
