# Student API for Unity Client

## Overview

This document describes the current student-facing API contract implemented by the Laravel backend.

Base URL example:

```text
http://localhost:8000
```

All student API routes are under `/api/v1`.

## Authentication Model

- Student login returns a Sanctum bearer token.
- Send that token on protected requests using:

```http
Authorization: Bearer <token>
Accept: application/json
```

- There is no refresh-token flow right now.
- Log out by calling the logout endpoint with the current bearer token.

## Endpoints

### 1. Student Login

`POST /api/v1/auth/login`

Use the student LRN and 6-digit PIN from the teacher-issued credential slip.

Request body:

```json
{
  "lrn": "123456789012",
  "pin": "123456"
}
```

Validation rules:

- `lrn`: required, string, exactly 12 characters
- `pin`: required, string, exactly 6 characters

Success response `200 OK`:

```json
{
  "message": "Login successful.",
  "token": "1|sanctum-token-value",
  "student": {
    "id": "2d80f4ef-4ad1-4f56-a0af-2c0329082d37",
    "full_name": "Juan Dela Cruz",
    "grade": 5,
    "section": "A",
    "must_change_password": true
  }
}
```

Failure responses:

- `401 Unauthorized`

```json
{
  "message": "The provided credentials are incorrect."
}
```

- `403 Forbidden`

```json
{
  "message": "This account has been deactivated."
}
```

- `422 Unprocessable Entity`

```json
{
  "message": "The lrn field is required. (example)",
  "errors": {
    "lrn": [
      "The lrn field is required."
    ]
  }
}
```

Unity notes:

- Save `token` securely after login.
- Save `student.id`, `grade`, and `section` locally if you need quick profile access.
- `must_change_password` is included in the payload even though the current student mobile API does not expose a password-change endpoint yet.

### 2. Current Authenticated User

`GET /api/v1/user`

Headers:

```http
Authorization: Bearer <token>
Accept: application/json
```

Success response `200 OK`:

Returns the authenticated user model as JSON.

Example:

```json
{
  "id": "2d80f4ef-4ad1-4f56-a0af-2c0329082d37",
  "role": "student",
  "full_name": "Juan Dela Cruz",
  "username": "juan5a",
  "grade": 5,
  "section": "A",
  "teacher_id": "33f1db6e-f947-423a-8af7-bff3b16d3170",
  "classroom_id": null,
  "is_active": true,
  "must_change_password": true,
  "created_at": "2026-04-16T10:00:00.000000Z",
  "updated_at": "2026-04-16T10:00:00.000000Z"
}
```

Failure response:

- `401 Unauthorized` when token is missing or invalid.

### 3. Student Logout

`POST /api/v1/auth/logout`

Headers:

```http
Authorization: Bearer <token>
Accept: application/json
```

Success response `200 OK`:

```json
{
  "message": "Logged out successfully."
}
```

Failure response:

- `401 Unauthorized` when token is missing or invalid.

Unity notes:

- After a successful logout, discard the stored token.

### 4. Get Student Worlds

`GET /api/v1/student/worlds`

Headers:

```http
Authorization: Bearer <student-token>
Accept: application/json
```

Rules:

- Requires a valid Sanctum token.
- Requires the authenticated user role to be `student`.
- Returns only the subjects for the authenticated student's grade.

Success response `200 OK`:

```json
{
  "data": [
    {
      "id": "1aab2cc3-dd44-55ee-66ff-778899001122",
      "name": "English",
      "grade": 5,
      "world_theme": "Language Galaxy",
      "color_hex": "#3B82F6",
      "difficulty": "standard",
      "quarters": [
        {
          "id": "quarter-uuid",
          "quarter_number": 1,
          "current_unlock_week": 2,
          "is_globally_unlocked": false,
          "levels": [
            {
              "id": "level-uuid",
              "level_number": 1,
              "title": "Level 1",
              "unlock_week": 1,
              "is_unlocked": true
            },
            {
              "id": "level-uuid-2",
              "level_number": 3,
              "title": "Level 3",
              "unlock_week": 3,
              "is_unlocked": false
            }
          ]
        }
      ]
    }
  ]
}
```

World response fields:

- `id`: subject UUID
- `name`: subject name
- `grade`: student grade for this world
- `world_theme`: world visual/theme label
- `color_hex`: display color for UI
- `difficulty`: `easy`, `standard`, `hard`, or `null`
- `quarters`: ordered array of quarter objects

Quarter fields:

- `id`
- `quarter_number`: integer `1` to `4`
- `current_unlock_week`: integer unlock progress for that quarter
- `is_globally_unlocked`: boolean override
- `levels`: ordered array of levels

Level fields:

- `id`
- `level_number`
- `title`
- `unlock_week`
- `is_unlocked`: computed server-side using quarter unlock state

Expected behavior:

- Grade 5 students receive 3 worlds.
- Grade 6 students receive 3 worlds.
- Unlock state is already computed by the API, so Unity should use `is_unlocked` directly.

Failure responses:

- `401 Unauthorized` when token is missing or invalid.
- `403 Forbidden` when token belongs to a non-student user.

### 5. Get Full Student Sync State

`GET /api/v1/student/sync/state`

Headers:

```http
Authorization: Bearer <student-token>
Accept: application/json
```

This endpoint is intended to bootstrap the Unity client in one request.

Success response `200 OK`:

```json
{
  "worlds": [
    {
      "id": "subject-uuid",
      "name": "English",
      "grade": 5,
      "world_theme": "Language Galaxy",
      "color_hex": "#3B82F6",
      "difficulty": "hard",
      "quarters": []
    }
  ],
  "preferences": {
    "language": "fil",
    "master_volume": 60,
    "bgm_volume": 50,
    "sfx_volume": 40,
    "tts_enabled": false,
    "text_size": "large",
    "colorblind_mode": true
  },
  "difficulties": [
    {
      "subject_id": "subject-uuid",
      "subject_name": "English",
      "difficulty": "hard",
      "set_by": "teacher",
      "updated_at_by_teacher": "2026-04-16T10:00:00.000000Z"
    }
  ],
  "screen_time": {
    "scope": "global",
    "school_day_limit_min": 45,
    "weekend_limit_min": 60,
    "max_levels_school": 2,
    "max_levels_weekend": 3,
    "play_start_school": "15:00:00",
    "play_end_school": "20:00:00",
    "play_start_weekend": "08:00:00",
    "play_end_weekend": "20:00:00"
  },
  "badges": [
    {
      "badge_id": "badge-uuid",
      "name": "Fast Learner",
      "description": "Awarded for quick progress.",
      "icon": "fast-learner",
      "trigger_type": "progress",
      "earned_at": "2026-04-16T10:00:00.000000Z"
    }
  ],
  "grades": [
    {
      "subject_id": "subject-uuid",
      "subject_name": "English",
      "quarter_number": 1,
      "written_work": 92.0,
      "performance_task": 94.0,
      "quarterly_assessment": 91.0,
      "final_grade": 92.5,
      "computed_at": "2026-04-16T10:00:00.000000Z"
    }
  ]
}
```

Top-level sections:

- `worlds`: same structure as `/api/v1/student/worlds`, but returned inline instead of inside a `data` wrapper
- `preferences`: one object for the student’s current game/accessibility preferences
- `difficulties`: per-subject difficulty settings
- `screen_time`: current effective screen-time settings
- `badges`: earned badge history
- `grades`: computed academic records by subject/quarter

#### Preferences fields

- `language`: `en` or `fil`
- `master_volume`: integer
- `bgm_volume`: integer
- `sfx_volume`: integer
- `tts_enabled`: boolean
- `text_size`: `small`, `medium`, or `large`
- `colorblind_mode`: boolean

#### Difficulty fields

- `subject_id`
- `subject_name`
- `difficulty`: `easy`, `standard`, `hard`
- `set_by`: `system_default` or `teacher`
- `updated_at_by_teacher`: ISO-8601 string or `null`

#### Screen-time fields

- `scope`: currently `global`
- `school_day_limit_min`
- `weekend_limit_min`
- `max_levels_school`
- `max_levels_weekend`
- `play_start_school`
- `play_end_school`
- `play_start_weekend`
- `play_end_weekend`

Current fallback defaults used by the API if no setting row exists:

- `school_day_limit_min`: `45`
- `weekend_limit_min`: `60`
- `max_levels_school`: `2`
- `max_levels_weekend`: `3`
- `play_start_school`: `15:00:00`
- `play_end_school`: `20:00:00`
- `play_start_weekend`: `08:00:00`
- `play_end_weekend`: `20:00:00`

#### Badge fields

- `badge_id`
- `name`
- `description`
- `icon`
- `trigger_type`
- `earned_at`: ISO-8601 string

#### Grade fields

- `subject_id`
- `subject_name`
- `quarter_number`
- `written_work`
- `performance_task`
- `quarterly_assessment`
- `final_grade`
- `computed_at`: ISO-8601 string or `null`

Failure responses:

- `401 Unauthorized` when token is missing or invalid.
- `403 Forbidden` when token belongs to a non-student user.

### 6. Join Classroom Room

`POST /api/v1/student/join-room`

Headers:

```http
Authorization: Bearer <student-token>
Accept: application/json
```

Request body:

```json
{
  "room_code": "ABC123"
}
```

Validation rules:

- `room_code`: required, string, exactly 6 characters
- Room code is case-insensitive (auto-uppercased by the server)
- After validation passes, the server checks:
  - A classroom with this room code exists (otherwise `422` with `"No classroom found with this room code."`)
  - The classroom is active (otherwise `422` with `"This classroom is no longer active."`)

Success response `200 OK`:

```json
{
  "message": "Successfully joined the classroom.",
  "classroom": {
    "id": "classroom-uuid",
    "name": "Grade 5 - Section A",
    "grade": 5,
    "section": "A",
    "room_code": "ABC123"
  }
}
```

Already-in-classroom response `422 Unprocessable Entity`:

```json
{
  "message": "You are already in this classroom."
}
```

Failure responses:

- `401 Unauthorized` when token is missing or invalid.
- `403 Forbidden` when token belongs to a non-student user.
- `404 Not Found` if the room code passed initial validation but the classroom was deleted between validation and controller execution (race condition, unlikely).
- `422 Unprocessable Entity` for validation errors:

```json
{
  "message": "No classroom found with this room code.",
  "errors": {
    "room_code": [
      "No classroom found with this room code."
    ]
  }
}
```

Unity notes:

- Send the room code exactly as the student types it; the server normalizes to uppercase.
- On success, update the local student profile with the returned `classroom` data.
- A student can only belong to one classroom at a time. Joining a new room replaces the previous assignment.

## Enum Reference for Unity

### Language

- `en` = English
- `fil` = Filipino

### Text Size

- `small`
- `medium`
- `large`

### Difficulty

- `easy`
- `standard`
- `hard`

### Difficulty Source

- `system_default`
- `teacher`

### Screen Time Scope

- `global`
- `class`
- `student`

## Recommended Unity Integration Flow

1. Call `POST /api/v1/auth/login`.
2. Store the returned bearer token.
3. If the student has no `classroom_id`, prompt them to join a room via `POST /api/v1/student/join-room`.
4. Call `GET /api/v1/student/sync/state` on app bootstrap after login.
5. Use `worlds` to build the content map and unlock UI.
6. Use `preferences` to initialize language, audio, and accessibility settings.
7. Use `difficulties`, `badges`, and `grades` to hydrate the local player profile.
8. Optionally call `GET /api/v1/student/worlds` later when only world data needs refresh.
9. Call `POST /api/v1/auth/logout` when the user signs out.

## Error Handling Recommendations for Unity

- `401`: token missing, expired, revoked, or malformed. Return to login.
- `403`: authenticated but not allowed for this endpoint.
- `422`: validation issue on login request. Show field-level error if needed.
- `429`: too many login attempts. Retry later and show a rate-limit message.

## Current Verification Status

All tests and quality checks pass as of the latest session:

- `composer test` — **fully green**
  - Type coverage: **100%**
  - Unit tests: **237 tests / 1291 assertions** — all passing
  - Lint: Pint pass, Rector pass, frontend format pass
  - PHPStan: **0 errors**
- `composer lint` — clean (no auto-modifications needed)

Endpoint coverage:

- Student login, logout, current user — tested
- Student worlds, sync state — tested
- Student join-room — tested
- Enum values and labels — tested (15 tests)
- Model relationships and casts — tested (25 tests)
- EnsurePasswordChanged middleware — tested
