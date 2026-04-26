using System.IO;
using System.Linq; 
using System.Drawing; 
using ScottPlot;
using Color = System.Drawing.Color;

namespace Osnovnoi_proekt;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
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
                var record = Chtenie_COMTRADE.Prochitat(cfgPath, datPath);

                txtInfo.Clear();
                txtInfo.AppendText($"=== ОТЧЕТ ПО ФАЙЛУ: {Path.GetFileName(cfgPath)} ===\r\n\r\n");

                // 1. Сводная таблица каналов
                txtInfo.AppendText("СПИСОК КАНАЛОВ:\r\n");
                txtInfo.AppendText("----------------------------------------------------------\r\n");

                for (int i = 0; i < record.Kanaly.Count; i++)
                {
                    var k = record.Kanaly[i];
                    // Пытаемся сделать имя понятнее: если это фаза А, Б или С
                    string ponyatnoeImya = k.Nazvanie;
                    if (i == 6) ponyatnoeImya = "Ток фазы А ";
                    if (i == 7) ponyatnoeImya = "Ток фазы B ";
                    if (i == 8) ponyatnoeImya = "Ток фазы C ";

                    txtInfo.AppendText($"{i + 1,2}. {ponyatnoeImya,-25} [{k.Edinicy}]\r\n");
                }

                txtInfo.AppendText("----------------------------------------------------------\r\n\r\n");

                // 2. АНАЛИЗ ПИКОВЫХ ЗНАЧЕНИЙ (для ТКЗ)
                txtInfo.AppendText("РЕЗУЛЬТАТЫ АНАЛИЗА (Пиковые значения):\r\n");

                int[] tokiIdx = { 6, 7, 8 }; // Индексы наших токов
                char[] fazy = { 'A', 'B', 'C' };

                for (int j = 0; j < tokiIdx.Length; j++)
                {
                    int idx = tokiIdx[j];
                    // Находим максимальное значение тока по модулю во всем массиве данных
                    double maxTok = record.Dannye.Max(row => Math.Abs(row[idx]));

                    txtInfo.AppendText($"Фаза {fazy[j]}: {maxTok,8:F2} Ампер\r\n");
                }

                // 3. Отрисовка графика (как и была)
                OtrisovatGrafiki(record);
            }
        }
    }


    private void OtrisovatGrafiki(Zapis_COMTRADE record)
    {
        // 1. Полная очистка графика перед отрисовкой новых данных
        formsPlot1.Plot.Clear();

        // 2. Настройка параметров времени и каналов
        double shagVremeni = 0.001;
        int[] indeksiTokov = { 6, 7, 8 };
        // Указываем явно System.Drawing.Color, чтобы не было конфликта со ScottPlot
        System.Drawing.Color[] cveta = { System.Drawing.Color.Red, System.Drawing.Color.Green, System.Drawing.Color.Blue };

        // 3. Цикл отрисовки фазных токов
        for (int i = 0; i < indeksiTokov.Length; i++)
        {
            int idx = indeksiTokov[i];

            if (idx < record.Kanaly.Count)
            {
                double[] yData = record.Dannye.Select(row => row[idx]).ToArray();

                var signal = formsPlot1.Plot.Add.Signal(yData);
                signal.Data.Period = shagVremeni;
                signal.Color = ScottPlot.Color.FromColor(cveta[i]);

                // --- ИСПРАВЛЕНИЕ ТУТ: Сопоставляем индекс с человеческим названием ---
                string ponyatnoeImya = record.Kanaly[idx].Nazvanie;
                if (idx == 6) ponyatnoeImya = "Ток фазы А";
                if (idx == 7) ponyatnoeImya = "Ток фазы B";
                if (idx == 8) ponyatnoeImya = "Ток фазы C";

                signal.LegendText = ponyatnoeImya; // Теперь в легенде будет красиво
            }
        }

        // --- НОВОВВЕДЕНИЕ: ФИКСИРОВАННЫЙ МАСШТАБ ---
        // Устанавливаем лимиты: X от 0 до 3 сек, Y от -15000 до 15000 А
        formsPlot1.Plot.Axes.SetLimits(-0.1, 3, -15000, 15000);

        // 4. Оформление внешнего вида
        formsPlot1.Plot.Title("Осциллограммы токов короткого замыкания");
        formsPlot1.Plot.XLabel("Время, секунды");
        formsPlot1.Plot.YLabel("Ток, Амперы");

        // Настройка легенды (теперь она не будет плодиться)
        formsPlot1.Plot.Legend.IsVisible = true;
        formsPlot1.Plot.Legend.Alignment = Alignment.UpperRight;

        // 5. Финальное обновление элемента на форме
        formsPlot1.Refresh();
    }
}