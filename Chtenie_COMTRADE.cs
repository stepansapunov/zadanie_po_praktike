using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace Osnovnoi_proekt;

public static class Chtenie_COMTRADE
{
    public static Zapis_COMTRADE Prochitat(string cfgPath, string datPath)
    {
        Zapis_COMTRADE record = new Zapis_COMTRADE();
        string[] cfgLines = File.ReadAllLines(cfgPath);

        // 1. Читаем количество каналов (2-я строка)
        string[] line2 = cfgLines[1].Split(',');
        int countAnalog = int.Parse(line2[1].Replace("A", ""));

        // 2. Читаем данные каналов
        for (int i = 0; i < countAnalog; i++)
        {
            string[] chLine = cfgLines[i + 2].Split(',');
            var kanal = new Kanal_COMTRADE
            {
                Nomer = int.Parse(chLine[0]),
                Nazvanie = chLine[1],
                Faza = chLine[2],
                Edinicy = chLine[4],
                Koeff_A = double.Parse(chLine[5], CultureInfo.InvariantCulture),
                Koeff_B = double.Parse(chLine[6], CultureInfo.InvariantCulture)
            };
            record.Kanaly.Add(kanal);
        }

        // 3. Читаем частоту дискретизации (строка после каналов + 1)
        // В COMTRADE частота идет после определений всех каналов
        int lineFrequency = 2 + countAnalog + 2;
        if (cfgLines.Length > lineFrequency)
        {
            string[] freqLine = cfgLines[lineFrequency].Split(',');
            if (double.TryParse(freqLine[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double freq) && freq > 0)
            {
                // Вычисляем шаг: если частота 1000 Гц, то шаг 0.001 сек
                record.ShagVremeni = 10.0 / freq;
            }
        }

        // 4. Читаем .dat файл (данные)
        string[] datLines = File.ReadAllLines(datPath);
        foreach (string line in datLines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split(',');
            double[] row = new double[record.Kanaly.Count];

            for (int i = 0; i < record.Kanaly.Count; i++)
            {
                // Берем "сырое" число (пропуская первые два столбца ID и времени в DAT)
                double raw = double.Parse(parts[i + 2], CultureInfo.InvariantCulture);
                // Сразу масштабируем в реальные величины
                row[i] = record.Kanaly[i].Preobrazovat(raw);
            }
            record.Dannye.Add(row);
        }

        return record;
    }
}