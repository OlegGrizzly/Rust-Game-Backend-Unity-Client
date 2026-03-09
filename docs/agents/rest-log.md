# REST Agent Log

## [2026-03-09] Phase 3: AccountService + LeaderboardService

### Что сделано
- Создано 6 новых файлов, обновлён GameClient.cs

### Файлы
- `Runtime/Api/Services/AccountService.cs` — 5 методов IAccountClient
- `Runtime/Api/Services/LeaderboardService.cs` — 5 методов ILeaderboardClient
- `Runtime/Api/Models/AccountRequestModels.cs` — UpdateAccountRequest, BatchUserIdsRequest
- `Runtime/Api/Models/LeaderboardRequestModels.cs` — WriteRecordRequest, BatchRecordIdsRequest
- `Samples/Account/AccountExample.cs` — MonoBehaviour с ContextMenu
- `Samples/Leaderboard/LeaderboardExample.cs` — MonoBehaviour с ContextMenu
- `Runtime/Api/GameClient.cs` — Account и Leaderboard заглушки заменены на делегирование

### Endpoint маппинг

#### Account
- GetAccountAsync → GET /api/account/me
- UpdateAccountAsync → PUT /api/account/me (только ненулевые поля в body)
- DeleteAccountAsync → DELETE /api/account/me
- GetUserAsync → GET /api/account/{userId}
- GetUsersAsync → POST /api/account/batch

#### Leaderboard
- WriteLeaderboardRecordAsync → POST /api/leaderboard/leaderboards/{id}/record
- ListLeaderboardRecordsAsync → GET /api/leaderboard/leaderboards/{id}?limit={N}
- ListLeaderboardRecordsAroundUserAsync → GET /api/leaderboard/leaderboards/{id}/around/{userId}
- DeleteLeaderboardRecordAsync → DELETE /api/leaderboard/leaderboards/{id}/record
- GetLeaderboardRecordsByIdsAsync → POST /api/leaderboard/leaderboards/{id}/records/batch

### Особенности
- API Leaderboard возвращает массив `[...]`, сервис оборачивает в LeaderboardRecordList
- UpdateAccountRequest использует NullValueHandling.Ignore — только заданные поля отправляются
- WriteRecordRequest: subscore с DefaultValueHandling.Ignore (не отправляется если 0)

### Тесты
- 67/67 GREEN (50 Phase 2 + 17 Phase 3)
