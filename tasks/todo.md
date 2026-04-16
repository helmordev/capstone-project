# Join Room Bug Fix Plan

- [completed] Inspect the student room-join flow and confirm where the false failure message is produced.
- [completed] Patch `HubSelectionManager.cs` to only navigate on valid classroom data and avoid showing failure text on success.
- [completed] Review the change for edge cases and document the result.

## Review

- `HubSelectionManager` no longer shows raw server text as a success message when the payload already contains valid classroom data.
- Scene loading now requires a valid classroom id instead of only a non-null classroom object.
- Added a small in-flight guard so duplicate join requests do not race each other and produce conflicting UI.
- `HubSelectionManager.cs` builds cleanly from the script changes; the broader `dotnet build` was blocked by unrelated package-cache errors in `Library/PackageCache/com.unity.render-pipelines.core`.

# API Update Plan

- [completed] Review Unity API-call scripts and map old contract to `student-unity.md`.
- [completed] Update login flow to use `/api/v1/auth/login`, `lrn` payload, and new response fields.
- [completed] Update room-join flow to use `/api/v1/student/join-room`, `room_code` payload, and new response fields.
- [completed] Verify changes via diff review and document results.

## Review

- Updated base API URL defaults to `http://localhost:8000` in login and room-join scripts.
- Replaced deprecated endpoints with `POST /api/v1/auth/login` and `POST /api/v1/student/join-room`.
- Updated request payloads to match docs: `lrn` + `pin` for login, `room_code` for room join.
- Added `Accept: application/json` header and improved HTTP result handling for API errors.
- Aligned success parsing to new response contracts and persisted documented fields in `PlayerPrefs`.
- Removed refresh-token dependency because current API returns only a bearer token.
