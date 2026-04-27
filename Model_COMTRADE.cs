using System;
using System.Collections.Generic;

namespace Osnovnoi_proekt;

// 1. Описание одного канала (остается без изменений)
public class Kanal_COMTRADE
{
    public int Nomer { get; set; }
    public string Nazvanie { get; set; } = string.Empty;
    public string Faza { get; set; } = string.Empty;
    public string Edinicy { get; set; } = string.Empty;
    public double Koeff_A { get; set; }
    public double Koeff_B { get; set; }

    // Метод для пересчета "сырого" значения в реальное (Амперы/Вольты)
    public double Preobrazovat(double syroeZnachenie)
    {
        return Koeff_A * syroeZnachenie + Koeff_B;
    }
}

// 2. Модель всей осциллограммы (ПЕРЕИМЕНОВАЛИ В Model_COMTRADE)
public class Model_COMTRADE
{
    // Добавим название станции, оно нам пригодится для отчета
    public string NazvanieStancii { get; set; } = string.Empty;

    public List<Kanal_COMTRADE> Kanaly { get; set; } = new();
    public List<double[]> Dannye { get; set; } = new();

    // Шаг времени (1 делить на частоту дискретизации)
    public double ShagVremeni { get; set; } = 0.001;
}