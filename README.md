# FifteenthStandard.Storage

A library with several implementations of generic blob storage and key-value
pair storage clients.

---

## `IBlobStore`

A generic container for storing arbitrary blobs of bytes or text.

### `GetBytesAsync`

Get the contents of a blob as an array of bytes.

#### Parameters

Parameter     | Description
--------------|------------
`string path` | The path of the blob to get

#### Returns `Task<byte[]?>

A task representing the contents of the blob as an array of bytes, or `null` if
no blob exists at the given path.

### `GetStringAsync`

Get the contents of a blob as a UTF-8 encoded string.

#### Parameters

Parameter     | Description
--------------|------------
`string path` | The path of the blob to get

#### Returns `Task<string?>`

A task representing the contents of the blob as a string, or `null` if no blob
exists at the given path.

### `PutBytesAsync`

Set the contents of a blob to a given array of bytes. Creates a new blob if no
blob exists at the given path, or updates the contents of the existing blob if
one does.

#### Parameters

Parameter         | Description
------------------|------------
`string path`     | The path of the blob to set
`byte[] contents` | The new contents of the blob

#### Returns `Task`

A task which resolves once the blob contents have been set.

### `PutStringAsync`

Set the contents of a blob to a given string, encoded in UTF8. Creates a new
blob if no blob exists at the given path, or updates the contents of the
existing blob if one does.

#### Parameters

Parameter         | Description
------------------|------------
`string path`     | The path of the blob to set
`string contents` | The new contents of the blob

#### Returns `Task`

A task which resolves once the blob contents have been set.

### `RemoveAsync`

Remove a blob, if present.

#### Parameters

Parameter     | Description
--------------|------------
`string path` | The path of the blob to remove

#### Returns `Task`

A task which resolves once the blob has been removed.

### Implementations

#### `InMemoryBlobStore`

A fully in-memory blob store. Contents are wiped when the application exits.
Only suitable for testing.

#### `FileBlobStore`

Blobs are persisted to the local file system. Suitable for local application
development.

#### `AwsBlobStore`

Blobs are persisted to [Amazon S3][amazon-s3]. Suitable for production
applications deployed to AWS.

#### `AzureBlobStore`

Blobs are persisted to [Azure Blob Storage](azure-blob-storage). Suitable for
production applications deployed to Microsoft Azure.

---

## `IKeyValueStore`

A generic container for storing values indexed by a hash key and sort key.

### `GetAsync`

Get the value stored with the given hash key and sort key.

#### Parameters

Parameter        | Description
-----------------|------------
`string hashKey` | The hash key of the value to retrieve
`string sortKey` | The sort key of the value to retrieve

#### Returns `Task<T?>`

A task representing the value stored with the given hash key and sort key, or
`null` if no value is found.

### `GetRangeAsync`

Get all values stored with the given hash key and a sort key with the given
prefix.

#### Parameters

Parameter              | Description
-----------------------|------------
`string hashKey`       | The hash key of the values to retrieve
`string sortKeyPrefix` | The sort key prefix of the values to retrieve

#### Returns `Task<IEnumerable<T>>`

A task representing all values found with the given hash key and a sort key
with the given prefix.

### `GetRangeAsync`

Get all values stored with the given hash key and a sort key between the two
given sort keys.

#### Parameters

Parameter             | Description
----------------------|------------
`string hashKey`      | The hash key of the values to retrieve
`string sortKeyStart` | The starting sort key. Values with a sort key greater than or equal to this will be retrieved
`string sortKeyEnd`   | The ending sort key. Values with a sort key strictly less than this will be retrieved

#### Returns `Task<IEnumerable<T>>`

A task representing all values found with the given hash key and a sort key
between the two given sort keys.

### `GetRangeAsync`

Get a number of values stored with the given hash key starting from the given
sort key.

#### Parameters

Parameter             | Description
----------------------|------------
`string hashKey`      | The hash key of the values to retrieve
`string sortKeyStart` | The starting sort key. Values with a sort key greater than or equal to this will be retrieved
`int count`           | The number of values to retrieve. If positive, values with a sort key which is greater than or equal to the given sort key will be retrieved. If negative, values with a sort key which is strictly less than the given sort key will be retrieved

#### Returns `Task<IEnumerable<T>>`

A task representing all values found with the given hash key starting from the
given sort key.

### `PutAsync`

Set the value which is indexed by the given hash key and sort key. Creates a
new value if no value exists, or updates the existing value if one does.

#### Parameters

Parameter        | Description
-----------------|------------
`string hashKey` | The hash key of the value to set
`string sortKey` | The sort key of the value to set
`T value`        | The value to set

#### Returns `Task<T>`

A task which resolves to the new value once the value has been set.

### `RemoveAsync`

Remove the value indexed by the given hash key and sort key, if present.

#### Parameters

Parameter        | Description
-----------------|------------
`string hashKey` | The hash key of the value to remove
`string sortKey` | The sort key of the value to remove

#### Returns `Task`

A task which resolves once the value has been removed.

### Implementations

#### `InMemoryKeyValueStore`

A fully in-memory key-value store. Contents are wiped when the application
exits. Only suitable for testing.

#### `FilePerHashKeyValueStore`

Values are stored in JSON files, with one file per hash key. Within a file,
values are stored as a JSON object indexed by sort key. Suitable for local
application development.

#### `FilePerValueKeyValueStore`

Values are stored in JSON files, with one file per value. A directory exists
for each hash key, containing files for each sort key. Suitable for local
application development.

#### `AwsKeyValueStore`

Values are persisted to [Amazon DynamoDB][amazon-dynamodb]. Suitable for
production applications deployed to AWS.

#### `AzureKeyValueStore`

Values are persisted to [Azure Table Storage][azure-table-storage]. Suitable
for production applications deployed to Microsoft Azure.

[amazon-s3]: https://aws.amazon.com/s3/
[amazon-dynamodb]: https://aws.amazon.com/dynamodb/
[azure-blob-storage]: https://azure.microsoft.com/en-us/products/storage/blobs/
[azure-table-storage]: https://azure.microsoft.com/en-au/products/storage/tables/
