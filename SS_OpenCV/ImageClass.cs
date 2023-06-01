using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV.Structure;
using Emgu.CV;

namespace CG_OpenCV
{
    class ImageClass
    {


        public void BrightContrast(Image<Bgr, byte> img, int bright, double contrast)
        {
            // Adjust the brightness
            byte[] pixelValues = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                int newValue = i + bright;
                if (newValue > 255)
                    pixelValues[i] = 255;
                else if (newValue < 0)
                    pixelValues[i] = 0;
                else
                    pixelValues[i] = (byte)newValue;
            }

            Image<Bgr, byte> adjustedImg = new Image<Bgr, byte>(img.Width, img.Height);
            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    Bgr pixel = img[y, x];
                    pixel.Blue = pixelValues[(int)pixel.Blue];
                    pixel.Green = pixelValues[(int)pixel.Green];
                    pixel.Red = pixelValues[(int)pixel.Red];
                    adjustedImg[y, x] = pixel;
                }
            }

            // Adjust the contrast
            double contrastFactor = (contrast + 100) / 100.0;
            adjustedImg._Mul(contrastFactor);

            // Copy the adjusted image back to the original image
            adjustedImg.CopyTo(img);
        }

        public void RedChannel(Image<Bgr, byte> img)
        {
            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    Bgr pixel = img[y, x];
                    pixel.Green = pixel.Red;
                    pixel.Blue = pixel.Red;
                    img[y, x] = pixel;
                }
            }
        }


        public Image<Bgr, byte> Scale(Image<Bgr, byte> img, float scaleFactor)
        {
            int newWidth = (int)(img.Width * scaleFactor);
            int newHeight = (int)(img.Height * scaleFactor);

            Image<Bgr, byte> imgCopy = new Image<Bgr, byte>(newWidth, newHeight, new Bgr(0, 0, 0));

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int originalX = (int)(x / scaleFactor);
                    int originalY = (int)(y / scaleFactor);

                    if (originalX >= 0 && originalX < img.Width && originalY >= 0 && originalY < img.Height)
                    {
                        imgCopy[y, x] = img[originalY, originalX];
                    }
                }
            }

            return imgCopy;
        }

        public void NonUniform(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, float[,] matrix, float matrixWeight)
        {
            int width = img.Width;
            int height = img.Height;

            // Criação de uma nova janela para introdução dos pesos
            // Aqui, você precisa criar uma janela personalizada para a entrada dos pesos da matriz
            // Essa implementação depende da biblioteca gráfica que você está usando para criar interfaces de usuário (UI)
            // Portanto, substitua este trecho de código com a criação da sua janela personalizada

            // Obtém o fator de normalização
            float normalizationFactor = GetNormalizationFactor(matrix);

            // Aplica o filtro não uniforme em cada pixel da imagem
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Obtém a vizinhança do pixel atual
                    var neighborhood = GetNeighborhood(img, x, y);

                    // Aplica a matriz ponderada na vizinhança
                    float sumB = 0, sumG = 0, sumR = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            Bgr color = neighborhood[i, j];
                            float weight = matrix[i, j];

                            sumB += (float)(color.Blue * weight);
                            sumG += (float)(color.Green * weight);
                            sumR += (float)(color.Red * weight);

                        }
                    }

                    // Normaliza a soma dos valores
                    sumB /= normalizationFactor;
                    sumG /= normalizationFactor;
                    sumR /= normalizationFactor;

                    // Define o novo valor do pixel na imagem de cópia
                    imgCopy[y, x] = new Bgr((byte)sumB, (byte)sumG, (byte)sumR);
                }
            }
        }

        private Bgr[,] GetNeighborhood(Image<Bgr, byte> img, int x, int y)
        {
            Bgr[,] neighborhood = new Bgr[3, 3];

            // Obtém a vizinhança 3x3 do pixel (x, y)
            int startX = x - 1;
            int startY = y - 1;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int pixelX = startX + j;
                    int pixelY = startY + i;

                    // Verifica se o pixel está dentro dos limites da imagem
                    if (pixelX >= 0 && pixelX < img.Width && pixelY >= 0 && pixelY < img.Height)
                    {
                        neighborhood[i, j] = img[pixelY, pixelX];
                    }
                    else
                    {
                        // Preenche com preto caso o pixel esteja fora dos limites da imagem
                        neighborhood[i, j] = new Bgr(0, 0, 0);
                    }
                }
            }

            return neighborhood;
        }

        private float GetNormalizationFactor(float[,] matrix)
        {
            float sum = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    sum += matrix[i, j];
                }
            }
            return sum;
        }

        /// <summary>
        /// Image Negative using EmguCV library
        /// Slower method
        /// </summary>
        /// <param name="img">Image</param>
        // Função que retorna o valor médio dos pixels vizinhos de um pixel na posição (x,y)

        //Aula 4, Filtro de Media para redução do ruído
        public static int Mean(int[,] imagem, int x, int y)
        {
            int soma = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    soma += imagem[x + i, y + j];
                }
            }
            return soma / 9;
        }

        // Função que aplica o filtro de média 3x3 em uma imagem
        public static int[,] FiltroMedia3x3(int[,] imagem)
        {
            int altura = imagem.GetLength(0);
            int largura = imagem.GetLength(1);
            int[,] novaImagem = new int[altura, largura];

            // Processa apenas os pixels do interior da imagem
            for (int x = 1; x < altura - 1; x++)
            {
                for (int y = 1; y < largura - 1; y++)
                {
                    novaImagem[x, y] = Mean(imagem, x, y);
                }
            }

            // Processa os pixels da borda utilizando o modo de duplicação de linhas ou colunas
            for (int x = 1; x < altura - 1; x++)
            {
                // borda esquerda
                novaImagem[x, 0] = Mean(imagem, x, 0);
                // borda direita
                novaImagem[x, largura - 1] = Mean(imagem, x, largura - 1);
            }
            for (int y = 1; y < largura - 1; y++)
            {
                // borda superior
                novaImagem[0, y] = Mean(imagem, 0, y);
                // borda inferior
                novaImagem[altura - 1, y] = Mean(imagem, altura - 1, y);
            }
            // quinas
            novaImagem[0, 0] = Mean(imagem, 0, 0);
            novaImagem[0, largura - 1] = Mean(imagem, 0, largura - 1);
            novaImagem[altura - 1, 0] = Mean(imagem, altura - 1, 0);
            novaImagem[altura - 1, largura - 1] = Mean(imagem, altura - 1, largura - 1);

            return novaImagem;
        }

        public static void Negative(Image<Bgr, byte> img)
        {
            unsafe
            {
                // direct access to the image memory(sequencial)
                // direcion top left -> bottom right

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the image

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding)
                int x, y;

                if (nChan == 3) // image in RGB
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            //retrive 3 colour components
                            dataPtr[0] = (byte)(255 - dataPtr[0]);
                            dataPtr[1] = (byte)(255 - dataPtr[1]);
                            dataPtr[2] = (byte)(255 - dataPtr[2]);

                            // advance the pointer to the next pixel
                            dataPtr += nChan;
                        }

                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtr += padding;
                    }
                }
            }
        }

        /// <summary>
        /// Convert to gray
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static void ConvertToGray(Image<Bgr, byte> img)
        {
            unsafe
            {
                // direct access to the image memory(sequencial)
                // direcion top left -> bottom right

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte blue, green, red, gray;

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding)
                int x, y;

                if (nChan == 3) // image in RGB
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            //retrive 3 colour components
                            blue = dataPtr[0];
                            green = dataPtr[1];
                            red = dataPtr[2];

                            // convert to gray
                            gray = (byte)Math.Round(((int)blue + green + red) / 3.0);

                            // store in the image
                            dataPtr[0] = gray;
                            dataPtr[1] = gray;
                            dataPtr[2] = gray;

                            // advance the pointer to the next pixel
                            dataPtr += nChan;
                        }

                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtr += padding;
                    }
                }
            }
        }

        public static void Negative_old(Image<Bgr, byte> img)
        {
            int x, y;

            Bgr aux;
            for (y = 0; y < img.Height; y++)
            {
                for (x = 0; x < img.Width; x++)
                {
                    // acesso directo : mais lento 
                    aux = img[y, x];
                    img[y, x] = new Bgr(255 - aux.Blue, 255 - aux.Green, 255 - aux.Red);
                }
            }
        }

        public static void Translation(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, int dx, int dy)
        {

            unsafe
            {
                MIplImage m = img.MIplImage;
                byte blue;
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels;
                int widthstep = m.widthStep;

                byte* dataPtr = (byte*)m.imageData.ToPointer();
                int nx, ny;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        nx = x - dx;
                        ny = y - dy;


                        // calcula endereço do pixel no ponto (x,y)
                        blue = (byte)(dataPtr + y * widthstep + x * nChan)[0];
                    }
                }
            }

        }
        public static void Rotation(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, float angle)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                byte blue;
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels;
                int widthstep = m.widthStep;

                byte* dataPtr = (byte*)m.imageData.ToPointer();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // calcula endereço do pixel no ponto (x,y)
                        blue = (byte)(dataPtr + y * widthstep + x * nChan)[0];
                    }
                }
            }
        }


        //Projeto Final
        /// <summary>
        /// 
        /// 
        /// Function that solves the puzzle
        /// </summary>
        /// <param name="img">Input/Output image</param>
        /// <param name="imgCopy">Image Copy</param>
        /// <param name="Pieces_positions">List of positions (Left-x,Top-y,Right-x,Bottom-y) of all detected pieces</param>
        /// <param name="Pieces_angle">List of detected pieces' angles</param>
        /// <param name="level">Level of image</param>
        public static void puzzle(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, out List<int[]> Pieces_positions, out List<int> Pieces_angle, int level)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                byte blue, red, green, blueBackground, redBackground, greenBackground;
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels;
                int widthstep = m.widthStep;

                byte* dataPtr = (byte*)m.imageData.ToPointer();
                int nx, ny;

                Pieces_positions = new List<int[]>();
                int[] piece_vector = new int[4];

                piece_vector[0] = 65;   // x- Top-Left 
                piece_vector[1] = 385;  // y- Top-Left
                piece_vector[2] = 1089; // x- Bottom-Right
                piece_vector[3] = 1411; // y- Bottom-Right

                Pieces_positions.Add(piece_vector);



                Pieces_angle = new List<int>();
                Pieces_angle.Add(0); // angle

                // calcula endereço do pixel no ponto (x,y)
                blueBackground = (byte)(dataPtr + 0 * widthstep + 0 * nChan)[0];
                greenBackground = (byte)(dataPtr + 0 * widthstep + 0 * nChan)[1];
                redBackground = (byte)(dataPtr + 0 * widthstep + 0 * nChan)[2];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)


                    {

                        // calcula endereço do pixel no ponto (x,y)
                        blue = (byte)(dataPtr + y * widthstep + x * nChan)[0];
                        green = (byte)(dataPtr + y * widthstep + x * nChan)[1];
                        red = (byte)(dataPtr + y * widthstep + x * nChan)[2];

                        if(blue == blueBackground && green == greenBackground && red == redBackground)
                        {
                            (dataPtr + y * widthstep + x * nChan)[0] = 0;
                            (dataPtr + y * widthstep + x * nChan)[1] = 0;
                            (dataPtr + y * widthstep + x * nChan)[2] = 0;
                        }
                 
                    }
                }


                return;
            }
        }



    }
  
}


