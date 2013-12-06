using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace ServeRick.Modules
{
    /// <summary>
    /// Class creating captcha images
    /// </summary>
    public class Captcha
    {
        private static readonly HatchBrush[] _bgBrushes;

        /// <summary>
        /// Width of generated captchas
        /// </summary>
        public readonly int Width;

        /// <summary>
        /// Height of generated captchas
        /// </summary>
        public readonly int Height;

        private readonly Font _font;

        private readonly int _fontSize;

        private readonly int _padding;

        private readonly Random _rnd = new Random();

        private readonly Region _region;

        static Captcha()
        {
            var brushes = new List<HatchBrush>();
            foreach (var style in new[]{
                HatchStyle.DarkUpwardDiagonal,
                HatchStyle.DarkHorizontal,
                HatchStyle.DarkVertical,
                HatchStyle.DarkDownwardDiagonal
            })
            {
                brushes.Add(new HatchBrush(style, Color.White));
            }

            _bgBrushes = brushes.ToArray();
        }

        public Captcha(int width, int height)
        {
            Width = width;
            Height = height;

            _region = new Region(new Rectangle(new Point(0, 0), new Size(Width, Height)));
            _fontSize = (int)(Height * 0.75);
            _padding = (int)(Height * 0.15);

            _font = new Font(new FontFamily("arial"), _fontSize, FontStyle.Bold);
        }

        public Bitmap Create(string code)
        {
            var bm = new Bitmap(Width, Height, PixelFormat.Format16bppRgb555);

            using (var gr = Graphics.FromImage(bm))
            {
                var randBrush = _rnd.Next(_bgBrushes.Length);

                gr.FillRegion(_bgBrushes[randBrush], _region);
                var rotation = new Matrix();
                var length = code.Length;
                for (var i = 0; i <= length - 1; i++)
                {
                    var charString = code.Substring(i, 1);
                    rotation.Reset();

                    var x = Width / (length + 1) * i;
                    var y = Height / 2;

                    var rotX = x;
                    var rotY = _rnd.Next(Height / 3) - _padding;
                    //Rotate text Random
                    var rotationRange = 20;
                    rotation.RotateAt(_rnd.Next(-rotationRange, rotationRange), new PointF(rotX, rotY));
                    gr.Transform = rotation;

                    gr.DrawString(charString, _font, Brushes.Black, new PointF(rotX, rotY));
                    gr.ResetTransform();
                }
                gr.Save();
            }

            return bm;
        }
    }
}
