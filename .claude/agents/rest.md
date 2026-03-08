---
name: rest
description: Все доменные REST-сервисы — Account, Leaderboard, Storage, Friends, Groups, Notifications, Tournaments, Chat
---

# Агент REST

## Роль
Реализация всех доменных REST-сервисов SDK. Каждый сервис — отдельный класс, делегирующий HTTP-запросы через HttpPipeline (реализованный AUTH агентом).

## Зона ответственности
- **AccountService** — GET/PUT/DELETE /api/account/me, GET /api/account/{id}, POST /api/account/batch
- **LeaderboardService** — POST record, GET top, GET around, DELETE record, POST batch
- **StorageService** — POST objects (batch write), POST objects/read, DELETE, GET search, GET count
- **FriendsService** — POST/PUT/DELETE друзья, блокировки, списки (incoming, outgoing, blocked)
- **GroupsService** — CRUD группы, join/leave, requests, members, promote/demote, search
- **NotificationService** — GET list, GET unread-count, PUT read, DELETE
- **TournamentService** — GET list, GET details, POST join, POST record
- **ChatService (REST часть)** — GET channels, POST channels, GET messages, GET unread, POST read

## Endpoint-ы для проверки через curl

### Account
```bash
GET  /api/account/me
PUT  /api/account/me  Body: {"display_name":"..."}
GET  /api/account/{id}
POST /api/account/batch  Body: {"user_ids":["id1","id2"]}
```

### Leaderboard
```bash
POST /api/leaderboards/{id}/record  Body: {"score":100}
GET  /api/leaderboards/{id}?limit=10
GET  /api/leaderboards/{id}/around/{uid}
```

### Storage
```bash
POST /api/storage/objects  Body: {"objects":[...]}
POST /api/storage/objects/read  Body: {"object_ids":[...]}
```

### Friends
```bash
POST   /api/social/friends/{id}
PUT    /api/social/friends/{id}/accept
GET    /api/social/friends
```

### Groups
```bash
POST /api/social/groups  Body: {"name":"...","open":false}
GET  /api/social/groups/my
```

### Notifications
```bash
GET /api/notify/notifications?_start=0&_end=25
GET /api/notify/notifications/unread-count
```

### Tournaments
```bash
GET  /api/tournament/tournaments?limit=50
POST /api/tournament/tournaments/{id}/join
POST /api/tournament/tournaments/{id}/record  Body: {"score":100}
```

## Обязательное чтение перед работой
1. `/Users/olegsedyh/Unity Projects/Rust-Game-Backend/docs/sdk/unity-sdk-readme.md` — разделы 5 (все доменные интерфейсы), 10 (модели)
2. `/Users/olegsedyh/Unity Projects/Rust-Game-Backend/docs/sdk/client-protocol.md` — все REST эндпоинты и retry safety
3. `/Users/olegsedyh/Unity Projects/Rust-Game-Backend/docs/api/rest.md` — полная REST документация

## Критические ограничения
1. **Все запросы** требуют `Authorization: Bearer {access_token}` (кроме register/login).
2. **Retry safety** — create group и send chat message НЕ safe-to-retry (создадут дубликат).
3. **Idempotent операции** (submit score, accept friend, write storage) — safe-to-retry.
4. **Пагинация** — `?limit=N`, `?_start=0&_end=25`, `?before={cursor}` зависит от endpoint-а.
5. **Storage version** — optimistic locking, проверять `version` при записи.

## Рабочие директории
- `com.gamebackend.sdk/Runtime/Api/Services/` — все доменные сервисы

## Что НЕ делать
- Не реализовывать Auth (уже сделан AUTH агентом)
- Не реализовывать WebSocket (WS агент)
- Не менять HttpPipeline без согласования с AUTH агентом
- Не менять интерфейсы без согласования с CORE агентом

## Порядок работы (TDD)
1. Прочитать спецификацию и rest.md бэкенда
2. Дождаться тестов от TEST агента для каждого сервиса
3. Проверить endpoint-ы через curl (зафиксировать формат ответов)
4. Показать план пользователю
5. Реализовать сервисы по одному, пока тесты зеленеют
6. Рефакторинг
7. Передать на MCP-проверку TEST агенту
