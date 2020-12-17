namespace BarcodeGenerator
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class DeltaDistance
    {
        public string Data { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }
        public float FontSize { get; set; } = 17.0f;
        public float Scale { get; set; }

        private Hashtable DeltaDistanceCode = new Hashtable()
        {
          {
            '0',
            "1010001010001010"
          },
          {
            '1',
            "1000101000101010"
          },
          {
            '2',
            "1010001010101000"
          },
          {
            '3',
            "1010001010101000"
          },
          {
            '4',
            "1000101010001010"
          },
          {
            '5',
            "1010001000101010"
          },
          {
            '6',
            "1000101010100010"
          },
          {
            '7',
            "1000101010101000"
          },
          {
            '8',
            "1010100010100010"
          },
          {
            '9',
            "1010100010101000"
          },
          {
            'K',
            "1010100010001010"
          },
          {
            'L',
            "1000100010101010"
          },
          {
            'M',
            "1010101000101000"
          },
          {
            'O',
            "1010101010001000"
          },
          {
            'k',
            "1010100010001010"
          },
          {
            'l',
            "1000100010101010"
          },
          {
            'm',
            "1010101000101000"
          },
          {
            'o',
            "1010101010001000"
          },
          {
            '$',
            "1010101010100000"
          },
          {
            '%',
            "1010101000100010"
          }
        };

        private Dictionary<string, int> DeltaDistanceDictionary = new Dictionary<string, int>()
        {
            {"0", 0 },
            {"1", 1 },
            {"2", 2 },
            {"3", 3 },
            {"4", 4 },
            {"5", 5 },
            {"6", 6 },
            {"7", 7 },
            {"8", 8 },
            {"9", 9 },
            {"K", 10 },
            {"L", 11 },
            {"M", 12 },
            {"O", 14 }
        };

        private string StartCode = "1010101010100000";
        private string StopCode = "1010101000100010";

        public DeltaDistance(string data, float width, float height, float scale)
        {
            Data = data;
            Width = width;
            Height = height;
            Scale = scale;
        }

        #region Encode
        public string Encode()
        {
            if (Data.Length < 2)
                throw new Exception("Delta Distance format required min 2 char");
            if (!new Regex("^[0-9KLMO]+$").IsMatch(Data.ToUpper()))
                throw new Exception("Delta Distance data contains invalid charecter");
            //Data += CalculateCheckSymbol();
            return GetEncodedData();
        }

        private string GetEncodedData()
        {
            var encodedData = StartCode + " ";
            for (int i = 0; i < Data.Length; i++)
                encodedData += DeltaDistanceCode[Data[i]] + " ";
            encodedData += StopCode;
            return encodedData;
        }

        private string CalculateCheckSymbol()
        {
            var data = Reverse(Data);
            var mult = new List<int>();
            int sum = 0;
            for (int i = 2; i < Data.Length + 2; i++)
            {
                if (i % 2 == 0)
                    mult.Add(DeltaDistanceDictionary[data[i - 2].ToString().ToUpper()] * 2);
                else
                    mult.Add(DeltaDistanceDictionary[data[i - 2].ToString().ToUpper()]);
            }
            foreach (int digit in mult)
            {
                if (digit > 9)
                    sum += digit % 10;
                else
                    sum += digit;
            } 
            int res = ClosestDec(sum) - sum;
            return res.ToString();
        }

        private int ClosestDec(int digit)
        {
            int res = digit;
            while (res % 10 != 0)
                res++;
            return res;
        }

        private string Reverse(string str)
        {
            string reverseStr = "";
            for (int i = str.Length - 1; i >= 0; i--)
            {
                reverseStr += str[i];
            }
            return reverseStr;
        }

        public Bitmap GenerateImage()
        {
            if (Data.Length < 2)
                throw new Exception("Delta Distance format required min 2 char");
            if (!new Regex("^[0-9KLMO]+$").IsMatch(Data.ToUpper()))
                throw new Exception("Delta Distance data contains invalid charecter");
            Bitmap bitmap = new Bitmap((int)(float)((double)Width * Scale * 300.0), (int)(float)((double)Height * Scale * 100.0));
            Graphics g = Graphics.FromImage((Image)bitmap);
            DrawBarcode(g, new Point(0, 0));
            g.Dispose();
            return bitmap;
        }

        private void DrawBarcode(Graphics g, Point pt)
        {
            Data = "$" + Data + "%";
            float num1 = Width * Scale;
            float num2 = Height * Scale;
            float width = num1 / 87f;
            GraphicsState gstate = g.Save();
            g.PageUnit = GraphicsUnit.Inch;
            g.PageScale = 1f;
            SolidBrush solidBrush = new SolidBrush(Color.Black);
            float x1 = 0.0f;
            StringBuilder stringBuilder = new StringBuilder();
            float num3 = pt.X;
            float y = pt.Y;
            float num4 = 0.0f;
            Font font = new Font("Arial", FontSize * Scale);
            string str1 = string.Join("", Data.Select((x => DeltaDistanceCode[x].ToString() + "0")));
            string str2 = str1.Substring(0, str1.Length - 1);
            string text = str2;
            float height = g.MeasureString(text, font).Height;
            int startIndex = 0;
            for (startIndex = 0; startIndex < str2.Length; ++startIndex)
            {
                if (text.Substring(startIndex, 1) == "1")
                {
                    if (num3 == pt.X)
                        num3 = x1;
                    g.FillRectangle((Brush)solidBrush, x1, y, width, num2 - height);
                }
                x1 += width;
                num4 = x1;
            }
            g.Restore(gstate);
            Data = Data.Substring(1, Data.Length - 2);
        }
        #endregion

        #region Decode
        public string Decode(string str)
        {
            var decoded = "";
            var codeParts = str.Trim().Split(' ');
            for (int i = 1; i < codeParts.Length - 1; i++)
            {
                decoded += GetKeyFromHach(codeParts[i]);
            }
            return decoded;
        }

        public string GetKeyFromHach(string value)
        {
            foreach (char key in DeltaDistanceCode.Keys)
                if ((string)DeltaDistanceCode[key] == value)
                    return key.ToString().ToUpper();
            throw new Exception("Wrong code format.");
        }
        #endregion
	}
}
