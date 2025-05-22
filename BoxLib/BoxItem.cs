using Box.Sdk.Gen.Schemas;

namespace BoxLib
{
    public class BoxItem
    {
        public string Id { get; }
        public string Name { get; }
        public string Type { get; } // "file", "folder", or "web_link"

        public BoxItem(FileFullOrFolderMiniOrWebLink item)
        {
            if (item.FolderMini != null)
            {
                Id = item.FolderMini.Id;
                Name = item.FolderMini.Name ?? "";
                Type = "folder";
            }
            else if (item.FileFull != null)
            {
                Id = item.FileFull.Id;
                Name = item.FileFull.Name ?? "";
                Type = "file";
            }
            else if (item.WebLink != null)
            {
                Id = item.WebLink.Id;
                Name = item.WebLink.Name ?? "";
                Type = "web_link";
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
            Type = "file";
        }
    }
}
