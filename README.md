# **TBC – Telegram Bot Constructor**

Конструктор Telegram‑ботов на ASP.NET 8 + React. Позволяет собирать сценарии из узлов (*Text / Action / Question*), автоматически генерирует C#‑код, собирает Docker‑контейнер и разворачивает его рядом с основным API.

---

## 1. Требования

* **Visual Studio 2022** (или Rider) с .NET 8 SDK и поддержкой Docker.
* **Docker Desktop** + WSL 2 / Hyper‑V.

## 2. Запуск из Visual Studio

1. Откройте решение **TBC.sln**.
2. В выпадающем списке стартап‑проектов выберите **docker‑compose**.
3. Нажмите **Run** (зелёный «жук»).
4. VS соберёт образы *api*, *frontend*, *postgres* и поднимет контейнеры. Первый раз это может занять **несколько минут**.
5. После сообщения `tbc_Api | Now listening on: http://0.0.0.0:80` откройте:

   * фронт – [http://localhost](http://localhost)

> **⚠️ Первое создание и сборка Docker‑контейнера для нового бота может занимать 2‑4 минуты** – происходит генерация кода, `dotnet publish` и docker build.

## 3. Запуск через чистый Docker Compose

```bash
# клон репо (если ещё нет)
git clone https://github.com/PiVasya/TBC.git
cd TBC

# сборка и запуск
docker compose up --build -d

# остановка
docker compose down -v
```

Docker поднимет:

* **proxy** – Nginx (порт 80)
* **tbc_Api** – backend + генератор ботов (экспонирует 80, внутренний Postgres — через переменную `ConnectionStrings__Default`)
* **tbc_Frontend** – React SPA (проксируется nginx’ом)
* **tbc_Db** – Postgres 17 (порт 5433 наружу).
  Учётные данные по умолчанию:

  * user `postgres`
  * password `secret`
  * db `tbcdb`

## 4. Настройка Telegram‑бота

### 4.1 Получаем токен

1. В Telegram напишите **@BotFather**.
2. Команда `/newbot` → придумайте имя и username (должен заканчиваться на **_bot**).
3. BotFather вернёт **HTTP API token** – строку вида `123456:ABC‑DEF…`.

### 4.2 Admin Chat ID

* Самый простой способ – написать любому боту [@getmyid_bot](https://t.me/getmyid_bot) и скопировать `Your chat id`.

### 4.3 Добавляем бота в конструктор

В UI нажмите **“Создать бота”** и введите:

* `Name` – любое
* `Token` – строка BotFather
* `AdminChatId` – число из п. 4.2

#### Пример для быстрого теста

```
Username:  @GSTU2_bot
Token:     7892436869:AAETivJE3lvMqKsWhxFrchmbcVkb0UMxpAM
AdminId:   1202503239
```

> **Эти данные публичные и предназначены только для демонстрации!** Создайте своего бота для продакшена.

### 4.4 Ограничения

* Некоторые узлы и расширенные функции находятся в разработке – возможны ошибки или временное отсутствие функциональности.
* После каждого редактирования схемы нажмите **«Пересобрать»** – старые контейнеры остановятся, новый соберётся с учётом изменений.

---

Happy coding!
