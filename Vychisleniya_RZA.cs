using System;
using System.Collections.Generic;
using System.Numerics;

namespace Osnovnoi_proekt;

public static class Vychisleniya_RZA
{
    // Метод превращает кусок синусоиды в один комплексный вектор (Фазор)
    // Данные - массив значений, N - сколько точек в одном периоде (20 мс)
    public static Complex Garmonika(double[] samples, int startIndex, int pointsPerPeriod)
    {
        double real = 0;
        double imag = 0;

        for (int i = 0; i < pointsPerPeriod; i++)
        {
            double angle = 2 * Math.PI * i / pointsPerPeriod;
            real += samples[startIndex + i] * Math.Cos(angle);
            imag += samples[startIndex + i] * Math.Sin(angle);
        }

        // Коэффициент 2/N для получения амплитудного значения
        return new Complex(real * 2.0 / pointsPerPeriod, -imag * 2.0 / pointsPerPeriod);
    }

    // Расчет симметричных составляющих
    public static (Complex i0, Complex i1, Complex i2) Simmetrichnye(Complex fA, Complex fB, Complex fC)
    {
        Complex a = new Complex(-0.5, Math.Sqrt(3) / 2.0); // Оператор поворота 120 градусов
        Complex a2 = a * a; // 240 градусов

        Complex i0 = (fA + fB + fC) / 3.0;
        Complex i1 = (fA + a * fB + a2 * fC) / 3.0;
        Complex i2 = (fA + a2 * fB + a * fC) / 3.0;

        return (i0, i1, i2);
    }
}