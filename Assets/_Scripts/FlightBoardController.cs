using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json.Linq; 

public class FlightBoardController : MonoBehaviour
{
    private static readonly string BaseUrl = "https://flight-board.reaport.ru";
    public TMP_Text arrivalFlightsText; // UI элемент для отображения прилетающих рейсов
    public TMP_Text departureFlightsText; // UI элемент для отображения вылетающих рейсов

    private bool isUpdating = false; // Флаг для управления обновлением

    // Метод для запуска обновлений
    public void StartUpdating()
    {
        if (!isUpdating)
        {
            isUpdating = true;
            StartCoroutine(FetchFlightData()); // Запускаем корутину
        }
    }

    // Метод для остановки обновлений
    public void StopUpdating()
    {
        isUpdating = false; // Останавливаем обновления
    }

    private IEnumerator FetchFlightData()
    {
        while (isUpdating) // Пока обновления включены, продолжаем обновлять данные
        {
            // Загружаем данные о прилетающих рейсах
            UnityWebRequest requestArrivalFlights = UnityWebRequest.Get($"{BaseUrl}/api/arrivalflight/all");
            yield return requestArrivalFlights.SendWebRequest();

            if (requestArrivalFlights.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Ошибка загрузки данных о прилетающих рейсах: {requestArrivalFlights.error}");
                arrivalFlightsText.text = "Ошибка загрузки данных.";
            }
            else
            {
                string arrivalFlightsJson = requestArrivalFlights.downloadHandler.text;

                // Парсим JSON с прилетающими рейсами
                JArray arrivalFlightsArray = JArray.Parse(arrivalFlightsJson);
                string arrivalFlightsInfo = "";

                // Ограничиваем вывод первыми 5 рейсами
                int count = Mathf.Min(5, arrivalFlightsArray.Count);
                for (int i = 0; i < count; i++)
                {
                    var flight = arrivalFlightsArray[i];
                    string flightId = flight["flightId"]?.ToString();
                    string departureCity = flight["departureCity"]?.ToString();
                    string arrivalCity = flight["arrivalCity"]?.ToString();
                    string arrivalTime = flight["arrivalTime"]?.ToString();
                    bool hasLanded = flight["hasLanded"]?.ToObject<bool>() ?? false;

                    arrivalFlightsInfo += $"{flightId}: {departureCity} -> {arrivalCity}, Arrival Time: {arrivalTime}, Landed: {hasLanded}\n";
                }

                arrivalFlightsText.text = arrivalFlightsInfo; // Выводим на экран
            }

            // Загружаем данные о вылетающих рейсах
            UnityWebRequest requestDepartureFlights = UnityWebRequest.Get($"{BaseUrl}/api/flight/all");
            yield return requestDepartureFlights.SendWebRequest();

            if (requestDepartureFlights.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Ошибка загрузки данных о вылетающих рейсах: {requestDepartureFlights.error}");
                departureFlightsText.text = "Ошибка загрузки данных.";
            }
            else
            {
                string departureFlightsJson = requestDepartureFlights.downloadHandler.text;

                // Парсим JSON с вылетающими рейсами
                JArray departureFlightsArray = JArray.Parse(departureFlightsJson);
                string departureFlightsInfo = "";

                // Ограничиваем вывод первыми 5 рейсами
                int count = Mathf.Min(5, departureFlightsArray.Count);
                for (int i = 0; i < count; i++)
                {
                    var flight = departureFlightsArray[i];
                    string flightId = flight["flightId"]?.ToString();
                    string cityFrom = flight["cityFrom"]?.ToString();
                    string cityTo = flight["cityTo"]?.ToString();
                    string departureTime = flight["departureTime"]?.ToString();

                    departureFlightsInfo += $"{flightId}: {cityFrom} -> {cityTo}, Departure Time: {departureTime}\n";
                }

                departureFlightsText.text = departureFlightsInfo; // Выводим на экран
            }

            // Подождём перед следующим обновлением
            yield return new WaitForSeconds(5);
        }
    }
}
