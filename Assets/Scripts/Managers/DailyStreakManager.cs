using System;
using UnityEngine;

public class DailyStreakManager : MonoBehaviour
{
    void Start()
    {
        CheckStreak();
    }

    void CheckStreak()
    {
        string lastLogin = PlayerPrefs.GetString("lastLoginDate", "");
        int currentStreak = PlayerPrefs.GetInt("currentStreak", 1);

        DateTime today = DateTime.Today;

        if (string.IsNullOrEmpty(lastLogin))
        {
            // Primer ingreso
            PlayerPrefs.SetString("lastLoginDate", today.ToString());
            PlayerPrefs.SetInt("currentStreak", 1);
        }
        else
        {
            DateTime lastDate = DateTime.Parse(lastLogin);
            TimeSpan difference = today - lastDate;

            if (difference.Days == 1)
            {
                // Suma 1 a la racha
                currentStreak++;
                PlayerPrefs.SetInt("currentStreak", currentStreak);
            }
            else if (difference.Days > 1)
            {
                // Se rompió la racha
                currentStreak = 1;
                PlayerPrefs.SetInt("currentStreak", currentStreak);
            }

            // Guardar la fecha de hoy
            PlayerPrefs.SetString("lastLoginDate", today.ToString());
        }

        Debug.Log("Tu racha actual es: " + currentStreak + " días");
    }
}