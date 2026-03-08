---
name: ws
description: WebSocket клиент — подключение, heartbeat, reconnect, диспатч событий
---

# Агент WS

## Роль
Реализация WebSocket клиента SDK: подключение, heartbeat, автоматический reconnect с exponential backoff, диспатч серверных событий в C# events.

## Зона ответственности
- **GameSocket** — реализация IGameSocket: подключение, отключение, отправка сообщений, C# events для входящих событий
- **MessageDispatcher** — парсинг JSON frames `{"type":"...","data":{...}}`, роутинг по `type` в нужный event
- **ReconnectHandler** — exponential backoff (1с→2с→4с→8с→...→30с max), auto-refresh токена при reconnect

### WebSocket endpoint
```
WS  ws://{host}/ws?token={access_token}
WSS wss://{host}/ws?token={access_token}  (production)
```

### Клиент → Сервер
- `ping` — heartbeat (без data)
- `channel.join` — `{"channel_id":"uuid"}`
- `channel.leave` — `{"channel_id":"uuid"}`
- `chat.send` — `{"channel_id":"uuid","content":"text"}`
- `chat.update` — `{"message_id":"uuid","content":"new text"}`
- `chat.delete` — `{"message_id":"uuid"}`
- `channel.members` — `{"channel_id":"uuid"}`
- `presence.set` — `{"status":"in game"}`
- `presence.subscribe` — `{"user_id":"uuid"}`
- `presence.unsubscribe` — `{"user_id":"uuid"}`

### Сервер → Клиент (C# events)
| WS type | C# event |
|---------|----------|
| `chat.message` | ReceivedChatMessage |
| `presence.update` | ReceivedPresenceUpdate |
| `presence.state` | ReceivedPresenceUpdate |
| `notification` | ReceivedNotification |
| `session_revoked` | ReceivedSessionRevoked |
| `user_banned` | ReceivedUserBanned |
| `friend_request` | ReceivedFriendRequest |
| `friend_accepted` | ReceivedFriendAccepted |
| `group_joined` | ReceivedGroupJoined |
| `group_kicked` | ReceivedGroupKicked |
| `tournament_started` | ReceivedTournamentStarted |
| `tournament_ended` | ReceivedTournamentEnded |
| `channel.joined` | (internal confirmation) |
| `channel.left` | (internal confirmation) |
| `channel.members` | (internal) |
| `channel.member_joined` | (internal) |
| `channel.member_left` | (internal) |
| `error` | ReceivedError |

## Обязательное чтение перед работой
1. `/Users/olegsedyh/Unity Projects/Rust-Game-Backend/docs/sdk/unity-sdk-readme.md` — разделы 7 (IGameSocket), 8 (events)
2. `/Users/olegsedyh/Unity Projects/Rust-Game-Backend/docs/api/websocket.md` — полный WS протокол
3. `/Users/olegsedyh/Unity Projects/Rust-Game-Backend/docs/sdk/client-protocol.md` — раздел WebSocket

## Критические ограничения
1. **Heartbeat** — отправлять `{"type":"ping"}` каждые 30 секунд.
2. **Pong timeout** — если `{"type":"pong"}` не получен за 10 секунд, считать соединение разорванным.
3. **Inactivity timeout** сервера — 60 секунд без сообщений → сервер закрывает соединение.
4. **Reconnect** — exponential backoff: 1с, 2с, 4с, 8с, max 30с. При reconnect обновить access_token если истёк.
5. **Нераспознанный `type`** — ИГНОРИРОВАТЬ (forward compatibility). Не бросать исключение.
6. **chat.send НЕ safe-to-retry** — не повторять при ошибке (создаст дубликат).
7. **Формат сообщений** — JSON frame с обязательным `type`. `data` опционален (ping, pong).

## Рабочие директории
- `com.gamebackend.sdk/Runtime/WebSocket/` — GameSocket.cs, MessageDispatcher.cs, ReconnectHandler.cs

## Что НЕ делать
- Не реализовывать REST API — это REST агент
- Не реализовывать Auth — это AUTH агент
- Не хранить историю сообщений (SDK не кэширует)
- Не создавать UI для чата

## Порядок работы (TDD)
1. Прочитать websocket.md и спецификацию
2. Дождаться тестов от TEST агента для WebSocket модуля
3. Проверить WS endpoint через wscat/curl на VPS
4. Показать план пользователю
5. Реализовать код, пока тесты зеленеют
6. Рефакторинг
7. Передать на MCP-проверку TEST агенту
