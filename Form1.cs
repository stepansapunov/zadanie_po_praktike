using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace Osnovnoi_proekt;

public partial class Form1 : Form
{
    // Объект текущей осциллограммы и путь к исходному конфигурационному файлу
    private Model_COMTRADE? currentRecord;
    private string? currentFilePath;

    public Form1()
    {
        InitializeComponent();
        // Регистрация обработчика события изменения состояния выбора сигналов
        clbSignals.ItemCheck += clbSignals_ItemCheck;
    }

    // Обработка выбора и загрузки файлов COMTRADE через диалоговое окно
    private void btnOpen_Click_1(object sender, EventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "Файлы конфигурации (*.cfg)|*.cfg";

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            string cfgPath = ofd.FileName;
            currentFilePath = cfgPath;
            string datPath = cfgPath.Replace(".cfg", ".dat").Replace(".CFG", ".DAT");

            if (File.Exists(datPath))
            {
                // Инициализация процесса парсинга структуры файлов CFG/DAT
                currentRecord = Chtenie_COMTRADE.Prochitat(cfgPath, datPath);

                // Динамическое формирование списка доступных сигналов в интерфейсе
                clbSignals.Items.Clear();
                for (int i = 0; i < currentRecord.Kanaly.Count; i++)
                {
                    var k = currentRecord.Kanaly[i];
                    string krasivoeImya = PoluchitPonyatnoeImya(i, k.Nazvanie, k.Edinicy);
                    clbSignals.Items.Add($"{i + 1}: {krasivoeImya} [{k.Edinicy}]");
                }

                // Идентификация измерительных каналов по физическому типу (Ток/Напряжение)
                List<int> tokiIdx = new List<int>();
                List<int> napryazhIdx = new List<int>();

                for (int i = 0; i < currentRecord.Kanaly.Count; i++)
                {
                    var k = currentRecord.Kanaly[i];
                    string name = k.Nazvanie.ToUpper();
                    string unit = k.Edinicy.ToUpper();

                    if (name.Contains("I") || name.Contains("ТОК") || unit == "A" || unit == "А")
                    {
                        tokiIdx.Add(i);
                    }
                    else if (name.Contains("U") || name.Contains("НАПР") || unit == "V" || unit == "В")
                    {
                        napryazhIdx.Add(i);
                    }
                }

                // Формирование информационного отчета по параметрам загруженного файла
                txtInfo.Clear();
                txtInfo.AppendText($"=== ОТЧЕТ ПО ФАЙЛУ: {Path.GetFileName(cfgPath)} ===\r\n\r\n");

                // Вычисление и вывод амплитудных (пиковых) параметров токовых цепей
                if (tokiIdx.Count > 0)
                {
                    double maxUdarnyi = currentRecord.Dannye.Max(row =>
                        tokiIdx.Max(idx => Math.Abs(row[idx])));

                    txtInfo.AppendText($"УДАРНЫЙ ТОК КЗ (i_уд): {maxUdarnyi:F2} А\r\n");
                    txtInfo.AppendText("------------------------------------------\r\n");
                    txtInfo.AppendText("ПИКОВЫЕ ЗНАЧЕНИЯ ТОКОВ:\r\n");

                    foreach (int idx in tokiIdx)
                    {
                        var k = currentRecord.Kanaly[idx];
                        string krasivoeImya = PoluchitPonyatnoeImya(idx, k.Nazvanie, k.Edinicy);
                        double maxFaza = currentRecord.Dannye.Max(row => Math.Abs(row[idx]));
                        txtInfo.AppendText($"{krasivoeImya}: {maxFaza,10:F2} {k.Edinicy}\r\n");
                    }
                }

                if (tokiIdx.Count > 0 && napryazhIdx.Count > 0) txtInfo.AppendText("\r\n");

                // Вычисление и вывод амплитудных параметров цепей напряжения
                if (napryazhIdx.Count > 0)
                {
                    txtInfo.AppendText("ПИКОВЫЕ ЗНАЧЕНИЯ НАПРЯЖЕНИЙ:\r\n");
                    foreach (int idx in napryazhIdx)
                    {
                        var k = currentRecord.Kanaly[idx];
                        string krasivoeImya = PoluchitPonyatnoeImya(idx, k.Nazvanie, k.Edinicy);
                        double maxFaza = currentRecord.Dannye.Max(row => Math.Abs(row[idx]));
                        txtInfo.AppendText($"{krasivoeImya}: {maxFaza,10:F2} {k.Edinicy}\r\n");
                    }
                }

                if (tokiIdx.Count == 0 && napryazhIdx.Count == 0)
                {
                    txtInfo.AppendText("ВНИМАНИЕ: Аналоговые каналы не распознаны.\r\n");
                }

                // Сброс и очистка графических областей перед новой визуализацией
                formsPlot1.Plot.Clear();
                formsPlot1.Refresh();

                formsPlotCurrents.Plot.Clear();
                formsPlotCurrents.Refresh();

                formsPlotVoltages.Plot.Clear();
                formsPlotVoltages.Refresh();
            }
        }
    }

    // Делегирование обновления графиков при взаимодействии с чек-листом
    private void clbSignals_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        BeginInvoke((MethodInvoker)delegate
        {
            OtrisovatVybrannye();
        });
    }

    // Основной метод визуализации выбранных пользователем осциллограмм
    private void OtrisovatVybrannye()
    {
        if (currentRecord == null || currentRecord.Dannye.Count == 0) return;

        formsPlot1.Plot.Clear();

        // Цветовая палитра для дифференциации сигналов на графике
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
            // Извлечение вектора значений для выбранного канала
            double[] yData = currentRecord.Dannye.Select(row => row[idx]).ToArray();
            var signal = formsPlot1.Plot.Add.Signal(yData);
            signal.Data.Period = currentRecord.ShagVremeni;
            signal.Color = ScottPlot.Color.FromColor(cveta[colorIdx % cveta.Length]);

            string edinicy = currentRecord.Kanaly[idx].Edinicy.ToUpper();
            string name = PoluchitPonyatnoeImya(idx, currentRecord.Kanaly[idx].Nazvanie, currentRecord.Kanaly[idx].Edinicy);
            signal.LegendText = $"{name} ({edinicy})";

            // Распределение сигналов по осям ординат в зависимости от физической природы
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

        // Настройка масштабирования и оформления координатных осей
        double dlitelnost = currentRecord.Dannye.Count * currentRecord.ShagVremeni;
        formsPlot1.Plot.Axes.Margins(0, 0.1);
        formsPlot1.Plot.Axes.SetLimitsX(0, dlitelnost);
        formsPlot1.Plot.Axes.AutoScaleY();

        formsPlot1.Plot.Axes.Left.IsVisible = estToki;
        formsPlot1.Plot.YLabel(estToki ? "Ток, А" : "", size: 24);
        formsPlot1.Plot.Axes.Right.IsVisible = estNapryazheniya;
        formsPlot1.Plot.Axes.Right.Label.Text = estNapryazheniya ? "Напряжение, В" : "";
        formsPlot1.Plot.Axes.Right.Label.FontSize = 24;

        formsPlot1.Plot.Title("Осциллограмма выбранных сигналов", size: 24);
        formsPlot1.Plot.XLabel("Время, секунды", size: 24);
        formsPlot1.Plot.Legend.IsVisible = true;
        formsPlot1.Plot.Legend.Alignment = Alignment.LowerLeft;
        formsPlot1.Plot.Legend.FontSize = 30;
        formsPlot1.Refresh();
    }

    // Алгоритм интерпретации технических наименований каналов в инженерную терминологию
    private string PoluchitPonyatnoeImya(int index, string originalName, string unit)
    {
        string name = originalName.Trim().ToUpper();
        string u = unit.Trim().ToUpper();
        string faza = "?";

        // 1. ОПРЕДЕЛЯЕМ ФАЗУ
        if (name.Contains("A") || name.Contains("А")) faza = "A";
        else if (name.Contains("B") || name.Contains("В")) faza = "B";
        else if (name.Contains("C") || name.Contains("С")) faza = "C";

        if (faza == "?") // Если буквы нет в имени, определяем по порядку (0,1,2 -> A,B,C)
        {
            int f = (index % 3);
            if (f == 0) faza = "A";
            else if (f == 1) faza = "B";
            else if (f == 2) faza = "C";
        }

        // 2. ЛОГИКА ДЛЯ НАПРЯЖЕНИЙ (V)
        if (u.Contains("V") || u.Contains("В"))
        {
            if (name.Contains("US") || name.Contains("ШИН") || name.Contains("0001") || name.Contains("0002"))
                return $"U шин (фаза {faza})";

            return $"U линии (фаза {faza})";
        }

        // 3. ЛОГИКА ДЛЯ ТОКОВ (A)
        if (u.Contains("A") || u.Contains("А"))
        {
            // Ищем маркеры конца линии
            if (name.Contains("IR") || name.Contains("КОН") || name.Contains("0004") || name.Contains("0005"))
                return $"I линии кон. (фаза {faza})";

            // По умолчанию считаем началом линии (самый частый случай)
            return $"I линии нач. (фаза {faza})";
        }

        return originalName;
    }

    // Метод комплексного анализа режима и расчета симметричных составляющих
    private void VychislitSimmetrichnye()
    {
        // 0. ПРОВЕРКА: Загружен ли файл?
        if (currentRecord == null)
        {
            MessageBox.Show("Для проведения анализа сначала необходимо открыть файл COMTRADE!",
                            "Файл не загружен",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
            return;
        }

        // 1. ГРУППИРУЕМ ВЫБРАННЫЕ КАНАЛЫ ПО ОБЪЕКТАМ
        var groups = new Dictionary<string, List<int>>();

        foreach (int i in clbSignals.CheckedIndices)
        {
            // Вместо ручной обрезки имен используем нашу "умную" функцию.
            // Она сама разберется, где начало, где конец линии, и какая там фаза.
            string prettyName = PoluchitPonyatnoeImya(i, currentRecord.Kanaly[i].Nazvanie, currentRecord.Kanaly[i].Edinicy);

            // Отрезаем часть с фазой: "I линии нач. (фаза A)" -> "I линии нач."
            // Теперь группа будет называться одинаково для всех трёх фаз.
            string groupName = prettyName.Split('(')[0].Trim();

            if (!groups.ContainsKey(groupName)) groups[groupName] = new List<int>();
            groups[groupName].Add(i);
        }

        // 2. ПРОВЕРКА: Выбрано ли хоть что-нибудь?
        if (clbSignals.CheckedIndices.Count == 0)
        {
            MessageBox.Show("Пожалуйста, выберите сигналы галочками (минимум 3 фазы одного объекта) для проведения расчета.",
                            "Ничего не выбрано",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            return;
        }

        // --- Дальше идет твой блок валидации (uGroups, iGroups и т.д.) ---
        int uGroups = 0;
        int iGroups = 0;
        int idxUA = -1, idxUB = -1, idxUC = -1;
        int idxIA = -1, idxIB = -1, idxIC = -1;

        foreach (var g in groups)
        {
            if (g.Value.Count != 3)
            {
                string displayName = clbSignals.Items[g.Value[0]].ToString();
                string clearName = displayName.Split(':')[1].Split('(')[0].Trim();

                MessageBox.Show($"Ошибка в объекте '{clearName}'!\n\n" +
                                $"Для расчета симметричных составляющих нужно выбрать все три фазы (A, B, C).\n" +
                                $"Сейчас для этого объекта выбрано фаз: {g.Value.Count}.",
                                "Недостаточно данных",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            int a = -1, b = -1, c = -1;
            foreach (int idx in g.Value)
            {
                string name = PoluchitPonyatnoeImya(idx, currentRecord.Kanaly[idx].Nazvanie, currentRecord.Kanaly[idx].Edinicy).ToUpper();
                string faza = currentRecord.Kanaly[idx].Faza.ToUpper().Trim();

                if (name.Contains("ФАЗА A") || name.Contains("ФАЗА А") || faza == "A" || faza == "А") a = idx;
                else if (name.Contains("ФАЗА B") || name.Contains("ФАЗА В") || faza == "B" || faza == "В") b = idx;
                else if (name.Contains("ФАЗА C") || name.Contains("ФАЗА С") || faza == "C" || faza == "С") c = idx;
            }

            if (a == -1 || b == -1 || c == -1)
            {
                MessageBox.Show($"В группе '{g.Key}' не удалось однозначно определить фазы A, B и C.\nПроверьте, что выбраны разные фазы одного объекта.",
                                "Ошибка фазировки",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            string unit = currentRecord.Kanaly[a].Edinicy.ToUpper();
            if (unit.Contains("V") || unit.Contains("В")) { uGroups++; idxUA = a; idxUB = b; idxUC = c; }
            else { iGroups++; idxIA = a; idxIB = b; idxIC = c; }
        }

        if (uGroups == 0 && iGroups == 0)
        {
            MessageBox.Show("Не удалось найти полную группу из 3-х фаз (A, B, C). проверьте названия каналов!", "Ошибка");
            return;
        }

        if (uGroups > 1 || iGroups > 1)
        {
            MessageBox.Show("Нельзя выбирать сразу две разные группы напряжений или токов.\n" +
                            "Выберите три фазы только одного напряжения и/или три фазы только одного тока.", "Слишком много данных");
            return;
        }

        // Основной вычислительный цикл анализа установившегося режима методом Фортескью
        int pointsPerPeriod = (int)Math.Round(0.02 / currentRecord.ShagVremeni);
        int totalPoints = currentRecord.Dannye.Count;
        int length = totalPoints - pointsPerPeriod;

        if (length <= 0) return;

        double[] i1 = new double[length], i2 = new double[length], i0 = new double[length];
        double[] u1 = new double[length], u0 = new double[length];
        double[] timeAxis = new double[length];

        double[] rawUA = uGroups > 0 ? currentRecord.Dannye.Select(r => r[idxUA]).ToArray() : new double[totalPoints];
        double[] rawUB = uGroups > 0 ? currentRecord.Dannye.Select(r => r[idxUB]).ToArray() : new double[totalPoints];
        double[] rawUC = uGroups > 0 ? currentRecord.Dannye.Select(r => r[idxUC]).ToArray() : new double[totalPoints];
        double[] rawIA = iGroups > 0 ? currentRecord.Dannye.Select(r => r[idxIA]).ToArray() : new double[totalPoints];
        double[] rawIB = iGroups > 0 ? currentRecord.Dannye.Select(r => r[idxIB]).ToArray() : new double[totalPoints];
        double[] rawIC = iGroups > 0 ? currentRecord.Dannye.Select(r => r[idxIC]).ToArray() : new double[totalPoints];

        for (int i = 0; i < length; i++)
        {
            if (iGroups > 0)
            {
                var fA = Vychisleniya_RZA.Garmonika(rawIA.Skip(i).Take(pointsPerPeriod).ToArray(), 0, pointsPerPeriod);
                var fB = Vychisleniya_RZA.Garmonika(rawIB.Skip(i).Take(pointsPerPeriod).ToArray(), 0, pointsPerPeriod);
                var fC = Vychisleniya_RZA.Garmonika(rawIC.Skip(i).Take(pointsPerPeriod).ToArray(), 0, pointsPerPeriod);
                var res = Vychisleniya_RZA.Simmetrichnye(fA, fB, fC);
                i1[i] = res.i1.Magnitude / Math.Sqrt(2);
                i2[i] = res.i2.Magnitude / Math.Sqrt(2);
                i0[i] = res.i0.Magnitude / Math.Sqrt(2);
            }

            if (uGroups > 0)
            {
                var fA = Vychisleniya_RZA.Garmonika(rawUA.Skip(i).Take(pointsPerPeriod).ToArray(), 0, pointsPerPeriod);
                var fB = Vychisleniya_RZA.Garmonika(rawUB.Skip(i).Take(pointsPerPeriod).ToArray(), 0, pointsPerPeriod);
                var fC = Vychisleniya_RZA.Garmonika(rawUC.Skip(i).Take(pointsPerPeriod).ToArray(), 0, pointsPerPeriod);
                var res = Vychisleniya_RZA.Simmetrichnye(fA, fB, fC);
                u1[i] = res.i1.Magnitude / Math.Sqrt(2);
                u0[i] = res.i0.Magnitude / Math.Sqrt(2);
            }
            timeAxis[i] = i * currentRecord.ShagVremeni;
        }

        OtrisovatAnaliz(timeAxis, i1, i2, i0, u1, u0);

        // Генерация сводного технического отчета по результатам анализа ТКЗ
        txtInfo.Clear();
        string fileName = System.IO.Path.GetFileName(currentFilePath ?? "Unknown.cfg");
        txtInfo.AppendText($"=== ОТЧЕТ ПО ФАЙЛУ: {fileName} ===\r\n\r\n");

        if (iGroups > 0)
        {
            double i_ud = Math.Max(rawIA.Max(), Math.Max(rawIB.Max(), rawIC.Max()));
            txtInfo.AppendText($"УДАРНЫЙ ТОК КЗ (i_уд): {i_ud:F2} A\r\n");
            txtInfo.AppendText("------------------------------------------\r\n");
            txtInfo.AppendText("ПИКОВЫЕ ЗНАЧЕНИЯ ВЫБРАННЫХ ФАЗ:\r\n");
            txtInfo.AppendText($"Ток A: {rawIA.Max(),10:F2} A\r\n");
            txtInfo.AppendText($"Ток B: {rawIB.Max(),10:F2} A\r\n");
            txtInfo.AppendText($"Ток C: {rawIC.Max(),10:F2} A\r\n");
        }

        if (iGroups > 0 && uGroups > 0) txtInfo.AppendText("\r\n");

        if (uGroups > 0)
        {
            txtInfo.AppendText($"Напр A: {rawUA.Max(),10:F2} V\r\n");
            txtInfo.AppendText($"Напр B: {rawUB.Max(),10:F2} V\r\n");
            txtInfo.AppendText($"Напр C: {rawUC.Max(),10:F2} V\r\n");
        }

        txtInfo.AppendText("\r\n РАСЧЕТ ПОСЛЕДОВАТЕЛЬНОСТЕЙ \r\n");
        if (iGroups > 0)
        {
            txtInfo.AppendText($"I1 (прямая) max:   {i1.Max(),10:F2} A\r\n");
            txtInfo.AppendText($"I2 (обратная) max: {i2.Max(),10:F2} A\r\n");
            txtInfo.AppendText($"I0 (нулевая) max:  {i0.Max(),10:F2} A\r\n");
        }

        if (iGroups > 0 && uGroups > 0) txtInfo.AppendText("\r\n");

        if (uGroups > 0)
        {
            txtInfo.AppendText($"U1 (прямая) max:   {u1.Max(),10:F2} V\r\n");
            txtInfo.AppendText($"U0 (нулевая) max:  {u0.Max(),10:F2} V\r\n");
        }
        txtInfo.AppendText("\r\n");
    }

    // Визуализация временных диаграмм симметричных составляющих токов и напряжений
    private void OtrisovatAnaliz(double[] time, double[] i1, double[] i2, double[] i0, double[] u1, double[] u0)
    {
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
        formsPlotCurrents.Plot.Legend.Alignment = Alignment.LowerLeft;
        formsPlotCurrents.Plot.Legend.FontSize = 30;
        formsPlotCurrents.Refresh();

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
        formsPlotVoltages.Plot.Legend.Alignment = Alignment.LowerLeft;
        formsPlotVoltages.Plot.Legend.FontSize = 30;
        formsPlotVoltages.Refresh();
    }

    private void btnCalculate_Click(object sender, EventArgs e)
    {
        VychislitSimmetrichnye();
    }

    private void выполнитьРасчетТКЗToolStripMenuItem_Click(object sender, EventArgs e)
    {
        VychislitSimmetrichnye();
    }

    // Сохранение выбранной области данных в новый файл формата COMTRADE
    private void btnSave_Click(object sender, EventArgs e)
    {
        if (currentRecord == null)
        {
            MessageBox.Show("Сначала откройте исходный файл!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

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

        SaveFileDialog sfd = new SaveFileDialog();
        sfd.Filter = "COMTRADE CFG (*.cfg)|*.cfg";
        sfd.Title = "Сохранить выбранные сигналы";
        sfd.FileName = "Result_Analysis";

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            string path = sfd.FileName.Replace(".cfg", "").Replace(".CFG", "");

            try
            {
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