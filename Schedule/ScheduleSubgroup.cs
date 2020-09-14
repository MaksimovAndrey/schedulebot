using System;
using System.Collections.Generic;
using System.Text;

namespace Schedulebot.Schedule
{
    public class ScheduleSubgroup
    {
        public ScheduleWeek[] weeks;
        public long PhotoId { get; set; } = 0; // вынести

        public int WeeksCount { get; }

        public ScheduleSubgroup(int weeksCount = 2)
        {
            WeeksCount = weeksCount;
            weeks = new ScheduleWeek[WeeksCount];
            for (int i = 0; i < WeeksCount; ++i)
                weeks[i] = new ScheduleWeek();
        }

        public void SortLectures()
        {
            for (int i = 0; i < WeeksCount; i++)
            {
                weeks[i].SortLectures();
            }
        }

        /// <summary>
        /// Ищет изменения в расписании этой подгруппы, а также сохраняет актуальные фотографии
        /// <br>Обращаемся к новой подгруппе, передаем устаревшую подгруппу</br>
        /// </summary>
        /// <param name="oldSubgroup">Устаревшая подгруппа</param>
        /// <returns>Строка с изменения в расписании</returns>
        public string GetChangesAndKeepActualPhotoIds(ScheduleSubgroup oldSubgroup)
        {
            StringBuilder changesBuilder = new StringBuilder();
            for (int dayIndex = 0; dayIndex < 6; dayIndex++)
            {
                if (oldSubgroup.weeks[0].days[dayIndex] != weeks[0].days[dayIndex]
                    || oldSubgroup.weeks[1].days[dayIndex] != weeks[1].days[dayIndex])
                {
                    string[] changesWeek = new string[2];
                    for (int currentWeek = 0; currentWeek < 2; currentWeek++)
                    {
                        changesWeek[currentWeek] =
                            weeks[currentWeek].days[dayIndex].GetChanges(
                                oldSubgroup.weeks[currentWeek].days[dayIndex].lectures);
                    }

                    if (changesWeek[0] == changesWeek[1])
                    {
                        if (changesWeek[0] != "")
                        {
                            changesBuilder.Append("· ");
                            changesBuilder.Append(Utils.Converter.IndexToDay(dayIndex));
                            changesBuilder.Append('\n');
                            changesBuilder.Append(changesWeek[0]);
                            changesBuilder.Append('\n');
                        }
                    }
                    else
                    {
                        for (int currentWeek = 0; currentWeek < 2; currentWeek++)
                        {
                            if (changesWeek[currentWeek] != "")
                            {
                                changesBuilder.Append("· ");
                                changesBuilder.Append(Utils.Converter.IndexToDay(dayIndex));
                                changesBuilder.Append(" (");
                                changesBuilder.Append(currentWeek == 0 ? "верхняя" : "нижняя");
                                changesBuilder.Append(")\n");
                                changesBuilder.Append(changesWeek[currentWeek]);
                                changesBuilder.Append('\n');
                            }
                        }
                    }
                }
                // тут сохраняем в новое расписание загруженные фотографии (если они актуальны)
                for (int currentWeek = 0; currentWeek < 2; currentWeek++)
                {
                    if (oldSubgroup.weeks[currentWeek].days[dayIndex]
                        == weeks[currentWeek].days[dayIndex])
                    {
                        weeks[currentWeek].days[dayIndex].PhotoId
                            = oldSubgroup.weeks[currentWeek].days[dayIndex].PhotoId;
                    }
                }
            }
            return changesBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is ScheduleSubgroup subgroup
                && EqualityComparer<ScheduleWeek[]>.Default.Equals(weeks, subgroup.weeks);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(weeks);
        }

        public static bool operator ==(ScheduleSubgroup schedule1, ScheduleSubgroup schedule2)
        {
            if (schedule1.WeeksCount != schedule2.WeeksCount)
                return false;
            for (int i = 0; i < schedule1.WeeksCount; ++i)
            {
                if (schedule1.weeks[i] != schedule2.weeks[i])
                    return false;
            }
            return true;
        }

        public static bool operator !=(ScheduleSubgroup schedule1, ScheduleSubgroup schedule2)
        {
            return !(schedule1 == schedule2);
        }
    }
}