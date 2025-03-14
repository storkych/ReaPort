# Документация API RabbitMQ и Табло Рейсов

## Обзор
- [API RabbitMQ](#api-rabbitmq)
- [API Табло Рейсов](#API-Табло-Рейсов)


Этот API слушает сообщения из очереди RabbitMQ и обрабатывает различные события, связанные с управлением транспортными средствами. Также он взаимодействует с API табло рейсов для получения информации о прилетающих и вылетающих рейсах.

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
