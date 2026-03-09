# Auth Agent Log

## [2026-03-09] Phase 2: Transport + Auth реализация

### Что сделано
- Создано 8 новых файлов, модифицировано 3 существующих
- Все 50 тестов GREEN (12 Phase 1 + 38 Phase 2)

### Созданные файлы
- `Runtime/Api/GameSessionImpl.cs` — IGameSession, JWT base64url decode (средняя часть), извлечение sub/username/display_name/exp
- `Runtime/Api/TokenManager.cs` — управление сессией, LoadFromStorage, SetSession, Clear, concurrent refresh через SemaphoreSlim(1,1)
- `Runtime/Api/HttpPipeline.cs` — auth header → send → 401 auto-refresh → retry → error mapping → deserialize
- `Runtime/Api/Pipeline/RetryMiddleware.cs` — exponential backoff с jitter, 429 Retry-After header
- `Runtime/Api/Services/AuthService.cs` — 13 методов IAuthClient (3 Authenticate + 3 Login + Refresh + Logout + List/Revoke/RevokeAll + Link/Unlink)
- `Runtime/Api/Models/AuthRequestModels.cs` — request body модели с [JsonProperty]
- `Runtime/Api/GameClient.cs` — фасад IGameClient, auth делегирует в AuthService, остальное NotImplementedException
- `Samples/Auth/AuthExample.cs` — MonoBehaviour с [ContextMenu] для всех 13 auth методов

### Модифицированные файлы
- `Runtime/Transport/Serialization/NewtonsoftSerializer.cs` — namespace изменён с GameBackend.Transport.Serialization на GameBackend.Transport
- `Runtime/Core/Interfaces/IGameSession.cs` — удалён static class GameSession (реализация перенесена в Api слой)
- `Tests/Editor/Auth/AuthServiceTests.cs` — async void → async Task (NUnit совместимость)

### Архитектурные решения
- GameSessionImpl вместо GameSession — избежание конфликта с удалённым static class в Core
- JWT decode без криптоверификации — только base64url decode payload для извлечения claims
- HttpPipeline: при 401 → один retry после auto-refresh, при повторном 401 → throw + clear tokens
- TokenManager: SemaphoreSlim(1,1) для concurrent safety при refresh
- AuthService: baseUrl передаётся в конструктор, формирует полные URL для endpoints

### Проблемы и решения
- Namespace mismatch NewtonsoftSerializer: CORE создал в GameBackend.Transport.Serialization, тесты ожидали GameBackend.Transport → исправлено
- async void в NUnit: TEST написал тесты с async void, NUnit не поддерживает → исправлено на async Task
- Static GameSession в Core нарушал Dependency Rule → удалён, реализация только в Api слое

### Тесты
- 50 тестов GREEN, 0 failed, 0 skipped
