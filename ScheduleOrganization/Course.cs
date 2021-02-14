using Schedulebot.Parsing;
using Schedulebot.Vk;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model.Keyboard;

namespace Schedulebot
{
    public class Course
    {
        public List<Group> groups;

        public Course()
        {
            groups = new List<Group>();
        }

        /// <summary>
        /// Обновляет расписание
        /// </summary>
        /// <param name="updateProperties">Информация, необходимая для отрисовки и загрузки фотографии расписания</param>
        /// <param name="dictionaries">Словари для продвинутого парсинга нового расписания</param>
        /// <returns>Список фотографий расписания на неделю, которые необходимо загрузить</returns>
        //public List<PhotoUploadProperties> Update(UpdateProperties updateProperties, Dictionaries dictionaries)
        //{
        //    isUpdating = true;
        //    List<Group> newGroups = Parser.Mapper(PathsToFile, dictionaries);
        //    if (newGroups == null || newGroups.Count == 0)
        //    {
        //        isBroken = true;
        //        return null;
        //    }
        //    else
        //    {
        //        List<(int, int, string)> groupSubgroupToUpdateList = UpdateGroupsAndGetChanges(newGroups);

        //        List<PhotoUploadProperties> photosToUpload = new List<PhotoUploadProperties>();
        //        for (int i = 0; i < groupSubgroupToUpdateList.Count; i++)
        //        {
        //            groups[groupSubgroupToUpdateList[i].Item1].DrawSubgroupSchedule(
        //                groupSubgroupToUpdateList[i].Item2, ref updateProperties);

        //            photosToUpload.Add(new PhotoUploadProperties(updateProperties.photoUploadProperties));
        //            if (groupSubgroupToUpdateList[i].Item3 != "new")
        //                photosToUpload[i].Message += "\n\n• Изменения\n\n" + groupSubgroupToUpdateList[i].Item3;
        //        }
        //        isBroken = false;
        //        return photosToUpload;
        //    }
        //}

        /// <summary>
        /// Сравнивает старые и новые группы, ищет изменения в расписании
        /// <br>А также переносит id актуальных фотографий</br>
        /// </summary>
        /// <param name="newGroups">Cписок новых групп</param>
        /// <returns>Список, состоящий из (Индекс группы, подгруппа, изменения в расписании(new, если старых групп не было))</returns>
        //public List<(int, int, string)> UpdateGroupsAndGetChanges(List<Group> newGroups)
        //{
        //    List<(int, int, string)> groupSubgroupChangesTuples = new List<(int, int, string)>(); // index of a group, subgroup
        //    for (int newGroupIndex = 0; newGroupIndex < newGroups.Count; ++newGroupIndex)
        //    {
        //        if (groups == null || groups.Count == 0)
        //        {
        //            groupSubgroupChangesTuples.Add((newGroupIndex, 0, "new"));
        //            groupSubgroupChangesTuples.Add((newGroupIndex, 1, "new"));
        //            continue;
        //        }
        //        for (int groupIndex = 0; groupIndex < groups.Count; ++groupIndex)
        //        {
        //            if (groups[groupIndex].name == newGroups[newGroupIndex].name)
        //            {
        //                var subgroupChangesTuples = newGroups[newGroupIndex]
        //                    .GetChangesAndKeepActualPhotoIds(groups[groupIndex]);
        //                for (int i = 0; i < subgroupChangesTuples.Count; i++)
        //                    groupSubgroupChangesTuples.Add((
        //                        newGroupIndex,
        //                        subgroupChangesTuples[i].Item1,
        //                        subgroupChangesTuples[i].Item2));
        //                break;
        //            }
        //        }
        //    }
        //    groups = newGroups;
        //    return groupSubgroupChangesTuples;
        //}

        //[Obsolete("Используйте CompareGroupsAndGetChanges")]
        //public List<(int, int)> CompareGroups(ref List<Group> newGroups)
        //{
        //    List<(int, int)> groupSubgroupTuplesToUpdate = new List<(int, int)>(); // index of a group, subgroup
        //    for (int currentNewGroup = 0; currentNewGroup < newGroups.Count; ++currentNewGroup)
        //    {
        //        for (int currentGroup = 0; currentGroup < groups.Count; ++currentGroup)
        //        {
        //            if (groups[currentGroup].name == newGroups[currentNewGroup].name)
        //            {
        //                for (int currentSubgroup = 0; currentSubgroup < 2; currentSubgroup++)
        //                {
        //                    for (int currentWeek = 0; currentWeek < 2; currentWeek++)
        //                    {
        //                        for (int currentDay = 0; currentDay < 6; currentDay++)
        //                        {
        //                            if (groups[currentGroup].subgroups[currentSubgroup].weeks[currentWeek].days[currentDay]
        //                                == newGroups[currentNewGroup].subgroups[currentSubgroup].weeks[currentWeek].days[currentDay])
        //                            {
        //                                newGroups[currentNewGroup].subgroups[currentSubgroup].weeks[currentWeek].days[currentDay].PhotoId
        //                                    = groups[currentGroup].subgroups[currentSubgroup].weeks[currentWeek].days[currentDay].PhotoId;
        //                            }
        //                        }
        //                    }
        //                    if (groups[currentGroup].subgroups[currentSubgroup]
        //                        == newGroups[currentNewGroup].subgroups[currentSubgroup])
        //                    {
        //                        newGroups[currentNewGroup].subgroups[currentSubgroup].PhotoId
        //                            = groups[currentGroup].subgroups[currentSubgroup].PhotoId;
        //                    }
        //                    else
        //                    {
        //                        groupSubgroupTuplesToUpdate.Add((currentNewGroup, currentSubgroup));
        //                    }
        //                }
        //                break;
        //            }
        //        }
        //    }
        //    return groupSubgroupTuplesToUpdate;
        //}
    }
}