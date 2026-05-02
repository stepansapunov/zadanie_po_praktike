using ScottPlot;
using ScottPlot.WinForms;
using System;
using System.Drawing; // Для System.Drawing.Color
using System.Linq;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace Osnovnoi_proekt
{
    public partial class FormAnalysis : Form
    {
        // Предохранители для синхронизации, чтобы графики не "зациклились"
        private bool isSyncingI = false;
        private bool isSyncingU = false;

        public FormAnalysis(double[] time,
                            double[] phaseIA, double[] phaseIB, double[] phaseIC, double[] i1, double[] i2, double[] i0,
                            double[] phaseUA, double[] phaseUB, double[] phaseUC, double[] u1, double[] u0)
        {
            InitializeComponent();
            SetupCharts(time, phaseIA, phaseIB, phaseIC, i1, i2, i0, phaseUA, phaseUB, phaseUC, u1, u0);
        }

        private void SetupCharts(double[] time,
                         double[] phaseIA, double[] phaseIB, double[] phaseIC, double[] i1, double[] i2, double[] i0,
                         double[] phaseUA, double[] phaseUB, double[] phaseUC, double[] u1, double[] u0)
        {
            // 1. Безопасный расчет периода. Если время пустое, ставим дефолт, чтобы не вылетело с ошибкой
            double period = (time != null && time.Length > 1) ? time[1] - time[0] : 0.001;

            // Сразу очищаем все графики
            formsPlot1.Plot.Clear();
            formsPlot2.Plot.Clear();
            formsPlot3.Plot.Clear();
            formsPlot4.Plot.Clear();

            // 2. ОТРИСОВКА ТОКОВ (Левый столбец)
            // Проверяем, есть ли данные в фазе А тока
            if (phaseIA != null && phaseIA.Length > 0)
            {
                ConfigurePlot(formsPlot1, "Мгновенные токи", "Ia", "Ib", "Ic", phaseIA, phaseIB, phaseIC, period, Color.Red, Color.Green, Color.Blue);
                ConfigurePlot(formsPlot2, "Последовательности I", "I1", "I2", "I0", i1, i2, i0, period, Color.Red, Color.Blue, Color.Green);
            }
            else
            {
                formsPlot1.Plot.Title("Токи: данные отсутствуют");
                formsPlot2.Plot.Title("Последовательности I: нет данных");
            }

            // 3. ОТРИСОВКА НАПРЯЖЕНИЙ (Правый столбец)
            // Проверяем, есть ли данные в фазе А напряжения
            if (phaseUA != null && phaseUA.Length > 0)
            {
                ConfigurePlot(formsPlot3, "Мгновенные напряжения", "Ua", "Ub", "Uc", phaseUA, phaseUB, phaseUC, period, Color.Red, Color.Green, Color.Blue);

                var pltU = formsPlot4.Plot;
                pltU.Title("Последовательности U");

                var sU1 = pltU.Add.Signal(u1);
                sU1.LegendText = "U1 (Прямая)";
                sU1.Color = ScottPlot.Color.FromColor(Color.Red);
                sU1.Data.Period = period;

                var sU0 = pltU.Add.Signal(u0);
                sU0.LegendText = "U0 (Нулевая)";
                sU0.Color = ScottPlot.Color.FromColor(Color.Green);
                sU0.Data.Period = period;

                pltU.ShowLegend(Alignment.LowerLeft);
            }
            else
            {
                formsPlot3.Plot.Title("Напряжения: данные отсутствуют");
                formsPlot4.Plot.Title("Последовательности U: нет данных");
            }

            // --- 4. РАЗДЕЛЬНАЯ СИНХРОНИЗАЦИЯ ПО СТОЛБЦАМ ---
            // (Твой код синхронизации оставляем как есть, он работает отлично)

            var currentPlots = new[] { formsPlot1, formsPlot2 };
            foreach (var master in currentPlots)
            {
                master.Plot.RenderManager.AxisLimitsChanged += (s, e) =>
                {
                    if (isSyncingI) return;
                    isSyncingI = true;
                    var limits = master.Plot.Axes.GetLimits();
                    foreach (var slave in currentPlots)
                    {
                        if (slave == master) continue;
                        slave.Plot.Axes.SetLimitsX(limits.Left, limits.Right);
                        slave.Refresh();
                    }
                    isSyncingI = false;
                };
            }

            var voltagePlots = new[] { formsPlot3, formsPlot4 };
            foreach (var master in voltagePlots)
            {
                master.Plot.RenderManager.AxisLimitsChanged += (s, e) =>
                {
                    if (isSyncingU) return;
                    isSyncingU = true;
                    var limits = master.Plot.Axes.GetLimits();
                    foreach (var slave in voltagePlots)
                    {
                        if (slave == master) continue;
                        slave.Plot.Axes.SetLimitsX(limits.Left, limits.Right);
                        slave.Refresh();
                    }
                    isSyncingU = false;
                };
            }

            // Финальное обновление
            var allPlots = new[] { formsPlot1, formsPlot2, formsPlot3, formsPlot4 };
            foreach (var fp in allPlots)
            {
                fp.Plot.Axes.Margins(0, 0.1);
                fp.Refresh();
            }
        }
        // Вспомогательный метод с ЯВНЫМ указанием System.Drawing.Color
        private void ConfigurePlot(ScottPlot.WinForms.FormsPlot fp, string title, string l1, string l2, string l3,
                                   double[] d1, double[] d2, double[] d3, double period,
                                   System.Drawing.Color c1, System.Drawing.Color c2, System.Drawing.Color c3)
        {
            var plt = fp.Plot;
            plt.Title(title);

            // Добавляем сигналы и конвертируем цвета в формат ScottPlot
            var s1 = plt.Add.Signal(d1);
            s1.LegendText = l1;
            s1.Color = ScottPlot.Color.FromColor(c1);
            s1.Data.Period = period;

            var s2 = plt.Add.Signal(d2);
            s2.LegendText = l2;
            s2.Color = ScottPlot.Color.FromColor(c2);
            s2.Data.Period = period;

            var s3 = plt.Add.Signal(d3);
            s3.LegendText = l3;
            s3.Color = ScottPlot.Color.FromColor(c3);
            s3.Data.Period = period;

            plt.ShowLegend(Alignment.LowerLeft);
        }
    }
}