// See https://aka.ms/new-console-template for more information
using Box.Sdk.Gen;
using Box.Sdk.Gen.Schemas;

var auth = new BoxJwtAuth(JwtConfig.FromConfigFile("../boxapp-config.json"));
var client = new BoxClient(auth);

var folder = await client.Folders.GetFolderItemsAsync("0");

if (folder.Entries != null)
{
    Console.WriteLine("Items in the root folder:" + folder.Entries.Count);
}
else
{
    Console.WriteLine("No entries found in the root folder.");
}

