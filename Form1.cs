using System.IO;
using System.Linq;
using System.Drawing;
using ScottPlot;
using System.Numerics; 
using Color = System.Drawing.Color;

namespace Osnovnoi_proekt;

public partial class Form1 : Form
{
    // НОВОЕ: Переменная для хранения текущей записи (чтобы обращаться к ней из разных мест)
    private Zapis_COMTRADE currentRecord;

    public Form1()
    {
        InitializeComponent();

        // НОВОЕ: Подписываемся на событие изменения галочки
        clbSignals.ItemCheck += clbSignals_ItemCheck;
    }

    private void btnOpen_Click_1(object sender, EventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "Файлы конфигурации (*.cfg)|*.cfg";

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            string cfgPath = ofd.FileName;
            string datPath = cfgPath.Replace(".cfg", ".dat").Replace(".CFG", ".DAT");

            if (File.Exists(datPath))
            {
                // 1. Сохраняем запись в переменную класса
                currentRecord = Chtenie_COMTRADE.Prochitat(cfgPath, datPath);

                // 2. Заполняем список выбора сигналов
                clbSignals.Items.Clear();
                for (int i = 0; i < currentRecord.Kanaly.Count; i++)
                {
                    var k = currentRecord.Kanaly[i];
                    string krasivoeImya = PoluchitPonyatnoeImya(i, k.Nazvanie);
                    clbSignals.Items.Add($"{i + 1}: {krasivoeImya} [{k.Edinicy}]");
                }

                // 3. Отчет по пиковым значениям в текстовое поле
                txtInfo.Clear();
                txtInfo.AppendText($"=== ОТЧЕТ ПО ФАЙЛУ: {Path.GetFileName(cfgPath)} ===\r\n\r\n");
                txtInfo.AppendText("РЕЗУЛЬТАТЫ АНАЛИЗА (Пиковые значения):\r\n");

                int[] tokiIdx = { 6, 7, 8 };
                char[] fazy = { 'A', 'B', 'C' };

                for (int j = 0; j < tokiIdx.Length; j++)
                {
                    int idx = tokiIdx[j];
                    double maxTok = currentRecord.Dannye.Max(row => Math.Abs(row[idx]));
                    txtInfo.AppendText($"Фаза {fazy[j]}: {maxTok,8:F2} Ампер\r\n");
                }

                // 4. Очищаем основной график
                formsPlot1.Plot.Clear();
                formsPlot1.Refresh();

                // --- ВОТ СЮДА ВСТАВЛЯЕМ! ---
                // Теперь, когда данные загружены, считаем симметричные составляющие
                // Результат появится на второй вкладке (Анализ ТКЗ)
                VychislitSimmetrichnye();
                // ---------------------------
            }
        }
    }
    

    // НОВОЕ: Событие, которое срабатывает при клике на галочку
    private void clbSignals_ItemCheck(object sender, ItemCheckEventArgs e)
    {
        // Используем BeginInvoke, чтобы график обновился ПОСЛЕ того, как галочка поставится
        BeginInvoke((MethodInvoker)delegate
        {
            OtrisovatVybrannye();
        });
    }

    // НОВОЕ: Метод отрисовки только тех сигналов, где стоят галочки
    private void OtrisovatVybrannye()
    {
        if (currentRecord == null || currentRecord.Dannye.Count == 0) return;

        formsPlot1.Plot.Clear();

        System.Drawing.Color[] cveta = {
        Color.Red, Color.Green, Color.Blue,
        Color.Orange, Color.Purple, Color.Cyan,
        Color.Magenta, Color.Brown
    };

        int colorIdx = 0;
        bool estToki = false;
        bool estNapryazheniya = false;

        foreach (int idx in clbSignals.CheckedIndices)
        {
            double[] yData = currentRecord.Dannye.Select(row => row[idx]).ToArray();

            var signal = formsPlot1.Plot.Add.Signal(yData);
            signal.Data.Period = currentRecord.ShagVremeni;
            signal.Color = ScottPlot.Color.FromColor(cveta[colorIdx % cveta.Length]);

            string edinicy = currentRecord.Kanaly[idx].Edinicy.ToUpper();
            string name = PoluchitPonyatnoeImya(idx, currentRecord.Kanaly[idx].Nazvanie);
            signal.LegendText = $"{name} ({edinicy})";

            if (edinicy.Contains("V") || edinicy.Contains("В"))
            {
                signal.Axes.YAxis = formsPlot1.Plot.Axes.Right;
                estNapryazheniya = true;
            }
            else
            {
                signal.Axes.YAxis = formsPlot1.Plot.Axes.Left;
                estToki = true;
            }
            colorIdx++;
        }

        // --- ФИКС МАСШТАБА (чтобы не отдалять вручную) ---

        // 1. Считаем длительность
        double dlitelnost = currentRecord.Dannye.Count * currentRecord.ShagVremeni;
        if (dlitelnost <= 0) dlitelnost = 1.0;

        // 2. Вместо ZoomOut задаем постоянные отступы (0% по горизонтали, 10% по вертикали)
        formsPlot1.Plot.Axes.Margins(0, 0.1);

        // 3. Устанавливаем границы времени (X)
        formsPlot1.Plot.Axes.SetLimitsX(0, dlitelnost);

        // 4. Масштабируем только вертикаль (Y) — теперь с учетом Margins отступы будут фиксированными!
        formsPlot1.Plot.Axes.AutoScaleY();

        // 4. Добавим небольшой отступ сверху и снизу (10%), чтобы пики не касались края
        formsPlot1.Plot.Axes.ZoomOut(1.0, 1.1);

        // Настройка видимости осей
        formsPlot1.Plot.Axes.Left.IsVisible = estToki;
        formsPlot1.Plot.YLabel(estToki ? "Ток, А" : "");
        formsPlot1.Plot.Axes.Right.IsVisible = estNapryazheniya;
        formsPlot1.Plot.Axes.Right.Label.Text = estNapryazheniya ? "Напряжение, В" : "";

        formsPlot1.Plot.Title("Осциллограмма выбранных сигналов");
        formsPlot1.Plot.XLabel("Время, секунды");
        formsPlot1.Plot.Legend.IsVisible = true;

        formsPlot1.Refresh();
    }

    private string PoluchitPonyatnoeImya(int index, string originalnoeImya)
    {
        switch (index)
        {
            case 0: return "U шин (фаза А)";
            case 1: return "U шин (фаза B)";
            case 2: return "U шин (фаза C)";
            case 3: return "U линии (фаза А)";
            case 4: return "U линии (фаза B)";
            case 5: return "U линии (фаза C)";
            case 6: return "I линии нач. (фаза А)";
            case 7: return "I линии нач. (фаза B)";
            case 8: return "I линии нач. (фаза C)";
            case 9: return "I линии кон. (фаза А)";
            case 10: return "I линии кон. (фаза B)";
            case 11: return "I линии кон. (фаза C)";
            default: return originalnoeImya; // Если каналов больше 12, оставляем как есть
        }
    }

    private void Form1_Load(object sender, EventArgs e)
    {

    }

    private void formsPlot1_Load(object sender, EventArgs e)
    {

    }
    private void VychislitSimmetrichnye()
    {
        if (currentRecord == null || currentRecord.Dannye.Count < 200) return;

        // 1. Параметры: 50 Гц, шаг времени из файла
        int pointsPerPeriod = (int)(1.0 / (50 * currentRecord.ShagVremeni));
        int totalPoints = currentRecord.Dannye.Count;

        // Массивы для хранения результатов (модули векторов)
        double[] i1_mag = new double[totalPoints - pointsPerPeriod];
        double[] i2_mag = new double[totalPoints - pointsPerPeriod];
        double[] i0_mag = new double[totalPoints - pointsPerPeriod];
        double[] timeAxis = new double[totalPoints - pointsPerPeriod];

        // Индексы фазных токов (7, 8, 9 каналы -> индексы 6, 7, 8)
        int idxA = 6, idxB = 7, idxC = 8;

        // 2. Цикл скользящего окна
        for (int i = 0; i < totalPoints - pointsPerPeriod; i++)
        {
            // Выделяем массивы для Фурье
            double[] sliceA = currentRecord.Dannye.Select(row => row[idxA]).Skip(i).Take(pointsPerPeriod).ToArray();
            double[] sliceB = currentRecord.Dannye.Select(row => row[idxB]).Skip(i).Take(pointsPerPeriod).ToArray();
            double[] sliceC = currentRecord.Dannye.Select(row => row[idxC]).Skip(i).Take(pointsPerPeriod).ToArray();

            // Получаем комплексные фазоры (используем наш класс Vychisleniya_RZA)
            var fA = Vychisleniya_RZA.Garmonika(sliceA, 0, pointsPerPeriod);
            var fB = Vychisleniya_RZA.Garmonika(sliceB, 0, pointsPerPeriod);
            var fC = Vychisleniya_RZA.Garmonika(sliceC, 0, pointsPerPeriod);

            // Считаем последовательности
            var (i0, i1, i2) = Vychisleniya_RZA.Simmetrichnye(fA, fB, fC);

            // Записываем действующие значения (Magnitude / sqrt(2))
            i1_mag[i] = i1.Magnitude / Math.Sqrt(2);
            i2_mag[i] = i2.Magnitude / Math.Sqrt(2);
            i0_mag[i] = i0.Magnitude / Math.Sqrt(2);

            timeAxis[i] = i * currentRecord.ShagVremeni;
        }

        // 3. Отрисовка на втором графике
        OtrisovatAnaliz(timeAxis, i1_mag, i2_mag, i0_mag);
    }
    private void OtrisovatAnaliz(double[] time, double[] i1, double[] i2, double[] i0)
    {
        formsPlotAnalysis.Plot.Clear();

        // Прямая последовательность - обычно красная
        var sig1 = formsPlotAnalysis.Plot.Add.Signal(i1);
        sig1.Data.Period = currentRecord.ShagVremeni;
        sig1.LegendText = "I1 (Прямая)";
        sig1.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Red);

        // Обратная - синяя
        var sig2 = formsPlotAnalysis.Plot.Add.Signal(i2);
        sig2.Data.Period = currentRecord.ShagVremeni;
        sig2.LegendText = "I2 (Обратная)";
        sig2.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Blue);

        // Нулевая - зеленая или черная
        var sig0 = formsPlotAnalysis.Plot.Add.Signal(i0);
        sig0.Data.Period = currentRecord.ShagVremeni;
        sig0.LegendText = "I0 (Нулевая)";
        sig0.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Green);

        // Настройка осей
        formsPlotAnalysis.Plot.Title("Симметричные составляющие тока");
        formsPlotAnalysis.Plot.XLabel("Время, сек");
        formsPlotAnalysis.Plot.YLabel("Действующее значение, А");
        formsPlotAnalysis.Plot.Legend.IsVisible = true;

        formsPlotAnalysis.Plot.Axes.AutoScale();
        formsPlotAnalysis.Refresh();
    }
}