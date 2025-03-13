using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json.Linq; // Для работы с JSON

public class FlightBoardController : MonoBehaviour
{
    private static readonly string BaseUrl = "https://flight-board.reaport.ru";
    public TMP_Text arrivalFlightsText; // UI элемент для отображения прилетающих рейсов
    public TMP_Text departureFlightsText; // UI элемент для отображения вылетающих рейсов

    private void Start()
    {
        // Загружаем данные сразу при старте
        StartCoroutine(FetchFlightData());
    }

    private IEnumerator FetchFlightData()
    {
        // Загружаем данные о прилетающих рейсах
        UnityWebRequest requestArrivalFlights = UnityWebRequest.Get($"{BaseUrl}/api/arrivalflight/all");
        yield return requestArrivalFlights.SendWebRequest();

        if (requestArrivalFlights.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Ошибка загрузки данных о прилетающих рейсах: {requestArrivalFlights.error}");
            arrivalFlightsText.text = "Ошибка загрузки данных.";
            yield break;
        }

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

        // Загружаем данные о вылетающих рейсах
        UnityWebRequest requestDepartureFlights = UnityWebRequest.Get($"{BaseUrl}/api/flight/all");
        yield return requestDepartureFlights.SendWebRequest();

        if (requestDepartureFlights.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Ошибка загрузки данных о вылетающих рейсах: {requestDepartureFlights.error}");
            departureFlightsText.text = "Ошибка загрузки данных.";
            yield break;
        }

        string departureFlightsJson = requestDepartureFlights.downloadHandler.text;

        // Парсим JSON с вылетающими рейсами
        JArray departureFlightsArray = JArray.Parse(departureFlightsJson);
        string departureFlightsInfo = "";

        // Ограничиваем вывод первыми 5 рейсами
        count = Mathf.Min(5, departureFlightsArray.Count);
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
}
