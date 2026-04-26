using System.IO;


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
                txtInfo.AppendText($"Файл успешно прочитан!\r\n");
                txtInfo.AppendText($"Всего сигналов: {record.Kanaly.Count}\r\n");
                txtInfo.AppendText("--------------------------\r\n");

                foreach (var kanal in record.Kanaly)
                {
                    txtInfo.AppendText($"{kanal.Nomer}: {kanal.Nazvanie} (Фаза {kanal.Faza}) - {kanal.Edinicy}\r\n");
                }
            }
            else
            {
                MessageBox.Show("Файл данных (.dat) не найден рядом с .cfg!");
            }
        }
    }

}

