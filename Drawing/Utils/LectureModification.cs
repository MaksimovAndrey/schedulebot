using SixLabors.Fonts;

namespace Schedulebot.Drawing.Utils
{
    public static class LectureModification
    {
        public static string TruncateSubject(string subject, int maxWidth, RendererOptions rendererOptions)
        {
            var fontRectangle = TextMeasurer.Measure(subject, rendererOptions);
            if (fontRectangle.Width > maxWidth)
            {
                do
                {
                    subject = subject[0..^1];
                    fontRectangle = TextMeasurer.Measure(subject, rendererOptions);
                }
                while (fontRectangle.Width > maxWidth);
                subject = subject[0..^1];
                while (subject[^1] == ' ' || subject[^1] == '(' || subject[^1] == '.')
                    subject = subject[0..^1].TrimEnd();
                subject += "...";
            }
            return subject;
        }
    }
}
