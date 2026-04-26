using System;
using System.Collections.Generic;

namespace Osnovnoi_proekt;

// Класс для одного канала (одна строка в CFG)
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

// Класс для всей осциллограммы
public class Zapis_COMTRADE
{
    public List<Kanal_COMTRADE> Kanaly { get; set; } = new();
    public List<double[]> Dannye { get; set; } = new();

    // Тот самый шаг времени для плавности графиков
    public double ShagVremeni { get; set; } = 0.001;
}