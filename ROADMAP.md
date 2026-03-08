# TDD Roadmap: Unity SDK для Game Backend

## Context

Проект — чистый scaffolding (шаблон StansAssets). Реального кода SDK нет. Нужно реализовать полный Unity SDK клиент для микросервисного Rust бэкенда: REST API (9 доменов) + WebSocket (чат, presence, push). SDK — низкоуровневый транспортный слой без UI/кэширования.

Спецификации бэкенда: `docs/sdk/unity-sdk-readme.md`, `docs/sdk/client-protocol.md`, `docs/api/rest.md`, `docs/api/websocket.md` в `/Users/olegsedyh/Unity Projects/Rust-Game-Backend/`.

## Стратегическое решение: .asmdef

**НЕ разделять на 4 .asmdef сразу.** Оставить один `GameBackend.asmdef` до Phase 5. Логическое разделение через папки (Core/, Transport/, Api/, WebSocket/). Физическое разделение — после стабилизации контрактов.

---

## Phase 0: Подготовка scaffolding

**Агент:** CORE | **Deliverable:** Проект компилируется, 0 тестов, 0 ошибок

| Действие | Файл |
|----------|------|
| Удалить | `Runtime/RuntimeExample.cs`, `Runtime/AssemblyInfo.cs` |
| Удалить | `Editor/EditorExample.cs`, `Editor/AssemblyInfo.cs` |
| Удалить | `Tests/Runtime/RuntimeExampleTest.cs`, `Tests/Editor/EditorExampleTest.cs` |
| Изменить | `Runtime/GameBackend.asmdef` — добавить `rootNamespace: "GameBackend"` |
| Изменить | `Tests/Runtime/GameBackend.Tests.asmdef` — добавить `Unity.Newtonsoft.Json` |
| Создать | Структуру папок: `Runtime/Core/{Interfaces,Models,Exceptions}`, `Runtime/Transport/Interfaces`, `Runtime/Api/`, `Runtime/WebSocket/` |

---

## Phase 1: Core контракты + Transport интерфейсы

**Агенты:** TEST (тесты десериализации) + CORE (контракты) — параллельно
**Deliverable:** ~25 файлов контрактов, ~10 тестов (Red — не компилируются без реализации)

### TEST пишет тесты моделей:
- `Tests/Editor/Core/Models/AuthResponseTests.cs` — десериализация snake_case JSON
- `Tests/Editor/Core/Models/AccountTests.cs`, `LeaderboardRecordTests.cs`
- `Tests/Editor/Core/Exceptions/GameApiExceptionTests.cs`

### CORE создает контракты:

**Интерфейсы** (`Runtime/Core/Interfaces/`):
- `IAuthClient.cs` — 8 методов (3 register + 3 login + refresh + logout)
- `IAccountClient.cs` — 5 методов (get/update/delete account, get user, batch)
- `ILeaderboardClient.cs` — 5 методов (write/list/around/delete/batch)
- `IChatClient.cs` — 5 методов (channels, messages, unread, mark read)
- `IStorageClient.cs` — 5 методов (write/read/delete/search/count)
- `IFriendsClient.cs` — 10 методов (add/accept/reject/remove/block/unblock + lists)
- `IGroupsClient.cs` — 14 методов (CRUD + join/leave/members/roles/search)
- `INotificationClient.cs` — 4 метода (list/unread/read/delete)
- `ITournamentClient.cs` — 4 метода (list/get/join/record)
- `IGameClient.cs` — фасад (наследует все 9 + Session, RestoreSession, ClearSession, NewSocket)
- `IGameSession.cs` — AuthToken, RefreshToken, UserId, IsExpired
- `IGameSocket.cs` — IsConnected, 12+ events, Connect/Close/Send

**Модели** (`Runtime/Core/Models/`): AuthResponse, Account, LeaderboardRecord, Channel, ChatMessage, StorageObject, Friend, Group, Notification, Tournament, Presence, WebSocketEnvelope, HttpRequest/HttpResponse (~12 файлов)

**Исключения** (`Runtime/Core/Exceptions/`): GameApiException (StatusCode, ErrorMessage)

**Конфигурация** (`Runtime/Core/`): RetryConfiguration

**Transport интерфейсы** (`Runtime/Transport/Interfaces/`): IHttpAdapter, IWebSocketAdapter, ISerializer, ITokenStorage

---

## Phase 2: Transport реализации + Auth (первый сквозной запрос)

**Агенты:** TEST → CORE → AUTH (последовательно)
**Deliverable:** Register/login работает, auto-refresh при 401, retry с backoff. ~28 тестов Green.

### TEST пишет тесты Auth:
- `Tests/Editor/Mocks/MockHttpAdapter.cs` — очередь ответов
- `Tests/Editor/Mocks/MockTokenStorage.cs` — in-memory
- `Tests/Editor/Auth/AuthServiceTests.cs` — ~12 тестов (register 3 провайдера, login, refresh, logout, 401/403)
- `Tests/Editor/Auth/GameSessionTests.cs` — ~6 тестов (JWT decode, IsExpired)

### CORE создает адаптеры:
- `Runtime/Transport/Serialization/NewtonsoftSerializer.cs` — snake_case, ISO 8601
- `Runtime/Transport/Http/UnityWebRequestAdapter.cs` — IHttpAdapter → UnityWebRequest
- `Runtime/Transport/Storage/PlayerPrefsTokenStorage.cs` — refresh_token в PlayerPrefs

### AUTH реализует:
- `Runtime/Api/GameSession.cs` — JWT base64 decode, expiry check
- `Runtime/Api/Pipeline/TokenManager.cs` — concurrent refresh (SemaphoreSlim)
- `Runtime/Api/Pipeline/HttpPipeline.cs` — auth header → send → 401 auto-refresh → retry → error mapping
- `Runtime/Api/Pipeline/RetryMiddleware.cs` — exponential backoff, jitter, 429 Retry-After
- `Runtime/Api/Services/AuthService.cs` — IAuthClient
- `Runtime/Api/GameClient.cs` — фасад (auth работает, остальное NotImplementedException)
- `Samples/Auth/AuthExample.cs`

---

## Phase 3: Account + Leaderboard (MVP)

**Агенты:** TEST + REST — параллельно | **Deliverable:** MVP готов. ~44 тестов Green.

### Паттерн TDD для каждого домена:
1. TEST: curl-проверка endpoint-ов на VPS
2. TEST: тесты с MockHttpAdapter (Red)
3. REST: реализация сервиса (Green)
4. Подключение к GameClient
5. MCP-проверка
6. Sample

### Файлы:
- `Tests/Editor/Api/AccountServiceTests.cs` (~8 тестов)
- `Tests/Editor/Api/LeaderboardServiceTests.cs` (~8 тестов)
- `Runtime/Api/Services/AccountService.cs`
- `Runtime/Api/Services/LeaderboardService.cs`
- `Samples/Account/AccountExample.cs`, `Samples/Leaderboard/LeaderboardExample.cs`

---

## Phase 4: WebSocket + Chat (реалтайм)

**Агенты:** TEST + WS + REST — параллельно (WS и REST независимы)
**Deliverable:** WebSocket с heartbeat/reconnect + чат. ~68 тестов Green.

> Phase 3 и Phase 4 могут выполняться параллельно (обе зависят от Phase 2)

### Файлы WS:
- `Tests/Editor/Mocks/MockWebSocketAdapter.cs`
- `Tests/Editor/WebSocket/GameSocketTests.cs` (~12 тестов)
- `Tests/Editor/WebSocket/ReconnectHandlerTests.cs` (~6 тестов)
- `Runtime/Transport/WebSocket/NativeWebSocketAdapter.cs`
- `Runtime/WebSocket/GameSocket.cs` — dispatch по type, fire events
- `Runtime/WebSocket/ReconnectHandler.cs` — exponential backoff 1s→30s
- `Runtime/WebSocket/HeartbeatManager.cs` — ping 30s, pong timeout 10s
- `Runtime/WebSocket/WebSocketMessageDispatcher.cs`

### Файлы Chat REST:
- `Tests/Editor/Api/ChatServiceTests.cs` (~6 тестов)
- `Runtime/Api/Services/ChatService.cs`
- `Samples/Chat/ChatExample.cs`, `Samples/WebSocket/WebSocketExample.cs`

---

## Phase 5: Разделение .asmdef + оставшиеся 5 доменов

**Агенты:** CORE (split) → TEST + REST (5 доменов параллельно)
**Deliverable:** 4 .asmdef, все 9 доменов. ~114 тестов Green.

### 5a: CORE разделяет .asmdef

| .asmdef | references | noEngineReferences |
|---------|-----------|-------------------|
| `Runtime/Core/GameBackend.Core.asmdef` | Newtonsoft | **true** |
| `Runtime/Transport/GameBackend.Transport.asmdef` | Core, Newtonsoft, NativeWebSocket, UniTask | false |
| `Runtime/Api/GameBackend.Api.asmdef` | Core, Transport, UniTask, Newtonsoft | false |
| `Runtime/WebSocket/GameBackend.WebSocket.asmdef` | Core, Transport, UniTask | false |

Удалить старый `Runtime/GameBackend.asmdef`. Обновить тестовые .asmdef. Все существующие тесты зеленые.

### 5b: 5 доменов (TDD паттерн для каждого)

| Домен | Тестов | Ключевые особенности |
|-------|--------|---------------------|
| Storage | ~8 | Batch read/write, optimistic locking (version) |
| Friends | ~12 | 10 методов, mutual pending auto-accept |
| Groups | ~14 | create/send chat НЕ safe-to-retry, роли |
| Notifications | ~6 | Пагинация, mark read |
| Tournaments | ~6 | join, record, 409 already joined |

Для каждого: `Tests/Editor/Api/{Service}Tests.cs` + `Runtime/Api/Services/{Service}Service.cs` + `Samples/{Name}/{Name}Example.cs`

---

## Phase 6: Editor tools

**Агент:** EDITOR
**Deliverable:** Settings window в Project Settings, Session Inspector

- `Editor/GameBackendSettings.cs` — ScriptableObject (scheme, host, port)
- `Editor/GameBackendSettingsWindow.cs` — SettingsProvider
- `Editor/SessionInspectorWindow.cs` — EditorWindow (UserId, token expiry, IsAuthenticated)
- Обновить `Editor/GameBackend.Editor.asmdef` — references на Core, Api

Опционально: `Runtime/Integrations/VContainer/GameBackendVContainerExtensions.cs`

---

## Samples (10 примеров)

Каждый Sample — MonoBehaviour с `[ContextMenu("GameBackend/...")]` для каждого метода интерфейса. Позволяет тестировать методы прямо из Inspector без написания UI. Общий хелпер `Samples/_Shared/SampleHelper.cs` создаёт и настраивает GameClient.

| Phase | Sample | Методы с [ContextMenu] |
|-------|--------|----------------------|
| 2 | `Samples/Auth/AuthExample.cs` | RegisterUsername, RegisterEmail, RegisterDevice, LoginUsername, LoginEmail, LoginDevice, Refresh, Logout |
| 3 | `Samples/Account/AccountExample.cs` | GetMyAccount, UpdateDisplayName, DeleteAccount, GetUserById, GetUsersBatch |
| 3 | `Samples/Leaderboard/LeaderboardExample.cs` | SubmitScore, GetTopScores, GetAroundMe, DeleteMyRecord |
| 4 | `Samples/Chat/ChatExample.cs` | ListChannels, CreateChannel, GetMessages, GetUnread, MarkRead |
| 4 | `Samples/WebSocket/WebSocketExample.cs` | Connect, Disconnect, SendMessage, JoinChannel, LeaveChannel, SetPresence |
| 5 | `Samples/Storage/StorageExample.cs` | WriteObjects, ReadObjects, DeleteObject, Search, GetCount |
| 5 | `Samples/Friends/FriendsExample.cs` | SendRequest, AcceptRequest, RejectRequest, RemoveFriend, Block, Unblock, ListFriends, ListIncoming, ListOutgoing, ListBlocked |
| 5 | `Samples/Groups/GroupsExample.cs` | CreateGroup, UpdateGroup, DeleteGroup, JoinGroup, RequestJoin, AcceptRequest, RejectRequest, KickMember, PromoteMember, DemoteMember, LeaveGroup, ListMembers, SearchGroups, ListMyGroups |
| 5 | `Samples/Notifications/NotificationsExample.cs` | ListNotifications, GetUnreadCount, MarkAsRead, DeleteNotification |
| 5 | `Samples/Tournaments/TournamentsExample.cs` | ListTournaments, GetTournament, JoinTournament, SubmitRecord |

Все 10 Samples уже объявлены в `package.json` — пользователь импортирует их через Package Manager.

---

## Ключевые архитектурные решения

1. **HttpPipeline** — центральная точка: auth header → send → 401 refresh → retry → error mapping. Сервисы знают только URL и модели.
2. **Моки на IHttpAdapter** — unit-тесты мокают HTTP ответы, тестируя весь стек сериализации.
3. **GameSession парсит JWT** — base64 decode без криптоверификации (делает сервер).
4. **Concurrent refresh** — SemaphoreSlim, один refresh на N запросов с 401.
5. **WS dispatch** — `Dictionary<string, Action<string>>`, неизвестные type игнорируются.

---

## Документирование субагентами

Каждый агент ведёт лог своих действий в `docs/agents/{agent}-log.md`. Создаётся в Phase 0 вместе со структурой папок.

| Агент | Лог-файл | Что документирует |
|-------|----------|-------------------|
| CORE | `docs/agents/core-log.md` | Созданные интерфейсы, модели, решения по контрактам, изменения .asmdef |
| AUTH | `docs/agents/auth-log.md` | Auth flow, pipeline middleware, решения по JWT/refresh, проблемы |
| REST | `docs/agents/rest-log.md` | Каждый сервис: endpoint маппинг, особенности пагинации, retry policy |
| WS | `docs/agents/ws-log.md` | WS протокол, event dispatch, reconnect стратегия, heartbeat |
| TEST | `docs/agents/test-log.md` | curl-проверки (запрос/ответ), покрытие тестами, найденные баги |
| EDITOR | `docs/agents/editor-log.md` | Settings window, inspector, интеграции |

**Формат записи в логе:**
```
## Phase N: {название}
**Дата:** YYYY-MM-DD
**Что сделано:** список созданных/изменённых файлов
**Решения:** архитектурные решения и их обоснования
**Проблемы:** найденные проблемы и их решения
**Тесты:** количество тестов, статус (Red/Green)
```

Логи помогают другим агентам понимать контекст и не дублировать работу. TEST агент обязательно читает логи CORE/AUTH/REST/WS перед написанием тестов.

---

## Верификация

- После каждой фазы: `run_tests` через MCP Unity (EditMode)
- После Phase 2: curl к реальному бэкенду через SSH на VPS
- После Phase 4: ручная проверка WebSocket подключения
- `read_console` после каждого изменения .cs файлов
