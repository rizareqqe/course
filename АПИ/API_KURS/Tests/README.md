# API Smoke Tests

Запускать после старта API:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tests\api_smoke_tests.ps1
```

Покрываются сценарии:

- вход администратора и редактора;
- чтение справочников и списка фильмов;
- фильтрация и статистика;
- CRUD для сущности `actors`;
- проверка ограничения прав редактора.
