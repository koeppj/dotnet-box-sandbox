using System.Collections.Concurrent;
using BoxLib;

namespace BoxCli
{
    public class BoxItemFetcher
    {
        private readonly BoxUtils _boxUtils;
        private readonly ConcurrentBag<BoxItem> _items = new ConcurrentBag<BoxItem>();
        private volatile bool _isPopulated = false;
        private BoxItem[] _itemsSnapshot = Array.Empty<BoxItem>();
        private Task? _currentPopulationTask;
        private readonly object _populationLock = new object();

        public BoxItemFetcher(BoxUtils boxUtils)
        {
            _boxUtils = boxUtils ?? throw new ArgumentNullException(nameof(boxUtils));
        }

        /// <summary>
        /// Populates the internal ConcurrentBag<BoxItem> with all items in the specified folder,
        /// following pagination using NextMarker.
        /// </summary>
        /// <param name="folderId">The ID of the folder to fetch items from.</param>
        public async Task PopulateItemsAsync(string folderId)
        {
            lock (_populationLock)
            {
                if (_currentPopulationTask != null && !_currentPopulationTask.IsCompleted)
                {
                    // Signal the first invocation to quit by returning early.
                    return;
                }
                // Start a new population task.
                _currentPopulationTask = PopulateItemsInternalAsync(folderId);
            }
            await _currentPopulationTask;
        }

        private async Task PopulateItemsInternalAsync(string folderId)
        {
            var done = false;
            string? nextMarker = null;
            _isPopulated = false;
            _items.Clear();
            while (!done)
            {
                var result = await _boxUtils.ListFolderItemsAsync(folderId, nextMarker);
                foreach (var item in result.Items)
                {
                    _items.Add(item);
                }
                if (result.NextMarker == null)
                {
                    done = true;
                }
                else
                {
                    nextMarker = result.NextMarker;
                }
            }
            _isPopulated = true;
            _itemsSnapshot = _items.ToArray();

        }

        /// <summary>
        /// Returns a thread-safe, readonly snapshot of the BoxItems as an array.
        /// If population is not complete, refreshes the snapshot first.
        /// </summary>
        public BoxItem[] GetItemsSnapshot()
        {
            if (!_isPopulated)
            {
                _itemsSnapshot = _items.ToArray();
            }
            return _itemsSnapshot;
        }

        public string? GetItemIdByName(string itemName)
        {
            var item = GetItemsSnapshot().Where(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (item != null)
            {
                return item.Id;
            }
            return null;
        }

        public class PartialMatchResult
        {
            public BoxItem? FirstMatch { get; set; }
            public bool HasMultipleMatches { get; set; }
        }

        public PartialMatchResult GetItemByPartialName(string partialName)
        {
            var matches = GetItemsSnapshot()
                .Where(i => i.Name.StartsWith(partialName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return new PartialMatchResult
            {
                FirstMatch = matches.FirstOrDefault(),
                HasMultipleMatches = matches.Count > 1
            };
        }
    }
}