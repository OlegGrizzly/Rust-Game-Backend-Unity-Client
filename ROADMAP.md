# TDD Roadmap: Unity SDK для Game Backend

## Context

Проект — чистый scaffolding (шаблон StansAssets). Реального кода SDK нет. Нужно реализовать полный Unity SDK клиент для микросервисного Rust бэкенда: REST API (9 доменов) + WebSocket (чат, presence, push). SDK — низкоуровневый транспортный слой без UI/кэширования.

> **Scope:** SDK покрывает только client-facing API. Admin endpoints (`/api/*/admin/*`) НЕ входят в scope SDK.

Спецификации бэкенда: `docs/sdk/unity-sdk-readme.md`, `docs/sdk/client-protocol.md`, `docs/api/rest.md`, `docs/api/websocket.md` в `/Users/olegsedyh/Unity Projects/Rust-Game-Backend/`.

## Стратегическое решение: .asmdef

**НЕ разделять на 4 .asmdef сразу.** Оставить один `GameBackend.asmdef` до Phase 5. Логическое разделение через папки (Core/, Transport/, Api/, WebSocket/). Физическое разделение — после стабилизации контрактов.

---

## Phase 0: Подготовка scaffolding ✅

**Агент:** CORE | **Deliverable:** Проект компилируется, 0 тестов, 0 ошибок

- [x] Удалить `Runtime/RuntimeExample.cs`, `Runtime/AssemblyInfo.cs`
- [x] Удалить `Editor/EditorExample.cs`, `Editor/AssemblyInfo.cs`
- [x] Удалить `Tests/Runtime/RuntimeExampleTest.cs`, `Tests/Editor/EditorExampleTest.cs`
- [x] Изменить `Runtime/GameBackend.asmdef` — добавить `rootNamespace: "GameBackend"`
- [x] Изменить `Editor/GameBackend.Editor.asmdef` — добавить `rootNamespace`, reference на `GameBackend`
- [x] Изменить `Tests/Runtime/GameBackend.Tests.asmdef` — добавить `Unity.Newtonsoft.Json`
- [x] Изменить `Tests/Editor/GameBackend.Editor.Tests.asmdef` — добавить `Unity.Newtonsoft.Json`
- [x] Добавить `com.unity.nuget.newtonsoft-json` в `package.json` dependencies
- [x] Создать структуру папок: `Runtime/Core/{Interfaces,Models,Exceptions}`, `Runtime/Transport/Interfaces`, `Runtime/Api/`, `Runtime/WebSocket/`
- [x] Создать `docs/agents/` с лог-файлами для 6 агентов

---

## Phase 1: Core контракты + Transport интерфейсы ✅

**Агенты:** TEST (тесты десериализации) + CORE (контракты) — параллельно
**Deliverable:** 29 файлов контрактов, 4 файла тестов (12 тестов)

### TEST пишет тесты моделей:
- [x] `Tests/Editor/Core/Models/AuthModelsTests.cs` — десериализация AuthResponse, AuthUser, SessionInfo
- [x] `Tests/Editor/Core/Models/AccountModelsTests.cs` — Account (nullable fields, ban fields), User
- [x] `Tests/Editor/Core/Models/LeaderboardModelsTests.cs` — LeaderboardRecord, LeaderboardRecordList
- [x] `Tests/Editor/Core/Exceptions/GameApiExceptionTests.cs` — конструктор, наследование, nullable

### CORE создает контракты:

**Интерфейсы** (`Runtime/Core/Interfaces/`):
- [x] `IAuthClient.cs` — 13 методов (3 Authenticate + 3 Login + RefreshSession + Logout + ListSessions + RevokeSession + RevokeAllSessions + LinkProvider + UnlinkProvider)
- [x] `IAccountClient.cs` — 5 методов (get/update/delete account, get user, batch)
- [x] `ILeaderboardClient.cs` — 5 методов (write/list/around/delete/batch)
- [x] `IChatClient.cs` — 5 методов (channels, messages, unread, mark read)
- [x] `IStorageClient.cs` — 5 методов (write/read/delete/search/count)
- [x] `IFriendsClient.cs` — 10 методов (add/accept/reject/remove/block/unblock + lists)
- [x] `IGroupsClient.cs` — 15 методов (CRUD + join/requestJoin/acceptRequest/rejectRequest/listRequests/kick/promote/demote/leave/members/search/myGroups)
- [x] `INotificationClient.cs` — 4 метода (list/unread/read/delete)
- [x] `ITournamentClient.cs` — 4 метода (list/get/join/record)
- [x] `IGameClient.cs` — фасад (наследует все 9 + Session, IsAuthenticated, RestoreSession, ClearSession, GlobalRetryConfiguration, NewSocket)
- [x] `IGameSession.cs` — AuthToken, RefreshToken, UserId, Username, DisplayName, ExpireTime, RefreshExpireTime, IsExpired, IsRefreshExpired, HasExpired, HasRefreshExpired + static GameSession.Restore
- [x] `IGameSocket.cs` — IDisposable, IsConnected, 14 events, Connect/Close/SendChatMessage

**Модели** (`Runtime/Core/Models/` — 11 файлов, ~35 классов):
- [x] AuthModels.cs (AuthResponse, AuthUser, SessionInfo), AccountModels.cs (Account + ban fields, User), LeaderboardModels.cs, ChatModels.cs, StorageModels.cs, FriendModels.cs, GroupModels.cs, NotificationModels.cs, TournamentModels.cs, WebSocketModels.cs (PresenceUpdate, BanInfo, WebSocketEnvelope), HttpModels.cs (HttpRequest, HttpResponse)

**Исключения** (`Runtime/Core/Exceptions/`):
- [x] GameApiException (StatusCode, ErrorMessage, ErrorCode, RequestId)

**Конфигурация** (`Runtime/Core/`):
- [x] RetryConfiguration (BaseDelayMs, MaxRetries, RetryListener)

**Transport интерфейсы** (`Runtime/Transport/Interfaces/`):
- [x] IHttpAdapter, IWebSocketAdapter, ISerializer, ITokenStorage

---

## Phase 2: Transport реализации + Auth (первый сквозной запрос) ⬜

**Агенты:** TEST → CORE → AUTH (последовательно)
**Deliverable:** Authenticate/login работает, auto-refresh при 401, retry с backoff. ~28 тестов Green.

### TEST пишет тесты Auth:
- [ ] `Tests/Editor/Mocks/MockHttpAdapter.cs` — очередь ответов
- [ ] `Tests/Editor/Mocks/MockTokenStorage.cs` — in-memory
- [ ] `Tests/Editor/Auth/AuthServiceTests.cs` — ~20 тестов (authenticate 3 провайдера, login 3, refresh, logout, listSessions, revokeSession, revokeAllSessions, linkProvider, unlinkProvider, 401/403)
- [ ] `Tests/Editor/Auth/GameSessionTests.cs` — ~6 тестов (JWT decode, IsExpired)

### CORE создает адаптеры:
- [ ] `Runtime/Transport/Serialization/NewtonsoftSerializer.cs` — snake_case, ISO 8601
- [ ] `Runtime/Transport/Http/UnityWebRequestAdapter.cs` — IHttpAdapter → UnityWebRequest
- [ ] `Runtime/Transport/Storage/PlayerPrefsTokenStorage.cs` — refresh_token в PlayerPrefs

### AUTH реализует:
- [ ] `Runtime/Api/GameSession.cs` — JWT base64 decode, expiry check
- [ ] `Runtime/Api/Pipeline/TokenManager.cs` — concurrent refresh (SemaphoreSlim)
- [ ] `Runtime/Api/Pipeline/HttpPipeline.cs` — auth header → send → 401 auto-refresh → retry → error mapping
- [ ] `Runtime/Api/Pipeline/RetryMiddleware.cs` — exponential backoff, jitter, 429 Retry-After
- [ ] `Runtime/Api/Services/AuthService.cs` — IAuthClient
- [ ] `Runtime/Api/GameClient.cs` — фасад (auth работает, остальное NotImplementedException)
- [ ] `Samples/Auth/AuthExample.cs`

---

## Phase 3: Account + Leaderboard (MVP) ⬜

**Агенты:** TEST + REST — параллельно | **Deliverable:** MVP готов. ~44 тестов Green.

### Паттерн TDD для каждого домена:
1. TEST: curl-проверка endpoint-ов на VPS
2. TEST: тесты с MockHttpAdapter (Red)
3. REST: реализация сервиса (Green)
4. Подключение к GameClient
5. MCP-проверка
6. Sample

### Account:
- [ ] `Tests/Editor/Api/AccountServiceTests.cs` (~8 тестов)
- [ ] `Runtime/Api/Services/AccountService.cs`
- [ ] `Samples/Account/AccountExample.cs`

### Leaderboard:
- [ ] `Tests/Editor/Api/LeaderboardServiceTests.cs` (~8 тестов)
- [ ] `Runtime/Api/Services/LeaderboardService.cs`
- [ ] `Samples/Leaderboard/LeaderboardExample.cs`

---

## Phase 4: WebSocket + Chat (реалтайм) ⬜

**Агенты:** TEST + WS + REST — параллельно (WS и REST независимы)
**Deliverable:** WebSocket с heartbeat/reconnect + чат. ~68 тестов Green.

> Phase 3 и Phase 4 могут выполняться параллельно (обе зависят от Phase 2)

### WebSocket:
- [ ] `Tests/Editor/Mocks/MockWebSocketAdapter.cs`
- [ ] `Tests/Editor/WebSocket/GameSocketTests.cs` (~12 тестов)
- [ ] `Tests/Editor/WebSocket/ReconnectHandlerTests.cs` (~6 тестов)
- [ ] `Runtime/Transport/WebSocket/NativeWebSocketAdapter.cs`
- [ ] `Runtime/WebSocket/GameSocket.cs` — dispatch по type, fire events
- [ ] `Runtime/WebSocket/ReconnectHandler.cs` — exponential backoff 1s→30s
- [ ] `Runtime/WebSocket/HeartbeatManager.cs` — ping 30s, pong timeout 10s, server inactivity timeout 60s
- [ ] `Runtime/WebSocket/WebSocketMessageDispatcher.cs`
- [ ] `Samples/WebSocket/WebSocketExample.cs`

### Chat REST:
- [ ] `Tests/Editor/Api/ChatServiceTests.cs` (~6 тестов)
- [ ] `Runtime/Api/Services/ChatService.cs`
- [ ] `Samples/Chat/ChatExample.cs`

---

## Phase 5: Разделение .asmdef + оставшиеся 5 доменов ⬜

**Агенты:** CORE (split) → TEST + REST (5 доменов параллельно)
**Deliverable:** 4 .asmdef, все 9 доменов. ~114 тестов Green.

### 5a: CORE разделяет .asmdef
- [ ] `Runtime/Core/GameBackend.Core.asmdef` — Newtonsoft, noEngineReferences: **true**
- [ ] `Runtime/Transport/GameBackend.Transport.asmdef` — Core, Newtonsoft, NativeWebSocket, UniTask
- [ ] `Runtime/Api/GameBackend.Api.asmdef` — Core, Transport, UniTask, Newtonsoft
- [ ] `Runtime/WebSocket/GameBackend.WebSocket.asmdef` — Core, Transport, UniTask
- [ ] Удалить старый `Runtime/GameBackend.asmdef`, обновить тестовые .asmdef
- [ ] Все существующие тесты зелёные

### 5b: Storage
- [ ] `Tests/Editor/Api/StorageServiceTests.cs` (~8 тестов, batch read/write, optimistic locking)
- [ ] `Runtime/Api/Services/StorageService.cs`
- [ ] `Samples/Storage/StorageExample.cs`

### 5c: Friends
- [ ] `Tests/Editor/Api/FriendsServiceTests.cs` (~12 тестов, mutual pending auto-accept)
- [ ] `Runtime/Api/Services/FriendsService.cs`
- [ ] `Samples/Friends/FriendsExample.cs`

### 5d: Groups
- [ ] `Tests/Editor/Api/GroupsServiceTests.cs` (~15 тестов, create НЕ safe-to-retry, роли)
- [ ] `Runtime/Api/Services/GroupsService.cs`
- [ ] `Samples/Groups/GroupsExample.cs`

### 5e: Notifications
- [ ] `Tests/Editor/Api/NotificationsServiceTests.cs` (~6 тестов, пагинация, mark read)
- [ ] `Runtime/Api/Services/NotificationsService.cs`
- [ ] `Samples/Notifications/NotificationsExample.cs`

### 5f: Tournaments
- [ ] `Tests/Editor/Api/TournamentsServiceTests.cs` (~6 тестов, 409 already joined)
- [ ] `Runtime/Api/Services/TournamentsService.cs`
- [ ] `Samples/Tournaments/TournamentsExample.cs`

---

## Phase 6: Editor tools ⬜

**Агент:** EDITOR
**Deliverable:** Settings window в Project Settings, Session Inspector

- [ ] `Editor/GameBackendSettings.cs` — ScriptableObject (scheme, host, port)
- [ ] `Editor/GameBackendSettingsWindow.cs` — SettingsProvider
- [ ] `Editor/SessionInspectorWindow.cs` — EditorWindow (UserId, Username, DisplayName, token expiry, IsAuthenticated)
- [ ] Обновить `Editor/GameBackend.Editor.asmdef` — references на Core, Api
- [ ] `Editor/DebugConsoleIntegration.cs` — логирование SDK HTTP запросов/ответов в Editor Console (опционально)

Опциональные интеграции (отдельные .asmdef с `defineConstraints`):
- [ ] `Runtime/Integrations/VContainer/GameBackendVContainerExtensions.cs` — DI регистрация IGameClient, IGameSocket
- [ ] `Runtime/Integrations/R3/GameSocketR3Extensions.cs` — Observable обёртки для IGameSocket events
- [ ] `Runtime/Integrations/MessagePipe/GameSocketMessagePipeExtensions.cs` — Pub/Sub адаптеры для WS событий

---

## Samples (10 примеров)

Каждый Sample — MonoBehaviour с `[ContextMenu("GameBackend/...")]` для каждого метода интерфейса. Позволяет тестировать методы прямо из Inspector без написания UI. Общий хелпер `Samples/_Shared/SampleHelper.cs` создаёт и настраивает GameClient.

| Phase | Sample | Методы с [ContextMenu] |
|-------|--------|----------------------|
| 2 | `Samples/Auth/AuthExample.cs` | AuthenticateUsername, AuthenticateEmail, AuthenticateDevice, LoginUsername, LoginEmail, LoginDevice, RefreshSession, Logout, ListSessions, RevokeSession, RevokeAllSessions, LinkProvider, UnlinkProvider |
| 3 | `Samples/Account/AccountExample.cs` | GetMyAccount, UpdateDisplayName, DeleteAccount, GetUserById, GetUsersBatch |
| 3 | `Samples/Leaderboard/LeaderboardExample.cs` | SubmitScore, GetTopScores, GetAroundMe, DeleteMyRecord |
| 4 | `Samples/Chat/ChatExample.cs` | ListChannels, CreateChannel, ListMessages, GetUnread, MarkChannelRead |
| 4 | `Samples/WebSocket/WebSocketExample.cs` | Connect, Disconnect, SendMessage, UpdateMessage, DeleteMessage, JoinChannel, LeaveChannel, GetMembers, SetPresence, SubscribePresence, UnsubscribePresence |
| 5 | `Samples/Storage/StorageExample.cs` | WriteStorageObjects, ReadStorageObjects, DeleteStorageObject, SearchStorageObjects, CountStorageObjects |
| 5 | `Samples/Friends/FriendsExample.cs` | SendRequest, AcceptRequest, RejectRequest, RemoveFriend, Block, Unblock, ListFriends, ListIncoming, ListOutgoing, ListBlocked |
| 5 | `Samples/Groups/GroupsExample.cs` | CreateGroup, UpdateGroup, DeleteGroup, JoinGroup, RequestJoin, AcceptRequest, RejectRequest, ListRequests, KickMember, PromoteMember, DemoteMember, LeaveGroup, ListMembers, SearchGroups, ListMyGroups |
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

## Clean Architecture + SOLID

### Dependency Rule (зависимости только внутрь)

```
Core  ←──  Transport  ←──  Api
                       ←──  WebSocket
```

- **Core** НЕ знает о Transport, Api, WebSocket
- **Transport** НЕ знает об Api и WebSocket
- **Api** и **WebSocket** НЕ знают друг о друге
- Циклические зависимости запрещены

### Границы слоёв через интерфейсы

- Core определяет контракты (IHttpAdapter, ISerializer, ITokenStorage)
- Transport реализует контракты
- Api и WebSocket используют только абстракции из Core/Transport
- Конкретные реализации инжектятся снаружи (DIP)

### Core — ядро без внешних зависимостей

- `noEngineReferences: true` — Core не зависит от UnityEngine
- Только Newtonsoft.Json для атрибутов моделей
- Можно использовать в не-Unity проектах (.NET)

### Расширяемость без модификации (OCP)

- Новый REST сервис = новый файл + новый интерфейс, **ноль изменений** в существующих сервисах
- Новый транспорт (кастомный HTTP) = новая реализация IHttpAdapter
- Новая платформа хранения = новая реализация ITokenStorage
- Новый WS event = добавить case в dispatcher, не трогая существующие

### Тестируемость

- Мокаем только границу слоёв (IHttpAdapter, IWebSocketAdapter)
- Тесты проверяют весь стек через границу: сериализация → pipeline → сервис → десериализация
- Core тестируется без Unity (noEngineReferences)

### SOLID

- **SRP**: Каждый сервис отвечает за один домен (AuthService — только auth, AccountService — только account). HttpPipeline отвечает только за HTTP механику, TokenManager — только за токены.
- **OCP**: Новые доменные сервисы добавляются без изменения существующих. Pipeline расширяется middleware без модификации ядра.
- **LSP**: Все реализации IHttpAdapter (UnityWebRequestAdapter, MockHttpAdapter) взаимозаменяемы. GameClient реализует IGameClient без нарушения контракта.
- **ISP**: 9 отдельных доменных интерфейсов вместо одного монолитного. Gameplay-код инжектит `ILeaderboardClient`, а не весь `IGameClient`.
- **DIP**: Сервисы зависят от абстракций (IHttpAdapter, ISerializer, ITokenStorage), а не от конкретных реализаций. Позволяет подменять транспорт для тестов и разных платформ.

---

## E2E тестирование через MCP Unity

После каждой фазы TEST агент проводит E2E проверку через MCP Unity:

| Phase | E2E сценарий |
|-------|-------------|
| 0 | `read_console` — 0 ошибок компиляции |
| 1 | `run_tests` EditMode — тесты десериализации моделей Green |
| 2 | `run_tests` EditMode — Auth тесты Green. E2E: создать GameClient → AuthenticateUsername → проверить Session.UserId != null → Logout |
| 3 | `run_tests` EditMode — Account + Leaderboard тесты Green. E2E: Login → GetAccount → UpdateAccount(displayName) → WriteLeaderboardRecord → ListLeaderboardRecords → проверить score |
| 4 | `run_tests` EditMode + PlayMode — WS тесты Green. E2E: Login → NewSocket → Connect → JoinChannel → SendChatMessage → проверить ReceivedChatMessage event → Close |
| 5 | `run_tests` EditMode — все ~114 тестов Green. E2E: полный цикл Storage (write→read→delete), Friends (add→accept→list→remove), Groups (create→join→leave→delete) |
| 6 | `run_tests` EditMode — все тесты Green. Проверить Settings window открывается, значения сохраняются |

**Процедура E2E:**
1. `read_console` — убедиться 0 ошибок компиляции
2. `run_tests` с `test_mode: "EditMode"` — все unit-тесты зелёные
3. Через MCP Unity: создать GameObject с Sample компонентом, вызвать [ContextMenu] методы, проверить Debug.Log вывод через `read_console`

---

## Инструкции ручного тестирования для Examples

Каждый Sample файл содержит в шапке XML-комментарий `<summary>` с пошаговой инструкцией ручного тестирования:

```csharp
/// <summary>
/// Auth Example — ручное тестирование аутентификации.
///
/// Инструкция:
/// 1. Создайте пустой GameObject в сцене
/// 2. Добавьте компонент AuthExample
/// 3. В Inspector задайте Host, Port (или оставьте по умолчанию)
/// 4. Правый клик на компоненте → GameBackend → AuthenticateUsername
/// 5. Проверьте Console: "Authenticated as {userId}"
/// 6. Правый клик → GameBackend → ListSessions — должна показать 1 сессию
/// 7. Правый клик → GameBackend → Logout — "Logged out"
///
/// Ожидаемые ошибки:
/// - "401 Unauthorized" при вызове методов без авторизации
/// - "409 Conflict" при повторной аутентификации того же username
/// </summary>
```

Формат единый для всех 10 Samples:
1. **Setup** — как добавить компонент в сцену
2. **Шаги** — порядок вызова [ContextMenu] методов
3. **Ожидаемый результат** — что должно появиться в Console
4. **Ожидаемые ошибки** — какие ошибки нормальны в определённых сценариях

Дополнительно в `Samples/_Shared/TESTING.md` — общая инструкция:
- Как настроить подключение к бэкенду
- Как импортировать Samples через Package Manager
- Порядок тестирования (Auth → Account → Leaderboard → Chat → WS → Storage → Friends → Groups → Notifications → Tournaments)

---

## Верификация

- После каждого изменения .cs: `read_console` через MCP Unity на ошибки компиляции
- После каждой фазы: E2E тестирование (см. таблицу выше)
- После Phase 2: curl к реальному бэкенду через SSH на VPS
- После Phase 4: ручная проверка WebSocket подключения через Sample
