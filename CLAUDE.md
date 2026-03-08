# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Обзор проекта

Unity SDK клиент для микросервисного игрового бэкенда на Rust (Axum + Tonic gRPC). SDK — низкоуровневый транспортный слой: HTTP запросы к REST API, WebSocket соединение, управление JWT токенами (auto-refresh), retry с backoff. SDK НЕ содержит UI, игровую логику, кэширование или state management.

Бэкенд-проект: `/Users/olegsedyh/Unity Projects/Rust-Game-Backend`. Спецификации в бэкенд-проекте: `docs/sdk/unity-sdk-readme.md`, `docs/sdk/client-protocol.md`, `docs/api/rest.md`, `docs/api/websocket.md`.

## Команды и тестирование

SDK — Unity-пакет, сборка и тесты запускаются через Unity Editor или MCP Unity:

- **Компиляция**: после изменения .cs файлов проверять `read_console` через MCP Unity на ошибки компиляции
- **Тесты (MCP Unity)**: `run_tests` с `test_mode: "EditMode"` или `"PlayMode"`, можно фильтровать по `test_filter`
- **Тесты (CLI)**: `unity -runTests -testPlatform EditMode -projectPath PackageSampleProject -testResults results.xml`
- **Единичный тест**: `run_tests` с `test_filter: "ClassName.MethodName"`

Тесты лежат в `com.gamebackend.sdk/Tests/Runtime/` (PlayMode) и `com.gamebackend.sdk/Tests/Editor/` (EditMode). Для обнаружения тестов пакет должен быть указан в `testables` в `PackageSampleProject/Packages/manifest.json`.

## Структура репозитория

```
com.gamebackend.sdk/           — Unity-пакет (основной код)
  Runtime/                     — Runtime-код (GameBackend.asmdef)
  Editor/                      — Editor-код (GameBackend.Editor.asmdef)
  Tests/Runtime/               — Runtime-тесты (GameBackend.Tests.asmdef)
  Tests/Editor/                — Editor-тесты (GameBackend.Editor.Tests.asmdef)
  Samples/                     — Примеры использования
PackageSampleProject/          — Unity-проект для разработки/тестирования пакета
  Packages/manifest.json       — подключение пакета через file: ссылку
```

## Стек технологий

- **Unity**: 2021.3+ LTS, .NET Standard 2.1
- **Платформы**: Standalone (Win/Mac/Linux), Android, iOS, WebGL
- **Транспорт**: REST API (JSON over HTTPS) + WebSocket (JSON frames)
- **Зависимости**: UniTask (async/await), Newtonsoft.Json (сериализация), NativeWebSocket (WS)
- **Опциональные**: VContainer (DI), R3 (Reactive), MessagePipe (Pub/Sub)

## Архитектура SDK (4 слоя, каждый = отдельный .asmdef)

**Текущее состояние**: пакет содержит шаблонные asmdef (`GameBackend`, `GameBackend.Editor`, `GameBackend.Tests`, `GameBackend.Editor.Tests`). Разделение на 4 слоя ниже — целевая архитектура, которую нужно реализовать.

```
GameBackend.Core       — интерфейсы, модели, контракты (зависит только от Newtonsoft)
GameBackend.Transport  — адаптеры (IHttpAdapter, IWebSocketAdapter, ISerializer, ITokenStorage)
GameBackend.Api        — REST клиент (GameClient фасад, доменные сервисы, TokenManager, HttpPipeline)
GameBackend.WebSocket  — WS клиент (GameSocket, ReconnectHandler, heartbeat)
```

Граф зависимостей: `Core ← Transport ← Api`, `Core ← Transport ← WebSocket`. Циклические зависимости запрещены.

Опциональные модули (отдельные .asmdef): `GameBackend.VContainer`, `GameBackend.R3`, `GameBackend.MessagePipe`.

## Ключевые интерфейсы

- `IGameClient` — фасад REST API (наследует 9 доменных интерфейсов: IAuthClient, IAccountClient, ILeaderboardClient, IChatClient, IStorageClient, IFriendsClient, IGroupsClient, INotificationClient, ITournamentClient)
- `IGameSession` — сессия с JWT токенами (AuthToken, RefreshToken, UserId, IsExpired)
- `IGameSocket` — WebSocket (события: ReceivedChatMessage, ReceivedPresenceUpdate, ReceivedNotification и др.)
- `IHttpAdapter`, `IWebSocketAdapter`, `ISerializer`, `ITokenStorage` — абстракции транспорта (DIP)

## Протокол взаимодействия с бэкендом

**Base URL**: все запросы через Envoy gateway (`/api/*`)

**Аутентификация**: JWT. access_token (15 мин) в `Authorization: Bearer {token}`. При 401 — auto-refresh через `POST /api/auth/refresh`, при неудаче — очистка токенов.

**Сериализация**: JSON, поля в `snake_case`, даты ISO 8601 UTC, UUID строки с дефисами, enum в snake_case. Десериализация без strict mode (новые поля не ломают клиент).

**Ошибки**: `{ "error": "..." }`. Коды: 401 (auto-refresh), 403 (бан), 404, 409 (конфликт), 429 (rate limit + backoff).

**WebSocket**: `wss://{host}/ws?token={access_token}`. JSON frames с `{"type": "...", "data": {...}}`. Heartbeat каждые 30с (`ping`/`pong`). Reconnect с exponential backoff (1с→30с max). Нераспознанный `type` — игнорировать.

**Retry**: только safe-to-retry операции (idempotent). Небезопасные (create group, send chat) — не повторять.

## REST API эндпоинты

| Путь | Сервис |
|------|--------|
| `/api/auth/*` | Аутентификация (register, login, refresh, sessions) |
| `/api/account/*` | Профили (me, batch, удаление) |
| `/api/leaderboard/*` | Лидерборды (record, top, around) |
| `/api/ws/channels/*` | Чат (каналы, история, непрочитанные) |
| `/api/storage/*` | Key-value хранилище (batch read/write, search) |
| `/api/social/friends/*` | Друзья (заявки, блокировки) |
| `/api/social/groups/*` | Группы/кланы (создание, участники, роли) |
| `/api/notify/*` | Уведомления (список, unread count, mark read) |
| `/api/tournament/*` | Турниры (список, вступление, результаты) |
| `/ws` | WebSocket (чат, присутствие, server-push) |

## WebSocket события (server → client)

`chat.message`, `presence.update`, `presence.state`, `channel.joined`, `channel.left`, `channel.members`, `channel.member_joined`, `channel.member_left`, `session_revoked`, `user_banned`, `notification`, `friend_request`, `friend_accepted`, `group_joined`, `group_kicked`, `tournament_started`, `tournament_ended`

## Принципы разработки

- Все методы возвращают `UniTask<T>`, принимают `CancellationToken ct = default` последним аргументом
- В gameplay-коде инжектить доменные интерфейсы (`ILeaderboardClient`), а не `IGameClient`
- Сессия хранится внутри `GameClient` — методы не принимают IGameSession
- `GameClient` создаётся через `new GameClient(scheme, host, port)` или через VContainer DI
- Токены автоматически сохраняются/загружаются через `ITokenStorage` (по умолчанию PlayerPrefs)

## TDD-подход

Разработка ведётся по TDD: сначала TEST агент пишет тесты на основе спецификации и curl-проверки endpoint-ов, затем реализующий агент пишет код пока тесты не станут зелёными. После реализации — проверка через MCP Unity.

Цикл: `curl-проверка → тесты (Red) → контракты (компилируется, падает) → реализация (Green) → рефакторинг → MCP-проверка`

## Агенты

В `.claude/agents/` лежат 6 специализированных субагентов:

| Агент | Файл | Зона ответственности |
|-------|------|---------------------|
| CORE | core.md | Интерфейсы, модели, адаптеры транспорта |
| AUTH | auth.md | Аутентификация, JWT, TokenManager, HttpPipeline |
| REST | rest.md | Доменные REST-сервисы (Account, Leaderboard, Storage, Friends, Groups, Notifications, Tournaments) |
| WS | ws.md | WebSocket клиент, heartbeat, reconnect, события |
| TEST | test.md | TDD-драйвер: curl-проверка, тесты ДО кода, MCP Unity верификация |
| EDITOR | editor.md | Editor tools, Settings window, интеграции (VContainer, R3, MessagePipe) |

Порядок реализации: `CORE → AUTH → REST + WS (параллельно) → EDITOR`. TEST агент работает параллельно со всеми как драйвер TDD.
