# Core Agent Log

## [2026-03-09] Phase 0: Подготовка scaffolding

### Что сделано
- Удалены 6 шаблонных .cs файлов и их .meta: RuntimeExample, EditorExample, RuntimeExampleTest, EditorExampleTest, AssemblyInfo (Runtime и Editor)
- `GameBackend.asmdef` — добавлен `rootNamespace: "GameBackend"`
- `GameBackend.Editor.asmdef` — добавлен `rootNamespace: "GameBackend.Editor"`, добавлена ссылка на `GameBackend`
- `GameBackend.Tests.asmdef` — добавлена ссылка `Unity.Newtonsoft.Json`
- `GameBackend.Editor.Tests.asmdef` — добавлена ссылка `Unity.Newtonsoft.Json`
- `package.json` — добавлена зависимость `com.unity.nuget.newtonsoft-json: 3.2.1`
- Создана структура папок: Core/ (Interfaces, Models, Exceptions), Transport/ (Interfaces), Api/, WebSocket/
- Созданы лог-файлы для 6 агентов в docs/agents/

### Архитектурные решения
- `rootNamespace` в .asmdef гарантирует, что Unity Editor будет генерировать новые файлы с правильным namespace без ручного исправления
- `GameBackend.Editor.asmdef` ссылается на `GameBackend` чтобы Editor-код мог использовать Core-типы
- `Unity.Newtonsoft.Json` добавлен в тестовые .asmdef для десериализации DTO в тестах
- `com.unity.nuget.newtonsoft-json` в package.json обеспечивает автоматическую установку зависимости при импорте пакета
- Папки Core/, Transport/, Api/, WebSocket/ — логическое разделение слоёв внутри единого .asmdef (физическое разделение планируется в Phase 5)

### Зависимости
- Ничего не блокирует Phase 1

### Тесты
- Статус: 0 тестов, 0 ошибок компиляции

## [2026-03-09] Phase 1: Core контракты + Transport интерфейсы

### Что сделано
- Создано 29 файлов контрактов (12 интерфейсов + 11 моделей + 1 исключение + 1 конфигурация + 4 transport интерфейса)
- Удалены .gitkeep из папок с .cs файлами

### Интерфейсы (`Runtime/Core/Interfaces/`)
- `IAuthClient.cs` — 13 методов (8 из спеки + 5 из REST API: ListSessions, RevokeSession, RevokeAllSessions, LinkProvider, UnlinkProvider)
- `IAccountClient.cs` — 5 методов, `ILeaderboardClient.cs` — 5, `IChatClient.cs` — 5, `IStorageClient.cs` — 5
- `IFriendsClient.cs` — 10 методов, `IGroupsClient.cs` — 15, `INotificationClient.cs` — 4, `ITournamentClient.cs` — 4
- `IGameClient.cs` — фасад (9 интерфейсов + Session, IsAuthenticated, RestoreSession, ClearSession, GlobalRetryConfiguration, NewSocket)
- `IGameSession.cs` — 11 свойств/методов + static GameSession.Restore
- `IGameSocket.cs` — IDisposable, IsConnected, 14 events, 3 methods

### Модели (`Runtime/Core/Models/` — 11 файлов)
- AuthModels.cs (AuthResponse, AuthUser, SessionInfo), AccountModels.cs (Account с полями бана, User)
- LeaderboardModels.cs, ChatModels.cs, StorageModels.cs, FriendModels.cs, GroupModels.cs
- NotificationModels.cs, TournamentModels.cs, WebSocketModels.cs, HttpModels.cs

### Исключения и конфигурация
- `GameApiException.cs` — StatusCode, ErrorMessage, ErrorCode (nullable), RequestId (nullable)
- `RetryConfiguration.cs` — BaseDelayMs, MaxRetries, RetryListener

### Transport (`Runtime/Transport/Interfaces/` — 4 файла)
- IHttpAdapter, IWebSocketAdapter, ISerializer, ITokenStorage

### Архитектурные решения
- Модели сгруппированы по домену (1 файл = связанные DTO) — уменьшает количество файлов с ~35 до 11
- Account расширен полями бана (is_banned, ban_reason, banned_at, banned_by) — реальный API их возвращает, полезно для клиента
- UpdatedAt в Account — `DateTime?` (nullable), т.к. API может вернуть null
- Коллекции в моделях: `IReadOnlyList<T>` (поддерживает .Count), входные параметры интерфейсов: `IEnumerable<T>`
- HttpModels без [JsonProperty] — internal transport DTO, не JSON от сервера
- GameSession.Restore — static метод с throw NotImplementedException (реализация в Api слое)

### Зависимости
- Все интерфейсы готовы для Phase 2 (AUTH: AuthService, GameSession, HttpPipeline, TokenManager)
- Модели готовы для Phase 2+ (TEST: тесты десериализации)

### Тесты
- 12 тестов десериализации написаны TEST агентом (параллельно)
- Статус: ожидает проверки компиляции в Unity Editor
