using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VkNet.Enums;
using VkNet.Model;
using VkNet.Model.Attachments;

namespace Schedulebot.Vk
{
    public class IVkStuffConstructor
    {
        public static VkStuff Construct(string filename)
        {
            return Read(filename);
        }

        private static VkStuff Read(string filename)
        {
            string key = null;
            string keyPhotos = null;
            long groupId = 0;
            long mainAlbumId = 0;
            long adminId = 0;
            string groupUrl = null;
            Photo textCommandsInfo = null;
            Document subscribeInfo = null;

            using (StreamReader streamReader = new StreamReader(filename, Encoding.Default))
            {
                string str, value;
                while (!streamReader.EndOfStream)
                {
                    str = streamReader.ReadLine();
                    if (!str.Contains(':'))
                        continue;

                    value = str.Substring(str.IndexOf(':') + 1);
                    str = str.Substring(0, str.IndexOf(':'));
                    switch (str)
                    {
                        case "key":
                            key = value;
                            break;
                        case "keyPhotos":
                            keyPhotos = value;
                            break;
                        case "groupId":
                            groupId = long.Parse(value);
                            break;
                        case "mainAlbumId":
                            mainAlbumId = long.Parse(value);
                            break;
                        case "groupUrl":
                            groupUrl = value;
                            break;
                        case "adminId":
                            adminId = long.Parse(value);
                            break;
                        case "textCommandsInfo":
                            textCommandsInfo = new Photo()
                            {
                                AlbumId = long.Parse(value.Substring(value.IndexOf('-') + 1, value.IndexOf('_') - value.IndexOf('-') - 1)),
                                Id = long.Parse(value.Substring(value.IndexOf('_') + 1))
                            };
                            break;
                        case "subscribeInfo":
                            subscribeInfo = new Document()
                            {
                                Type = DocumentTypeEnum.Gif,
                                Id = long.Parse(value.Substring(value.IndexOf('_') + 1))
                            };
                            break;
                    }
                }
            }

            textCommandsInfo.OwnerId = -groupId;
            subscribeInfo.OwnerId = -groupId;

            return new VkStuff(key, keyPhotos, groupId, mainAlbumId, adminId, groupUrl, textCommandsInfo, subscribeInfo);
        }
    }
}
