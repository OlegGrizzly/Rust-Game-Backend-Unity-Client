# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Обзор проекта

Unity SDK клиент для микросервисного игрового бэкенда на Rust (Axum + Tonic gRPC). SDK — низкоуровневый транспортный слой: HTTP запросы к REST API, WebSocket соединение, управление JWT токенами (auto-refresh), retry с backoff. SDK НЕ содержит UI, игровую логику, кэширование или state management.

Бэкенд-проект расположен в `/Users/olegsedyh/Unity Projects/Rust-Game-Backend`. Спецификация SDK: `docs/sdk/unity-sdk-readme.md`, протокол: `docs/sdk/client-protocol.md`, REST API: `docs/api/rest.md`, WebSocket API: `docs/api/websocket.md`.

## Стек технологий

- **Unity**: 2021.3+ LTS, .NET Standard 2.1
- **Платформы**: Standalone (Win/Mac/Linux), Android, iOS, WebGL
- **Транспорт**: REST API (JSON over HTTPS) + WebSocket (JSON frames)
- **Зависимости**: UniTask (async/await), Newtonsoft.Json (сериализация), NativeWebSocket (WS)
- **Опциональные**: VContainer (DI), R3 (Reactive), MessagePipe (Pub/Sub)

## Архитектура SDK (4 слоя, каждый = отдельный .asmdef)

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
