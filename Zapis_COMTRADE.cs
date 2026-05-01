using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Osnovnoi_proekt;


// Класс реализации экспорта данных в международный формат COMTRADE (стандарт IEEE C37.111-1999)

public static class Zapis_COMTRADE
{
    public static void Sohranit(string baseFilePath, Model_COMTRADE source, List<int> selectedIndices)
    {
        string cfgPath = baseFilePath + ".cfg";
        string datPath = baseFilePath + ".dat";

        // Формирование конфигурационного файла (.cfg)
        using (StreamWriter sw = new StreamWriter(cfgPath, false, Encoding.Default))
        {
            // Сведения о станции, идентификатор устройства и версия используемого стандарта
            sw.WriteLine($"{source.NazvanieStancii}, Created_by_the_best_team, 1999");

            // Информация о количестве измерительных каналов: общее число, аналоговые и цифровые (дискретные)
            sw.WriteLine($"{selectedIndices.Count},{selectedIndices.Count}A,0D");

            for (int i = 0; i < selectedIndices.Count; i++)
            {
                var k = source.Kanaly[selectedIndices[i]];
                // Параметры информационного канала: индекс, имя, фаза, компонента, единицы измерения, 
                // масштабирующие коэффициенты (A, B), временной сдвиг, минимальное и максимальное значения
                sw.WriteLine($"{i + 1},{k.Nazvanie},{k.Faza},,{k.Edinicy},1.0,0.0,0.0,-32767,32767");
            }

            sw.WriteLine("50"); // Номинальная частота электрической сети, Гц
            sw.WriteLine("1");  // Количество этапов дискретизации

            // Расчет частоты дискретизации на основе шага времени и запись общего количества выборок
            double freq = 1.0 / source.ShagVremeni;
            sw.WriteLine($"{freq:F0},{source.Dannye.Count}");

            // Метки даты и времени записи
            // Используется текущее системное время с точностью до микросекунд
            string currentTime = DateTime.Now.ToString("dd/MM/yyyy,HH:mm:ss.ffffff");

            sw.WriteLine(currentTime); // Метка времени первой выборки
            sw.WriteLine(currentTime); // Метка времени возникновения события (триггера)

            sw.WriteLine("ASCII"); // Способ хранения данных (текстовый формат)
            sw.WriteLine("1");     // Множитель временных меток
        }

        // Формирование файла мгновенных значений (.dat)
        using (StreamWriter swDat = new StreamWriter(datPath, false, Encoding.Default))
        {
            for (int i = 0; i < source.Dannye.Count; i++)
            {
                string sampleNum = (i + 1).ToString();

                // Вычисление относительной временной метки выборки в микросекундах
                double timeUsec = i * source.ShagVremeni * 1000000.0;

                // Сериализация физических величин выбранных каналов в текстовую строку с разделителями
                var values = selectedIndices.Select(idx =>
                    source.Dannye[i][idx].ToString("F3", CultureInfo.InvariantCulture));

                // Структура строки данных: номер пробы, время (мкс), значения сигналов
                swDat.WriteLine($"{sampleNum},{timeUsec:F0},{string.Join(",", values)}");
            }
        }
    }
}