using System;
using System.Collections.Generic;

namespace Osnovnoi_proekt;

// Модель данных измерительного канала согласно спецификации COMTRADE
public class Kanal_COMTRADE
{
    public int Nomer { get; set; }
    public string Nazvanie { get; set; } = string.Empty;
    public string Faza { get; set; } = string.Empty;
    public string Edinicy { get; set; } = string.Empty;
    public double Koeff_A { get; set; }
    public double Koeff_B { get; set; }

    // Преобразование дискретного значения в первичную физическую величину (линейная аппроксимация)
    public double Preobrazovat(double syroeZnachenie)
    {
        return Koeff_A * syroeZnachenie + Koeff_B;
    }
}

// Объектная модель данных осциллограммы
public class Model_COMTRADE
{
    // Идентификатор объекта (энергообъекта или станции)
    public string NazvanieStancii { get; set; } = string.Empty;

    // Список описаний информационных каналов
    public List<Kanal_COMTRADE> Kanaly { get; set; } = new();

    // Массив мгновенных значений векторов измерительных сигналов
    public List<double[]> Dannye { get; set; } = new();

    // Период дискретизации сигналов (интервал между выборками), сек
    public double ShagVremeni { get; set; } = 0.001;
}