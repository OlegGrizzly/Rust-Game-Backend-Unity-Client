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
- Статус: ожидает проверки компиляции в Unity Editor
- Покрытие: AuthResponse, AuthUser, SessionInfo, Account, User, LeaderboardRecord, LeaderboardRecordList, GameApiException
