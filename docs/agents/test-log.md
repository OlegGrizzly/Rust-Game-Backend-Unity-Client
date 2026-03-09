# Test Agent Log

## [2026-03-09] Phase 1: Тесты десериализации моделей

### Что сделано
- Создано 4 файла тестов (12 тест-методов) в `Tests/Editor/Core/`
- Создана структура папок: `Tests/Editor/Core/Models/`, `Tests/Editor/Core/Exceptions/`

### Файлы тестов
- `Tests/Editor/Core/Models/AuthModelsTests.cs` — 3 теста (AuthResponse, AuthUser, SessionInfo)
- `Tests/Editor/Core/Models/AccountModelsTests.cs` — 3 теста (Account all fields, nullable fields, User)
- `Tests/Editor/Core/Models/LeaderboardModelsTests.cs` — 3 теста (LeaderboardRecord, null metadata, LeaderboardRecordList)
- `Tests/Editor/Core/Exceptions/GameApiExceptionTests.cs` — 3 теста (constructor, inheritance, nullable fields)

### Результаты curl-проверки
- Curl-проверка не проводилась (Phase 1 — контракты, нет сетевых вызовов)
- JSON в тестах взят из `docs/api/rest.md` (реальные ответы API)

### Архитектурные решения
- Тесты используют `JsonConvert.DeserializeObject<T>` напрямую — проверяют только [JsonProperty] маппинг
- JSON в тестах максимально приближен к реальным ответам API (взят из rest.md)
- Обнаружено расхождение: Account в SDK спеке не имеет полей бана (is_banned, ban_reason, banned_at, banned_by), но реальный API их возвращает → модель расширена

### Зависимости
- Зависит от CORE: модели (`GameBackend.Core.Models`), исключения (`GameBackend.Core.Exceptions`)
- Блокирует Phase 2: тесты Auth (MockHttpAdapter, AuthServiceTests)

### Тесты
- 12 тестов написаны
- Статус: 12/12 GREEN
- Покрытие: AuthResponse, AuthUser, SessionInfo, Account, User, LeaderboardRecord, LeaderboardRecordList, GameApiException

## [2026-03-09] Phase 2: Тесты Auth + моки

### Что сделано
- Создано 5 файлов (3 мока + 2 файла тестов, 38 тест-методов)

### Моки (`Tests/Editor/Mocks/`)
- `MockHttpAdapter.cs` — Queue-based IHttpAdapter, EnqueueResponse/EnqueueException, SentRequests для assertion
- `MockTokenStorage.cs` — In-memory ITokenStorage, ClearCallCount для assertion
- `TestJwtHelper.cs` — Создание тестовых JWT с base64url encoding, BuildAuthResponseJson(), BuildErrorJson()

### Тесты (`Tests/Editor/Auth/`)
- `AuthServiceTests.cs` — 22 теста:
  - Authenticate: Username, Email, Device (3 провайдера)
  - Login: Username, Email, Device (3 провайдера)
  - RefreshSession, Logout
  - ListSessions, RevokeSession, RevokeAllSessions
  - LinkProvider, UnlinkProvider
  - Error handling: 400, 401, 403, 409
  - Auto-refresh: 401 → refresh → retry, 401 → refresh fails → throw + clear
  - 403 → throw without retry
- `GameSessionTests.cs` — 10 тестов:
  - Constructor: UserId, Username, DisplayName, AuthToken, RefreshToken
  - IsExpired: future (false), past (true)
  - HasExpired: before expiry (false), after expiry (true)
  - ExpireTime: correct unix timestamp

### Архитектурные решения
- Тесты используют реальный NewtonsoftSerializer — проверяют полный стек сериализации
- MockHttpAdapter позволяет проверять отправленные запросы (URL, method, headers, body)
- TestJwtHelper создаёт реалистичные JWT (header.payload.signature) с настраиваемыми claims
- async Task вместо async void для NUnit совместимости

### Тесты
- 38 новых тестов + 12 Phase 1 = 50 total
- Статус: 50/50 GREEN, 0 failed, 0 skipped

## [2026-03-09] Phase 3: Тесты Account + Leaderboard

### Что сделано
- Создано 2 файла тестов (16 тест-методов) в `Tests/Editor/Api/`
- Обновлены тесты десериализации LeaderboardModels

### Файлы
- `Tests/Editor/Api/AccountServiceTests.cs` — 8 тестов (GET/PUT/DELETE account, GET user, POST batch, 401, 404)
- `Tests/Editor/Api/LeaderboardServiceTests.cs` — 8 тестов (POST record, GET top, GET around, DELETE, POST batch, 401, 404)

### Исправления после первого прогона
- 401-тесты падали: HttpPipeline при 401 пытается auto-refresh (второй HTTP запрос), но MockHttpAdapter имел только 1 ответ в очереди → добавлен второй EnqueueResponse(401) для refresh

### Тесты
- 17 новых тестов + 50 Phase 2 = 67 total
- Статус: 67/67 GREEN
