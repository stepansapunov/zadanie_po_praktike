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
    public enum TipOtcheta { Toki, Napryazheniya, VidKZ, Vse, Analiz }

    public Form1()
    {
        InitializeComponent();

        clbSignals.ItemCheck += clbSignals_ItemCheck;

        // --- НАСТРОЙКА КОНТЕКСТНОГО МЕНЮ (Пункт 2.5.5) ---
        ContextMenuStrip menu = new ContextMenuStrip();

        // ПУНКТ 1: Выполнить расчет ТКЗ (Самый быстрый путь - всё и сразу)
        menu.Items.Add("Выполнить расчёт ТКЗ и построить графики", null, (s, e) => VychislitSimmetrichnye(TipOtcheta.Analiz));

        // Добавляем разделитель для красоты
        menu.Items.Add(new ToolStripSeparator());

        // ПУНКТ 2: Выполнить обработку сигналов (Выпадающее меню для частных случаев)
        var subMenu = new ToolStripMenuItem("Выполнить обработку сигналов");
        subMenu.DropDownItems.Add("Рассчитать токи (I1, I2, I0)", null, (s, e) => VychislitSimmetrichnye(TipOtcheta.Toki));
        subMenu.DropDownItems.Add("Рассчитать напряжения (U1, U0)", null, (s, e) => VychislitSimmetrichnye(TipOtcheta.Napryazheniya));
        subMenu.DropDownItems.Add("Определить ударный ток и вид КЗ", null, (s, e) => VychislitSimmetrichnye(TipOtcheta.VidKZ));
        subMenu.DropDownItems.Add("Вывести полный технический отчет", null, (s, e) => VychislitSimmetrichnye(TipOtcheta.Vse));

        menu.Items.Add(subMenu);

        // Привязываем всё это к списку сигналов
        clbSignals.ContextMenuStrip = menu;
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

                // Формирование краткого информационного отчета при загрузке файла
                txtInfo.Clear();
                txtInfo.AppendText($"=== ФАЙЛ ЗАГРУЖЕН: {Path.GetFileName(cfgPath)} ===\r\n\r\n");

                // Выводим только общую статистику по каналам
                if (tokiIdx.Count > 0 || napryazhIdx.Count > 0)
                {
                    txtInfo.AppendText($"Обнаружено каналов тока: {tokiIdx.Count}\r\n");
                    txtInfo.AppendText($"Обнаружено каналов напряжения: {napryazhIdx.Count}\r\n");
                    txtInfo.AppendText("------------------------------------------\r\n");
                    txtInfo.AppendText("ИНСТРУКЦИЯ:\r\n");
                    txtInfo.AppendText("1. Отметьте галочками 3 фазы одного объекта.\r\n");
                    txtInfo.AppendText("2. Нажмите правой кнопкой мыши на список сигналов.\r\n");
                    txtInfo.AppendText("3. Выберите нужный вид обработки в меню.");
                }
                else
                {
                    txtInfo.AppendText("ВНИМАНИЕ: Аналоговые каналы не распознаны.\r\n");
                }

                if (tokiIdx.Count > 0 && napryazhIdx.Count > 0) txtInfo.AppendText("\r\n");



                // Сброс и очистка графических областей перед новой визуализацией
                plotI.Plot.Clear();
                plotI.Refresh();

                plotI.Plot.Clear();
                plotU.Plot.Clear();
                plotI.Refresh();
                plotU.Refresh();
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

        // Очищаем оба графика по новым именам
        plotI.Plot.Clear();
        plotU.Plot.Clear();

        Color[] cveta = { Color.Red, Color.Green, Color.Blue, Color.Orange, Color.Purple, Color.Cyan };
        int colorIdx = 0;

        foreach (int idx in clbSignals.CheckedIndices)
        {
            var k = currentRecord.Kanaly[idx];
            double[] yData = currentRecord.Dannye.Select(row => row[idx]).ToArray();
            string edinicy = k.Edinicy.ToUpper();

            bool isVoltage = edinicy.Contains("V") || edinicy.Contains("В") || k.Nazvanie.ToUpper().StartsWith("U");

            // Выбираем график: если напряжение — в plotU, иначе — в plotI
            var targetPlot = isVoltage ? plotU : plotI;

            var signal = targetPlot.Plot.Add.Signal(yData);
            signal.Data.Period = currentRecord.ShagVremeni;
            signal.Color = ScottPlot.Color.FromColor(cveta[colorIdx % cveta.Length]);
            signal.LegendText = PoluchitPonyatnoeImya(idx, k.Nazvanie, k.Edinicy);

            colorIdx++;
        }

        // Настройка внешнего вида
        ConfigureMainPlot(plotI, "ОСЦИЛЛОГРАММА ТОКОВ", "Ток, А");
        ConfigureMainPlot(plotU, "ОСЦИЛЛОГРАММА НАПРЯЖЕНИЙ", "Напряжение, В");

        // Включаем синхронизацию
        SyncMainPlots();
    }

    // Обновленный метод синхронизации под новые имена
    private bool isSyncingMain = false;
    private void SyncMainPlots()
    {
        var plots = new[] { plotI, plotU };
        foreach (var master in plots)
        {
            master.Plot.RenderManager.AxisLimitsChanged += (s, e) =>
            {
                if (isSyncingMain) return;
                isSyncingMain = true;
                var limits = master.Plot.Axes.GetLimits();
                foreach (var slave in plots)
                {
                    if (slave == master) continue;
                    slave.Plot.Axes.SetLimitsX(limits.Left, limits.Right);
                    slave.Refresh();
                }
                isSyncingMain = false;
            };
        }
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
    private void VychislitSimmetrichnye(TipOtcheta tip = TipOtcheta.Vse)
    {
        // 1. Очистка и заголовок — теперь это происходит ВСЕГДА первым делом
        txtInfo.Clear();
        string fileName = (currentRecord != null) ? Path.GetFileName(currentFilePath ?? "Unknown.cfg") : "Файл не выбран";
        txtInfo.AppendText($"=== ОТЧЕТ: {fileName} ===\r\n\r\n");

        if (currentRecord == null)
        {
            txtInfo.AppendText("ОШИБКА: Сначала откройте файл COMTRADE!\r\n");
            return;
        }

        if (clbSignals.CheckedIndices.Count == 0)
        {
            txtInfo.AppendText("ОШИБКА: Не выбраны сигналы (отметьте галочками фазы A, B, C).\r\n");
            return;
        }

        // 2. ГРУППИРОВКА (Твоя надежная логика)
        var groups = new Dictionary<string, List<int>>();
        foreach (int i in clbSignals.CheckedIndices)
        {
            string prettyName = PoluchitPonyatnoeImya(i, currentRecord.Kanaly[i].Nazvanie, currentRecord.Kanaly[i].Edinicy);
            string groupName = prettyName.Split('(')[0].Trim();
            if (!groups.ContainsKey(groupName)) groups[groupName] = new List<int>();
            groups[groupName].Add(i);
        }

        int uGroups = 0, iGroups = 0;
        int idxUA = -1, idxUB = -1, idxUC = -1;
        int idxIA = -1, idxIB = -1, idxIC = -1;

        foreach (var g in groups)
        {
            if (g.Value.Count != 3) continue;

            int a = -1, b = -1, c = -1;
            foreach (int idx in g.Value)
            {
                // Используем максимально широкий поиск фаз
                string name = PoluchitPonyatnoeImya(idx, currentRecord.Kanaly[idx].Nazvanie, currentRecord.Kanaly[idx].Edinicy).ToUpper();
                string faza = currentRecord.Kanaly[idx].Faza.ToUpper().Trim();

                if (name.Contains("ФАЗА A") || name.Contains("ФАЗА А") || faza == "A" || faza == "А") a = idx;
                else if (name.Contains("ФАЗА B") || name.Contains("ФАЗА В") || faza == "B" || faza == "В") b = idx;
                else if (name.Contains("ФАЗА C") || name.Contains("ФАЗА С") || faza == "C" || faza == "С") c = idx;
            }

            if (a == -1 || b == -1 || c == -1) continue;

            string unit = currentRecord.Kanaly[a].Edinicy.ToUpper();
            // Распознаем Напряжения по единицам (V, В) или имени (U)
            if (unit.Contains("V") || unit.Contains("В") || currentRecord.Kanaly[a].Nazvanie.ToUpper().StartsWith("U"))
            {
                uGroups++; idxUA = a; idxUB = b; idxUC = c;
            }
            else
            {
                iGroups++; idxIA = a; idxIB = b; idxIC = c;
            }
        }

        if (uGroups == 0 && iGroups == 0)
        {
            txtInfo.AppendText("ОШИБКА: Не удалось собрать группу из 3-х фаз (A, B, C). Проверьте выбор каналов.\r\n");
            return;
        }

        // 3. РАСЧЕТЫ
        int pointsPerPeriod = (int)Math.Round(0.02 / currentRecord.ShagVremeni);
        int length = currentRecord.Dannye.Count - pointsPerPeriod;
        if (length <= 0) return;

        double[] i1 = new double[length], i2 = new double[length], i0 = new double[length];
        double[] u1 = new double[length], u0 = new double[length];
        double[] timeAxis = new double[length];

        // Подготовка массивов (извлекаем данные один раз)
        double[] rIA = idxIA != -1 ? currentRecord.Dannye.Select(r => r[idxIA]).ToArray() : new double[0];
        double[] rIB = idxIB != -1 ? currentRecord.Dannye.Select(r => r[idxIB]).ToArray() : new double[0];
        double[] rIC = idxIC != -1 ? currentRecord.Dannye.Select(r => r[idxIC]).ToArray() : new double[0];
        double[] rUA = idxUA != -1 ? currentRecord.Dannye.Select(r => r[idxUA]).ToArray() : new double[0];
        double[] rUB = idxUB != -1 ? currentRecord.Dannye.Select(r => r[idxUB]).ToArray() : new double[0];
        double[] rUC = idxUC != -1 ? currentRecord.Dannye.Select(r => r[idxUC]).ToArray() : new double[0];

        for (int i = 0; i < length; i++)
        {
            if (iGroups > 0)
            {
                var res = Vychisleniya_RZA.Simmetrichnye(
                    Vychisleniya_RZA.Garmonika(rIA.Skip(i).Take(pointsPerPeriod).ToArray(), 0, pointsPerPeriod),
                    Vychisleniya_RZA.Garmonika(rIB.Skip(i).Take(pointsPerPeriod).ToArray(), 0, pointsPerPeriod),
                    Vychisleniya_RZA.Garmonika(rIC.Skip(i).Take(pointsPerPeriod).ToArray(), 0, pointsPerPeriod));
                i1[i] = res.i1.Magnitude / Math.Sqrt(2);
                i2[i] = res.i2.Magnitude / Math.Sqrt(2);
                i0[i] = res.i0.Magnitude / Math.Sqrt(2);
            }
            if (uGroups > 0)
            {
                var res = Vychisleniya_RZA.Simmetrichnye(
                    Vychisleniya_RZA.Garmonika(rUA.Skip(i).Take(pointsPerPeriod).ToArray(), 0, pointsPerPeriod),
                    Vychisleniya_RZA.Garmonika(rUB.Skip(i).Take(pointsPerPeriod).ToArray(), 0, pointsPerPeriod),
                    Vychisleniya_RZA.Garmonika(rUC.Skip(i).Take(pointsPerPeriod).ToArray(), 0, pointsPerPeriod));
                u1[i] = res.i1.Magnitude / Math.Sqrt(2);
                u0[i] = res.i0.Magnitude / Math.Sqrt(2);
            }
            timeAxis[i] = i * currentRecord.ShagVremeni;
        }

        

        void PrintVidKZ()
        {
            if (iGroups > 0)
            {
                double[] s = { rIA.Max(), rIB.Max(), rIC.Max() }; Array.Sort(s);
                string vid = "Не определен";
                if (s[0] > s[2] * 0.65) vid = "К3 (Трехфазное симметричное)";
                else if (i0.Max() > i1.Max() * 0.15) vid = (s[1] - s[0] > s[2] * 0.2) ? "К1-1 (Двухфазное на землю)" : "К1 (Однофазное на землю)";
                else if (s[1] > s[2] * 0.4) vid = "К2 (Двухфазное)";
                else vid = "К1 (Однофазное без земли)";

                txtInfo.AppendText("=======================================\r\n");
                txtInfo.AppendText($" АНАЛИЗ РЕЖИМА: {vid}\r\n");
                txtInfo.AppendText($" УДАРНЫЙ ТОК (i_уд): {s[2],15:F2} A\r\n");
                txtInfo.AppendText("=======================================\r\n");
            }
        }

        void PrintToki()
        {
            txtInfo.AppendText("\r\n--- ПАРАМЕТРЫ ТОКА ---\r\n");
            if (iGroups > 0)
            {
                txtInfo.AppendText($" Фаза A (пик): {rIA.Max(),15:F2} A\r\n");
                txtInfo.AppendText($" Фаза B (пик): {rIB.Max(),15:F2} A\r\n");
                txtInfo.AppendText($" Фаза C (пик): {rIC.Max(),15:F2} A\r\n");
                txtInfo.AppendText("------------------------------------------\r\n");
                txtInfo.AppendText($" I1 (прямая):  {i1.Max(),15:F2} A\r\n");
                txtInfo.AppendText($" I2 (обратная): {i2.Max(),15:F2} A\r\n");
                txtInfo.AppendText($" I0 (нулевая):  {i0.Max(),15:F2} A\r\n");
            }
            else
            {
                txtInfo.AppendText(" [!] Токовые сигналы не выбраны.\r\n");
            }
        }

        void PrintU()
        {
            txtInfo.AppendText("\r\n--- ПАРАМЕТРЫ НАПРЯЖЕНИЯ ---\r\n");
            if (uGroups > 0)
            {
                txtInfo.AppendText($" Фаза A (пик): {rUA.Max(),15:F2} V\r\n");
                txtInfo.AppendText($" Фаза B (пик): {rUB.Max(),15:F2} V\r\n");
                txtInfo.AppendText($" Фаза C (пик): {rUC.Max(),15:F2} V\r\n");
                txtInfo.AppendText("------------------------------------------\r\n");
                txtInfo.AppendText($" U1 (прямая):  {u1.Max(),15:F2} V\r\n");
                txtInfo.AppendText($" U0 (нулевая):  {u0.Max(),15:F2} V\r\n");
            }
            else
            {
                txtInfo.AppendText(" [!] Сигналы напряжения не выбраны.\r\n");
            }
        }

        // 3. ФОРМИРОВАНИЕ ИТОГОВОГО ТЕКСТА
        txtInfo.Clear();
        txtInfo.AppendText($"ОТЧЕТ ПО ФАЙЛУ: {Path.GetFileName(currentFilePath)}\r\n");

        if (tip == TipOtcheta.Toki)
            PrintToki();
        else if (tip == TipOtcheta.Napryazheniya)
            PrintU();
        else if (tip == TipOtcheta.VidKZ)
            PrintVidKZ();
        // Добавляем проверку для Analiz, чтобы он выводил полный текст так же, как Vse
        else if (tip == TipOtcheta.Vse || tip == TipOtcheta.Analiz)
        {
            PrintVidKZ();
            PrintToki();
            PrintU();

            if (iGroups == 0 || uGroups == 0)
            {
                txtInfo.AppendText("\r\nПримечание: Для полного анализа выберите фазы A, B, C одного объекта и повторите расчет.\r\n");
            }
        }

        // Эта часть у тебя уже на месте, она отвечает за открытие окна
        if (tip == TipOtcheta.Analiz && (iGroups > 0 || uGroups > 0))
        {

            var analysisWindow = new FormAnalysis(timeAxis, rIA, rIB, rIC, i1, i2, i0, rUA, rUB, rUC, u1, u0);
            analysisWindow.Show();
        }
    }

    

    private void btnCalculate_Click(object sender, EventArgs e)
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

    private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {

    }

    private void выполнитьРасчетТКЗToolStripMenuItem_Click(object sender, EventArgs e)
    {
        VychislitSimmetrichnye(TipOtcheta.Analiz);
    }

    private void ConfigureMainPlot(ScottPlot.WinForms.FormsPlot fp, string title, string yLabel)
    {
        var plt = fp.Plot;
        plt.Title(title, size: 20);
        plt.YLabel(yLabel, size: 16);
        plt.XLabel("Время, секунды", size: 16);

        // Убираем пустые места по бокам
        plt.Axes.Margins(0, 0.1);

        // Настраиваем легенду (список сигналов)
        plt.Legend.IsVisible = true;
        plt.Legend.Alignment = Alignment.UpperRight;
        plt.Legend.FontSize = 14;

        fp.Refresh();
    }
}