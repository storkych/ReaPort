![Unity](https://img.shields.io/badge/Unity-000000?style=flat-square&logo=unity&logoColor=white)
![Unity Web Request](https://img.shields.io/badge/Unity%20Web%20Request-000000?style=flat-square&logo=unity&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?style=flat-square&logo=rabbitmq&logoColor=white)
![Newtonsoft.Json](https://img.shields.io/badge/Newtonsoft.Json-FF69B4?style=flat-square&logo=json&logoColor=white)


## Скачать билд:
[![Download ZIP](https://img.shields.io/badge/Download%20ZIP-blue?style=flat-square&logo=download&logoColor=white)]()


# Документация API RabbitMQ и Табло Рейсов

## Обзор
- [API RabbitMQ](#api-rabbitmq)
- [API Табло Рейсов](#API-Табло-Рейсов)
- [Описание проекта и установка](#Описание-проекта-и-установка)

Приложение отображает работу модулей системы reaport в 3D.
API слушает сообщения из очереди RabbitMQ и обрабатывает различные события, связанные с работой аэропорта. Также он взаимодействует с API табло рейсов для получения информации о прилетающих и вылетающих рейсах.

---

# API RabbitMQ

## Детали подключения

- **Имя очереди**: `events_queue`
- **URI RabbitMQ**: `amqp://john:123@rabbitmq.reaport.ru`
- **Обменник**: Не используется (прямое потребление очереди)
- **Формат сообщений**: JSON

## Инициализация и запуск

Слушатель реализован как компонент `MonoBehaviour` в Unity и инициализируется при `Start()`:

```csharp
void Start()
{
    ConnectToRabbitMQ();
}
```

Функция `ConnectToRabbitMQ` устанавливает соединение и начинает потребление сообщений:

```csharp
void ConnectToRabbitMQ()
```

- Подключается к RabbitMQ с указанными учетными данными.
- Объявляет очередь `events_queue`.
- Создает асинхронного потребителя для обработки входящих сообщений.
- Логирует статус подключения.

## Обработка сообщений

Сообщения принимаются в формате JSON и обрабатываются функцией:

```csharp
void ProcessMessage(string message)
```

### Ожидаемый формат сообщения

Все сообщения закодированы в JSON и содержат минимум два поля:

```json
{
  "type": "event_type",
  "data": {}
}
```

- `type`: Определяет тип события.
- `data`: Содержит специфические данные события.

## Поддерживаемые типы событий

### 1. `vehicle_registered`

**Описание**: Регистрирует транспортное средство в определенном узле.

#### Пример сообщения

```json
{
  "type": "vehicle_registered",
  "data": {
    "garrage_node_id": "node_01",
    "vehicle_id": "V1234",
    "vehicle_type": "Truck"
  }
}
```

#### Логика обработки

```csharp
string nodeId = data["garrage_node_id"]?.ToString();
string vehicleId = data["vehicle_id"]?.ToString();
string vehicleType = data["vehicle_type"]?.ToString();
VehicleManager.Instance.SpawnVehicle(nodeId, vehicleId, vehicleType);
```

### 2. `vehicle_left_node`

**Описание**: Обновляет перемещение транспортного средства между узлами.

#### Пример сообщения

```json
{
  "type": "vehicle_left_node",
  "data": {
    "from": "node_01",
    "to": "node_02",
    "vehicle_id": "V1234",
    "with_airplane": false,
    "distance": "10.5"
  }
}
```

#### Логика обработки

```csharp
string fromNode = data["from"]?.ToString();
string toNode = data["to"]?.ToString();
string vehicleId = data["vehicle_id"]?.ToString();
bool withAirplane = bool.Parse(data["with_airplane"]?.ToString());
float distance = float.Parse(data["distance"]?.ToString());
VehicleManager.Instance.MoveVehicle(vehicleId, fromNode, toNode, distance, withAirplane);
```

### 3. `vehicle_takeoff`

**Описание**: Обрабатывает событие взлета транспортного средства.

#### Пример сообщения

```json
{
  "type": "vehicle_takeoff",
  "data": {
    "vehicle_id": "V1234"
  }
}
```

#### Логика обработки

```csharp
string vehicleId = data["vehicle_id"]?.ToString();
VehicleManager.Instance.HandleTakeoff(vehicleId);
```

### 4. `map_refreshed`

**Описание**: Вызывает обновление карты.

#### Пример сообщения

```json
{
  "type": "map_refreshed",
  "data": {}
}
```

#### Логика обработки

```csharp
VehicleManager.Instance.RefreshMap();
```

## Корректное завершение работы

```csharp
void OnApplicationQuit()
{
    channel?.Close();
    connection?.Close();
}
```

---

# API Табло Рейсов

## Базовый URL

`https://flight-board.reaport.ru`

## Методы API

### 1. Получение прилетающих рейсов

**Запрос:**
```http
GET /api/arrivalflight/all
```

**Ответ (пример):**
```json
[
  {
    "flightId": "SU123",
    "departureCity": "Москва",
    "arrivalCity": "Сочи",
    "arrivalTime": "14:30",
    "hasLanded": false
  }
]
```

### 2. Получение вылетающих рейсов

**Запрос:**
```http
GET /api/flight/all
```

**Ответ (пример):**
```json
[
  {
    "flightId": "SU456",
    "cityFrom": "Сочи",
    "cityTo": "Москва",
    "departureTime": "15:45"
  }
]
```

## Использование в Unity

Запросы выполняются с помощью `UnityWebRequest`. Данные обновляются каждые 5 секунд.

```csharp
UnityWebRequest request = UnityWebRequest.Get("https://flight-board.reaport.ru/api/arrivalflight/all");
yield return request.SendWebRequest();
```

Обновление данных выполняется в корутине:

```csharp
private IEnumerator FetchFlightData()
```

Преобразование JSON в объекты осуществляется через `Newtonsoft.Json.Linq.JArray.Parse()`.

**Пример обработки данных:**

```csharp
JArray flightsArray = JArray.Parse(responseText);
foreach (var flight in flightsArray)
{
    string flightId = flight["flightId"]?.ToString();
    string departureCity = flight["departureCity"]?.ToString();
    string arrivalCity = flight["arrivalCity"]?.ToString();
    Debug.Log($"Рейс {flightId}: {departureCity} -> {arrivalCity}");
}
```


# Описание проекта и установка

## Требования

Для запуска проекта необходима версия ![Unity](https://img.shields.io/badge/Unity-000000?style=flat-square&logo=unity&logoColor=white) 2022.3.16f1.  
[Подробнее о версии 2022.3.16f1](https://unity.com/ru/releases/editor/whats-new/2022.3.16)  

Для удобства рекомендуется установить [Unity Hub](https://unity.com/download), который упростит процесс установки и управления версиями Unity.

## Структура проекта

Проект включает в себя следующие папки и файлы:

```
+---Assets
|   |   Airport.unity        # Основная сцена аэропорта
|   |   
|   +---Plugins
|   |       RabbitMQ.Client.dll           # Библиотека для работы с RabbitMQ
|   |       System.Runtime.CompilerServices.Unsafe.dll  # Библиотека для небезопасных операций с памятью
|   |       System.Threading.Channels.dll          # Библиотека для работы с каналами потоков
|   +---SimpleAirport               # Ассет с 3D моделями аэропорта
|   +---TextMesh Pro               # Качественный текст UI
|   +---_Animations
|   |       # Анимации и контроллеры анимаций машинок обслуживания
|   +---_Prefabs
|   |   |   
|   |   +---UI
|   |   |   # Блоки для чата    
|   |   \---Vehicles
|   |       # Машины разных типов    
|   \---_Scripts
|           FlightBoardController.cs    # Работа с табло расписания
|           MessageBlock.cs             # Блок сообщения в чате логов
|           Node.cs                     # Узел передвижения
|           RabbitMQListener.cs        # Чтение и обработка очереди RabbitMQ
|           UiManager.cs               # Менеджер интерфейса
|           Vehicle.cs                 # Транспортное средство (машины и самолёты)
|           VehicleManager.cs          # Менеджер всех ТС на карте
```




