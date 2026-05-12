# Тестовое задание

Асинхронная система обработки PDF-документов на основе микросервисной архитектуры.

## Архитектура

- **API Gateway** – принимает HTTP-запросы (загрузка PDF, список документов, получение текста). Использует CQRS + MediatR.
- **Background Worker** – асинхронно обрабатывает PDF (извлечение текста через PdfPig)..
- **Outbox Pattern** – гарантированная доставка событий (таблица `OutboxMessages` + фоновый процесс).
- **Соглашение об именовании очередей**: `{префикс}.{тип_события}` (например, `pdf.documentuploaded`).
- **Общее файловое хранилище** – Docker volume для обмена PDF-файлами между сервисами.

## Технологии

- .NET 8
- ASP.NET Core, Entity Framework Core, MediatR
- PostgreSQL, RabbitMQ
- Docker, Docker Compose
- PdfPig (извлечение текста)

## Запуск

### Требования

- Docker (version 20.10+)
- Docker Compose (version 2.0+)

```bash
docker-compose up --build
```

После успешного запуска будут доступны:
- **API Gateway**: http://localhost:5000/swagger

3. Swagger содержит следующие эндпоинты:
   - `POST /api/Documents/upload` – загрузка PDF-файл (multipart/form-data)
   - `GET /api/Documents` – список документов с их статусами
   - `GET /api/Documents/{id}/text` – извлечённый текст после обработки


