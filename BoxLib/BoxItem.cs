using Box.Sdk.Gen.Schemas;

namespace BoxLib
{
    public enum BoxItemType
    {
        File,
        Folder,
        WebLink
    }

    public class BoxItem
    {
        public string Id { get; }
        public string Name { get; }
        public BoxItemType Type { get; } // Use enum now

        public BoxItem(FileFullOrFolderMiniOrWebLink item)
        {
            if (item.FolderMini != null)
            {
                Id = item.FolderMini.Id;
                Name = item.FolderMini.Name ?? "";
                Type = BoxItemType.Folder;
            }
            else if (item.FileFull != null)
            {
                Id = item.FileFull.Id;
                Name = item.FileFull.Name ?? "";
                Type = BoxItemType.File;
            }
            else if (item.WebLink != null)
            {
                Id = item.WebLink.Id;
                Name = item.WebLink.Name ?? "";
                Type = BoxItemType.WebLink;
            }
            else
            {
                throw new ArgumentException("Unknown Box item type", nameof(item));
            }
        }

        public BoxItem(FileFull item)
        {
            Id = item.Id;
            Name = item.Name ?? "";
            Type = BoxItemType.File;
        }
    }
}
