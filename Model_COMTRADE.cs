using System;
using System.Collections.Generic;

namespace Osnovnoi_proekt
{
    
    public class AnalogovyiKanal
    {
        public int Nomer { get; set; }           
        public string Nazvanie { get; set; }    
        public string Faza { get; set; }        
        public string Edinicy { get; set; }     = string.Empty;
        public double Koeff_A { get; set; }     
        public double Koeff_B { get; set; }     

        
        public double Preobrazovat(double syroeZnachenie)
        {
            
            return Koeff_A * syroeZnachenie + Koeff_B;
        }
    }

    
    public class Zapis_COMTRADE
    {
        public List<AnalogovyiKanal> Kanaly { get; set; } = new List<AnalogovyiKanal>();
        public List<double[]> Dannye { get; set; } = new List<double[]>(); 
        public double Chastota { get; set; }  
    }
}