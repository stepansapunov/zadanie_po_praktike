using System.IO;
using System.Linq;
using System.Drawing;
using ScottPlot;
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
                // Сохраняем запись в переменную класса
                currentRecord = Chtenie_COMTRADE.Prochitat(cfgPath, datPath);

                // --- НОВОЕ: Заполняем список выбора сигналов ---
                // Заполняем CheckedListBox красивыми названиями
                clbSignals.Items.Clear();
                for (int i = 0; i < currentRecord.Kanaly.Count; i++)
                {
                    var k = currentRecord.Kanaly[i];
                    string krasivoeImya = PoluchitPonyatnoeImya(i, k.Nazvanie);
                    clbSignals.Items.Add($"{i + 1}: {krasivoeImya} [{k.Edinicy}]");
                }
                // -----------------------------------------------

                // Твой старый код вычета пиков (оставляем для инфы в txtInfo)
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

                // Очищаем график (ждем, пока пользователь сам выберет сигналы)
                formsPlot1.Plot.Clear();
                formsPlot1.Refresh();
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
}