using Schedulebot.Schedule;
using System.Linq;
using Schedulebot.Drawing.Utils;
using Schedulebot.Parsing.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Drawing;
using SixLabors.Fonts;
using System.Numerics;

namespace Schedulebot.Drawing.Day
{
    public static class Drawer
    {
        public static byte[] Draw(DrawerInfo drawerInfo)
        {
            Image image = new Image<Rgb24>(ScheduleBot.configuration, RenderInfo.Image.width, RenderInfo.Image.height);
            // Метадата
            image.Metadata.VerticalResolution = 72;
            image.Metadata.HorizontalResolution = 72;

            image.Mutate(x => x.Fill(RenderInfo.backgroundColor)); // Заливаем фон
            int pos = RenderInfo.Border.size; // y

            // Рисуем количество пар
            image.Mutate(x => x.DrawText(
                RenderInfo.textGraphicsOptions,
                Schedulebot.Utils.Converter.LecturesCountToString(drawerInfo.day.lectures.Count),
                RenderInfo.lecturesCountFont,
                RenderInfo.textColor,
                new Vector2(RenderInfo.timeCenterX, pos + RenderInfo.stringHeightIndent)
            ));

            // Рисуем день и группу
            image.Mutate(x => x.DrawText(
                RenderInfo.textGraphicsOptions,
                drawerInfo.group + Constants.delimiter
                    + Schedulebot.Utils.Converter.DayOfWeekToString(drawerInfo.day.Date.DayOfWeek),
                RenderInfo.dayOfWeekFont,
                RenderInfo.textColor,
                new Vector2(RenderInfo.cellCenterX, pos + RenderInfo.stringHeightIndent)
            ));

            pos += RenderInfo.stringHeight;
            // Отделяем
            image.Mutate(x => x.FillPolygon(RenderInfo.Border.color, new PointF[] {
                new Vector2(RenderInfo.Border.size, pos),
                new Vector2(RenderInfo.Image.width - RenderInfo.Border.size, pos),
                new Vector2(RenderInfo.Image.width - RenderInfo.Border.size, pos + RenderInfo.Line.size),
                new Vector2(RenderInfo.Border.size, pos + RenderInfo.Line.size)
            }));
            pos += RenderInfo.Line.size; // Переносим координату

            // Проходим по парам
            for (int currentLecture = 0; currentLecture < drawerInfo.day.lectures.Count; currentLecture++)
            {
                DrawTime(ref image, pos, drawerInfo.day.lectures[currentLecture].TimeStart, drawerInfo.day.lectures[currentLecture].TimeEnd);
                DrawLecture(ref image, pos, drawerInfo.day.lectures[currentLecture]);

                image.Mutate(x => x.FillPolygon(RenderInfo.Line.color, new PointF[] {
                    new Vector2(RenderInfo.timeEndPosX, pos),
                    new Vector2(RenderInfo.timeEndPosX + RenderInfo.Line.size, pos),
                    new Vector2(RenderInfo.timeEndPosX + RenderInfo.Line.size, pos + RenderInfo.cellHeight),
                    new Vector2(RenderInfo.timeEndPosX, pos + RenderInfo.cellHeight)
                }));

                pos += RenderInfo.cellHeight;
                // Отделяем
                image.Mutate(x => x.FillPolygon(RenderInfo.Line.color, new PointF[] {
                    new Vector2(RenderInfo.Border.size, pos),
                    new Vector2(RenderInfo.Image.width - RenderInfo.Border.size, pos),
                    new Vector2(RenderInfo.Image.width - RenderInfo.Border.size, pos + RenderInfo.Line.size),
                    new Vector2(RenderInfo.Border.size, pos + RenderInfo.Line.size)
                }));
                pos += RenderInfo.Line.size; // Переносим координату
            }

            // Рисуем подвал
            image.Mutate(x => x.DrawText(
                RenderInfo.textGraphicsOptions,
                Constants.groupUrl + Constants.delimiter + Constants.version, //drawerInfo.vkGroupUrl + Constants.delimiter + Constants.version,
                RenderInfo.headerFont,
                RenderInfo.textColor,
                new Vector2(RenderInfo.Image.width / 2, pos + RenderInfo.stringHeightIndent)
            ));

            pos += RenderInfo.stringHeight;

            // Рисуем границы
            // Верх
            image.Mutate(x => x.FillPolygon(RenderInfo.Border.color, new PointF[] {
                new Vector2(0, 0),
                new Vector2(RenderInfo.Image.width, 0),
                new Vector2(RenderInfo.Image.width, RenderInfo.Border.size),
                new Vector2(0, RenderInfo.Border.size)
            }));
            // Низ
            image.Mutate(x => x.FillPolygon(RenderInfo.Border.color, new PointF[] {
                new Vector2(0, pos),
                new Vector2(RenderInfo.Image.width, pos),
                new Vector2(RenderInfo.Image.width, pos + RenderInfo.Border.size),
                new Vector2(0, pos + RenderInfo.Border.size)
            }));
            // Лево
            image.Mutate(x => x.FillPolygon(RenderInfo.Border.color, new PointF[] {
                new Vector2(0, RenderInfo.Border.size),
                new Vector2(RenderInfo.Border.size, RenderInfo.Border.size),
                new Vector2(RenderInfo.Border.size, pos),
                new Vector2(0, pos)
            }));
            // Право
            image.Mutate(x => x.FillPolygon(RenderInfo.Border.color, new PointF[] {
                new Vector2(RenderInfo.Image.width - RenderInfo.Border.size, RenderInfo.Border.size),
                new Vector2(RenderInfo.Image.width, RenderInfo.Border.size),
                new Vector2(RenderInfo.Image.width, pos),
                new Vector2(RenderInfo.Image.width - RenderInfo.Border.size, pos)
            }));

            pos += RenderInfo.Border.size;
            image.Mutate(x => x.Crop(RenderInfo.Image.width, pos)); // Обрезаем

            var result = Schedulebot.Utils.Converter.ImageToByteArray(image);
            image.Dispose();

            return result;
        }

        private static void DrawTime(ref Image image, int pos, string timeStart, string timeEnd)
        {
            image.Mutate(x => x.DrawText(
                new TextGraphicsOptions()
                {
                    TextOptions = new TextOptions()
                    {
                        HorizontalAlignment = HorizontalAlignment.Left
                    }
                },
                timeStart,
                RenderInfo.lecturesCountFont,
                RenderInfo.textColor,
                new Vector2(RenderInfo.timeCenterX - 37, pos + 13)
            ));

            image.Mutate(x => x.DrawText(
                new TextGraphicsOptions()
                {
                    TextOptions = new TextOptions()
                    {
                        HorizontalAlignment = HorizontalAlignment.Right
                    }
                },
                timeEnd,
                RenderInfo.lecturesCountFont,
                RenderInfo.textColor,
                new Vector2(RenderInfo.timeCenterX + 39, pos + RenderInfo.cellHeight - 43)
            ));
        }

        private static void DrawLecture(ref Image image, int pos, ScheduleLecture lecture)
        {
            image.Mutate(x => x.DrawText(
                RenderInfo.textGraphicsOptions,
                lecture.LectureHall + Constants.delimiter + lecture.Type,
                RenderInfo.lecturesCountFont,
                RenderInfo.textColor,
                new Vector2(RenderInfo.cellCenterX, pos + RenderInfo.stringHeightIndent)
            ));

            image.Mutate(x => x.DrawText(
                RenderInfo.textGraphicsOptions,
                lecture.Lecturer,
                RenderInfo.lecturesCountFont,
                RenderInfo.textColor,
                new Vector2(RenderInfo.cellCenterX, pos + RenderInfo.stringHeightIndent + RenderInfo.lectureHeightIndent)
            ));

            image.Mutate(x => x.DrawText(
                RenderInfo.textGraphicsOptions,
                LectureModification.TruncateSubject(lecture.Subject, RenderInfo.cellWidth - 10, RenderInfo.subjectRendererOptions),
                RenderInfo.lecturesCountFont,
                RenderInfo.textColor,
                new Vector2(RenderInfo.cellCenterX, pos + RenderInfo.stringHeightIndent + RenderInfo.lectureHeightIndent * 2)
            ));
        }
    }
}
