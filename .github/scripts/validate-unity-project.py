#!/usr/bin/env python3

from __future__ import annotations

import re
import subprocess
import sys
from collections import defaultdict
from pathlib import Path, PurePosixPath


GUID_LINE = re.compile(r"(?m)^guid:\s*([^\s]+)\s*$")
VALID_GUID = re.compile(r"^[0-9a-f]{32}$")


def git_output(root: Path, *args: str) -> bytes:
    return subprocess.check_output(["git", *args], cwd=root)


def tracked_asset_paths(root: Path) -> set[str]:
    output = git_output(root, "ls-files", "-z", "--", "Assets")
    return {item.decode("utf-8") for item in output.split(b"\0") if item}


def required_asset_directories(asset_paths: set[str]) -> set[str]:
    directories: set[str] = set()

    for path in asset_paths:
        if path.endswith(".meta"):
            continue

        parent = PurePosixPath(path).parent
        while parent != PurePosixPath("Assets"):
            directories.add(parent.as_posix())
            parent = parent.parent

    return directories


def validate_metadata(root: Path, asset_paths: set[str]) -> list[str]:
    errors: list[str] = []
    required_directories = required_asset_directories(asset_paths)
    assets = {path for path in asset_paths if not path.endswith(".meta")}
    metadata = {path for path in asset_paths if path.endswith(".meta")}

    required_metadata = {f"{path}.meta" for path in assets | required_directories}
    for path in sorted(required_metadata - metadata):
        errors.append(f"missing tracked metadata file: {path}")

    valid_targets = assets | required_directories
    for path in sorted(metadata):
        contents = (root / path).read_text(encoding="utf-8", errors="replace")
        is_folder_metadata = re.search(r"(?m)^folderAsset:\s*yes\s*$", contents) is not None
        if path.removesuffix(".meta") not in valid_targets and not is_folder_metadata:
            errors.append(f"orphaned metadata file: {path}")

    paths_by_guid: defaultdict[str, list[str]] = defaultdict(list)
    for relative_path in sorted(metadata):
        contents = (root / relative_path).read_text(encoding="utf-8", errors="replace")
        match = GUID_LINE.search(contents)
        if match is None:
            errors.append(f"metadata has no GUID: {relative_path}")
            continue

        guid = match.group(1)
        if VALID_GUID.fullmatch(guid) is None:
            errors.append(f"metadata has an invalid GUID '{guid}': {relative_path}")
            continue

        paths_by_guid[guid].append(relative_path)

    for guid, paths in sorted(paths_by_guid.items()):
        if len(paths) > 1:
            errors.append(f"duplicate GUID {guid}: {', '.join(paths)}")

    return errors


def validate_case_collisions(asset_paths: set[str]) -> list[str]:
    errors: list[str] = []
    paths_by_case: defaultdict[str, list[str]] = defaultdict(list)

    for path in asset_paths:
        paths_by_case[path.casefold()].append(path)

    for paths in paths_by_case.values():
        if len(paths) > 1:
            errors.append(f"case-insensitive path collision: {', '.join(sorted(paths))}")

    return sorted(errors)


def validate_project_settings(root: Path) -> list[str]:
    errors: list[str] = []
    settings = {
        "ProjectSettings/EditorSettings.asset": (r"(?m)^\s*m_SerializationMode:\s*2\s*$", "Force Text asset serialization"),
        "ProjectSettings/VersionControlSettings.asset": (r"(?m)^\s*m_Mode:\s*Visible Meta Files\s*$", "Visible Meta Files mode"),
    }

    for relative_path, (pattern, description) in settings.items():
        path = root / relative_path
        if not path.is_file():
            errors.append(f"missing Unity settings file: {relative_path}")
            continue

        contents = path.read_text(encoding="utf-8", errors="replace")
        if re.search(pattern, contents) is None:
            errors.append(f"Unity project must use {description}: {relative_path}")

    return errors


def main() -> int:
    root = Path(git_output(Path.cwd(), "rev-parse", "--show-toplevel").decode().strip())
    asset_paths = tracked_asset_paths(root)
    errors = [
        *validate_metadata(root, asset_paths),
        *validate_case_collisions(asset_paths),
        *validate_project_settings(root),
    ]

    if errors:
        print("Unity project validation failed:", file=sys.stderr)
        for error in errors:
            print(f"  - {error}", file=sys.stderr)
        return 1

    metadata_count = sum(path.endswith(".meta") for path in asset_paths)
    print(f"Unity metadata is consistent ({metadata_count} tracked .meta files checked).")
    print("Unity serialization and version control settings are consistent.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
