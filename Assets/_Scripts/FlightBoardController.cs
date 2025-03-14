using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json.Linq; 

public class FlightBoardController : MonoBehaviour
{
    private static readonly string BaseUrl = "https://flight-board.reaport.ru";
    public TMP_Text arrivalFlightsText; // UI ������� ��� ����������� ����������� ������
    public TMP_Text departureFlightsText; // UI ������� ��� ����������� ���������� ������

    private bool isUpdating = false; // ���� ��� ���������� �����������

    // ����� ��� ������� ����������
    public void StartUpdating()
    {
        if (!isUpdating)
        {
            isUpdating = true;
            StartCoroutine(FetchFlightData()); // ��������� ��������
        }
    }

    // ����� ��� ��������� ����������
    public void StopUpdating()
    {
        isUpdating = false; // ������������� ����������
    }

    private IEnumerator FetchFlightData()
    {
        while (isUpdating) // ���� ���������� ��������, ���������� ��������� ������
        {
            // ��������� ������ � ����������� ������
            UnityWebRequest requestArrivalFlights = UnityWebRequest.Get($"{BaseUrl}/api/arrivalflight/all");
            yield return requestArrivalFlights.SendWebRequest();

            if (requestArrivalFlights.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"������ �������� ������ � ����������� ������: {requestArrivalFlights.error}");
                arrivalFlightsText.text = "������ �������� ������.";
            }
            else
            {
                string arrivalFlightsJson = requestArrivalFlights.downloadHandler.text;

                // ������ JSON � ������������ �������
                JArray arrivalFlightsArray = JArray.Parse(arrivalFlightsJson);
                string arrivalFlightsInfo = "";

                // ������������ ����� ������� 5 �������
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

                arrivalFlightsText.text = arrivalFlightsInfo; // ������� �� �����
            }

            // ��������� ������ � ���������� ������
            UnityWebRequest requestDepartureFlights = UnityWebRequest.Get($"{BaseUrl}/api/flight/all");
            yield return requestDepartureFlights.SendWebRequest();

            if (requestDepartureFlights.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"������ �������� ������ � ���������� ������: {requestDepartureFlights.error}");
                departureFlightsText.text = "������ �������� ������.";
            }
            else
            {
                string departureFlightsJson = requestDepartureFlights.downloadHandler.text;

                // ������ JSON � ����������� �������
                JArray departureFlightsArray = JArray.Parse(departureFlightsJson);
                string departureFlightsInfo = "";

                // ������������ ����� ������� 5 �������
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

                departureFlightsText.text = departureFlightsInfo; // ������� �� �����
            }

            // ������� ����� ��������� �����������
            yield return new WaitForSeconds(5);
        }
    }
}
