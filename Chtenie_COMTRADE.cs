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
        string[] cfgLines = File.ReadAllLines(cfgPath).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        // Считывание идентификатора станции и общего количества информационных каналов
        record.NazvanieStancii = cfgLines[0].Split(',')[0];
        string[] counts = cfgLines[1].Split(',');
        int nAn = int.Parse(counts[1].Replace("A", "").Trim());
        int nDig = counts.Length > 2 ? int.Parse(counts[2].Replace("D", "").Trim()) : 0;

        // Итерация по списку каналов для извлечения параметров масштабирования и единиц измерения
        for (int i = 0; i < nAn; i++)
        {
            string[] L = cfgLines[i + 2].Split(',');
            record.Kanaly.Add(new Kanal_COMTRADE
            {
                Nazvanie = L[1].Trim(),
                Edinicy = L[4].Trim(),
                // Коэффициенты преобразования цифрового кода в первичную величину (Y = A*X + B)
                Koeff_A = double.Parse(L[5].Trim(), CultureInfo.InvariantCulture),
                Koeff_B = double.Parse(L[6].Trim(), CultureInfo.InvariantCulture)
            });
        }

        // Поиск параметров частоты дискретизации в структуре конфигурационного файла
        record.ShagVremeni = 0.001; // Интервал дискретизации по умолчанию (1 мс)
        for (int i = 2 + nAn + nDig; i < cfgLines.Length; i++)
        {
            string[] parts = cfgLines[i].Split(',');
            // Поиск строки, содержащей частоту выборки и общее количество отсчетов
            if (parts.Length >= 2 && double.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double freq))
            {
                if (freq > 100)
                {
                    // Расчет шага дискретизации (период опроса)
                    record.ShagVremeni = 1.0 / freq;
                    break;
                }
            }
        }

        // Парсинг файла данных (.dat) и восстановление мгновенных значений физических величин
        string[] datLines = File.ReadAllLines(datPath);
        foreach (string line in datLines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] p = line.Split(',').Select(s => s.Trim()).ToArray();
            double[] row = new double[record.Kanaly.Count];

            for (int i = 0; i < record.Kanaly.Count; i++)
            {
                // Пропуск временной метки и номера пробы для получения доступа к отсчетам сигналов
                if (p.Length > i + 2 && double.TryParse(p[i + 2], NumberStyles.Any, CultureInfo.InvariantCulture, out double raw))
                    // Масштабирование сигнала согласно коэффициентам из CFG-файла
                    row[i] = record.Kanaly[i].Preobrazovat(raw);
            }
            record.Dannye.Add(row);
        }
        return record;
    }
}