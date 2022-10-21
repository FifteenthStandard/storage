# `IKeyValueStore`

A generic container for storing values indexed by a hash key and sort key.

## Methods

---

### `Task<T?> GetAsync<T>(string hashKey, string sortKey)`

Get the value stored with the given hash key and sort key.

### Parameters

#### `hashKey` [String]

The hash key of the value to retrieve.

#### `sortKey` [String]

The sort key of the value to retrieve.

### Returns

#### [Task]&lt;T?&gt;

A task representing the value stored with the given hash key and sort key, or
`null` if no value is found.

---

### `Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyPrefix)`

Get all values stored with the given hash key and a sort key with the given
prefix.

### Parameters

#### `hashKey` [String]

The hash key of values to retrieve.

#### `sortKeyPrefix` [String]

A sort key prefix of values to retrieve.

### Returns

#### [Task]&lt;IEnumerable&lt;T&gt;&gt;

A task representing all values found with the given hash key and a sort key
with the given prefix.

---

### `Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyStart, string sortKeyEnd)`

Get all values stored with the given hash key and a sort key between the two
given sort keys.

### Parameters

#### `hashKey` [String]

The hash key of values to retrieve.

#### `sortKeyStart` [String]

The starting sort key. Values with a sort key greater than or equal to this
will be retrieved.

#### `sortKeyEnd` [String]

The ending sort key. Values with a sort key strictly less than this will be
retrieved.

### Returns

#### [Task]&lt;IEnumerable&lt;T&gt;&gt;

A task representing all values found with the given hash key and a sort key
between the two given sort keys.

---

### `Task<IEnumerable<T>> GetRangeAsync<T>(string hashKey, string sortKeyStart, int count)`

Get a number of values stored with the given hash key starting from the given
sort key.

### Parameters

#### `hashKey` [String]

The hash key of values to retrieve.

#### `sortKeyStart` [String]

The starting sort key.

#### `count` [Int]

The number of values to retrieve. If positive, values with a sort key which is
greater than or equal to the given sort key will be retrieved. If negative,
values with a sort key which is strictly less than the given sort key will be
retrieved.

### Returns

#### [Task]&lt;IEnumerable&lt;T&gt;&gt;

A task representing all values found with the given hash key starting from the
given sort key.

---

### `Task<T> PutAsync<t>(string hashKey, string sortKey, T value)`

Set the value which is indexed by the given hash key and sort key. Creates a
new value if no value exists, or updates the existing value if one does.

### Parameters

#### `hashKey` [String]

The hash key of the value to set.

#### `sortKey` [String]

The sort key of the value to set.

#### `value` T

The value to set.

### Returns

#### [Task]

A task which resolves once the value has been set.

---

### `Task RemoveAsync(string hashKey, string sortKey)`

Remove the value indexed by the given hash key and sort key, if present.

### Parameters

#### `hashKey` [String]

The hash key of the value to remove.

#### `sortKey` [String]

The sort key of the value to remove.

### Returns

#### [Task]

A task which resolves once the value has been removed.

---

## Implementations

### `InMemoryKeyValueStore`

A fully in-memory key-value store. Contents are wiped when the application
exits. Only suitable for testing.


### `FilePerHashKeyValueStore`

Values are stored in JSON files, with one file per hash key. Within a file,
values are stored as a JSON object indexed by sort key. Suitable for local
application development.


### `FilePerValueKeyValueStore`

Values are stored in JSON files, with one file per value. A directory exists
for each hash key, containing files for each sort key. Suitable for local
application development.


### `AzureKeyValueStore`

Values are persisted to [Azure Table Storage](azure-table-storage). Suitable
for production applications deployed to Microsoft Azure.


[Int]: https://learn.microsoft.com/en-us/dotnet/api/system.int32
[String]: https://learn.microsoft.com/en-us/dotnet/api/system.string
[Task]: https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task

[azure-table-storage]: https://azure.microsoft.com/en-au/products/storage/tables/