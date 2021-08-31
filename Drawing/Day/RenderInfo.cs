using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace Schedulebot.Drawing.Day
{
    public struct RenderInfo
    {
        public struct Image
        {
            public const int width = 588; // ширина
            public const int height = 1500; // высота
        }

        private static readonly FontCollection collection = new FontCollection();

#if DEBUG
        private const string path = @"C:\Main\Projects\Repositories\schedulebot\.fonts\";
#else
        private const string path = "/media/projects/sbtest/fonts/";
#endif
        private static readonly FontFamily mediumFamily = collection.Install(path + "PTRootUI-Medium.ttf");
        private static readonly FontFamily lightFamily = collection.Install(path + "PTRootUI-Light.ttf");
        private static readonly FontFamily regularFamily = collection.Install(path + "PTRootUI-Regular.ttf");

        public static readonly Font headerFont = mediumFamily.CreateFont(22, FontStyle.Regular);
        public static readonly Font lecturesCountFont = regularFamily.CreateFont(22, FontStyle.Regular);
        public static readonly Font dayOfWeekFont = mediumFamily.CreateFont(22, FontStyle.Regular);
        public static readonly Font footerFont = lecturesCountFont;
        public static readonly Font subjectFont = lightFamily.CreateFont(22, FontStyle.Regular);
        public static readonly Font timeFont = subjectFont;

        public static readonly TextGraphicsOptions textGraphicsOptions = new TextGraphicsOptions()
        {
            TextOptions = new TextOptions()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                //VerticalAlignment = VerticalAlignment.Center,
                ApplyKerning = true,
                DpiX = 72,
                DpiY = 72,
            },
        };

        public static readonly RendererOptions subjectRendererOptions = new RendererOptions(subjectFont, 72);

        public static readonly Color textColor = Color.Black;
        public static readonly Color backgroundColor = Color.White;

        public const int stringHeight = 40;
        public const int stringHeightIndent = 7;
        public const int lectureHeightIndent = 25;
        public const int cellHeight = 95;
        public const int cellWidth = Image.width - cellStartPosX - Border.size;
        public const int timeWidth = 114;
        public const int timeCenterX = Border.size + timeWidth / 2;
        public const int timeEndPosX = Border.size + timeWidth;
        public const int cellStartPosX = Border.size + timeWidth + Line.size;


        public const int cellCenterX = cellStartPosX + cellWidth / 2;

        public const int betweenSubjectStrIndent = 10;
        public const int lectureTopIndent = 15;

        public struct Line
        {
            public static readonly Color color = new Color(new Rgb24(211, 211, 211));
            public const int size = 4; // ширина
        }
        public struct Line2
        {
            public static readonly Color color = new Color(new Rgb24(211, 211, 211));
            public const int size = 2; // ширина
        }
        public struct Indicator
        {
            public static readonly Color color = new Color(new Rgb24(112, 112, 112));
            public const int size = 2; // ширина
        }
        public struct Border
        {
            public static readonly Color color = new Color(new Rgb24(211, 211, 211));
            public const int size = 8; // ширина
        }
    }
}