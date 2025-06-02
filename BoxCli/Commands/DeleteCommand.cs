using System.CommandLine;
using BoxLib;

namespace BoxCli
{
    partial class Program
    {
        public async Task DoDelete(string itemId)
        {
            try
            {
                await boxUtils.DeleteFile(itemId);
                Console.WriteLine($"Item '{itemId}' deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting item: {ex.Message}");
            }
            finally
            {
                try
                {
                    await boxItemFetcher.PopulateItemsAsync(GetCurrentFolderId());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error refreshing items: {ex.Message}");
                }
            }
        }

        private Command DeleteCommand()
        {
            var itemArg = new System.CommandLine.Argument<string>("itemId", "ID of the file or folder to delete")
            {
                Arity = System.CommandLine.ArgumentArity.ExactlyOne
            };
            var command = new System.CommandLine.Command("del", "Delete a file or folder")
            {
                itemArg
            };
            command.SetHandler(async (item) =>
            {
                await DoDelete(item);
            }, itemArg);
            return command; 
        }
    }
}