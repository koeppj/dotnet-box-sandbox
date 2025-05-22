using Box.Sdk.Gen;
using Box.Sdk.Gen.Managers;


namespace BoxLib
{
    public class BoxUtils
    {
        private readonly BoxClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxUtils"/> class using the specified Box JWT configuration file.
        /// </summary>
        /// <param name="configFile">Path to the Box JWT config JSON file.</param>
        public BoxUtils(string configFile, string? asUserId = null)
        {
            // Validate the config file path
            if (string.IsNullOrEmpty(configFile) || !System.IO.File.Exists(configFile))
            {
                throw new ArgumentException("Invalid config file path.", nameof(configFile));
            }
            // Initialize the Box client using JWT authentication
            var auth = new BoxJwtAuth(JwtConfig.FromConfigFile(configFile));
            var client = new BoxClient(auth);
            if (!string.IsNullOrEmpty(asUserId))
            {
                _client = client.WithAsUserHeader(asUserId);
            }
            else
            {
                _client = client;
            }
        }

        // <summary>
        // Lists items in a specified folder.
        // </summary>
        // <description>
        // Lists all items (files, folders, and web links) in the specified Box folder 
        // (see https://developer.box.com/reference/get-folders-id-items/).
        // </description>
        // <param name="folderId">The ID of the folder to list items from.</param>
        public async Task<List<BoxItem>> ListFolderItemsAsync(string folderId)
        {
            GetFolderByIdHeaders _apiHeaders = new GetFolderByIdHeaders();
            var folder = await _client.Folders.GetFolderItemsAsync(folderId);
            var result = new List<BoxItem>();
            if (folder.Entries != null)
            {
                if (folder.Entries.Count == 0)
                {
                    Console.WriteLine($"Folder {folderId} is empty.");
                    return new List<BoxItem>();
                }
                foreach (var item in folder.Entries)
                {
                    Console.WriteLine($"{GetItemType(item)} called '{GetNameFromItem(item)}' with ID {GetIdFromItem(item)}");
                    result.Add(new BoxItem(item));
                }
                return result;
            }
            else
            {
                Console.WriteLine($"No entries found in folder {folderId}.");
                return new List<BoxItem>();
            }
        }

        /// <summary>
        /// Finds and prints the Box item (file, folder, or web link) corresponding to the specified path.
        /// The path is split by '/' and traversed from the root folder to locate the item.
        /// </summary>
        /// <description>
        /// Starting from the root folder, this method traverses the Box folder structure
        /// by calling getFolderItemsAsync for each part of the path.  If the item is found based on
        /// the path part, it loops back with the new folder ID.  If the item is not found, it breaks out of the loop.
        /// </description>
        /// <param name="path">The slash-separated path to the item (e.g., "Folder1/Subfolder2/File.txt").</param>
        public async Task<BoxItem> ItemByPathAsync(string path)
        {
            var pathParts = path.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
            var folderId = "0"; // Start with the root folder ID
            var found = true;
            Box.Sdk.Gen.Schemas.FileFullOrFolderMiniOrWebLink? foundItem = null;
            foreach (var part in pathParts)
            {
                var items = await _client.Folders.GetFolderItemsAsync(folderId);
                if (items.Entries != null)
                {
                    var item = FindItemInEntries(part, items.Entries);
                    if (item == null)
                    {
                        found = false;
                        break;
                    }
                    foundItem = item;
                    folderId = GetIdFromItem(item);
                }
                else
                {
                    found = false;
                    break;
                }
            }
            if (found && foundItem != null)
            {
                return new BoxItem(foundItem);
            }
            else
            {
                throw new BoxException($"Item '{path}' not found.");
            }   
        }

        /// <summary>
        /// Uploads a file to the specified Box folder.
        /// </summary>
        /// <param name="folderId">The ID of the Box folder to upload the file to.</param>
        /// <param name="filePath">The local path to the file to be uploaded.</param>
        /// <throws>BoxException</throws>
        /// <description>
        /// Uploads a file to the specified Box folder using the Box API.
        /// The file is read as a stream and uploaded to the Box folder.
        /// If the upload is successful, the method returns the uploaded file's BoxItem.
        /// If the upload fails, a BoxException is thrown.
        /// </description>
        /// <returns>A task representing the asynchronous upload operation.</returns>
        public async Task<BoxItem?> UploadFile(string folderId, string filePath)
        {
            // Check if the file is empty
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                throw new ArgumentException("File does not exist or is empty.", nameof(filePath));
            }
            var fileName = Path.GetFileName(filePath);
            // Get the file content as a stream
            using var fileContentStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var uploadResult = await _client.Uploads.UploadFileAsync(
                requestBody: new UploadFileRequestBody(
                    attributes: new UploadFileRequestBodyAttributesField(
                        name: fileName,
                        parent: new UploadFileRequestBodyAttributesParentField(id: folderId)),
                    file: fileContentStream
                )
            );
            if (uploadResult.Entries != null && uploadResult.Entries.Count > 0)
            {
                var uploadedFile = uploadResult.Entries[0];
                return new BoxItem(uploadedFile);
            }
            else
            {
                throw new BoxException("Upload failed: No entries found in the upload result.");
            }
        }

        public async Task ApplyMatadataToItem(string itemId)
        {
            Dictionary<string, object> metadata = new Dictionary<string, object>
            {
                { "documentnumber", "12345" },
                { "batchname", "A Batch" },
                { "doctype", "TITLE" },
                { "issueDate", DateTime.UtcNow.ToString("yyyy-MM-ddT00:00:00Z") },
            };
            await _client.FileMetadata.CreateFileMetadataByIdAsync(
                itemId,
                CreateFileMetadataByIdScope.Enterprise,
                "titleDocuments",
                metadata
            );
        }

        public async Task<string> DeleteFile(string itemId)
        {
            await _client.Files.DeleteFileByIdAsync(itemId);
            return "File deleted successfully.";
        }

        private static Box.Sdk.Gen.Schemas.FileFullOrFolderMiniOrWebLink? FindItemInEntries(
            string itemName,
            IReadOnlyList<Box.Sdk.Gen.Schemas.FileFullOrFolderMiniOrWebLink> entries)
        {
            foreach (var entry in entries)
            {
                if (entry.FolderMini != null && entry.FolderMini.Name == itemName)
                {
                    return entry;
                }
                else if (entry.FileFull != null && entry.FileFull.Name == itemName)
                {
                    return entry;
                }
                else if (entry.WebLink != null && entry.WebLink.Name == itemName)
                {
                    return entry;
                }
            }
            return null;
        }

        private static string GetIdFromItem(
            Box.Sdk.Gen.Schemas.FileFullOrFolderMiniOrWebLink item)
        {
            if (item.FolderMini != null)
            {
                return item.FolderMini.Id;
            }
            else if (item.FileFull != null)
            {
                return item.FileFull.Id;
            }
            else if (item.WebLink != null)
            {
                return item.WebLink.Id;
            }
            throw new InvalidOperationException("Unknown item type");
        }

        private static string GetItemType(
            Box.Sdk.Gen.Schemas.FileFullOrFolderMiniOrWebLink item)
        {
            if (item.FolderMini != null)
            {
                return "Folder";
            }
            else if (item.FileFull != null)
            {
                return "File";
            }
            else if (item.WebLink != null)
            {
                return "Web Link";
            }
            throw new InvalidOperationException("Unknown item type");
        }

        private static string GetNameFromItem(
            Box.Sdk.Gen.Schemas.FileFullOrFolderMiniOrWebLink item)
        {
            if (item.FolderMini != null)
            {
                return item.FolderMini.Name ?? "FolderMini.Name is null";
            }
            else if (item.FileFull != null)
            {
                return item.FileFull.Name ?? "FileFull.Name is null";
            }
            else if (item.WebLink != null)
            {
                return item.WebLink.Name ?? "WebLink.Name is null";
            }
            throw new InvalidOperationException("Unknown item type");
        }
    }
}