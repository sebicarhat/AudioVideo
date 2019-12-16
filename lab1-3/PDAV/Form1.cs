using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace PDAV
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }

        int[,] Q = new int[,]
                {{6, 4, 4, 6, 10, 16, 20, 24},
                {5, 5, 6, 8, 10, 23, 24, 22},
                {6, 5, 6, 10, 16, 23, 28, 22},
                {6, 7, 9, 12, 20, 35, 32, 25},
                {7, 9, 15, 22, 27, 44, 41, 31},
                {10, 14, 22, 26, 32, 42, 45, 37},
                {20, 26, 31, 35, 41, 48, 48, 40},
                {29, 37, 38, 39, 45, 40, 41, 40}}; 


        public class Pixel
        {
            public int r;
            public int g;
            public int b;

            public Pixel() {
                r = 0;
                g = 0;
                b = 0;
            }

            public Pixel(int rx, int gx, int bx)
            {
                r = rx;
                g = gx;
                b = bx;
            }

            
        }

        public class Image
        {
            public string type;
            public string comm;
            public int width;
            public int height;
            public Pixel[,] pixelMatrix;
            public Image(int widthx, int heightx)
            {
                width = widthx;
                height = heightx;

                pixelMatrix = new Pixel[height,width];
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                        pixelMatrix[i, j] = new Pixel();
            }
          
        }

        public class Point
        {
            public int x;
            public int y;
            public Point(int xval, int yval)
            {
                x=xval;
                y=yval;
            }
        }

        public class block
        {
            public Point pos;
            public Pixel[,] pixelMatrix;
            public block(int width, int height, Point p)
            {
                pos = new Point(p.x, p.y);
                pixelMatrix = new Pixel[height, width];
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                        pixelMatrix[i, j] = new Pixel();
            }

            public void toString() {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                        Console.Write(pixelMatrix[i, j].r + " ");
                    Console.WriteLine();
                }
                Console.WriteLine("------------");
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                        Console.Write(pixelMatrix[i, j].g + " ");
                    Console.WriteLine();
                }
                Console.WriteLine("------------");
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                        Console.Write(pixelMatrix[i, j].b + " ");
                    Console.WriteLine();
                }
                Console.WriteLine("------------");
            }
        }

        public void readImage(ref string type, ref string comm, ref string maxPixelValue, ref int width, ref int height, ref Image img) { 
            using (var op = new OpenFileDialog())
            {
                op.InitialDirectory = "*";
                op.Title = "Insert a text";
                op.FileName = "";
                op.Filter = "Ppm files|*.ppm|All Files|*.";
                if (op.ShowDialog() != DialogResult.OK)
                    return;
                using (var stream = System.IO.File.OpenRead(op.FileName))
                using (var reader = new StreamReader(stream))
                {
                    type = reader.ReadLine();
                    comm = reader.ReadLine();
                    string [] dimarray = reader.ReadLine().Split(' ');
                    maxPixelValue = reader.ReadLine();
                    width = int.Parse(dimarray[0]);
                    height = int.Parse(dimarray[1]);
                    img = new Image(width, height);
                    int[] byteArray = new int[160000];

                    //read rgb image pixel matrix
                    for (int i = 0; i < height; i++)
                        for(int j = 0;j < width; j++)
                        {
                            img.pixelMatrix[i,j].r = int.Parse(reader.ReadLine());
                            img.pixelMatrix[i,j].g = int.Parse(reader.ReadLine());
                            img.pixelMatrix[i,j].b = int.Parse(reader.ReadLine());
                        }
                    

        
            }
            }
        }

        void quantize(ref block[,] dctblocks, int width, int height)
        {
            
            for (int i = 0; i <height/8; i++)
                for (int j = 0; j < width / 8; j++) {
                    for (int k = 0; k < 8; k++)
                        for (int l = 0; l < 8; l++)
                        {
                            dctblocks[i, j].pixelMatrix[k, l].r /= Q[k, l];
                            dctblocks[i, j].pixelMatrix[k, l].g /= Q[k, l];
                            dctblocks[i, j].pixelMatrix[k, l].b /= Q[k, l];
                        }
                }
        
        }

        void deQuantize(ref block[,] dctblocks, int width, int height)
        {
            for (int i = 0; i < height / 8; i++)
                for (int j = 0; j < width / 8; j++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        for (int l = 0; l < 8; l++)
                        {
                            dctblocks[i, j].pixelMatrix[k, l].r *= Q[k, l];
                            dctblocks[i, j].pixelMatrix[k, l].g *= Q[k, l];
                            dctblocks[i, j].pixelMatrix[k, l].b *= Q[k, l];
                        }
                    }
                }

        }

        block[,] dctTransform(block[,] YUVblocks, int width, int height)
        {
            block[,] dctBlocks = new block[height / 8, width / 8];

            for (int i = 0; i < height / 8; i++)
                for (int j = 0; j < width / 8; j++)
                {
                    dctBlocks[i, j] = dctMatrixTransform(YUVblocks[i, j], 8, 8);
                }
            return dctBlocks;
        }

        block[,] dctInvTransform(block[,] dctblocks, int width, int height)
        {
            block[,] YUVBlocks = new block[height / 8, width / 8];

            for (int i = 0; i < height / 8; i++)
                for (int j = 0; j < width / 8; j++)
                {
                    YUVBlocks[i, j] = dctInvMatrixTransform(dctblocks[i, j], 8, 8);
                }

            return YUVBlocks;
        }

        //dctMatrixTransform will return an 8x8 block with Y,U and V matrices for which was applied DCT
        block dctMatrixTransform(block matrix, int n, int m) 
        { 
            int i, j, k, l; 
  
            block dct = new block(8,8,new Point(matrix.pos.x,matrix.pos.y)); 
  
            double sum,sum2,sum3,ci,cj;

            for (i = 0; i < 8; i++) { 
                for (j = 0; j < 8; j++) {
  
                    // sum will temporarily store the sum
                    sum = 0;
                    sum2 = 0;
                    sum3 = 0;
                    ci = ((i == 0) ? 1 / Math.Sqrt(2) : 1);
                    cj = ((j == 0) ? 1 / Math.Sqrt(2) : 1);
                    for (k = 0; k < m; k++) { 
                        for (l = 0; l < n; l++) {

                            double aux = Math.Cos(((2 * k + 1) * i * Math.PI) / 16) *
                                        Math.Cos(((2 * l + 1) * j * Math.PI) / 16) ;

                            sum +=( matrix.pixelMatrix[k, l].r - 128) * aux;
                            sum2 += (matrix.pixelMatrix[k, l].g - 128) * aux;
                            sum3 += (matrix.pixelMatrix[k, l].b - 128) * aux;
                        } 
                    }

                    double aux2 = Math.Floor((sum * ci * cj) / 4);
                    if(aux2>0)
                    dct.pixelMatrix[i, j].r = Convert.ToInt32(Math.Floor((sum * ci * cj) / 4));
                    else
                    dct.pixelMatrix[i, j].r = Convert.ToInt32(Math.Ceiling((sum * ci * cj) / 4));

                    aux2 = Math.Floor((sum2 * ci * cj) / 4);
                    if(aux2>0)
                    dct.pixelMatrix[i, j].g = Convert.ToInt32(Math.Floor((sum2 * ci * cj) / 4));
                    else
                    dct.pixelMatrix[i, j].g = Convert.ToInt32(Math.Ceiling((sum2 * ci * cj) / 4));

                    aux2 = Math.Floor((sum3 * ci * cj) / 4);
                    if (aux2 > 0)
                    dct.pixelMatrix[i, j].b = Convert.ToInt32(Math.Floor((sum3 * ci * cj) / 4)); 
                    else
                    dct.pixelMatrix[i, j].b = Convert.ToInt32(Math.Ceiling((sum3 * ci * cj) / 4)); 
                }
            }
            return dct;  
    }

        //dctInvMatrixTransform will return an 8x8 block with Y,U and V matrices for which was applied inverse DCT
        block dctInvMatrixTransform(block dct, int n, int m)
        {
            int i, j, k, l;
            block matrix = new block(8, 8, new Point(dct.pos.x, dct.pos.y));
            double s,s2,s3,ci,cj;
            for (i = 0; i < m; i++)
                for (j = 0; j < n; j++)
                {
                s = 0; 
                s2 = 0; 
                s3 = 0;
                

                for (k = 0; k < m; k++)
                    for (l = 0; l < n; l++)
                    {
                        ci = ((k == 0) ? 1 / Math.Sqrt(2) : 1);
                        cj = ((l == 0) ? 1 / Math.Sqrt(2) : 1);
                        double aux = Math.Cos(((2 * i + 1) * k * Math.PI) / 16) *
                                        Math.Cos(((2 * j + 1) * l * Math.PI) / 16) *ci *cj;

                        s += dct.pixelMatrix[k, l].r * aux ;
                        s2 += dct.pixelMatrix[k, l].g * aux;
                        s3 += dct.pixelMatrix[k, l].b  * aux;
                    }
                matrix.pixelMatrix[i, j].r = Convert.ToInt32(s / 4)+128;
                matrix.pixelMatrix[i, j].g = Convert.ToInt32(s2 / 4)+128;
                matrix.pixelMatrix[i, j].b = Convert.ToInt32(s3 / 4)+128;
                }
            return matrix;
        }

        //encoding with 4:2:0 subsampling (U and V blocks are 4x4 size and each pixel stores average of 2x2 pixels)
        public void encoding(int width, int height, Image image, ref block[,] Yblocks, ref block[,] UVblocks)
        {

            //convert to YUV image pixel matrix
            var YUVimg = new Image(width, height);
                    for (int i = 0; i < height; i++)
                        for (int j = 0; j < width; j++)
                        {
                            YUVimg.pixelMatrix[i, j].r = Convert.ToInt32(0.299 * image.pixelMatrix[i, j].r + 0.587 * image.pixelMatrix[i, j].g + 0.114 * image.pixelMatrix[i, j].b);
                            YUVimg.pixelMatrix[i, j].g = Convert.ToInt32(128 - 0.1687 * image.pixelMatrix[i, j].r - 0.3312 * image.pixelMatrix[i, j].g + 0.5 * image.pixelMatrix[i, j].b);
                            YUVimg.pixelMatrix[i, j].b = Convert.ToInt32(128 + 0.5 * image.pixelMatrix[i, j].r - 0.4186 * image.pixelMatrix[i, j].g - 0.0813 * image.pixelMatrix[i, j].b);   
                        }


                    for (int i = 0; i < height / 8; i++)
                    {
                        for (int j = 0; j < width / 8; j++)
                        {
                            block Yblock = new block(8, 8, new Point(i*8, j*8));
                            for (int k = 0; k < 8; k++)
                                for (int l = 0; l < 8; l++)
                                    Yblock.pixelMatrix[k, l].r = YUVimg.pixelMatrix[i*8+k,j*8+l].r;
                            Yblocks[i, j] = Yblock;

                            block UVblock = new block(4, 4, new Point(i*8, j*8));
                            for (int k = 0; k < 4; k++)
                                for (int l = 0; l < 4; l++)
                                {

                                    UVblock.pixelMatrix[k, l].g = UVblock.pixelMatrix[k, l].g = UVblock.pixelMatrix[k, l].g = UVblock.pixelMatrix[k, l].g =
                                        (YUVimg.pixelMatrix[i * 8 + k * 2, j * 8 + l * 2].g +
                                        YUVimg.pixelMatrix[i * 8 + k * 2 + 1, j * 8 + l * 2].g +
                                        YUVimg.pixelMatrix[i * 8 + k * 2, j * 8 + l * 2 + 1].g +
                                        YUVimg.pixelMatrix[i * 8 + k * 2 + 1, j * 8 + l * 2 + 1].g) / 4;

                                    UVblock.pixelMatrix[k, l].b =
                                        (YUVimg.pixelMatrix[i * 8 + k * 2, j * 8 + l * 2].b +
                                        YUVimg.pixelMatrix[i * 8 + k * 2 + 1, j * 8 + l * 2].b +
                                        YUVimg.pixelMatrix[i * 8 + k * 2, j * 8 + l * 2 + 1].b +
                                        YUVimg.pixelMatrix[i * 8 + k * 2 + 1, j * 8 + l * 2 + 1].b) / 4;
                                }
                            UVblocks[i, j] = UVblock;
                        }
                    }
         
        }

  
        public void upSampling(int width, int height, ref block[,] YUVblocks, block[,] Yblocks, block[,] UVblocks)
        {
            for (int i = 0; i < height / 8; i++)
            {
                for (int j = 0; j < width / 8; j++)
                {
                    //for Y block just copy elements because it wasn't subsampled
                    block YUVblock = new block(8, 8, new Point(i * 8, j * 8));
                    for (int k = 0; k < 8; k++)
                        for (int l = 0; l < 8; l++)
                        {
                            YUVblock.pixelMatrix[k, l].r = Yblocks[i,j].pixelMatrix[k,l].r;  
                        }

                    //for U and V block apply upSampling because was 4:2:0 subsampled
                    for (int k = 0; k < 4; k++)
                        for (int l = 0; l < 4; l++)
                        {
                            YUVblock.pixelMatrix[k * 2, l * 2].g = YUVblock.pixelMatrix[k * 2 + 1, l * 2].g = YUVblock.pixelMatrix[k * 2, l * 2 + 1].g = YUVblock.pixelMatrix[k * 2 + 1, l * 2 + 1].g =
                                UVblocks[i, j].pixelMatrix[k, l].g;//u block
                            YUVblock.pixelMatrix[k * 2, l * 2].b = YUVblock.pixelMatrix[k * 2 + 1, l * 2].b = YUVblock.pixelMatrix[k * 2, l * 2 + 1].b = YUVblock.pixelMatrix[k * 2 + 1, l * 2 + 1].b =
                                UVblocks[i, j].pixelMatrix[k, l].b;//v block
                        }
                    YUVblocks[i, j] = YUVblock;

                }
            }
        }

        Image decoding(int width,int height, block[,] Yblocks, block[,] UVblocks){

            Image newImage = new Image(width, height);

                    for(int i=0;i<height/8;i++)
                        for (int j = 0; j <width/8; j++)
                        {
                            int x = Yblocks[i, j].pos.x;
                            int y = Yblocks[i, j].pos.y;
                            for (int k = 0; k < 8; k++)
                                for (int l = 0; l < 8; l++)
                                    newImage.pixelMatrix[x+k, y+l].r = Yblocks[i, j].pixelMatrix[k, l].r;
                        }

                    for (int i = 0; i <height/8; i++)
                        for (int j = 0; j <width/8; j++)
                        {
                            int x = UVblocks[i, j].pos.x;
                            int y = UVblocks[i, j].pos.y;

                            for (int k = 0; k < 4; k++)
                                for (int l = 0; l < 4; l++)
                                {
                                    newImage.pixelMatrix[x + k * 2, y + l * 2].g = UVblocks[i, j].pixelMatrix[k, l].g;
                                    newImage.pixelMatrix[x + k * 2 + 1, y + l * 2].g = UVblocks[i, j].pixelMatrix[k, l].g;
                                    newImage.pixelMatrix[x + k * 2, y + l * 2 + 1].g = UVblocks[i, j].pixelMatrix[k, l].g;
                                    newImage.pixelMatrix[x + k * 2 + 1, y + l * 2 + 1].g = UVblocks[i, j].pixelMatrix[k, l].g;

                                    newImage.pixelMatrix[x + k * 2, y + l * 2].b = UVblocks[i, j].pixelMatrix[k, l].b;
                                    newImage.pixelMatrix[x + k * 2 + 1, y + l * 2].b = UVblocks[i, j].pixelMatrix[k, l].b;
                                    newImage.pixelMatrix[x + k * 2, y + l * 2 + 1].b = UVblocks[i, j].pixelMatrix[k, l].b;
                                    newImage.pixelMatrix[x + k * 2 + 1, y + l * 2 + 1].b = UVblocks[i, j].pixelMatrix[k, l].b;

                                }
                        }

                    for(int i=0;i<height;i++)
                        for (int j = 0; j < width; j++)
                        {
                            int y = newImage.pixelMatrix[i, j].r;
                            int u = newImage.pixelMatrix[i, j].g;
                            int v = newImage.pixelMatrix[i, j].b;

                            int x1 = Convert.ToInt32(y + 1.140 * (v - 128));
                            if (x1 < 0) x1 = 0;
                            if (x1 > 255) x1 = 255;
                            int x2 = Convert.ToInt32(y - 0.344 * (u - 128) - 0.711 * (v - 128));
                            if (x2 < 0) x2 = 0;
                            if (x2 > 255) x2 = 255;
                            int x3 = Convert.ToInt32(y + 1.772 * (u - 128));
                            if (x3 < 0) x3 = 0;
                            if (x3 > 255) x3 = 255;

                            newImage.pixelMatrix[i, j].r = x1;
                            newImage.pixelMatrix[i, j].g = x2;
                            newImage.pixelMatrix[i, j].b = x3;
                        }
            return newImage;
        }

        Image decoding(int width, int height, block[,] YUVblocks)
        {
            Image newImage = new Image(width, height);
            for (int i = 0; i < height / 8; i++)
                for (int j = 0; j < width / 8; j++)
                {
                    int x = YUVblocks[i, j].pos.x;
                    int y = YUVblocks[i, j].pos.y;
                    for (int k = 0; k < 8; k++)
                        for (int l = 0; l < 8; l++)
                        {
                            newImage.pixelMatrix[x + k, y + l].r = YUVblocks[i, j].pixelMatrix[k, l].r;
                            newImage.pixelMatrix[x + k, y + l].g = YUVblocks[i, j].pixelMatrix[k, l].g;
                            newImage.pixelMatrix[x + k, y + l].b = YUVblocks[i, j].pixelMatrix[k, l].b;
                        }
                }

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    int y = newImage.pixelMatrix[i, j].r;
                    int u = newImage.pixelMatrix[i, j].g;
                    int v = newImage.pixelMatrix[i, j].b;

                    int x1 = Convert.ToInt32(y + 1.140 * (v - 128));
                    if (x1 < 0) x1 = 0;
                    if (x1 > 255) x1 = 255;
                    int x2 = Convert.ToInt32(y - 0.344 * (u - 128) - 0.711 * (v - 128));
                    if (x2 < 0) x2 = 0;
                    if (x2 > 255) x2 = 255;
                    int x3 = Convert.ToInt32(y + 1.772 * (u - 128));
                    if (x3 < 0) x3 = 0;
                    if (x3 > 255) x3 = 255;

                    newImage.pixelMatrix[i, j].r = x1;
                    newImage.pixelMatrix[i, j].g = x2;
                    newImage.pixelMatrix[i, j].b = x3;
                }
            return newImage;
        }

        public void saveImage(string type, string comm, string maxPixelValue, int width, int height, Image newImage) {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"E:\PDAV\PDAV\bin\Debug\newimage.ppm"))
                    {
                        file.WriteLine(type);
                        file.WriteLine(comm);
                        file.WriteLine(width.ToString() + " " + height.ToString());
                        file.WriteLine(maxPixelValue);

                        for (int i = 0; i < height; i++)
                        {
                            
                            for (int j = 0; j < width; j++)
                            {
                                file.WriteLine(newImage.pixelMatrix[i, j].r);
                                file.WriteLine(newImage.pixelMatrix[i, j].g);
                                file.WriteLine(newImage.pixelMatrix[i, j].b);
                            }
                        }
                    }
                    

                }

        int [] entropyEncoding (block[,] dctBlocks, int width, int height)
        {
            int [] auxByteArray = new int[1000000];
            int length = 0;
            for (int i = 0; i < height/8; i++)
                for (int j = 0; j < width/8; j++)
                {
                    //array obtained after traverse y,u and v matrix
                    int[] zigZagArray = new int[1000];
                    zigZagArray = zigZagMatrix(dctBlocks[i, j],8,8);

                    int zeros = 0;
                    int l = 64 * 3;

                    //to see if dct->zigzag array are working good
                    if (i == 0 && j == 0)
                    {
                        Console.WriteLine(dctBlocks[i, j].pos.x + " " + dctBlocks[i, j].pos.y);
                        Console.WriteLine("YUV blocks before entropy encoding:");
                        dctBlocks[i, j].toString();

                        Console.WriteLine("ZigZag array before entropy encoding:");
                        for (int p = 0; p < l; p++)
                        Console.Write(zigZagArray[p] + " ");
                        Console.WriteLine();
                    }


                    for (int p = 0; p < l; p++)
                    {

                        //add (size,amplitude) for first elem of each y/u/v block
                        if (p == 0 || p == 64 || p == 128) {
                            zeros = 0;
                            auxByteArray[++length] = getSize(zigZagArray[p]);
                            auxByteArray[++length] = zigZagArray[p];

                        }
                        else
                        //if are last elem from each y/u/v block
                        if (p == 63 || p == 127 || p == 191)
                        {
                            if (zigZagArray[p] == 0)//if it have 0 value add (0,0) pair
                                {
                                    auxByteArray[++length] = 0;
                                    auxByteArray[++length] = 0;
                                }
                                else //else add (runlight,size)(amplitude)
                                {
                                    auxByteArray[++length] = zeros;
                                    auxByteArray[++length] = getSize(zigZagArray[p]);
                                    auxByteArray[++length] = zigZagArray[p];
                                    zeros = 0;
                                }
                         }
                        //if are not first or last elems from yuv blocks
                        else
                            {
                                if (zigZagArray[p] != 0)
                                {
                                    auxByteArray[++length] = zeros;
                                    auxByteArray[++length] = getSize(zigZagArray[p]);
                                    auxByteArray[++length] = zigZagArray[p];
                                    zeros = 0;
                                }
                                else
                                {
                                    zeros++;
                                }
                            }
                    }
                    //store array length on 0 pos
                    auxByteArray[0] = length;

                   //to see if zigZag array -> entropped array are working good
                   if (i == 1 && j == 2)
                    {
                        
                        Console.WriteLine("Entropped array:");
                        for (int q = 0; q <= length; ++q)
                            Console.Write(auxByteArray[q] + " ");
                        Console.WriteLine();
                    }
                }
            return auxByteArray;
        }

        //length of YUV zigZag array 
        int getLength(int x) {
            if (x <= 63) return 63;
            else if (x <= 127)
                return 127;
            else if (x <= 191)
                return 191;
            else return 0;
        }

        block[,] entropyDecoding(int width, int height, int[] byteArray)
        {
            block[,] blocks = new block[height / 8, width / 8];
            int k = 0;
            for (int i = 0; i < height / 8; i++)
                for (int j = 0; j < width / 8; j++)
                {
                    int[] YUVArray = new int[193];
                    int lYUVArray = -1;
                    blocks[i, j] = new block(8, 8, new Point(0, 0));
                    
                    int ok = 0;
                    while (ok<3)
                    {
                        ++k;
                        //if is first elem of Y/U/V block
                        if (lYUVArray == -1 || lYUVArray == 63 || lYUVArray == 127)
                        {
                            k++;
                            YUVArray[++lYUVArray] = byteArray[k];
                        }
                        else
                        {
                            //if meeting (0,0) pair fill zigzag array with zeros
                            if (byteArray[k] == 0 && byteArray[k + 1] == 0)
                            {
                                ok++;
                                k++;
                                int q = getLength(lYUVArray);
                                while (lYUVArray < q)
                                    YUVArray[++lYUVArray] = 0;
                            }

                            else
                                //if doesn't exist 0 values before
                                if (byteArray[k] == 0)
                                {
                                    k += 2;
                                    YUVArray[++lYUVArray] = byteArray[k];
                                }
                                //if was 0 values adding before
                                else
                                {
                                    for (int p = 0; p < byteArray[k]; ++p)
                                        YUVArray[++lYUVArray] = 0;
                                    k += 2;
                                    YUVArray[++lYUVArray] = byteArray[k];
                                }
                        }
                    }

                    blocks[i, j] = blockFromZigZagArray(YUVArray,i*8,j*8);


                    //to see if ->entropped array->zigzag array->dct are working good
                    if (i == 1 && j == 2)
                    {
                        Console.WriteLine("ZigZag array after entropy decoding:");
                        for (int q = 0; q < 192; ++q)
                        {
                            Console.Write(YUVArray[q] + " ");
                        }
                        Console.WriteLine();
                        Console.WriteLine(blocks[i, j].pos.x + " " + blocks[i, j].pos.y);
                        Console.WriteLine("YUV blocks after entropy decoding:");
                        blocks[i, j].toString();

                    }
                }

            return blocks;
        }


        /* Desc: traverse in zigzag y,u and v matrix int this order and appending elems in array
         * INPUT: one block with yuv matrix
         * OUTPUT an array of (n*m)*3 size*/
        int[] zigZagMatrix(block arr, int n, int m)
        {
            
            int[] zigZagVector = new int[200];
            int k = -1;

            int row = 0, col = 0;

            // Boolean variable that will 
            // true if we need to increment 
            // 'row' valueotherwise false- 
            // if increment 'col' value 
            bool row_inc = false;

            // Print matrix of lower half 
            // zig-zag pattern 
            int mn = Math.Min(m, n);
            for (int len = 1; len <= mn; ++len)
            {
                for (int i = 0; i < len; ++i)
                {

                    zigZagVector[++k] = arr.pixelMatrix[row, col].r;
                    zigZagVector[k+64] = arr.pixelMatrix[row, col].g;
                    zigZagVector[k+128] = arr.pixelMatrix[row, col].b;

                    if (i + 1 == len)
                        break;

                    // If row_increment value is true 
                    // increment row and decrement col 
                    // else decrement row and increment 
                    // col 
                    if (row_inc)
                    {
                        ++row;
                        --col;
                    }
                    else
                    {
                        --row;
                        ++col;
                    }
                }

                if (len == mn)
                    break;

                // Update row or col valaue 
                // according to the last 
                // increment 
                if (row_inc)
                {
                    ++row;
                    row_inc = false;
                }
                else
                {
                    ++col;
                    row_inc = true;
                }
            }

            // Update the indexes of row 
            // and col variable 
            if (row == 0)
            {
                if (col == m - 1)
                    ++row;
                else
                    ++col;
                row_inc = true;
            }
            else
            {
                if (row == n - 1)
                    ++col;
                else
                    ++row;
                row_inc = false;
            }

            // Print the next half 
            // zig-zag pattern 
            int MAX = Math.Max(m, n) - 1;
            for (int len, diag = MAX; diag > 0; --diag)
            {

                if (diag > mn)
                    len = mn;
                else
                    len = diag;

                for (int i = 0; i < len; ++i)
                {
                    //Console.Write(arr.pixelMatrix[row, col].r + " ");
                    zigZagVector[++k] = arr.pixelMatrix[row, col].r;
                    zigZagVector[k+64] = arr.pixelMatrix[row, col].g;
                    zigZagVector[k+128] = arr.pixelMatrix[row, col].b;

                    if (i + 1 == len)
                        break;

                    // Update row or col value 
                    // according to the last 
                    // increment 
                    if (row_inc)
                    {
                        ++row;
                        --col;
                    }
                    else
                    {
                        ++col;
                        --row;
                    }
                }

                // Update the indexes of 
                // row and col variable 
                if (row == 0 || col == m - 1)
                {
                    if (col == m - 1)
                        ++row;
                    else
                        ++col;

                    row_inc = true;
                }

                else if (col == 0 || row == n - 1)
                {
                    if (row == n - 1)
                        ++col;
                    else
                        ++row;

                    row_inc = false;
                }
            }
            zigZagVector[k + 129] = -1000;

            return zigZagVector;  

        }

        /* Desc: make an 8x8 block of Y,U and V matrix from zigzag array
         * INPUT: the zigzag array
         * OUTPUT one block with yuv matrix*/
        block blockFromZigZagArray(int[] zigZagArray,int x, int y)
        {
            block block = new block(8,8,new Point(x,y));

            int row = 0, col = 0;
            int k = -1;
            // Boolean variable that will 
            // true if we need to increment 
            // 'row' valueotherwise false- 
            // if increment 'col' value 
            bool row_inc = false;

            // Print matrix of lower half 
            // zig-zag pattern 
            for (int len = 1; len <= 8; ++len)
            {
                for (int i = 0; i < len; ++i)
                {

                    //Console.Write(arr.pixelMatrix[row, col].r + " ");
                    block.pixelMatrix[row, col].r = zigZagArray[++k];
                    block.pixelMatrix[row, col].g = zigZagArray[k+64];
                    block.pixelMatrix[row, col].b = zigZagArray[k+128];


                    if (i + 1 == len)
                        break;

                    // If row_increment value is true 
                    // increment row and decrement col 
                    // else decrement row and increment 
                    // col 
                    if (row_inc)
                    {
                        ++row;
                        --col;
                    }
                    else
                    {
                        --row;
                        ++col;
                    }
                }

                if (len == 8)
                    break;

                // Update row or col valaue 
                // according to the last 
                // increment 
                if (row_inc)
                {
                    ++row;
                    row_inc = false;
                }
                else
                {
                    ++col;
                    row_inc = true;
                }
            }

            // Update the indexes of row 
            // and col variable 
            if (row == 0)
            {
                if (col == 8 - 1)
                    ++row;
                else
                    ++col;
                row_inc = true;
            }
            else
            {
                if (row == 8 - 1)
                    ++col;
                else
                    ++row;
                row_inc = false;
            }

            // Print the next half 
            // zig-zag pattern 
            int MAX = 7;
            for (int len, diag = MAX; diag > 0; --diag)
            {

                if (diag > 8)
                    len = 8;
                else
                    len = diag;

                for (int i = 0; i < len; ++i)
                {
                    //Console.Write(arr.pixelMatrix[row, col].r + " ");
                    block.pixelMatrix[row, col].r = zigZagArray[++k];
                    block.pixelMatrix[row, col].g = zigZagArray[k+64];
                    block.pixelMatrix[row, col].b = zigZagArray[k+128];

                    if (i + 1 == len)
                        break;

                    // Update row or col value 
                    // according to the last 
                    // increment 
                    if (row_inc)
                    {
                        ++row;
                        --col;
                    }
                    else
                    {
                        ++col;
                        --row;
                    }
                }

                // Update the indexes of 
                // row and col variable 
                if (row == 0 || col == 8 - 1)
                {
                    if (col == 8 - 1)
                        ++row;
                    else
                        ++col;

                    row_inc = true;
                }

                else if (col == 0 || row == 8 - 1)
                {
                    if (row == 8 - 1)
                        ++col;
                    else
                        ++row;

                    row_inc = false;
                }
            }

            return block;
        }

        int getSize(int val)
        {
            if (val == -1 || val == 1)
                return 1;
            else if (val == -3 || val == -2 || val == 2 || val == 3)
                return 2;
            else if ((val >= -7 && val <= -4) || (val >= 4 && val <= 7))
                return 3;
            else if ((val >= -15 && val <= -8) || (val >= 8 && val <= 15))
                return 4;
            else if ((val >= -31 && val <= -16) || (val >= 16 && val <= 31))
                return 5;
            else if ((val >= -63 && val <= -32) || (val >= 32 && val <= 63))
                return 6;
            else if ((val >= -127 && val <= -64) || (val >= 64 && val <= 127))
                return 7;
            else if ((val >= -255 && val <= -128) || (val >= 128 && val <= 255))
                return 8;
            else if ((val >= -511 && val <= -256) || (val >= 256 && val <= 511))
                return 9;
            else if ((val >= -1023 && val <= -512) || (val >= 512 && val <= 1023))
                return 10;
            else return 0;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string type=""; string comm=""; string[] dimarray= new string[2]; string maxPixelValue=""; int width=0; int height=0; Image img=new Image(800,600);

            readImage(ref type,ref comm,ref maxPixelValue,ref width,ref height,ref img);

            block[,] Yblocks = new block[height / 8, width / 8];
            block[,] UVblocks = new block[height / 8, width / 8];
            block[,] YUVblocks = new block[height / 8, width / 8];

            encoding(width,height,img,ref Yblocks,ref UVblocks);

            upSampling(width, height,ref  YUVblocks, Yblocks, UVblocks);
            block[,] dctblock = dctTransform(YUVblocks, width, height);
            quantize(ref dctblock, width, height);

            int[] entropyEncodedArray = entropyEncoding(dctblock, width, height);
            block[,] entropyDecodedBlocks = entropyDecoding(width, height, entropyEncodedArray);

            deQuantize(ref entropyDecodedBlocks, width, height);
            block[,] invDCT = dctInvTransform(entropyDecodedBlocks, width, height);

            Image decoded = decoding(width, height, invDCT);
            //for lab1 Image decoded = decoding(width, height, Yblocks, UVblocks);

            saveImage(type,comm,maxPixelValue,width,height,decoded);
            MessageBox.Show("Done");
                        
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }


}
}

