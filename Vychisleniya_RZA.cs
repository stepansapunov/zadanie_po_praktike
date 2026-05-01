using System;
using System.Collections.Generic;
using System.Numerics;

namespace Osnovnoi_proekt;

public static class Vychisleniya_RZA
{
    // Расчет параметров основной гармоники сигнала методом ортогональных составляющих (фильтр Фурье)
    public static Complex Garmonika(double[] samples, int startIndex, int pointsPerPeriod)
    {
        double real = 0;
        double imag = 0;

        for (int i = 0; i < pointsPerPeriod; i++)
        {
            // Вычисление фазового угла для текущей выборки сигнала
            double angle = 2 * Math.PI * i / pointsPerPeriod;

            // Накопление действительной и мнимой составляющих вектора
            real += samples[startIndex + i] * Math.Cos(angle);
            imag += samples[startIndex + i] * Math.Sin(angle);
        }

        // Нормирование амплитуды (коэффициент 2/N) и формирование комплексного числа (фазора)
        return new Complex(real * 2.0 / pointsPerPeriod, -imag * 2.0 / pointsPerPeriod);
    }

    // Разложение трехфазной системы векторов на симметричные составляющие (метод Фортескью)
    public static (Complex i0, Complex i1, Complex i2) Simmetrichnye(Complex fA, Complex fB, Complex fC)
    {
        // Фазовый оператор поворота на 120 градусов
        Complex a = new Complex(-0.5, Math.Sqrt(3) / 2.0);
        // Фазовый оператор поворота на 240 градусов
        Complex a2 = a * a;

        // Расчет векторов нулевой, прямой и обратной последовательностей
        Complex i0 = (fA + fB + fC) / 3.0;
        Complex i1 = (fA + a * fB + a2 * fC) / 3.0;
        Complex i2 = (fA + a2 * fB + a * fC) / 3.0;

        return (i0, i1, i2);
    }
}