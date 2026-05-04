# BookOrbit Backend API Reference For Frontend

Generated from the codebase on 2026-05-04.

This document is based on the checked-in backend code, not on assumptions.

## 1. Quick Start

### Route base

- Controller route template: `api/v{version:apiVersion}/...`
- All current controllers are tagged with API version `1.0`
- The examples in this document use: `http://localhost:7240/api/v1.0`

### Authentication

- Protected endpoints use JWT bearer auth.
- Send: `Authorization: Bearer <accessToken>`
- Login endpoint: `POST /identity/token`
- Refresh endpoint: `POST /identity/token/refresh`

### JSON and enum conventions

- JSON property names are `camelCase`.
- Enums are serialized as strings in `camelCase`.
- Example:
  - `StudentState.Active` becomes `"active"`
  - `BorrowingRequestState.Pending` becomes `"pending"`
  - `BookCopyCondition.LikeNew` becomes `"likeNew"`

### Error contract

Errors are returned as `ProblemDetails`.

- `status`: HTTP status code
- `title`: error type
- `detail`: main message for non-validation errors
- `instance`: request path
- `requestId`: added by custom problem details middleware
- `errors`: array of human-readable error messages

Validation failures return:

```json
{
  "status": 400,
  "title": "Validation Error",
  "detail": "One or more validation errors occurred.",
  "errors": ["..."]
}
```

### Pagination

All paged endpoints return:

```json
{
  "page": 1,
  "pageSize": 10,
  "totalPages": 3,
  "totalCount": 25,
  "items": []
}
```

Important backend behavior:

- `page` is clamped to at least `1`
- `pageSize` is clamped to at least `1`
- Default `pageSize` is `1` if frontend does not send it
- Invalid or missing `sortColumn` falls back to `createdAt desc`

### Rate limits

These are configured globally and applied per endpoint:

- `NormalRateLimite`: `60` requests per minute
- `SensitiveRateLimite`: `5` requests per minute
- `OnceAMinuteRateLimit`: `1` request per minute

### Caching

- Most GET list/detail endpoints use output caching for `60` seconds
- Image controller endpoints use client response caching for `1` day

### File upload rules

Applies to student personal photos and book cover images:

- Allowed extensions: `.jpg`, `.jpeg`, `.png`, `.webp`
- Max size: `2 MB`
- Upload endpoints expect `multipart/form-data`

### Shared JS helper

Use something like this in the frontend:

```js
const API_BASE = "http://localhost:7240/api/v1.0";

async function api(path, { method = "GET", token, body, isForm = false } = {}) {
  const headers = {};

  if (token) headers.Authorization = `Bearer ${token}`;
  if (!isForm && body !== undefined) headers["Content-Type"] = "application/json";

  const res = await fetch(`${API_BASE}${path}`, {
    method,
    headers,
    body: body === undefined ? undefined : isForm ? body : JSON.stringify(body)
  });

  if (!res.ok) {
    const problem = await res.json().catch(() => null);
    throw { status: res.status, problem };
  }

  if (res.status === 204) return null;

  const contentType = res.headers.get("content-type") || "";
  if (contentType.includes("application/json")) return res.json();
  return res.blob();
}
```

## 2. Data Model Relationships

- `AppUser` is the identity account.
- `Student` is the website domain profile linked to `AppUser.UserId`.
- `Book` is the catalog record.
- `BookCopy` is a physical copy owned by a student and linked to a book.
- `LendingListRecord` lists one book copy for borrowing.
- `BorrowingRequest` links one borrowing student to one lending list record.
- `BorrowingTransaction` is created after a borrowing request is accepted and the book is delivered.

## 3. Authorization Policies

This section describes what each policy actually allows according to the handlers.

| Policy constant | Policy name string | Allows | Does not allow | Admin bypass | Used by |
|---|---|---|---|---|---|
| `RegisteredUserPolicy` | `RegisteredUserAccess` | Any authenticated user with a valid JWT | Anonymous users | No | `GET /identity/users/me`, `POST /identity/users/me/change-password`, `GET /images/students/{studentId}` |
| `ActiveUserPolicy` | `ActiveUserAccess` | Authenticated user whose identity account has confirmed email | Anonymous users, tokens without a stored confirmed user | No | Currently unused by endpoints directly |
| `AdminOnlyPolicy` | `AdminOnlyAccess` | Confirmed authenticated user with role `admin` | Students, anonymous users, unconfirmed admins | N/A | Admin student/book/request/transaction endpoints |
| `StudentOnlyPolicy` | `StudentOnlyAccess` | Authenticated user with role `student` | Admins, anonymous users | No | `GET /students/me`, `POST /students/me/books/{bookId}/copies`, `POST /lendinglist/{id}/request` |
| `StudentOwnershipPolicy` | `StudentOwnerShipAccess` | Admin, or student whose `studentId` route value belongs to their user | Other students, non-students | Yes | `PATCH /students/{studentId}` |
| `RegisteredUserOwnershipPolicy` | `RegisteredUserOwnershipAccess` | Admin, or user whose `userId` route value matches token user id | Other users | Yes | Currently unused by endpoints directly |
| `ActiveStudentPolicy` | `ActiveStudentAccess` | Admin, or student whose student record is in state `active` | Pending, approved-only, rejected, banned, unbanned students | Yes | Most browsing/create catalog endpoints |
| `StudentOwnerOfBookCopyPolicy` | `StudentOwnerOfBookCopyAccess` | Admin, or student who owns the route `bookCopyId` | Other students | Yes | Book copy availability and list endpoints |
| `StudentOwnerOfLendingListRecordPolicy` | `StudentOwnerOfLendingListRecordAccess` | Admin, or student who owns the route `lendingListRecordId` through the listed book copy | Other students | Yes | Lending list contact-info and close |
| `BorrowingRequestBorrowingStudentPolicy` | `BorrowingRequestBorrowingStudentAccess` | Admin, or student who is the borrower on `borrowingRequestId` | Other students | Yes | Cancel borrowing request |
| `BorrowingRequestLendingStudentPolicy` | `BorrowingRequestLendingStudentAccess` | Admin, or student who owns the lending record/book copy on `borrowingRequestId` | Other students | Yes | Accept, reject, deliver borrowing request |
| `BorrowingRequestRelatedStudentPolicy` | `BorrowingRequestRelatedStudentAccess` | Admin, or either side of the borrowing request | Unrelated students | Yes | Borrowing request detail |
| `BorrowingTransactionBorrowingStudentPolicy` | `BorrowingTransactionBorrowingStudentAccess` | Admin, or student who is the borrower on `borrowingTransactionId` | Other students | Yes | Return/lost transaction |
| `BorrowingTransactionLendingStudentPolicy` | `BorrowingTransactionLendingStudentAccess` | Admin, or student who is the lender on `borrowingTransactionId` | Other students | Yes | Currently unused by endpoints directly |
| `BorrowingTransactionRelatedStudentPolicy` | `BorrowingTransactionRelatedStudentAccess` | Admin, or either side of the transaction | Unrelated students | Yes | Borrowing transaction detail |
| `StudentAcceptedForLendingListRecordPolicy` | `StudentAcceptedForLendingListRecordAccess` | Admin, or student with an accepted borrowing request for that lending list record | Everyone else | Yes | Currently unused by endpoints directly |

### Practical policy notes

- `StudentOnlyPolicy` does not require the student to be `active`.
- `ActiveStudentPolicy` is the main gate for normal marketplace usage.
- `AdminOnlyPolicy` also requires email-confirmed identity because it includes `ActiveUserRequirement`.
- Several policies exist but are not currently attached to any controller action.

## 4. Validation And Contract Notes

### Identity/password rules

Configured ASP.NET Identity password settings:

- Minimum length: `6`
- Must contain at least one digit
- Must contain at least `4` unique characters
- Uppercase not required
- Lowercase not required
- Symbol not required

Important implementation detail:

- `CreateStudent` does not validate password in the application validator; real enforcement happens in ASP.NET Identity.
- `ResetPassword` validator does not validate `newPassword`; real enforcement also happens in ASP.NET Identity.
- `ChangePassword` validator only enforces minimum length `6`; Identity still enforces the stronger stored password policy.

### Student fields

- `name`: `3-50` chars, Arabic/English letters and spaces only
- `universityMailAddress`: must be a valid email ending with `@std.mans.edu.eg`
- `phoneNumber`: optional, valid Egyptian mobile, normalized to a `201...` style value
- `telegramUserId`: optional, `5-32` chars, lowercase letters/digits/underscore, at least 3 letters
- At least one of `phoneNumber` or `telegramUserId` must exist when creating a student

### Book fields

- `title`: `3-200` chars, Arabic/English letters and spaces only
- `isbn`: must be valid ISBN-10 or ISBN-13 after removing spaces/hyphens
- `publisher`: `3-150` chars, Arabic/English letters and spaces only
- `author`: `3-150` chars, Arabic/English letters and spaces only
- `categories`: input is a list, stored as a flags enum on the book

### Book category filtering nuance

When filtering books by categories:

- frontend sends a list like `["fiction", "mystery"]`
- backend ORs the flags together
- filter logic is `(book.Category & selectedFlags) == selectedFlags`
- this means the book must contain **all selected categories**, not just any one of them

## 5. Response DTOs

### `TokenDto`

- `accessToken`: JWT access token
- `refreshToken`: refresh token string
- `expiresOnUtc`: UTC expiration time of the access token

### `AppUserDto`

- `name`: identity username
- `userId`: identity user id
- `email`: account email
- `roles`: array of role names
- `claims`: serialized claims from ASP.NET Identity
- `emailConfirmed`: whether the identity email is confirmed

Note: `claims` uses `System.Security.Claims.Claim` objects; frontend should not treat its JSON shape as a stable public contract.

### `StudentDto`

- `id`
- `name`
- `points`
- `state`
- `joinDate`

### `StudentDtoWithContactInfo`

- All `StudentDto` fields plus:
- `universityMailAddress`
- `phoneNumber`
- `telegramUserId`

### `StudentListItemDto`

- `id`
- `name`
- `phoneNumber`
- `telegramUserId`
- `universityMailAddress`
- `points`
- `state`
- `joinDate`

### `StudentContactInformationDto`

- `id`
- `phoneNumber`
- `telegramUserId`

### `BookDto`

- `id`
- `title`
- `isbn`
- `publisher`
- `category`
- `author`
- `bookCoverImageUrl`
- `status`

### `BookListItemDto`

- All major book fields plus:
- `availableCopiesCount`

### `BookCopyDtoWithBookDetails`

- `id`
- `bookId`
- `ownerId`
- `condition`
- `state`
- `ownerName`
- `title`
- `isbn`
- `publisher`
- `category`
- `author`
- `isListed`
- `bookCoverImageUrl`

### `BookCopyListItemDto`

- `id`
- `bookId`
- `ownerId`
- `condition`
- `state`
- `ownerName`
- `title`
- `isListed`
- `bookCoverImageUrl`

### `LendingListRecordDto`

- `id`
- `bookCopyId`
- `state`
- `borrowingDurationInDays`
- `cost`

### `LendingListRecordListItemDto`

- `id`
- `bookCopyId`
- `ownerId`
- `condition`
- `bookCopyState`
- `ownerName`
- `title`
- `state`
- `borrowingDurationInDays`
- `cost`
- `createdAt`

### `BorrowingRequestDto`

- `id`
- `borrowingStudentId`
- `lendingRecordId`
- `state`
- `expirationDateUtc`

### `BorrowingRequestListItemDto`

- `id`
- `borrowingStudentId`
- `lendingRecordId`
- `lendingStudentId`
- `borrowingStudentName`
- `bookTitle`
- `bookId`
- `bookCopyId`
- `state`
- `expirationDateUtc`
- `createdAtUtc`

### `BorrowingTransactionDto`

- `id`
- `borrowingRequestId`
- `lenderStudentId`
- `borrowerStudentId`
- `bookCopyId`
- `state`
- `expectedReturnDate`
- `actualReturnDate`

### `BorrowingTransactionListItemDto`

- `id`
- `borrowingRequestId`
- `lenderStudentId`
- `borrowerStudentId`
- `bookCopyId`
- `lenderStudentName`
- `borrowerStudentName`
- `bookTitle`
- `state`
- `expectedReturnDate`
- `actualReturnDate`
- `createdAtUtc`

### `BorrowingReviewDto`

- `id`
- `reviewerStudentId`
- `reviewedStudentId`
- `borrowingTransactionId`
- `description`
- `rating`

### `BorrowingReviewListItemDto`

- `id`
- `reviewerStudentId`
- `reviewedStudentId`
- `borrowingTransactionId`
- `description`
- `rating`
- `createdAtUtc`

### `BorrowingTransactionEventDto`

- `id`
- `borrowingTransactionId`
- `state`
- `createdAtUtc`
- `lastModifiedUtc`

### `BorrowingTransactionEventListItemDto`

- `id`
- `borrowingTransactionId`
- `state`
- `createdAtUtc`

### `ChatMessageDto`

- `id`
- `content`
- `senderId`
- `chatGroupId`
- `isRead`
- `createdAtUtc`

### `ChatGroupListItemDto`

- `chatGroupId`
- `otherStudentId`
- `otherStudentName`
- `otherStudentPersonalPhotoFileName`
- `createdAtUtc`

### `NotificationDto`

- `id`
- `studentId`
- `title`
- `message`
- `isRead`
- `type`
- `createdAtUtc`
- `lastModifiedUtc`

### `NotificationListItemDto`

- `id`
- `studentId`
- `title`
- `message`
- `isRead`
- `type`
- `createdAtUtc`

### `PointTransactionDto`

- `id`
- `studentId`
- `borrowingReviewId`
- `points`
- `reason`
- `direction`

### `PointTransactionListItemDto`

- `id`
- `studentId`
- `studentName`
- `borrowingReviewId`
- `points`
- `reason`
- `direction`
- `createdAtUtc`

## 6. State Machines

### Student state

- Initial state on create: `pending`
- Allowed transitions:
  - `pending -> approved | rejected | banned`
  - `approved -> active | banned`
  - `active -> banned`
  - `rejected -> pending | banned`
  - `banned -> unBanned`
  - `unBanned -> active | banned`
- `approve` also sets `joinDateUtc`
- `approve` requires confirmed email

### Book status

- Initial state on create: `pending`
- Allowed transitions:
  - `pending -> available | rejected`
- Available/rejected are terminal in current code

### Book copy state

- Initial state on create: `available`
- Allowed transitions:
  - `available -> unAvailable`
  - `borrowed -> available | lost`
  - `lost -> available | unAvailable`
  - `unAvailable -> available`

### Lending list record state

- Initial state on create: `available`
- Allowed transitions:
  - `available -> reserved | closed | expired`
  - `reserved -> borrowed | available`
- `borrowed`, `expired`, and `closed` are terminal in practice

### Borrowing request state

- Initial state on create: `pending`
- Allowed transitions:
  - `pending -> accepted | rejected | cancelled | expired`
  - `accepted -> cancelled | expired`

### Borrowing transaction state

- Initial state on create: `borrowed`
- Allowed transitions:
  - `borrowed -> returned | overdue | lost`
  - `overdue -> lost | returned`
  - `lost -> returned`

Return behavior:

- Returning on or before expected date sets transaction state to `returned`
- Returning after expected date sets transaction state to `overdue`
- In both cases `actualReturnDate` is set

## 7. Endpoint Reference

The examples below assume the shared `api()` helper above.

---

## 7.1 Identity

### `POST /identity/token`

- Full URL: `/api/v1.0/identity/token`
- Auth: none
- Body:
  - `email`: login email
  - `password`: plaintext password
- Returns: `200 OK` with `TokenDto`
- What it does:
  - authenticates the identity user
  - refuses login if email is not confirmed
  - issues a new access token and refresh token
  - deletes any previous refresh token for that user before creating the new one
- Use this when:
  - user logs in
  - frontend needs a fresh authenticated session
- Integrates with:
  - `POST /identity/token/refresh`
  - every protected endpoint
- JS:

```js
await api("/identity/token", {
  method: "POST",
  body: { email, password }
});
```

### `POST /identity/token/refresh`

- Full URL: `/api/v1.0/identity/token/refresh`
- Auth: none
- Body:
  - `refreshToken`: stored refresh token
  - `expiredAccessToken`: the previous access token used to recover user context
- Returns: `200 OK` with `TokenDto`
- What it does:
  - validates the refresh token against the stored token for the user
  - validates the signed access token without checking expiry
  - issues a fresh token pair
- Use this when:
  - access token expires
- Integrates with:
  - `POST /identity/token`
- JS:

```js
await api("/identity/token/refresh", {
  method: "POST",
  body: { refreshToken, expiredAccessToken }
});
```

### `GET /identity/users/me`

- Full URL: `/api/v1.0/identity/users/me`
- Auth: `RegisteredUserPolicy`
- Params: none
- Returns: `200 OK` with `AppUserDto`
- What it does:
  - returns identity account details for the current token owner
- Use this when:
  - frontend boots after login and needs roles/emailConfirmed/userId
- Integrates with:
  - `GET /students/me`
- JS:

```js
await api("/identity/users/me", { token });
```

### `POST /identity/users/send-email-confirmation`

- Full URL: `/api/v1.0/identity/users/send-email-confirmation`
- Auth: none
- Query params:
  - `email`: target account email
- Returns: `200 OK` with success message
- What it does:
  - generates a confirmation token
  - sends an email that links to the frontend confirm-email page
- Frontend link target from config:
  - `http://localhost:3000/confirm-email?token=...&email=...`
- Use this when:
  - user asks to resend confirmation email
- Integrates with:
  - `POST /identity/confirm-email`
- JS:

```js
await api(`/identity/users/send-email-confirmation?email=${encodeURIComponent(email)}`, {
  method: "POST"
});
```

### `POST /identity/confirm-email`

- Full URL: `/api/v1.0/identity/confirm-email`
- Auth: none
- Query params:
  - `email`: email from the confirmation link
  - `token`: base64url-encoded confirmation token
- Returns: `200 OK` with success message
- What it does:
  - decodes and validates the email confirmation token
  - marks the identity email as confirmed
- Use this when:
  - frontend confirm-email page loads from the email link
- Integrates with:
  - `POST /identity/token`
  - `PATCH /students/{studentId}/approve` because approval requires confirmed email
- JS:

```js
await api(`/identity/confirm-email?email=${encodeURIComponent(email)}&token=${encodeURIComponent(token)}`, {
  method: "POST"
});
```

### `POST /identity/users/send-reset-password`

- Full URL: `/api/v1.0/identity/users/send-reset-password`
- Auth: none
- Query params:
  - `email`: account email
- Returns: `200 OK` with success message
- What it does:
  - generates a password reset token
  - sends an email that links to the frontend reset-password page
- Frontend link target from config:
  - `http://localhost:3000/reset-password?token=...&email=...`
- Use this when:
  - user forgot password
- Integrates with:
  - `POST /identity/reset-password`
- JS:

```js
await api(`/identity/users/send-reset-password?email=${encodeURIComponent(email)}`, {
  method: "POST"
});
```

### `POST /identity/reset-password`

- Full URL: `/api/v1.0/identity/reset-password`
- Auth: none
- Body:
  - `email`: account email
  - `encodedToken`: base64url-encoded reset token
  - `newPassword`: new password
- Returns: `200 OK` with success message
- What it does:
  - decodes reset token
  - asks ASP.NET Identity to reset the password
- Use this when:
  - frontend reset-password page submits new password
- Integrates with:
  - `POST /identity/token`
- JS:

```js
await api("/identity/reset-password", {
  method: "POST",
  body: { email, encodedToken, newPassword }
});
```

### `POST /identity/users/me/change-password`

- Full URL: `/api/v1.0/identity/users/me/change-password`
- Auth: `RegisteredUserPolicy`
- Body:
  - `oldPassword`: current password
  - `newPassword`: new password
- Returns: `200 OK` with success message
- What it does:
  - changes the current authenticated user's password
  - backend uses the email from the token, not from frontend input
- Use this when:
  - logged-in user changes password from settings
- JS:

```js
await api("/identity/users/me/change-password", {
  method: "POST",
  token,
  body: { oldPassword, newPassword }
});
```

---

## 7.2 Student Account

### `GET /students/me`

- Full URL: `/api/v1.0/students/me`
- Auth: `StudentOnlyPolicy`
- Params: none
- Returns: `200 OK` with `StudentDtoWithContactInfo`
- What it does:
  - returns the student profile linked to the current user
- Use this when:
  - dashboard/profile page loads
- Integrates with:
  - `GET /identity/users/me`
  - `PATCH /students/{studentId}`
- JS:

```js
await api("/students/me", { token });
```

### `POST /students`

- Full URL: `/api/v1.0/students`
- Auth: none
- Content type: `multipart/form-data`
- Form fields:
  - `name`: student display name
  - `universityMailAddress`: `@std.mans.edu.eg` email
  - `password`: account password
  - `personalPhoto`: image file
  - `phoneNumber`: optional phone
  - `telegramUserId`: optional Telegram username/id
- Returns: `201 Created` with `StudentDtoWithContactInfo`
- What it does:
  - uploads the personal photo
  - creates the ASP.NET Identity user with role `student`
  - creates the student domain record in state `pending`
  - initializes student points to `1`
  - automatically triggers the email-confirmation email via domain event
- Important rules:
  - at least one of `phoneNumber` or `telegramUserId` must be provided
  - email, phone, and telegram id must be unique
- Use this when:
  - new user signs up
- Integrates with:
  - auto-calls the email confirmation flow internally
  - `POST /identity/confirm-email`
  - admin student approval/activation endpoints
- JS:

```js
const form = new FormData();
form.append("name", name);
form.append("universityMailAddress", universityMailAddress);
form.append("password", password);
form.append("personalPhoto", personalPhotoFile);
if (phoneNumber) form.append("phoneNumber", phoneNumber);
if (telegramUserId) form.append("telegramUserId", telegramUserId);

await api("/students", { method: "POST", body: form, isForm: true });
```

### `PATCH /students/{studentId}`

- Full URL: `/api/v1.0/students/{studentId}`
- Auth: `StudentOwnershipPolicy`
- Content type: `multipart/form-data`
- Route params:
  - `studentId`: student id to update
- Form fields:
  - `name`: updated name
  - `personalPhoto`: optional replacement image
- Returns: `204 No Content`
- What it does:
  - fetches current photo filename
  - optionally uploads a new image
  - updates student name and stored photo filename
  - deletes the old image on success, or the new image on failure
- Important rules:
  - banned students cannot be updated at domain level
  - owner student or admin can call it
- Use this when:
  - student edits profile
- Integrates with:
  - `GET /students/me`
  - `GET /images/students/{studentId}`
- JS:

```js
const form = new FormData();
form.append("name", name);
if (personalPhotoFile) form.append("personalPhoto", personalPhotoFile);

await api(`/students/${studentId}`, {
  method: "PATCH",
  token,
  body: form,
  isForm: true
});
```

---

## 7.3 Student Management (Admin)

### `GET /students`

- Full URL: `/api/v1.0/students`
- Auth: `AdminOnlyPolicy`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `name`, `state`, `joinDate`
  - `sortDirection`: `asc` or `desc`
  - `states`: list of `StudentState`
  - `emailConfirmed`: `true` or `false`
- Returns: `200 OK` with paged `StudentListItemDto`
- Search behavior:
  - searches normalized student `name`
  - searches normalized `phoneNumber`
  - searches normalized `universityMail`
  - searches normalized `telegramUserId`
- Use this when:
  - admin reviews new/pending/active/banned students
- Integrates with:
  - all admin student state endpoints
- JS:

```js
await api("/students?page=1&pageSize=20&sortColumn=createdAt&sortDirection=desc", { token });
```

### `GET /students/{studentId}`

- Full URL: `/api/v1.0/students/{studentId}`
- Auth: `ActiveStudentPolicy`
- Route params:
  - `studentId`
- Returns: `200 OK` with `StudentDtoWithContactInfo`
- What it does:
  - returns full student details including contact info
- Use this when:
  - admin or active student opens student details page
- JS:

```js
await api(`/students/${studentId}`, { token });
```

### `GET /students/profiles/{studentId}`

- Full URL: `/api/v1.0/students/profiles/{studentId}`
- Auth: `ActiveStudentPolicy`
- Route params:
  - `studentId`
- Returns: `200 OK` with `StudentDto`
- What it does:
  - returns the public/basic profile only, without contact details
- Use this when:
  - active student views another student's public profile
- Integrates with:
  - `GET /students/{studentId}/books/copies`
- JS:

```js
await api(`/students/profiles/${studentId}`, { token });
```

### `PATCH /students/{studentId}/approve`

- Full URL: `/api/v1.0/students/{studentId}/approve`
- Auth: `AdminOnlyPolicy`
- Route params:
  - `studentId`
- Returns: `204 No Content`
- What it does:
  - moves student from `pending` to `approved`
  - sets `joinDateUtc`
  - requires the identity email to already be confirmed
- Use this when:
  - admin approves a newly created student account
- Integrates with:
  - `PATCH /students/{studentId}/activate`
- JS:

```js
await api(`/students/${studentId}/approve`, { method: "PATCH", token });
```

### `PATCH /students/{studentId}/activate`

- Full URL: `/api/v1.0/students/{studentId}/activate`
- Auth: `AdminOnlyPolicy`
- Route params:
  - `studentId`
- Returns: `204 No Content`
- What it does:
  - moves student from `approved` to `active`
- Use this when:
  - admin wants the student to use marketplace features
- JS:

```js
await api(`/students/${studentId}/activate`, { method: "PATCH", token });
```

### `PATCH /students/{studentId}/ban`

- Full URL: `/api/v1.0/students/{studentId}/ban`
- Auth: `AdminOnlyPolicy`
- Route params:
  - `studentId`
- Returns: `204 No Content`
- What it does:
  - moves student to `banned` if transition is valid
- Use this when:
  - admin suspends an account
- JS:

```js
await api(`/students/${studentId}/ban`, { method: "PATCH", token });
```

### `PATCH /students/{studentId}/reject`

- Full URL: `/api/v1.0/students/{studentId}/reject`
- Auth: `AdminOnlyPolicy`
- Route params:
  - `studentId`
- Returns: `204 No Content`
- What it does:
  - moves student from `pending` to `rejected`
- Use this when:
  - admin declines onboarding
- JS:

```js
await api(`/students/${studentId}/reject`, { method: "PATCH", token });
```

### `PATCH /students/{studentId}/unban`

- Full URL: `/api/v1.0/students/{studentId}/unban`
- Auth: `AdminOnlyPolicy`
- Route params:
  - `studentId`
- Returns: `204 No Content`
- What it does:
  - moves student from `banned` to `unBanned`
- Integrates with:
  - `PATCH /students/{studentId}/activate`
- JS:

```js
await api(`/students/${studentId}/unban`, { method: "PATCH", token });
```

### `PATCH /students/{studentId}/pend`

- Full URL: `/api/v1.0/students/{studentId}/pend`
- Auth: `AdminOnlyPolicy`
- Route params:
  - `studentId`
- Returns: `204 No Content`
- What it does:
  - moves student from `rejected` back to `pending`
- Use this when:
  - admin wants to reopen a rejected registration for review
- JS:

```js
await api(`/students/${studentId}/pend`, { method: "PATCH", token });
```

---

## 7.4 Books

### `POST /books`

- Full URL: `/api/v1.0/books`
- Auth: `ActiveStudentPolicy`
- Content type: `multipart/form-data`
- Form fields:
  - `title`
  - `isbn`
  - `publisher`
  - `categories`: list of `BookCategory`
  - `author`
  - `coverImage`
- Returns: `201 Created` with `BookDto`
- What it does:
  - uploads cover image
  - creates a book in `pending` status
- Important rules:
  - at least one category must be selected
  - ISBN must be unique among books in status `pending` or `available`
- Use this when:
  - active student adds a new catalog title that does not exist yet
- Integrates with:
  - admin must later call `PATCH /books/{bookId}/available` before copies can be created
- JS:

```js
const form = new FormData();
form.append("title", title);
form.append("isbn", isbn);
form.append("publisher", publisher);
categories.forEach(c => form.append("categories", c));
form.append("author", author);
form.append("coverImage", coverImageFile);

await api("/books", { method: "POST", token, body: form, isForm: true });
```

### `PATCH /books/{bookId}`

- Full URL: `/api/v1.0/books/{bookId}`
- Auth: `AdminOnlyPolicy`
- Content type: `multipart/form-data`
- Route params:
  - `bookId`
- Form fields:
  - `title`
  - `coverImage`: optional replacement image
- Returns: `204 No Content`
- What it does:
  - updates title
  - optionally replaces stored cover image
- Important limitation:
  - publisher, author, ISBN, and categories are not editable through this endpoint
- JS:

```js
const form = new FormData();
form.append("title", title);
if (coverImageFile) form.append("coverImage", coverImageFile);

await api(`/books/${bookId}`, {
  method: "PATCH",
  token,
  body: form,
  isForm: true
});
```

### `PATCH /books/{bookId}/available`

- Full URL: `/api/v1.0/books/{bookId}/available`
- Auth: `AdminOnlyPolicy`
- Route params:
  - `bookId`
- Returns: `204 No Content`
- What it does:
  - moves a book from `pending` to `available`
- Use this when:
  - admin approves a newly submitted book
- Integrates with:
  - `POST /students/me/books/{bookId}/copies`
- JS:

```js
await api(`/books/${bookId}/available`, { method: "PATCH", token });
```

### `PATCH /books/{bookId}/reject`

- Full URL: `/api/v1.0/books/{bookId}/reject`
- Auth: `AdminOnlyPolicy`
- Route params:
  - `bookId`
- Returns: `204 No Content`
- What it does:
  - moves a book from `pending` to `rejected`
- JS:

```js
await api(`/books/${bookId}/reject`, { method: "PATCH", token });
```

### `DELETE /books/{bookId}`

- Full URL: `/api/v1.0/books/{bookId}`
- Auth: `AdminOnlyPolicy`
- Route params:
  - `bookId`
- Returns: `204 No Content`
- What it does:
  - deletes a book directly from the DB
- Important rule:
  - cannot delete a book if any `BookCopy` references it
- JS:

```js
await api(`/books/${bookId}`, { method: "DELETE", token });
```

### `GET /books/{bookId}`

- Full URL: `/api/v1.0/books/{bookId}`
- Auth: `ActiveStudentPolicy`
- Route params:
  - `bookId`
- Returns: `200 OK` with `BookDto`
- What it does:
  - returns full book record
- Integrates with:
  - `GET /books/{bookId}/copies`
- JS:

```js
await api(`/books/${bookId}`, { token });
```

### `GET /books`

- Full URL: `/api/v1.0/books`
- Auth: `ActiveStudentPolicy`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `title`, `publisher`, `author`
  - `sortDirection`: `asc` or `desc`
  - `categories`: list of `BookCategory`
  - `statuses`: list of `BookStatus`
- Returns: `200 OK` with paged `BookListItemDto`
- Search behavior:
  - searches normalized `title`
  - searches normalized `isbn`
  - searches normalized `publisher`
  - searches normalized `author`
- Filter behavior:
  - category filter uses "contains all selected categories" logic
- Use this when:
  - catalog browse/search page loads
- Integrates with:
  - `GET /books/{bookId}`
  - `GET /books/{bookId}/copies`
- JS:

```js
await api("/books?page=1&pageSize=20&searchTerm=clean&sortColumn=title&sortDirection=asc", { token });
```

---

## 7.5 Book Copies

### `POST /students/me/books/{bookId}/copies`

- Full URL: `/api/v1.0/students/me/books/{bookId}/copies`
- Auth: `StudentOnlyPolicy`
- Route params:
  - `bookId`
- Body:
  - `condition`: `new`, `likeNew`, `acceptable`, or `poor`
- Returns: `201 Created` with `BookCopyDtoWithBookDetails`
- What it does:
  - resolves current user's student id
  - creates a book copy owned by that student
- Important rules:
  - target book must exist and be in status `available`
  - owner student must exist and be in state `active`
- Use this when:
  - student wants to register a physical copy they own
- Integrates with:
  - `POST /students/me/books/copies/{bookCopyId}/list`
- JS:

```js
await api(`/students/me/books/${bookId}/copies`, {
  method: "POST",
  token,
  body: { condition }
});
```

### `PATCH /books/copies/{bookCopyId}`

- Full URL: `/api/v1.0/books/copies/{bookCopyId}`
- Auth: `AdminOnlyPolicy`
- Route params:
  - `bookCopyId`
- Body:
  - `condition`
- Returns: `204 No Content`
- What it does:
  - updates copy condition only
- JS:

```js
await api(`/books/copies/${bookCopyId}`, {
  method: "PATCH",
  token,
  body: { condition }
});
```

### `PATCH /books/copies/{bookCopyId}/available`

- Full URL: `/api/v1.0/books/copies/{bookCopyId}/available`
- Auth: `StudentOwnerOfBookCopyPolicy`
- Route params:
  - `bookCopyId`
- Returns: `204 No Content`
- What it does:
  - marks copy available if state transition allows it
- JS:

```js
await api(`/books/copies/${bookCopyId}/available`, { method: "PATCH", token });
```

### `PATCH /books/copies/{bookCopyId}/unavailable`

- Full URL: `/api/v1.0/books/copies/{bookCopyId}/unavailable`
- Auth: `StudentOwnerOfBookCopyPolicy`
- Route params:
  - `bookCopyId`
- Returns: `204 No Content`
- What it does:
  - marks copy unavailable
- Important rule:
  - fails if the copy is currently used by a lending list record in `available`, `reserved`, or `borrowed`
- JS:

```js
await api(`/books/copies/${bookCopyId}/unavailable`, { method: "PATCH", token });
```

### `GET /books/copies/{bookCopyId}`

- Full URL: `/api/v1.0/books/copies/{bookCopyId}`
- Auth: `ActiveStudentPolicy`
- Route params:
  - `bookCopyId`
- Returns: `200 OK` with `BookCopyDtoWithBookDetails`
- What it does:
  - returns one book copy with book metadata and `isListed`
- Use this when:
  - copy details page opens
- JS:

```js
await api(`/books/copies/${bookCopyId}`, { token });
```

### `GET /books/{bookId}/copies`

- Full URL: `/api/v1.0/books/{bookId}/copies`
- Auth: `ActiveStudentPolicy`
- Route params:
  - `bookId`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `bookTitle`, `ownerName`
  - `sortDirection`
  - `conditions`: list of `BookCopyCondition`
  - `states`: list of `BookCopyState`
- Returns: `200 OK` with paged `BookCopyListItemDto`
- Search behavior:
  - searches book `title`
  - searches owner `name`
- Filters:
  - implicitly filters by the route `bookId`
- Use this when:
  - user opens a book detail page and wants available/owned copies
- JS:

```js
await api(`/books/${bookId}/copies?page=1&pageSize=20&sortColumn=ownerName&sortDirection=asc`, { token });
```

### `GET /students/{studentId}/books/copies`

- Full URL: `/api/v1.0/students/{studentId}/books/copies`
- Auth: `ActiveStudentPolicy`
- Route params:
  - `studentId`
- Query params:
  - same as `GET /books/{bookId}/copies`
- Returns: `200 OK` with paged `BookCopyListItemDto`
- Search behavior:
  - searches book `title`
  - searches owner `name`
- Filters:
  - implicitly filters by the route `studentId` as owner
- Use this when:
  - frontend shows all copies owned by a student
- JS:

```js
await api(`/students/${studentId}/books/copies?page=1&pageSize=20`, { token });
```

### `GET /books/copies`

- Full URL: `/api/v1.0/books/copies`
- Auth: `ActiveStudentPolicy`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `bookTitle`, `ownerName`
  - `sortDirection`
  - `conditions`
  - `states`
- Returns: `200 OK` with paged `BookCopyListItemDto`
- Search behavior:
  - searches book `title`
  - searches owner `name`
- Use this when:
  - global browse across all copies
- JS:

```js
await api("/books/copies?page=1&pageSize=20&states=available", { token });
```

### `POST /students/me/books/copies/{bookCopyId}/list`

- Full URL: `/api/v1.0/students/me/books/copies/{bookCopyId}/list`
- Auth: `StudentOwnerOfBookCopyPolicy`
- Route params:
  - `bookCopyId`
- Query params:
  - `borrowingDurationInDays`: integer between `7` and `30`
- Returns: `201 Created` with `LendingListRecordDto`
- What it does:
  - creates a lending list record for the owned book copy
  - current cost is always the default `1` point
  - lending record expiration is now + `30` days
- Important rules:
  - book copy must be `available`
  - book copy cannot already have an active `available` lending record
- Use this when:
  - owner wants to list a copy for borrowing
- Integrates with:
  - `GET /lendinglist`
  - `POST /lendinglist/{lendingListRecordId}/request`
- JS:

```js
await api(`/students/me/books/copies/${bookCopyId}/list?borrowingDurationInDays=${borrowingDurationInDays}`, {
  method: "POST",
  token
});
```

---

## 7.6 Lending List

### `GET /lendinglist/{lendingListRecordId}`

- Full URL: `/api/v1.0/lendinglist/{lendingListRecordId}`
- Auth: `ActiveStudentPolicy`
- Route params:
  - `lendingListRecordId`
- Returns: `200 OK` with `LendingListRecordDto`
- What it does:
  - returns one lending list record
- JS:

```js
await api(`/lendinglist/${lendingListRecordId}`, { token });
```

### `GET /lendinglist`

- Full URL: `/api/v1.0/lendinglist`
- Auth: `ActiveStudentPolicy`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `cost`, `borrowingDuration`, `expirationDate`, `state`
  - `sortDirection`
  - `bookCopyId`
  - `bookId`
  - `ownerId`: optional filter by book copy owner student id
  - `states`: list of `LendingListRecordState`
- Returns: `200 OK` with paged `LendingListRecordListItemDto`
- Search behavior:
  - searches only the related book `title`
- Use this when:
  - browse all currently listed lending opportunities
- Integrates with:
  - `POST /lendinglist/{id}/request`
- JS:

```js
await api("/lendinglist?page=1&pageSize=20&states=available", { token });
```

### `POST /lendinglist/{lendingListRecordId}/request`

- Full URL: `/api/v1.0/lendinglist/{lendingListRecordId}/request`
- Auth: `StudentOnlyPolicy`
- Route params:
  - `lendingListRecordId`
- Returns: `201 Created` with `BorrowingRequestDto`
- What it does:
  - resolves current user to student
  - creates a borrowing request in `pending`
  - deducts points from the borrower equal to the lending record cost
  - sends an email notification to the owner via domain event
- Important rules:
  - lending record must be `available`
  - student cannot request their own copy
  - same student cannot create another `pending` or `accepted` request for the same lending record
- Important backend nuance:
  - this endpoint uses `StudentOnlyPolicy`, not `ActiveStudentPolicy`
  - handler also does not enforce active student state
  - so a confirmed non-active student can still create requests in current code
- Use this when:
  - borrower clicks "request this book"
- Integrates with:
  - incoming/outgoing borrowing request endpoints
- JS:

```js
await api(`/lendinglist/${lendingListRecordId}/request`, {
  method: "POST",
  token
});
```

### `GET /lendinglist/{lendingListRecordId}/contact-info`

- Full URL: `/api/v1.0/lendinglist/{lendingListRecordId}/contact-info`
- Auth: `StudentAcceptedForLendingListRecordPolicy`
- Route params:
  - `lendingListRecordId`
- Returns: `200 OK` with `StudentContactInformationDto`
- What it does:
  - returns the owner student's phone/telegram info
  - only works when the lending list record state is `reserved`
- Important:
  - allowed for admin, or a student with an accepted borrowing request for that lending list record
- JS:

```js
await api(`/lendinglist/${lendingListRecordId}/contact-info`, { token });
```

### `POST /lendinglist/{lendingListRecordId}/close`

- Full URL: `/api/v1.0/lendinglist/{lendingListRecordId}/close`
- Auth: `StudentOwnerOfLendingListRecordPolicy`
- Route params:
  - `lendingListRecordId`
- Returns: `204 No Content`
- What it does:
  - marks the record `closed`
  - prevents new borrowing requests through normal flow
- Use this when:
  - owner no longer wants the copy listed
- JS:

```js
await api(`/lendinglist/${lendingListRecordId}/close`, {
  method: "POST",
  token
});
```

---

## 7.7 Borrowing Requests

### `GET /borrowingrequests/{borrowingRequestId}`

- Full URL: `/api/v1.0/borrowingrequests/{borrowingRequestId}`
- Auth: `BorrowingRequestRelatedStudentPolicy`
- Route params:
  - `borrowingRequestId`
- Returns: `200 OK` with `BorrowingRequestDto`
- What it does:
  - returns one borrowing request
- Use this when:
  - request detail page opens
- JS:

```js
await api(`/borrowingrequests/${borrowingRequestId}`, { token });
```

### `GET /borrowingrequests`

- Full URL: `/api/v1.0/borrowingrequests`
- Auth: `AdminOnlyPolicy`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `expirationDate`, `state`, `borrowerName`, `bookTitle`
  - `sortDirection`
  - `borrowingStudentId`
  - `lendingListRecordId`
  - `lendingStudentId`
  - `states`
- Returns: `200 OK` with paged `BorrowingRequestListItemDto`
- Search behavior:
  - searches borrower `name`
  - searches book `title`
- Use this when:
  - admin needs global borrowing request review
- JS:

```js
await api("/borrowingrequests?page=1&pageSize=20&states=pending", { token });
```

### `GET /borrowingrequests/me/in`

- Full URL: `/api/v1.0/borrowingrequests/me/in`
- Auth: `ActiveStudentPolicy`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `expirationDate`, `state`, `borrowerName`, `bookTitle`
  - `sortDirection`
  - `borrowingStudentId`: optional extra filter
  - `lendingListRecordId`: optional extra filter
  - `states`
- Returns: `200 OK` with paged `BorrowingRequestListItemDto`
- What it does:
  - automatically sets `lendingStudentId` to current student id
- Use this when:
  - current owner views incoming requests on their listed copies
- Integrates with:
  - accept/reject/deliver endpoints
- JS:

```js
await api("/borrowingrequests/me/in?page=1&pageSize=20", { token });
```

### `GET /borrowingrequests/me/out`

- Full URL: `/api/v1.0/borrowingrequests/me/out`
- Auth: `ActiveStudentPolicy`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `expirationDate`, `state`, `borrowerName`, `bookTitle`
  - `sortDirection`
  - `lendingListRecordId`
  - `lendingStudentId`
  - `states`
- Returns: `200 OK` with paged `BorrowingRequestListItemDto`
- What it does:
  - automatically sets `borrowingStudentId` to current student id
- Use this when:
  - borrower views requests they created
- Integrates with:
  - cancel endpoint
- JS:

```js
await api("/borrowingrequests/me/out?page=1&pageSize=20", { token });
```

### `PATCH /borrowingrequests/{borrowingRequestId}/accept`

- Full URL: `/api/v1.0/borrowingrequests/{borrowingRequestId}/accept`
- Auth: `BorrowingRequestLendingStudentPolicy`
- Route params:
  - `borrowingRequestId`
- Returns: `204 No Content`
- What it does:
  - marks borrowing request `accepted`
  - marks the related lending list record `reserved`
  - sends an accepted email notification to the borrower via domain event
- Important rule:
  - only one accepted request can exist for the same lending record
- Use this when:
  - owner accepts a borrow request
- Integrates with:
  - `POST /borrowingrequests/{id}/deliver`
- JS:

```js
await api(`/borrowingrequests/${borrowingRequestId}/accept`, {
  method: "PATCH",
  token
});
```

### `PATCH /borrowingrequests/{borrowingRequestId}/reject`

- Full URL: `/api/v1.0/borrowingrequests/{borrowingRequestId}/reject`
- Auth: `BorrowingRequestLendingStudentPolicy`
- Route params:
  - `borrowingRequestId`
- Returns: `204 No Content`
- What it does:
  - marks request `rejected`
  - refunds the previously deducted points to the borrowing student
- Use this when:
  - owner declines request
- JS:

```js
await api(`/borrowingrequests/${borrowingRequestId}/reject`, {
  method: "PATCH",
  token
});
```

### `PATCH /borrowingrequests/{borrowingRequestId}/cancel`

- Full URL: `/api/v1.0/borrowingrequests/{borrowingRequestId}/cancel`
- Auth: `BorrowingRequestBorrowingStudentPolicy`
- Route params:
  - `borrowingRequestId`
- Returns: `204 No Content`
- What it does:
  - marks request `cancelled`
  - refunds the deducted points to the borrowing student
- Use this when:
  - borrower cancels their own request
- JS:

```js
await api(`/borrowingrequests/${borrowingRequestId}/cancel`, {
  method: "PATCH",
  token
});
```

### `POST /borrowingrequests/{borrowingRequestId}/otp`

- Full URL: `/api/v1.0/borrowingrequests/{borrowingRequestId}/otp`
- Auth: `BorrowingRequestLendingStudentPolicy`
- Route params:
  - `borrowingRequestId`
- Returns: `200 OK`
- What it does:
  - sends an OTP code to the lending student's email to confirm delivery
- Use this when:
  - owner wants to confirm delivery, call this first to receive the OTP
- Integrates with:
  - `POST /borrowingrequests/{id}/deliver`
- JS:

```js
await api(`/borrowingrequests/${borrowingRequestId}/otp`, {
  method: "POST",
  token
});
```

### `POST /borrowingrequests/{borrowingRequestId}/deliver`

- Full URL: `/api/v1.0/borrowingrequests/{borrowingRequestId}/deliver`
- Auth: `BorrowingRequestLendingStudentPolicy`
- Route params:
  - `borrowingRequestId`
- Body:
  - `otpCode`: the OTP code received via email from the `/otp` endpoint
- Returns: `200 OK` with `BorrowingTransactionDto`
- What it does:
  - verifies the OTP code first
  - creates borrowing transaction from an accepted request
  - sets expected return date to now + lending record borrowing duration
  - marks book copy `borrowed`
  - marks lending record `borrowed`
- Important rules:
  - borrowing request must already be `accepted`
  - OTP must be valid (call `POST /borrowingrequests/{id}/otp` first)
- Use this when:
  - owner confirms the handoff/delivery happened
- Integrates with:
  - borrowing transaction detail/return/lost endpoints
- JS:

```js
await api(`/borrowingrequests/${borrowingRequestId}/deliver`, {
  method: "POST",
  token,
  body: { otpCode }
});
```

---

## 7.8 Borrowing Transactions

### `GET /borrowingtransactions/{borrowingTransactionId}`

- Full URL: `/api/v1.0/borrowingtransactions/{borrowingTransactionId}`
- Auth: `BorrowingTransactionRelatedStudentPolicy`
- Route params:
  - `borrowingTransactionId`
- Returns: `200 OK` with `BorrowingTransactionDto`
- What it does:
  - returns one transaction
- Use this when:
  - current lender or borrower opens transaction detail
- JS:

```js
await api(`/borrowingtransactions/${borrowingTransactionId}`, { token });
```

### `GET /borrowingtransactions`

- Full URL: `/api/v1.0/borrowingtransactions`
- Auth: `AdminOnlyPolicy`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `expectedReturnDate`, `actualReturnDate`, `state`, `borrowerName`, `lenderName`, `bookTitle`
  - `sortDirection`
  - `borrowerStudentId`
  - `lenderStudentId`
  - `bookCopyId`
  - `borrowingRequestId`
  - `states`
- Returns: `200 OK` with paged `BorrowingTransactionListItemDto`
- Search behavior:
  - searches borrower `name`
  - searches lender `name`
  - searches book `title`
- JS:

```js
await api("/borrowingtransactions?page=1&pageSize=20&states=borrowed", { token });
```

### `GET /borrowingtransactions/me/in`

- Full URL: `/api/v1.0/borrowingtransactions/me/in`
- Auth: `ActiveStudentPolicy`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `expectedReturnDate`, `actualReturnDate`, `state`, `borrowerName`, `lenderName`, `bookTitle`
  - `sortDirection`
  - `borrowerStudentId`: optional extra filter
  - `bookCopyId`: optional extra filter
  - `borrowingRequestId`: optional extra filter
  - `states`
- Returns: `200 OK` with paged `BorrowingTransactionListItemDto`
- What it does:
  - automatically sets `lenderStudentId` to current student id
- Search behavior:
  - searches borrower `name`
  - searches lender `name`
  - searches book `title`
- Use this when:
  - current lender views incoming transactions on their lent copies
- JS:

```js
await api("/borrowingtransactions/me/in?page=1&pageSize=20", { token });
```

### `GET /borrowingtransactions/me/out`

- Full URL: `/api/v1.0/borrowingtransactions/me/out`
- Auth: `ActiveStudentPolicy`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `expectedReturnDate`, `actualReturnDate`, `state`, `borrowerName`, `lenderName`, `bookTitle`
  - `sortDirection`
  - `lenderStudentId`: optional extra filter
  - `bookCopyId`: optional extra filter
  - `borrowingRequestId`: optional extra filter
  - `states`
- Returns: `200 OK` with paged `BorrowingTransactionListItemDto`
- What it does:
  - automatically sets `borrowerStudentId` to current student id
- Search behavior:
  - searches borrower `name`
  - searches lender `name`
  - searches book `title`
- Use this when:
  - current borrower views transactions where they borrowed books
- JS:

```js
await api("/borrowingtransactions/me/out?page=1&pageSize=20", { token });
```

### `POST /borrowingtransactions/{borrowingTransactionId}/otp`

- Full URL: `/api/v1.0/borrowingtransactions/{borrowingTransactionId}/otp`
- Auth: `BorrowingTransactionBorrowingStudentPolicy`
- Route params:
  - `borrowingTransactionId`
- Returns: `200 OK`
- What it does:
  - sends an OTP code to the lender student's email to confirm returning
- Use this when:
  - borrower wants to return the book, call this first to receive the OTP
- Integrates with:
  - `PATCH /borrowingtransactions/{id}/return`
- JS:

```js
await api(`/borrowingtransactions/${borrowingTransactionId}/otp`, {
  method: "POST",
  token
});
```

### `PATCH /borrowingtransactions/{borrowingTransactionId}/return`

- Full URL: `/api/v1.0/borrowingtransactions/{borrowingTransactionId}/return`
- Auth: `BorrowingTransactionBorrowingStudentPolicy`
- Route params:
  - `borrowingTransactionId`
- Body:
  - `otpCode`: the OTP code received via email from the `/otp` endpoint
- Returns: `204 No Content`
- What it does:
  - verifies the OTP code first
  - marks transaction returned or overdue depending on current date vs expected return date
  - sets `actualReturnDate`
  - marks the book copy back to `available`
- Important rules:
  - OTP must be valid (call `POST /borrowingtransactions/{id}/otp` first)
- Use this when:
  - borrower returns the book
- JS:

```js
await api(`/borrowingtransactions/${borrowingTransactionId}/return`, {
  method: "PATCH",
  token,
  body: { otpCode }
});
```

### `PATCH /borrowingtransactions/{borrowingTransactionId}/lost`

- Full URL: `/api/v1.0/borrowingtransactions/{borrowingTransactionId}/lost`
- Auth: `BorrowingTransactionBorrowingStudentPolicy`
- Route params:
  - `borrowingTransactionId`
- Returns: `204 No Content`
- What it does:
  - marks transaction `lost`
  - marks the book copy `lost`
- Use this when:
  - borrower reports the copy as lost
- JS:

```js
await api(`/borrowingtransactions/${borrowingTransactionId}/lost`, {
  method: "PATCH",
  token
});
```

---

## 7.9 Images

### `GET /images/students/{studentId}`

- Full URL: `/api/v1.0/images/students/{studentId}`
- Auth: `RegisteredUserPolicy`
- Route params:
  - `studentId`
- Returns: image binary
- What it does:
  - resolves stored student photo filename
  - returns the image content
  - falls back to default student image if stored file is missing
- Use this when:
  - frontend needs a student's profile photo
- JS:

```js
const blob = await api(`/images/students/${studentId}`, { token });
const url = URL.createObjectURL(blob);
```

### `GET /images/books/{bookId}`

- Full URL: `/api/v1.0/images/books/{bookId}`
- Auth: `ActiveStudentPolicy`
- Route params:
  - `bookId`
- Returns: image binary
- What it does:
  - resolves stored book cover filename
  - returns image content
  - falls back to default cover image if stored file is missing
- Important note:
  - most book and book-copy DTOs already include `bookCoverImageUrl`
  - that URL is built from static files under `wwwroot/uploads/books`
  - frontend usually does not need this endpoint unless it specifically wants an authenticated image API route
- JS:

```js
const blob = await api(`/images/books/${bookId}`, { token });
const url = URL.createObjectURL(blob);
```

---

## 7.10 Borrowing Reviews

### `POST /borrowingtransactions/{borrowingTransactionId}/review`

- Full URL: `/api/v1.0/borrowingtransactions/{borrowingTransactionId}/review`
- Auth: `BorrowingTransactionLendingStudentPolicy`
- Route params:
  - `borrowingTransactionId`
- Body:
  - `description`: optional text review
  - `rating`: integer between `1` and `5`
- Returns: `201 Created` with `BorrowingReviewDto`
- What it does:
  - creates a borrowing review for the specified transaction
  - the reviewer is the lending student, the reviewed is the borrowing student
  - triggers a `BorrowingReviewCreatedEvent` that adjusts points based on the rating
- Important rules:
  - only one review per transaction
  - transaction must exist
- Use this when:
  - lender reviews the borrower after a transaction completes
- JS:

```js
await api(`/borrowingtransactions/${borrowingTransactionId}/review`, {
  method: "POST",
  token,
  body: { description, rating }
});
```

### `GET /borrowingreviews/{borrowingReviewId}`

- Full URL: `/api/v1.0/borrowingreviews/{borrowingReviewId}`
- Auth: `ActiveStudentPolicy`
- Route params:
  - `borrowingReviewId`
- Returns: `200 OK` with `BorrowingReviewDto`
- What it does:
  - returns one borrowing review
- JS:

```js
await api(`/borrowingreviews/${borrowingReviewId}`, { token });
```

### `GET /borrowingreviews`

- Full URL: `/api/v1.0/borrowingreviews`
- Auth: `ActiveStudentPolicy`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `rating`
  - `sortDirection`
  - `reviewerStudentId`
  - `reviewedStudentId`
  - `borrowingTransactionId`
- Returns: `200 OK` with paged `BorrowingReviewListItemDto`
- Search behavior:
  - if `searchTerm` is a valid GUID: searches `id`, `borrowingTransactionId`, `reviewerStudentId`, `reviewedStudentId`
  - otherwise: searches `description` text
- Use this when:
  - viewing reviews for a student or transaction
- JS:

```js
await api("/borrowingreviews?page=1&pageSize=20&reviewedStudentId=" + studentId, { token });
```

---

## 7.11 Chat

### `POST /chat/messages`

- Full URL: `/api/v1.0/chat/messages`
- Auth: `StudentOnlyPolicy`
- Body:
  - `receiverId`: student id of the message recipient
  - `content`: message text
- Returns: `201 Created` with `ChatMessageDto`
- What it does:
  - resolves current user to student
  - creates a chat group between the two students if one does not already exist
  - sends the message in that chat group
- Important rules:
  - sender and receiver must be different students
- Use this when:
  - student sends a direct message to another student
- JS:

```js
await api("/chat/messages", {
  method: "POST",
  token,
  body: { receiverId, content }
});
```

### `PATCH /chat/groups/{chatGroupId}/read`

- Full URL: `/api/v1.0/chat/groups/{chatGroupId}/read`
- Auth: `StudentOnlyPolicy`
- Route params:
  - `chatGroupId`
- Returns: `204 No Content`
- What it does:
  - marks all unread messages sent by the other participant as read for the current student
- Use this when:
  - student opens a chat conversation
- JS:

```js
await api(`/chat/groups/${chatGroupId}/read`, {
  method: "PATCH",
  token
});
```

### `GET /chat/groups/{chatGroupId}/messages`

- Full URL: `/api/v1.0/chat/groups/{chatGroupId}/messages`
- Auth: `ActiveStudentPolicy`
- Route params:
  - `chatGroupId`
- Query params:
  - `page`
  - `pageSize`
- Returns: `200 OK` with paged `ChatMessageDto`
- What it does:
  - returns paginated messages for the chat group, ordered by most recent first
  - current student must be a participant in the chat group
- Use this when:
  - loading chat history for a conversation
- JS:

```js
await api(`/chat/groups/${chatGroupId}/messages?page=1&pageSize=50`, { token });
```

### `GET /chat/groups`

- Full URL: `/api/v1.0/chat/groups`
- Auth: `ActiveStudentPolicy`
- Query params:
  - `page`
  - `pageSize`
- Returns: `200 OK` with paged `ChatGroupListItemDto`
- What it does:
  - returns all chat groups the current student participates in
  - includes the name and photo filename of the other participant
  - ordered by most recent first
- Use this when:
  - showing the chat inbox/conversation list
- JS:

```js
await api("/chat/groups?page=1&pageSize=20", { token });
```

---

## 7.12 Notifications

### `PATCH /notifications/read`

- Full URL: `/api/v1.0/notifications/read`
- Auth: `ActiveStudentPolicy`
- Body:
  - `maxTime`: ISO 8601 datetime cutoff; all unread notifications created at or before this time are marked as read
- Returns: `204 No Content`
- What it does:
  - marks all unread notifications for the current student as read up to the specified cutoff time
- Use this when:
  - student opens notification panel and frontend marks visible notifications as read
- JS:

```js
await api("/notifications/read", {
  method: "PATCH",
  token,
  body: { maxTime: new Date().toISOString() }
});
```

### `GET /notifications/{notificationId}`

- Full URL: `/api/v1.0/notifications/{notificationId}`
- Auth: `ActiveStudentPolicy`
- Route params:
  - `notificationId`
- Returns: `200 OK` with `NotificationDto`
- What it does:
  - returns one notification belonging to the current student
- JS:

```js
await api(`/notifications/${notificationId}`, { token });
```

### `GET /notifications`

- Full URL: `/api/v1.0/notifications`
- Auth: `ActiveStudentPolicy`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `title`, `type`, `isRead`
  - `sortDirection`
  - `isRead`: `true` or `false`
  - `types`: list of `NotificationType`
- Returns: `200 OK` with paged `NotificationListItemDto`
- Search behavior:
  - searches notification `title`
  - searches notification `message`
- What it does:
  - returns paginated notifications for the current student
- Use this when:
  - displaying the notification list/feed
- JS:

```js
await api("/notifications?page=1&pageSize=20&isRead=false", { token });
```

---

## 7.13 Point Transactions (Admin)

### `GET /pointtransactions/{pointTransactionId}`

- Full URL: `/api/v1.0/pointtransactions/{pointTransactionId}`
- Auth: `AdminOnlyPolicy`
- Route params:
  - `pointTransactionId`
- Returns: `200 OK` with `PointTransactionDto`
- What it does:
  - returns one point transaction
- JS:

```js
await api(`/pointtransactions/${pointTransactionId}`, { token });
```

### `GET /pointtransactions`

- Full URL: `/api/v1.0/pointtransactions`
- Auth: `AdminOnlyPolicy`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `points`, `reason`, `studentName`
  - `sortDirection`
  - `studentId`
  - `borrowingReviewId`
  - `reasons`: list of `PointTransactionReason`
- Returns: `200 OK` with paged `PointTransactionListItemDto`
- Search behavior:
  - searches student `name`
- Use this when:
  - admin reviews point activity history
- JS:

```js
await api("/pointtransactions?page=1&pageSize=20&studentId=" + studentId, { token });
```

---

## 7.14 Borrowing Transaction Events (Admin)

### `GET /borrowingtransactionevents/{borrowingTransactionEventId}`

- Full URL: `/api/v1.0/borrowingtransactionevents/{borrowingTransactionEventId}`
- Auth: `AdminOnlyPolicy`
- Route params:
  - `borrowingTransactionEventId`
- Returns: `200 OK` with `BorrowingTransactionEventDto`
- What it does:
  - returns one borrowing transaction event (state change record)
- JS:

```js
await api(`/borrowingtransactionevents/${borrowingTransactionEventId}`, { token });
```

### `GET /borrowingtransactionevents`

- Full URL: `/api/v1.0/borrowingtransactionevents`
- Auth: `AdminOnlyPolicy`
- Query params:
  - `page`
  - `pageSize`
  - `searchTerm`
  - `sortColumn`: `createdAt`, `updatedAt`, `state`
  - `sortDirection`
  - `borrowingTransactionId`
  - `states`: list of `BorrowingTransactionState`
- Returns: `200 OK` with paged `BorrowingTransactionEventListItemDto`
- Search behavior:
  - if `searchTerm` is a valid GUID: searches `id` and `borrowingTransactionId`
  - otherwise: no text search applies
- Use this when:
  - admin reviews the state change history of borrowing transactions
- JS:

```js
await api("/borrowingtransactionevents?page=1&pageSize=20&borrowingTransactionId=" + transactionId, { token });
```

## 8. Frontend Flows

### Case 1: Create account and reach normal student usage

1. Call `POST /students`
2. Backend automatically sends confirmation email
3. Frontend confirm-email page calls `POST /identity/confirm-email`
4. User logs in with `POST /identity/token`
5. Admin reviews via `GET /students`
6. Admin calls `PATCH /students/{studentId}/approve`
7. Admin calls `PATCH /students/{studentId}/activate`
8. Student can now use all `ActiveStudentPolicy` endpoints

### Case 2: Resend email confirmation

1. Call `POST /identity/users/send-email-confirmation?email=...`
2. User opens link from email
3. Frontend calls `POST /identity/confirm-email`
4. User logs in with `POST /identity/token`

### Case 3: Forgot password

1. Call `POST /identity/users/send-reset-password?email=...`
2. User opens reset-password frontend link from email
3. Frontend submits `POST /identity/reset-password`
4. User logs in again with `POST /identity/token`

### Case 4: Student creates a book and later lends a copy

1. Active student calls `POST /books`
2. Admin reviews and calls `PATCH /books/{bookId}/available`
3. Student calls `POST /students/me/books/{bookId}/copies`
4. Student calls `POST /students/me/books/copies/{bookCopyId}/list?borrowingDurationInDays=...`
5. Other active students discover it through `GET /lendinglist`

### Case 5: Borrow a listed book

1. Borrower browses `GET /lendinglist`
2. Borrower calls `POST /lendinglist/{lendingListRecordId}/request`
3. Owner views `GET /borrowingrequests/me/in`
4. Owner calls either:
   - `PATCH /borrowingrequests/{id}/accept`, or
   - `PATCH /borrowingrequests/{id}/reject`
5. If accepted, owner requests delivery OTP via `POST /borrowingrequests/{id}/otp`
6. Owner confirms delivery with borrower's OTP via `POST /borrowingrequests/{id}/deliver`
7. Borrower uses returned `BorrowingTransactionDto.id` for later transaction actions

### Case 6: Return a borrowed book

1. Borrower requests return OTP via `POST /borrowingtransactions/{transactionId}/otp`
2. Borrower later calls `PATCH /borrowingtransactions/{transactionId}/return` passing the OTP received by the owner
3. Backend:
   - marks transaction `returned` or `overdue`
   - sets `actualReturnDate`
   - marks copy back to `available`
4. Lender reviews borrower via `POST /borrowingtransactions/{transactionId}/review`

### Case 7: Report book copy lost

1. Owner delivers by calling `POST /borrowingrequests/{id}/deliver`
2. Borrower calls `PATCH /borrowingtransactions/{transactionId}/lost`
3. Backend marks both transaction and copy as `lost`

## 9. Important Frontend Caveats

### 1. Default page size is `1`

If frontend forgets to send `pageSize`, list endpoints only return one item.

### 2. Student images and book images are handled differently

- Student photos are served through the authenticated image endpoint
- Book cover URLs are usually already returned as static URLs in DTOs

### 3. Account creation does not make a usable marketplace student immediately

Creation only gives:

- identity account with role `student`
- student state `pending`
- email confirmation email

Marketplace access still needs:

- email confirmation
- admin approval
- admin activation

### 4. Borrowing requests deduct points immediately

On `POST /lendinglist/{id}/request`:

- borrower points are deducted immediately
- reject/cancel/expire paths refund those points

### 5. Chat System Considerations

The chat system establishes a 1-to-1 chat group automatically when the first message is sent using `POST /chat/messages`.
Unread message counts are managed on a per-group basis and must be cleared explicitly using `PATCH /chat/groups/{chatGroupId}/read`.

### 6. CORS in checked-in config does not currently include the frontend URL from email links

Current checked-in settings:

- API launch URL: `http://localhost:7240`
- Email links target frontend: `http://localhost:3000`
- CORS allowed origins in `appsettings.json`: `http://localhost:7240` and `https://localhost:7241`

If the frontend really runs on `http://localhost:3000`, browser calls may fail until backend config is updated.

### 7. Unused policies exist

Defined but not currently attached to controller actions:

- `ActiveUserPolicy`
- `RegisteredUserOwnershipPolicy`

These should not be assumed to protect any current route.
