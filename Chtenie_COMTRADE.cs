using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace Osnovnoi_proekt
{
    public class Chtenie_COMTRADE
    {
        public static Zapis_COMTRADE Prochitat(string put_CFG, string put_DAT)
        {
            var zapis = new Zapis_COMTRADE();

            
            string[] cfgStroki = File.ReadAllLines(put_CFG);

            
            string[] infoOKanalax = cfgStroki[1].Split(',');
            int kolvoAnalogovyx = int.Parse(infoOKanalax[1].Replace("A", "").Trim());

            
            for (int i = 2; i < 2 + kolvoAnalogovyx; i++)
            {
                string[] chasti = cfgStroki[i].Split(',');
                zapis.Kanaly.Add(new AnalogovyiKanal
                {
                    Nomer = int.Parse(chasti[0]),
                    Nazvanie = chasti[1].Trim(),
                    Faza = chasti[2].Trim(),
                    Edinicy = chasti[4].Trim(),
                    Koeff_A = double.Parse(chasti[5], CultureInfo.InvariantCulture),
                    Koeff_B = double.Parse(chasti[6], CultureInfo.InvariantCulture)
                });
            }

            
            string[] datStroki = File.ReadAllLines(put_DAT);

            foreach (string stroka in datStroki)
            {
                if (string.IsNullOrWhiteSpace(stroka)) continue;

                
                string[] chasti = stroka.Split(',');

                
                double[] znacheniya = new double[kolvoAnalogovyx];

                for (int i = 0; i < kolvoAnalogovyx; i++)
                {
                    
                    double val = double.Parse(chasti[i + 2], CultureInfo.InvariantCulture);

                    
                    znacheniya[i] = zapis.Kanaly[i].Preobrazovat(val);
                }

                zapis.Dannye.Add(znacheniya);
            }

            return zapis;
        }
    }
}