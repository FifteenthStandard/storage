# `IBlobStore`

A generic container for storing arbitrary blobs of bytes or text.

## Methods

---

### `Task<byte[]?> GetBytesAsync(string path)`

Get the contents of a blob as an array of bytes.

### Parameters

#### `path` [String]

The path of the blob to get.

### Returns

#### [Task]&lt;[Byte]\[\]?&gt;

A task representing the contents of the blob as an array of bytes, or `null` if
no blob exists at the given path.

---

### `Task<string?> GetStringAsync(string path)`

Get the contents of a blob as a UTF-8 encoded string.

### Parameters

#### `path` [String]

The path of the blob to get.

### Returns

#### [Task]&lt;[String]?&gt;

A task representing the contents of the blob as a string, or `null` if no blob
exists at the given path.

---

### `Task PutBytesAsync(string path, byte[] contents)`

Set the contents of a blob to a given array of bytes. Creates a new blob if no
blob exists at the given path, or updates the contents of the existing blob if
one does.

### Parameters

#### `path` [String]

The path of the blob to set.

#### `contents` [Byte]\[\]

The new contents of the blob.

### Returns

#### [Task]

A task which resolves once the blob contents have been set.

---

### `Task PutStringAsync(string path, string contents)`

Set the contents of a blob to a given string, encoded in UTF8. Creates a new
blob if no blob exists at the given path, or updates the contents of the
existing blob if one does.

### Parameters

#### `path` [String]

The path of the blob to set.

#### `contents` [String]

The new contents of the blob.

### Returns

#### [Task]

A task which resolves once the blob contents have been set.

---

### `Task RemoveAsync(string path)`

Remove a blob, if present.

### Parameters

#### `path` [String]

The path of the blob to remove.

### Returns

#### [Task]

A task which resolves once the blob has been removed.

---

## Implementations

### `InMemoryBlobStore`

A fully in-memory blob store. Contents are wiped when the application exits.
Only suitable for testing.


### `FileBlobStore`

Blobs are persisted to the local file system. Suitable for local application
development.


### `AzureBlobStore`

Blobs are persisted to [Azure Blob Storage](azure-blob-storage). Suitable for
production applications deployed to Microsoft Azure.


[Byte]: https://learn.microsoft.com/en-us/dotnet/api/system.byte
[String]: https://learn.microsoft.com/en-us/dotnet/api/system.string
[Task]: https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task

[azure-blob-storage]: https://azure.microsoft.com/en-gb/products/storage/blobs/