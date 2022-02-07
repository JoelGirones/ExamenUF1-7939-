using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace examenesteganografiar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnGenerarImatge_Click(object sender, RoutedEventArgs e)
        {
            WriteableBitmap bmp = generarImatge();

            this.imgImatge.Source = bmp;
            this.imgImatge.Stretch = Stretch.Uniform;
        }

        public WriteableBitmap generarImatge()
        {
            Random r = new Random();
     
            int nRandomR = 0, nRandomG = 0, nRandomB = 0;

            int dpi = 96;
            int width = 10;
            int height = 10;
            int nPixels = width * height;
            int bytesPerPixel = 3;//*3 per --> (RGB)
            byte[] bytesImatge = new byte[width * height * bytesPerPixel];


            nRandomR = r.Next(0, 256);

            int cadaWidth = 0;

            for (int i = 0; i < nPixels * bytesPerPixel; i += bytesPerPixel)
            {
                bytesImatge[i] = (byte)nRandomR;//R
                bytesImatge[i + 1] = (byte)nRandomG;//G
                bytesImatge[i + 2] = (byte)nRandomB;//B
                cadaWidth++;
                if (cadaWidth == width)
                {
                    nRandomR = r.Next(0, 256);
                    nRandomG = r.Next(0, 256);
                    nRandomB = r.Next(0, 256);
                    cadaWidth = 0;
                }
            }

            WriteableBitmap bmp = new WriteableBitmap(width, height, dpi, dpi, PixelFormats.Rgb24, null);

            bmp.WritePixels(new Int32Rect(0, 0, width, height), bytesImatge, width * bytesPerPixel, 0);

            return bmp;
        }

        public WriteableBitmap esteganografiar(WriteableBitmap img, string msg)
        {
            byte[] bytesImatges;
            int stride = img.PixelWidth * (img.Format.BitsPerPixel / 8);
            bytesImatges = new byte[img.PixelHeight * stride];//4(RGBA) / 8 per passar a BytesPerPixel. 
            img.CopyPixels(bytesImatges, stride, 0);

            string missatge = "exemple";
            int[] bitsAOcultar = new int[] { 5,6,7,13,14,15,22,23 };

            int[] missatgeBinari = new int[missatge.Length * 8];
            int[] lletraBinari = new int[8];
            int idxLletra = 0;
            foreach (char lletra in missatge)
            {
                lletraBinari = EnterABinari(lletra);
                lletraBinari.CopyTo(missatgeBinari, idxLletra);
                idxLletra += 8;
            }
            int missatgeBinariIdx = 0;
            for (int i = 0; i < bytesImatges.Length; i += 4)
            {
                if (missatgeBinariIdx < missatgeBinari.Length)
                {
                    int[] b = EnterABinari(bytesImatges[i]);
                    int[] g = EnterABinari(bytesImatges[i + 1]);
                    int[] r = EnterABinari(bytesImatges[i + 2]);

                    for (int j = 0; j < bitsAOcultar.Length; j++)
                    {
                        if (bitsAOcultar[j] >= 0 && bitsAOcultar[j] <= 7)
                            b[bitsAOcultar[j]] = missatgeBinari[missatgeBinariIdx];
                        else if (bitsAOcultar[j] >= 8 && bitsAOcultar[j] <= 15)
                            g[bitsAOcultar[j] - 8] = missatgeBinari[missatgeBinariIdx];
                        else if (bitsAOcultar[j] >= 16 && bitsAOcultar[j] <= 23)
                            r[bitsAOcultar[j] - 16] = missatgeBinari[missatgeBinariIdx];

                        missatgeBinariIdx++;
                    }

                    bytesImatges[i] = (byte)BinariAEnter(b);
                    bytesImatges[i + 1] = (byte)BinariAEnter(g);
                    bytesImatges[i + 2] = (byte)BinariAEnter(r);
                }
            }

            return img;
        }

        public static int[] EnterABinari(int valor)
        {
            if (valor > 255) throw new Exception("Error, el conversor només funciona fins a 8");

            int[] resultat = new int[8];

            for (int i = 0; i < 8; i++)
            {
                resultat[i] = valor >> (7 - i) & 1;
            }

            return resultat;
        }

        public static int BinariAEnter(int[] valor)
        {
            int resultat = 0;
            int multiplicador = 128;

            for (int i = 0; i < valor.Length; i++)
            {
                if (valor[i] == 1) resultat += multiplicador;
                multiplicador /= 2;
            }

            return resultat;
        }
    }
}
