using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Osnovnoi_proekt;

public static class Zapis_COMTRADE
{
    public static void Sohranit(string baseFilePath, Model_COMTRADE source, List<int> selectedIndices)
    {
        string cfgPath = baseFilePath + ".cfg";
        string datPath = baseFilePath + ".dat";

        // 1. ПИШЕМ CFG ФАЙЛ (Конфигурация)
        using (StreamWriter sw = new StreamWriter(cfgPath, false, Encoding.Default))
        {
            sw.WriteLine($"{source.NazvanieStancii},CreatedByArtyom,1999");
            // Указываем количество выбранных каналов (например, 3,3A,0D)
            sw.WriteLine($"{selectedIndices.Count},{selectedIndices.Count}A,0D");

            for (int i = 0; i < selectedIndices.Count; i++)
            {
                var k = source.Kanaly[selectedIndices[i]];
                // Записываем каналы по порядку 1, 2, 3...
                sw.WriteLine($"{i + 1},{k.Nazvanie},,,{k.Edinicy},1,0,0,-32767,32767");
            }

            sw.WriteLine("50");
            sw.WriteLine("1");
            sw.WriteLine($"{10.0 / source.ShagVremeni:F0},{source.Dannye.Count}");
            sw.WriteLine("01/01/2026,00:00:00.000000");
            sw.WriteLine("01/01/2026,00:00:01.000000");
            sw.WriteLine("ASCII"); // Оставляем текстовый формат для удобства проверки
            sw.WriteLine("1");
        }

        // 2. ПИШЕМ DAT ФАЙЛ (Данные текстом)
        using (StreamWriter swDat = new StreamWriter(datPath, false, Encoding.Default))
        {
            for (int i = 0; i < source.Dannye.Count; i++)
            {
                string sampleNum = (i + 1).ToString();
                string timeUsec = (i * source.ShagVremeni * 1000000).ToString("F0");

                // Выбираем из всей строки данных только те индексы, которые отметил пользователь
                var values = selectedIndices.Select(idx => source.Dannye[i][idx].ToString("F3", System.Globalization.CultureInfo.InvariantCulture));

                swDat.WriteLine($"{sampleNum},{timeUsec},{string.Join(",", values)}");
            }
        }
    }
}