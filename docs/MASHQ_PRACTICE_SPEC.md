# Mashq (Practice) Feature — Backend Spec

Handover from Azim → backend. Frontend page already built (`/mashq`, currently hidden); needs these entities + endpoints to go live.

## Goal

Question bank practice mode. Student picks filters (subject, grade, difficulty, topic, status) → server serves matching questions one-by-one → student answers → server returns verdict + explanation. 10 free questions/day; 2 tanga buys +10 more that day.

## Rules

- **Free quota:** 10 answered questions per day per user.
- **Paywall:** when quota exhausted, `POST /practice/quota/purchase` spends **2 tanga** from existing `UserBalance` → grants **+10** questions for the same day. Repeatable. Log to `TransactionHistory` like test purchases.
- **Day boundary:** Tashkent local day (use same `TimeHelper.GetDateTime()` convention as `Auditable.CreatedAt`).
- **Quota consumed on answer submit**, not on question fetch.
- **Correct answer / explanation must NEVER appear in question DTOs.** Only the answer endpoint reveals them, after the attempt row is written. (Old frontend mock leaked `correct: true` in options — do not replicate.)
- Re-answering an already-answered question is allowed (student retries mistakes). Attempts are append-only; a question's status for filtering = latest attempt.
- Auth required on all endpoints (JWT, existing scheme). `UserId` = real `User` (long), no TempUser support in v1.

## Entities (MilliyMock.Domain/Entities)

```csharp
using MilliyMock.Domain.Commons;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Domain.Entities;

public class PracticeQuestion : Auditable
{
    public SubjectType Subject { get; set; }
    public int? Grade { get; set; }                     // 5..11, null = any grade
    public PracticeDifficulty Difficulty { get; set; }
    public string Topic { get; set; } = null!;          // slug: "algebra", "geometriya", ...
    public string Text { get; set; } = null!;           // may contain LaTeX: $...$ and $$...$$
    public string OptionA { get; set; } = null!;
    public string OptionB { get; set; } = null!;
    public string OptionC { get; set; } = null!;
    public string OptionD { get; set; } = null!;
    public string CorrectLetter { get; set; } = null!;  // "A" | "B" | "C" | "D"
    public string? ExplanationTitle { get; set; }
    public string? Explanation { get; set; }            // paragraphs separated by \n\n, LaTeX allowed
    public bool IsActive { get; set; } = true;
}
```

```csharp
public class PracticeAttempt : Auditable
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public long PracticeQuestionId { get; set; }
    public PracticeQuestion PracticeQuestion { get; set; } = null!;
    public string ChosenLetter { get; set; } = null!;   // "A".."D"
    public bool IsCorrect { get; set; }                  // denormalized for fast status filter
}
// answer timestamp = Auditable.CreatedAt
// indexes: (UserId, PracticeQuestionId), (UserId, CreatedAt)
```

```csharp
public class PracticeSavedQuestion : Auditable
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public long PracticeQuestionId { get; set; }
    public PracticeQuestion PracticeQuestion { get; set; } = null!;
}
// unique index (UserId, PracticeQuestionId); unsave = hard delete or IsDeleted
```

```csharp
public class PracticeQuotaPurchase : Auditable
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public DateOnly Day { get; set; }        // Tashkent-local date the purchase applies to
    public int ExtraQuestions { get; set; }  // 10
    public int TangaSpent { get; set; }      // 2
}
// index: (UserId, Day)
```

New enum (MilliyMock.Domain/Enums):

```csharp
public enum PracticeDifficulty
{
    Easy = 1,    // oson
    Medium = 2,  // o'rta
    Hard = 3     // qiyin
}
```

## Quota algorithm

```
day       = today (Tashkent local, via TimeHelper)
used      = COUNT PracticeAttempt WHERE UserId = @u AND CreatedAt within day
extra     = SUM PracticeQuotaPurchase.ExtraQuestions WHERE UserId = @u AND Day = day
remaining = 10 + extra - used
```

`POST /answer` with `remaining <= 0` → reject (402 or 409 with error code `quota_exhausted`).

## API

All under `/api/practice`, `[Authorize]`.

### GET /api/practice/questions
Query: `subject` (SubjectType), `grade` (int, optional), `difficulty` (optional), `topic` (optional), `status` (optional: `unanswered | correct | incorrect | saved`), `count` (default 12, max 20).
Returns a randomized batch matching filters — **without** `CorrectLetter`, `Explanation*`:

```json
[{ "id": 17, "subject": 1, "grade": 9, "difficulty": 2, "topic": "algebra",
   "text": "...", "optionA": "...", "optionB": "...", "optionC": "...", "optionD": "...",
   "isSaved": false, "lastResult": null }]
```
`lastResult`: `null | "correct" | "incorrect"` from latest attempt (frontend paints the strip).
Status filter semantics: `unanswered` = no attempt exists; `correct`/`incorrect` = latest attempt result; `saved` = row in PracticeSavedQuestion.

### POST /api/practice/answer
Body: `{ "questionId": 17, "letter": "B" }`
Checks quota → writes PracticeAttempt → returns:

```json
{ "isCorrect": true, "correctLetter": "B",
  "explanationTitle": "...", "explanation": "...",
  "quotaRemaining": 6 }
```
Quota exhausted → 402 `{ "error": "quota_exhausted", "priceTanga": 2, "grantsQuestions": 10 }`.

### GET /api/practice/quota
`{ "freeLimit": 10, "used": 4, "extra": 0, "remaining": 6 }`

### POST /api/practice/quota/purchase
Atomically: check `UserBalance >= 2` → deduct 2 → insert PracticeQuotaPurchase(today, +10, 2) → TransactionHistory row.
Insufficient balance → same error shape as premium test purchase (frontend reuses top-up modal).

### POST /api/practice/save  /  DELETE /api/practice/save/{questionId}
Toggle saved state.

## Content seeding

Questions authored as JSON array (same field names as entity, `subject` as enum int, `difficulty` 1..3). Azim + Claude produce batches; committed to this repo under `seed/practice-questions/*.json`.
Loading mechanism = backend's choice, two options:
1. Idempotent startup seeder (insert where not exists by content hash) — preferred, no migration bloat;
2. Generated SQL INSERT script run manually with migration.
EF `HasData` NOT recommended — hundreds of LaTeX-heavy rows in migration files get unmanageable.

Sample question object:

```json
{
  "subject": 1, "grade": 9, "difficulty": 2, "topic": "algebra",
  "text": "$x^2 - 5x + 6 = 0$ tenglamaning ildizlari yig'indisini toping.",
  "optionA": "$5$", "optionB": "$6$", "optionC": "$-5$", "optionD": "$1$",
  "correctLetter": "A",
  "explanationTitle": "Viet teoremasi",
  "explanation": "Viet teoremasiga ko'ra $x_1 + x_2 = -\\frac{b}{a} = 5$.\n\nJavob: A."
}
```

## Non-goals v1

- Admin CRUD panel (v2; seed JSON only for now)
- Timers server-side (frontend cosmetic)
- Stats/analytics endpoints
- TempUser / anonymous practice
- `/mashq-junior` page (deleted)

## Frontend side (Azim, after API ready)

- Restore `/mashq` route (+ `requiresAuth`), delete junior page
- Wire filters → GET /questions; answer flow → POST /answer; quota chip; 402 → reuse tanga top-up modal
- Test against local API :5227 before deploy
