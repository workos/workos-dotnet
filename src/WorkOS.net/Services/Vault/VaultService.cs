// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>A Vault KV object.</summary>
    public class VaultObject
    {
        [JsonProperty("id")]
        public string Id { get; set; } = default!;

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("value")]
        public string? Value { get; set; }

        [JsonProperty("environment_id")]
        public string? EnvironmentId { get; set; }

        [JsonProperty("key_id")]
        public string? KeyId { get; set; }

        [JsonProperty("version_id")]
        public string? VersionId { get; set; }

        [JsonProperty("updated_at")]
        public string? UpdatedAt { get; set; }

        [JsonProperty("updated_by")]
        public string? UpdatedBy { get; set; }
    }

    /// <summary>A Vault object digest (summary).</summary>
    public class ObjectDigest
    {
        [JsonProperty("id")]
        public string Id { get; set; } = default!;

        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        [JsonProperty("updated_at")]
        public string? UpdatedAt { get; set; }
    }

    /// <summary>A Vault object version.</summary>
    public class ObjectVersion
    {
        [JsonProperty("version_id")]
        public string VersionId { get; set; } = default!;

        [JsonProperty("updated_at")]
        public string? UpdatedAt { get; set; }

        [JsonProperty("updated_by")]
        public string? UpdatedBy { get; set; }
    }

    /// <summary>A data key pair from the Vault key service.</summary>
    public class DataKeyPair
    {
        [JsonProperty("context")]
        public Dictionary<string, string>? Context { get; set; }

        [JsonProperty("data_key")]
        public DataKey? DataKey { get; set; }

        [JsonProperty("encrypted_keys")]
        public string EncryptedKeys { get; set; } = default!;
    }

    /// <summary>A plaintext data key.</summary>
    public class DataKey
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; } = default!;
    }

    /// <summary>Options for creating a Vault object.</summary>
    public class CreateVaultObjectOptions : BaseOptions
    {
        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        [JsonProperty("value")]
        public string Value { get; set; } = default!;

        [JsonProperty("key_context")]
        public Dictionary<string, string>? KeyContext { get; set; }
    }

    /// <summary>Options for updating a Vault object.</summary>
    public class UpdateVaultObjectOptions : BaseOptions
    {
        [JsonProperty("value")]
        public string Value { get; set; } = default!;

        [JsonProperty("version_check")]
        public string? VersionCheck { get; set; }
    }

    /// <summary>Options for listing Vault objects.</summary>
    public class ListVaultObjectsOptions : ListOptions
    {
    }

    /// <summary>Options for creating a data key.</summary>
    public class CreateDataKeyOptions : BaseOptions
    {
        [JsonProperty("context")]
        public Dictionary<string, string>? Context { get; set; }
    }

    /// <summary>Options for decrypting a data key.</summary>
    public class DecryptDataKeyOptions : BaseOptions
    {
        [JsonProperty("keys")]
        public string Keys { get; set; } = default!;
    }

    /// <summary>Vault KV storage, key operations, and client-side AES-GCM encrypt/decrypt.</summary>
    public class VaultService : Service
    {
        private const int IvSize = 12;
        private const int TagSize = 16;

        public VaultService()
        {
        }

        public VaultService(WorkOSClient client)
            : base(client)
        {
        }

        /// <summary>List Vault objects.</summary>
        public virtual async Task<WorkOSList<ObjectDigest>> ListObjects(
            ListVaultObjectsOptions? options = null,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = "/vault/v1/kv",
                Options = options,
                RequestOptions = requestOptions,
            };
            return await this.Client.MakeAPIRequest<WorkOSList<ObjectDigest>>(request, cancellationToken);
        }

        /// <summary>Create a Vault object.</summary>
        public virtual async Task<VaultObject> CreateObject(
            CreateVaultObjectOptions options,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Post,
                Path = "/vault/v1/kv",
                Options = options,
                RequestOptions = requestOptions,
            };
            return await this.Client.MakeAPIRequest<VaultObject>(request, cancellationToken);
        }

        /// <summary>Read a Vault object by ID (decrypted).</summary>
        public virtual async Task<VaultObject> ReadObject(
            string objectId,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/vault/v1/kv/{objectId}",
                RequestOptions = requestOptions,
            };
            return await this.Client.MakeAPIRequest<VaultObject>(request, cancellationToken);
        }

        /// <summary>Read a Vault object by name (decrypted).</summary>
        public virtual async Task<VaultObject> ReadObjectByName(
            string name,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/vault/v1/kv/name/{name}",
                RequestOptions = requestOptions,
            };
            return await this.Client.MakeAPIRequest<VaultObject>(request, cancellationToken);
        }

        /// <summary>Get Vault object metadata (no decryption).</summary>
        public virtual async Task<VaultObject> GetObjectMetadata(
            string objectId,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/vault/v1/kv/{objectId}/metadata",
                RequestOptions = requestOptions,
            };
            return await this.Client.MakeAPIRequest<VaultObject>(request, cancellationToken);
        }

        /// <summary>Update a Vault object.</summary>
        public virtual async Task<VaultObject> UpdateObject(
            string objectId,
            UpdateVaultObjectOptions options,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Put,
                Path = $"/vault/v1/kv/{objectId}",
                Options = options,
                RequestOptions = requestOptions,
            };
            return await this.Client.MakeAPIRequest<VaultObject>(request, cancellationToken);
        }

        /// <summary>Delete a Vault object.</summary>
        public virtual async Task DeleteObject(
            string objectId,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Delete,
                Path = $"/vault/v1/kv/{objectId}",
                RequestOptions = requestOptions,
            };
            await this.Client.MakeRawAPIRequest(request, cancellationToken);
        }

        /// <summary>List Vault object versions.</summary>
        public virtual async Task<List<ObjectVersion>> ListObjectVersions(
            string objectId,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/vault/v1/kv/{objectId}/versions",
                RequestOptions = requestOptions,
            };
            return await this.Client.MakeAPIRequest<List<ObjectVersion>>(request, cancellationToken);
        }

        /// <summary>Generate a data key for local encryption.</summary>
        public virtual async Task<DataKeyPair> CreateDataKey(
            CreateDataKeyOptions options,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Post,
                Path = "/vault/v1/keys/data-key",
                Options = options,
                RequestOptions = requestOptions,
            };
            return await this.Client.MakeAPIRequest<DataKeyPair>(request, cancellationToken);
        }

        /// <summary>Decrypt a previously generated data key.</summary>
        public virtual async Task<DataKey> DecryptDataKey(
            DecryptDataKeyOptions options,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Post,
                Path = "/vault/v1/keys/decrypt",
                Options = options,
                RequestOptions = requestOptions,
            };
            return await this.Client.MakeAPIRequest<DataKey>(request, cancellationToken);
        }

        /// <summary>Encrypt data locally using AES-256-GCM with a server-generated data key.</summary>
        /// <param name="data">The plaintext data to encrypt.</param>
        /// <param name="keyContext">The key context for data key generation.</param>
        /// <param name="associatedData">Optional additional authenticated data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A base64-encoded encrypted payload.</returns>
        public virtual async Task<string> Encrypt(
            string data,
            Dictionary<string, string> keyContext,
            string? associatedData = null,
            CancellationToken cancellationToken = default)
        {
            var keyPair = await this.CreateDataKey(
                new CreateDataKeyOptions { Context = keyContext },
                cancellationToken: cancellationToken);

            var plainKey = Convert.FromBase64String(keyPair.DataKey!.Key);
            var encryptedKeys = Encoding.UTF8.GetBytes(keyPair.EncryptedKeys);
            var iv = RandomNumberGenerator.GetBytes(IvSize);
            var plaintext = Encoding.UTF8.GetBytes(data);
            var ciphertext = new byte[plaintext.Length];
            var tag = new byte[TagSize];
            byte[] aad = associatedData != null ? Encoding.UTF8.GetBytes(associatedData) : Array.Empty<byte>();

            using var aes = new AesGcm(plainKey, TagSize);
            aes.Encrypt(iv, plaintext, ciphertext, tag, aad);

            // Encode key length as LEB128
            var keyLenBytes = EncodeLeb128((uint)encryptedKeys.Length);

            // Format: IV + tag + keyLenBytes + encryptedKeys + ciphertext
            var combined = new byte[IvSize + TagSize + keyLenBytes.Length + encryptedKeys.Length + ciphertext.Length];
            int offset = 0;
            Buffer.BlockCopy(iv, 0, combined, offset, IvSize);
            offset += IvSize;
            Buffer.BlockCopy(tag, 0, combined, offset, TagSize);
            offset += TagSize;
            Buffer.BlockCopy(keyLenBytes, 0, combined, offset, keyLenBytes.Length);
            offset += keyLenBytes.Length;
            Buffer.BlockCopy(encryptedKeys, 0, combined, offset, encryptedKeys.Length);
            offset += encryptedKeys.Length;
            Buffer.BlockCopy(ciphertext, 0, combined, offset, ciphertext.Length);

            return Convert.ToBase64String(combined);
        }

        /// <summary>Decrypt data encrypted with <see cref="Encrypt"/>.</summary>
        /// <param name="encryptedData">The base64-encoded encrypted payload.</param>
        /// <param name="associatedData">Optional additional authenticated data (must match encryption).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The decrypted plaintext string.</returns>
        public virtual async Task<string> Decrypt(
            string encryptedData,
            string? associatedData = null,
            CancellationToken cancellationToken = default)
        {
            var combined = Convert.FromBase64String(encryptedData);
            int offset = 0;

            var iv = new byte[IvSize];
            Buffer.BlockCopy(combined, offset, iv, 0, IvSize);
            offset += IvSize;

            var tag = new byte[TagSize];
            Buffer.BlockCopy(combined, offset, tag, 0, TagSize);
            offset += TagSize;

            var (keyLen, bytesRead) = DecodeLeb128(combined, offset);
            offset += bytesRead;

            var encryptedKeys = new byte[keyLen];
            Buffer.BlockCopy(combined, offset, encryptedKeys, 0, (int)keyLen);
            offset += (int)keyLen;

            var ciphertext = new byte[combined.Length - offset];
            Buffer.BlockCopy(combined, offset, ciphertext, 0, ciphertext.Length);

            var keyString = Encoding.UTF8.GetString(encryptedKeys);
            var decryptedKey = await this.DecryptDataKey(
                new DecryptDataKeyOptions { Keys = keyString },
                cancellationToken: cancellationToken);

            var plainKey = Convert.FromBase64String(decryptedKey.Key);
            var plaintext = new byte[ciphertext.Length];
            byte[] aad = associatedData != null ? Encoding.UTF8.GetBytes(associatedData) : Array.Empty<byte>();

            using var aes = new AesGcm(plainKey, TagSize);
            aes.Decrypt(iv, ciphertext, tag, plaintext, aad);

            return Encoding.UTF8.GetString(plaintext);
        }

        private static byte[] EncodeLeb128(uint value)
        {
            var result = new List<byte>();
            do
            {
                byte b = (byte)(value & 0x7F);
                value >>= 7;
                if (value != 0)
                {
                    b |= 0x80;
                }

                result.Add(b);
            }
            while (value != 0);
            return result.ToArray();
        }

        private static (uint Value, int BytesRead) DecodeLeb128(byte[] data, int offset)
        {
            uint result = 0;
            int shift = 0;
            int bytesRead = 0;
            while (true)
            {
                byte b = data[offset + bytesRead];
                result |= (uint)(b & 0x7F) << shift;
                bytesRead++;
                if ((b & 0x80) == 0)
                {
                    break;
                }

                shift += 7;
            }

            return (result, bytesRead);
        }
    }
}
