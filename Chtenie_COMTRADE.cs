using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;

namespace Osnovnoi_proekt;

public static class Chtenie_COMTRADE
{
    public static Model_COMTRADE Prochitat(string cfgPath, string datPath)
    {
        Model_COMTRADE record = new Model_COMTRADE();

        // Читаем всё, убирая пустые строки в конце, чтобы не поймать ошибку
        string[] cfgLines = File.ReadAllLines(cfgPath).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        if (cfgLines.Length < 2) throw new Exception("Некорректный CFG файл");

        // 1. Название станции (первая строка)
        record.NazvanieStancii = cfgLines[0].Split(',')[0];

        // 2. Количество каналов (вторая строка)
        string[] line2 = cfgLines[1].Split(',');
        // Используем Trim(), чтобы убрать пробелы вокруг числа
        int countAnalog = int.Parse(line2[1].Replace("A", "").Trim());

        // 3. Читаем данные аналоговых каналов
        for (int i = 0; i < countAnalog; i++)
        {
            // Строки каналов начинаются с 3-й (индекс 2)
            string[] chLine = cfgLines[i + 2].Split(',');

            var kanal = new Kanal_COMTRADE
            {
                Nomer = int.Parse(chLine[0].Trim()),
                Nazvanie = chLine[1].Trim(),
                Faza = chLine[2].Trim(), // Важно для поиска фаз А, B, C!
                Edinicy = chLine[4].Trim(),
                // Читаем коэффициенты масштабирования
                Koeff_A = double.Parse(chLine[5].Trim(), CultureInfo.InvariantCulture),
                Koeff_B = double.Parse(chLine[6].Trim(), CultureInfo.InvariantCulture)
            };
            record.Kanaly.Add(kanal);
        }

        // 4. Частота и шаг времени (строка идет после всех каналов + еще 2 строки)
        // Строка с частотой: 2 (шапка) + countAnalog + 2 (строка со статусом и еще одна)
        int lineFrequency = 2 + countAnalog + 2;

        if (cfgLines.Length > lineFrequency)
        {
            string[] freqLine = cfgLines[lineFrequency].Split(',');
            if (double.TryParse(freqLine[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double freq) && freq > 0)
            {
                // Твоя спец-формула (10.0 вместо 1.0)
                record.ShagVremeni = 10.0 / freq;
            }
        }

        // 5. Читаем .dat файл
        string[] datLines = File.ReadAllLines(datPath);
        foreach (string line in datLines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Разбиваем строку и убираем лишние пробелы у каждого элемента
            string[] parts = line.Split(',').Select(p => p.Trim()).ToArray();

            // В ASCII DAT файле: [0] - ID, [1] - время в мкс, [2...] - данные
            double[] row = new double[record.Kanaly.Count];

            for (int i = 0; i < record.Kanaly.Count; i++)
            {
                if (parts.Length > i + 2)
                {
                    // Читаем сырое число с точкой
                    if (double.TryParse(parts[i + 2], NumberStyles.Any, CultureInfo.InvariantCulture, out double raw))
                    {
                        // Масштабируем: результат = raw * A + B
                        row[i] = record.Kanaly[i].Preobrazovat(raw);
                    }
                }
            }
            record.Dannye.Add(row);
        }

        return record;
    }
}