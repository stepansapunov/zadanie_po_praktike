using System.IO;
using System.Linq;
using System.Drawing;
using ScottPlot;
using System.Numerics;
using Color = System.Drawing.Color;

namespace Osnovnoi_proekt;

public partial class Form1 : Form
{
    // Переменная для хранения текущей записи
    private Model_COMTRADE? currentRecord;

    public Form1()
    {
        InitializeComponent();
        // Подписываемся на событие изменения галочки
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
                // 1. Загружаем данные
                currentRecord = Chtenie_COMTRADE.Prochitat(cfgPath, datPath);

                // 2. Обновляем список сигналов (левая панель)
                clbSignals.Items.Clear();
                for (int i = 0; i < currentRecord.Kanaly.Count; i++)
                {
                    var k = currentRecord.Kanaly[i];
                    string krasivoeImya = PoluchitPonyatnoeImya(k.Nazvanie, k.Edinicy);
                    clbSignals.Items.Add($"{i + 1}: {krasivoeImya} [{k.Edinicy}]");
                }

                // 3. ДИНАМИЧЕСКИЙ ПОИСК КАНАЛОВ ТОКА
                // Ищем все каналы, где в названии есть "I", "Ток" или "Cur"
                List<int> tokiIdx = new List<int>();
                for (int i = 0; i < currentRecord.Kanaly.Count; i++)
                {
                    var k = currentRecord.Kanaly[i];
                    string name = k.Nazvanie.ToUpper();
                    string unit = k.Edinicy.ToUpper();

                    // Ищем токи по трем признакам:
                    // 1. В названии есть I (латинская) или ТОК
                    // 2. В единицах измерения есть A (латинская) или А (русская)
                    if (name.Contains("I") || name.Contains("ТОК") || name.Contains("CUR") ||
                        unit == "A" || unit == "А" || unit.Contains("[A]"))
                    {
                        tokiIdx.Add(i);
                    }
                }

                // 4. ЗАПОЛНЯЕМ ВКЛАДКУ "ОТЧЕТ"
                txtInfo.Clear();
                txtInfo.AppendText($"=== ОТЧЕТ ПО ФАЙЛУ: {Path.GetFileName(cfgPath)} ===\r\n\r\n");

                if (tokiIdx.Count > 0)
                {
                    // Считаем ударный ток (абсолютный максимум по всем найденным фазам)
                    double maxUdarnyi = currentRecord.Dannye.Max(row =>
                    {
                        double rowMax = 0;
                        foreach (int idx in tokiIdx)
                        {
                            if (idx < row.Length)
                                rowMax = Math.Max(rowMax, Math.Abs(row[idx]));
                        }
                        return rowMax;
                    });

                    txtInfo.AppendText($"УДАРНЫЙ ТОК КЗ (i_уд): {maxUdarnyi:F2} А\r\n");
                    txtInfo.AppendText("------------------------------------------\r\n");
                    txtInfo.AppendText("ПИКОВЫЕ ЗНАЧЕНИЯ ПО НАЙДЕННЫМ ФАЗАМ:\r\n");

                    // Выводим пики по каждому найденному каналу тока
                    foreach (int idx in tokiIdx)
                    {
                        var k = currentRecord.Kanaly[idx];

                        // Используем ту же функцию для красоты, что и в списке сигналов
                        string krasivoeImya = PoluchitPonyatnoeImya(k.Nazvanie, k.Edinicy);

                        double maxFaza = currentRecord.Dannye.Max(row => Math.Abs(row[idx]));

                        // Выводим в отчет уже "понятное" имя
                        txtInfo.AppendText($"{krasivoeImya}: {maxFaza,10:F2} {k.Edinicy}\r\n");
                    }
                }
                else
                {
                    txtInfo.AppendText("ВНИМАНИЕ: Каналы тока для расчета не определены.\r\n");
                    txtInfo.AppendText("Проверьте названия сигналов в файле.\r\n");
                }

                // 5. ГРАФИКИ
                formsPlot1.Plot.Clear();
                formsPlot1.Refresh();
            }
        }
    }

    private void clbSignals_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        BeginInvoke((MethodInvoker)delegate
        {
            OtrisovatVybrannye();
        });
    }

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
            string name = PoluchitPonyatnoeImya(currentRecord.Kanaly[idx].Nazvanie, currentRecord.Kanaly[idx].Edinicy);
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

        double dlitelnost = currentRecord.Dannye.Count * currentRecord.ShagVremeni;
        formsPlot1.Plot.Axes.Margins(0, 0.1);
        formsPlot1.Plot.Axes.SetLimitsX(0, dlitelnost);
        formsPlot1.Plot.Axes.AutoScaleY();

        formsPlot1.Plot.Axes.Left.IsVisible = estToki;
        formsPlot1.Plot.YLabel(estToki ? "Ток, А" : "", size: 24);
        formsPlot1.Plot.Axes.Right.IsVisible = estNapryazheniya;
        formsPlot1.Plot.Axes.Right.Label.Text = estNapryazheniya ? "Напряжение, В" : "";
        formsPlot1.Plot.Axes.Right.Label.FontSize = 24; // Устанавливаем размер для правой оси

        formsPlot1.Plot.Title("Осциллограмма выбранных сигналов", size: 24);
        formsPlot1.Plot.XLabel("Время, секунды", size: 24);
        formsPlot1.Plot.Legend.IsVisible = true;
        formsPlot1.Plot.Legend.Alignment = Alignment.LowerLeft; // Переносим вниз-влево
        formsPlot1.Plot.Legend.FontSize = 30; // Заодно держим размер, как договорились
        formsPlot1.Refresh();

    }

    private string PoluchitPonyatnoeImya(string originalName, string unit)
    {
        string name = originalName.ToUpper();
        string u = unit.ToUpper();
        string faza = "?";

        // 1. ОПРЕДЕЛЯЕМ ФАЗУ
        if (name.Contains("A")) faza = "A";
        else if (name.Contains("B")) faza = "B";
        else if (name.Contains("C")) faza = "C";

        // 2. ЕСЛИ ЭТО НАПРЯЖЕНИЕ (V или В)
        if (u.Contains("V") || u.Contains("В"))
        {
            // Если в коде есть 0002 и НЕТ тире (значит не линия)
            if (name.Contains("0002") && !name.Contains("-")) return $"U шин (фаза {faza})";
            if (name.Contains("0003")) return $"U линии (фаза {faza})";
            return $"Напряжение (фаза {faza})";
        }

        // 3. ЕСЛИ ЭТО ТОК (A или А)
        if (u.Contains("A") || u.Contains("А"))
        {
            // 0007 или наличие тире обычно говорит о токе линии
            if (name.Contains("0007") || name.Contains("-"))
            {
                if (name.Contains("0007")) return $"I линии нач. (фаза {faza})";
                if (name.Contains("0004")) return $"I линии кон. (фаза {faza})";
                return $"I линии (фаза {faza})";
            }
            return $"Ток (фаза {faza})";
        }

        return originalName;
    }

    private void VychislitSimmetrichnye()
    {
        // --- 1. ВАЛИДАЦИЯ (Защита от дурака) ---
        if (currentRecord == null)
        {
            MessageBox.Show("Файл не загружен!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Считаем точки на период (20 мс) строго по шагу текущего файла
        // Используем Round, чтобы не было "дрожания" из-за погрешности double
        int pointsPerPeriod = (int)Math.Round(0.02 / currentRecord.ShagVremeni);

        if (currentRecord.Dannye.Count < pointsPerPeriod || pointsPerPeriod <= 0)
        {
            MessageBox.Show("Недостаточно данных для анализа (нужно минимум 20 мс).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // --- 2. ДИНАМИЧЕСКИЙ ПОИСК ИНДЕКСОВ (Всеядность) ---
        int idxUA = -1, idxUB = -1, idxUC = -1;
        int idxIA = -1, idxIB = -1, idxIC = -1;

        for (int i = 0; i < currentRecord.Kanaly.Count; i++)
        {
            var k = currentRecord.Kanaly[i];
            // Прогоняем через нашу "умную" функцию и в верхний регистр
            string name = PoluchitPonyatnoeImya(k.Nazvanie, k.Edinicy).ToUpper();

            bool isU = name.Contains("U") || name.Contains("НАПР");
            bool isI = name.Contains("I") || name.Contains("ТОК");

            // Ищем фазу (латиница + кириллица)
            if (isU)
            {
                if (name.Contains("A") || name.Contains("А")) idxUA = i;
                if (name.Contains("B") || name.Contains("В")) idxUB = i;
                if (name.Contains("C") || name.Contains("С")) idxUC = i;
            }
            if (isI)
            {
                if (name.Contains("A") || name.Contains("А")) idxIA = i;
                if (name.Contains("B") || name.Contains("В")) idxIB = i;
                if (name.Contains("C") || name.Contains("С")) idxIC = i;
            }
        }

        // --- 3. ПОДГОТОВКА ДАННЫХ ---
        int totalPoints = currentRecord.Dannye.Count;
        int length = totalPoints - pointsPerPeriod;

        // Результаты
        double[] i1_mag = new double[length], i2_mag = new double[length], i0_mag = new double[length];
        double[] u1_mag = new double[length], u0_mag = new double[length];
        double[] timeAxis = new double[length];

        // Вытягиваем сырые данные в плоские массивы (если индекс -1, массив будет из нулей)
        double[] rawUA = idxUA != -1 ? currentRecord.Dannye.Select(r => r[idxUA]).ToArray() : new double[totalPoints];
        double[] rawUB = idxUB != -1 ? currentRecord.Dannye.Select(r => r[idxUB]).ToArray() : new double[totalPoints];
        double[] rawUC = idxUC != -1 ? currentRecord.Dannye.Select(r => r[idxUC]).ToArray() : new double[totalPoints];
        double[] rawIA = idxIA != -1 ? currentRecord.Dannye.Select(r => r[idxIA]).ToArray() : new double[totalPoints];
        double[] rawIB = idxIB != -1 ? currentRecord.Dannye.Select(r => r[idxIB]).ToArray() : new double[totalPoints];
        double[] rawIC = idxIC != -1 ? currentRecord.Dannye.Select(r => r[idxIC]).ToArray() : new double[totalPoints];

        // --- 4. ОСНОВНОЙ ЦИКЛ РАСЧЕТА ---
        for (int i = 0; i < length; i++)
        {
            // Вырезаем окна по актуальному pointsPerPeriod
            double[] sIA = rawIA.Skip(i).Take(pointsPerPeriod).ToArray();
            double[] sIB = rawIB.Skip(i).Take(pointsPerPeriod).ToArray();
            double[] sIC = rawIC.Skip(i).Take(pointsPerPeriod).ToArray();

            // Считаем токи (Гармоника -> Симметричные)
            var fIA = Vychisleniya_RZA.Garmonika(sIA, 0, pointsPerPeriod);
            var fIB = Vychisleniya_RZA.Garmonika(sIB, 0, pointsPerPeriod);
            var fIC = Vychisleniya_RZA.Garmonika(sIC, 0, pointsPerPeriod);
            var resI = Vychisleniya_RZA.Simmetrichnye(fIA, fIB, fIC);

            // Переводим в действующие значения (Magnitude / sqrt(2))
            i1_mag[i] = resI.i1.Magnitude / Math.Sqrt(2);
            i2_mag[i] = resI.i2.Magnitude / Math.Sqrt(2);
            i0_mag[i] = resI.i0.Magnitude / Math.Sqrt(2);

            // Вырезаем окна для напряжений
            double[] sUA = rawUA.Skip(i).Take(pointsPerPeriod).ToArray();
            double[] sUB = rawUB.Skip(i).Take(pointsPerPeriod).ToArray();
            double[] sUC = rawUC.Skip(i).Take(pointsPerPeriod).ToArray();

            var fUA = Vychisleniya_RZA.Garmonika(sUA, 0, pointsPerPeriod);
            var fUB = Vychisleniya_RZA.Garmonika(sUB, 0, pointsPerPeriod);
            var fUC = Vychisleniya_RZA.Garmonika(sUC, 0, pointsPerPeriod);
            var resU = Vychisleniya_RZA.Simmetrichnye(fUA, fUB, fUC);

            u1_mag[i] = resU.i1.Magnitude / Math.Sqrt(2);
            u0_mag[i] = resU.i0.Magnitude / Math.Sqrt(2);

            // Ось времени
            timeAxis[i] = i * currentRecord.ShagVremeni;
        }

        // 5. ОТРИСОВКА
        OtrisovatAnaliz(timeAxis, i1_mag, i2_mag, i0_mag, u1_mag, u0_mag);
    }

    private void OtrisovatAnaliz(double[] time, double[] i1, double[] i2, double[] i0, double[] u1, double[] u0)
    {
        // 1. График ТОКОВ (Верхний)
        formsPlotCurrents.Plot.Clear();
        var sI1 = formsPlotCurrents.Plot.Add.Signal(i1);
        sI1.Data.Period = currentRecord.ShagVremeni;
        sI1.LegendText = "I1 (Прямая)";
        sI1.Color = ScottPlot.Color.FromColor(Color.Red);

        var sI2 = formsPlotCurrents.Plot.Add.Signal(i2);
        sI2.Data.Period = currentRecord.ShagVremeni;
        sI2.LegendText = "I2 (Обратная)";
        sI2.Color = ScottPlot.Color.FromColor(Color.Blue);

        var sI0 = formsPlotCurrents.Plot.Add.Signal(i0);
        sI0.Data.Period = currentRecord.ShagVremeni;
        sI0.LegendText = "I0 (Нулевая)";
        sI0.Color = ScottPlot.Color.FromColor(Color.Green);

        formsPlotCurrents.Plot.Title("Симметричные составляющие ТОКА", size: 24);
        formsPlotCurrents.Plot.YLabel("Ток, А", size: 24);
        formsPlotCurrents.Plot.Legend.IsVisible = true;
        formsPlotCurrents.Plot.Axes.AutoScale();
        formsPlotCurrents.Plot.Legend.Alignment = Alignment.LowerLeft; // Сюда
        formsPlotCurrents.Plot.Legend.FontSize = 30;
        formsPlotCurrents.Refresh();

        // 2. График НАПРЯЖЕНИЙ (Нижний)
        formsPlotVoltages.Plot.Clear();
        var sU1 = formsPlotVoltages.Plot.Add.Signal(u1);
        sU1.Data.Period = currentRecord.ShagVremeni;
        sU1.LegendText = "U1 (Прямая)";
        sU1.Color = ScottPlot.Color.FromColor(Color.Red);

        var sU0 = formsPlotVoltages.Plot.Add.Signal(u0);
        sU0.Data.Period = currentRecord.ShagVremeni;
        sU0.LegendText = "U0 (Нулевая)";
        sU0.Color = ScottPlot.Color.FromColor(Color.Green);

        formsPlotVoltages.Plot.Title("Симметричные составляющие НАПРЯЖЕНИЯ", size: 24);
        formsPlotVoltages.Plot.YLabel("Напряжение, В", size: 24);
        formsPlotVoltages.Plot.XLabel("Время, сек", size: 24);
        formsPlotVoltages.Plot.Legend.IsVisible = true;
        formsPlotVoltages.Plot.Axes.AutoScale();
        formsPlotVoltages.Plot.Legend.Alignment = Alignment.LowerLeft; // И сюда
        formsPlotVoltages.Plot.Legend.FontSize = 30;
        formsPlotVoltages.Refresh();
    }

    private void btnCalculate_Click(object sender, EventArgs e)
    {
        VychislitSimmetrichnye();
    }

    private void выполнитьРасчетТКЗToolStripMenuItem_Click(object sender, EventArgs e)
    {
        VychislitSimmetrichnye(); // Твой метод расчёта
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        // 1. Проверяем, есть ли что сохранять
        if (currentRecord == null)
        {
            MessageBox.Show("Сначала откройте исходный файл!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // 2. Смотрим, какие галочки стоят в списке
        List<int> selected = new List<int>();
        foreach (int index in clbSignals.CheckedIndices)
        {
            selected.Add(index);
        }

        if (selected.Count == 0)
        {
            MessageBox.Show("Выберите хотя бы один сигнал галочкой!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // 3. Открываем окно выбора пути
        SaveFileDialog sfd = new SaveFileDialog();
        sfd.Filter = "COMTRADE CFG (*.cfg)|*.cfg";
        sfd.Title = "Сохранить выбранные сигналы";
        sfd.FileName = "Result_Analysis"; // Имя по умолчанию

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            // Убираем лишние расширения из пути
            string path = sfd.FileName.Replace(".cfg", "").Replace(".CFG", "");

            try
            {
                // Магия нашего нового класса!
                Zapis_COMTRADE.Sohranit(path, currentRecord, selected);

                MessageBox.Show("Файлы .cfg и .dat успешно созданы!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}