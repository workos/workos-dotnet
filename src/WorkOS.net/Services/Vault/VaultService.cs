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
        /// <param name="options">Filter and pagination options.</param>
        /// <param name="requestOptions">Per-request configuration overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A page of <see cref="ObjectDigest"/> results.</returns>
        public virtual async Task<WorkOSList<ObjectDigest>> ListObjectsAsync(
            ListVaultObjectsOptions? options = null,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return await this.GetAsync<WorkOSList<ObjectDigest>>("/vault/v1/kv", options, requestOptions, cancellationToken);
        }

        /// <summary>Compatibility wrapper for <see cref="ListObjectsAsync"/>.</summary>
        public virtual Task<WorkOSList<ObjectDigest>> ListObjects(
            ListVaultObjectsOptions? options = null,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return this.ListObjectsAsync(options, requestOptions, cancellationToken);
        }

        /// <summary>Create a Vault object.</summary>
        /// <param name="options">Object creation options.</param>
        /// <param name="requestOptions">Per-request configuration overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created <see cref="VaultObject"/>.</returns>
        public virtual async Task<VaultObject> CreateObjectAsync(
            CreateVaultObjectOptions options,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return await this.PostAsync<VaultObject>("/vault/v1/kv", options, requestOptions, cancellationToken);
        }

        /// <summary>Compatibility wrapper for <see cref="CreateObjectAsync"/>.</summary>
        public virtual Task<VaultObject> CreateObject(
            CreateVaultObjectOptions options,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return this.CreateObjectAsync(options, requestOptions, cancellationToken);
        }

        /// <summary>Read a Vault object by ID (decrypted).</summary>
        /// <param name="objectId">The Vault object identifier.</param>
        /// <param name="requestOptions">Per-request configuration overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The decrypted <see cref="VaultObject"/>.</returns>
        public virtual async Task<VaultObject> ReadObjectAsync(
            string objectId,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return await this.GetAsync<VaultObject>($"/vault/v1/kv/{Uri.EscapeDataString(objectId)}", null, requestOptions, cancellationToken);
        }

        /// <summary>Compatibility wrapper for <see cref="ReadObjectAsync"/>.</summary>
        public virtual Task<VaultObject> ReadObject(
            string objectId,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return this.ReadObjectAsync(objectId, requestOptions, cancellationToken);
        }

        /// <summary>Read a Vault object by name (decrypted).</summary>
        /// <param name="name">The Vault object name.</param>
        /// <param name="requestOptions">Per-request configuration overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The decrypted <see cref="VaultObject"/>.</returns>
        public virtual async Task<VaultObject> ReadObjectByNameAsync(
            string name,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return await this.GetAsync<VaultObject>($"/vault/v1/kv/name/{Uri.EscapeDataString(name)}", null, requestOptions, cancellationToken);
        }

        /// <summary>Compatibility wrapper for <see cref="ReadObjectByNameAsync"/>.</summary>
        public virtual Task<VaultObject> ReadObjectByName(
            string name,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return this.ReadObjectByNameAsync(name, requestOptions, cancellationToken);
        }

        /// <summary>Get Vault object metadata (no decryption).</summary>
        /// <param name="objectId">The Vault object identifier.</param>
        /// <param name="requestOptions">Per-request configuration overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The <see cref="VaultObject"/> metadata.</returns>
        public virtual async Task<VaultObject> GetObjectMetadataAsync(
            string objectId,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return await this.GetAsync<VaultObject>($"/vault/v1/kv/{Uri.EscapeDataString(objectId)}/metadata", null, requestOptions, cancellationToken);
        }

        /// <summary>Compatibility wrapper for <see cref="GetObjectMetadataAsync"/>.</summary>
        public virtual Task<VaultObject> GetObjectMetadata(
            string objectId,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return this.GetObjectMetadataAsync(objectId, requestOptions, cancellationToken);
        }

        /// <summary>Update a Vault object.</summary>
        /// <param name="objectId">The Vault object identifier.</param>
        /// <param name="options">Object update options.</param>
        /// <param name="requestOptions">Per-request configuration overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated <see cref="VaultObject"/>.</returns>
        public virtual async Task<VaultObject> UpdateObjectAsync(
            string objectId,
            UpdateVaultObjectOptions options,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return await this.PutAsync<VaultObject>($"/vault/v1/kv/{Uri.EscapeDataString(objectId)}", options, requestOptions, cancellationToken);
        }

        /// <summary>Compatibility wrapper for <see cref="UpdateObjectAsync"/>.</summary>
        public virtual Task<VaultObject> UpdateObject(
            string objectId,
            UpdateVaultObjectOptions options,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return this.UpdateObjectAsync(objectId, options, requestOptions, cancellationToken);
        }

        /// <summary>Delete a Vault object.</summary>
        /// <param name="objectId">The Vault object identifier.</param>
        /// <param name="requestOptions">Per-request configuration overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public virtual async Task DeleteObjectAsync(
            string objectId,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            await this.DeleteAsync($"/vault/v1/kv/{Uri.EscapeDataString(objectId)}", null, requestOptions, cancellationToken);
        }

        /// <summary>Compatibility wrapper for <see cref="DeleteObjectAsync"/>.</summary>
        public virtual Task DeleteObject(
            string objectId,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return this.DeleteObjectAsync(objectId, requestOptions, cancellationToken);
        }

        /// <summary>List Vault object versions.</summary>
        /// <param name="objectId">The Vault object identifier.</param>
        /// <param name="requestOptions">Per-request configuration overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of <see cref="ObjectVersion"/> entries.</returns>
        public virtual async Task<List<ObjectVersion>> ListObjectVersionsAsync(
            string objectId,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return await this.GetAsync<List<ObjectVersion>>($"/vault/v1/kv/{Uri.EscapeDataString(objectId)}/versions", null, requestOptions, cancellationToken);
        }

        /// <summary>Compatibility wrapper for <see cref="ListObjectVersionsAsync"/>.</summary>
        public virtual Task<List<ObjectVersion>> ListObjectVersions(
            string objectId,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return this.ListObjectVersionsAsync(objectId, requestOptions, cancellationToken);
        }

        /// <summary>Generate a data key for local encryption.</summary>
        /// <param name="options">Data key creation options.</param>
        /// <param name="requestOptions">Per-request configuration overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="DataKeyPair"/> containing the plaintext and encrypted key.</returns>
        public virtual async Task<DataKeyPair> CreateDataKeyAsync(
            CreateDataKeyOptions options,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return await this.PostAsync<DataKeyPair>("/vault/v1/keys/data-key", options, requestOptions, cancellationToken);
        }

        /// <summary>Compatibility wrapper for <see cref="CreateDataKeyAsync"/>.</summary>
        public virtual Task<DataKeyPair> CreateDataKey(
            CreateDataKeyOptions options,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return this.CreateDataKeyAsync(options, requestOptions, cancellationToken);
        }

        /// <summary>Decrypt a previously generated data key.</summary>
        /// <param name="options">Data key decryption options.</param>
        /// <param name="requestOptions">Per-request configuration overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The decrypted <see cref="DataKey"/>.</returns>
        public virtual async Task<DataKey> DecryptDataKeyAsync(
            DecryptDataKeyOptions options,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return await this.PostAsync<DataKey>("/vault/v1/keys/decrypt", options, requestOptions, cancellationToken);
        }

        /// <summary>Compatibility wrapper for <see cref="DecryptDataKeyAsync"/>.</summary>
        public virtual Task<DataKey> DecryptDataKey(
            DecryptDataKeyOptions options,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return this.DecryptDataKeyAsync(options, requestOptions, cancellationToken);
        }

        /// <summary>Encrypt data locally using AES-256-GCM with a server-generated data key.</summary>
        /// <param name="data">The plaintext data to encrypt.</param>
        /// <param name="keyContext">The key context for data key generation.</param>
        /// <param name="associatedData">Optional additional authenticated data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A base64-encoded encrypted payload.</returns>
        public virtual async Task<string> EncryptAsync(
            string data,
            Dictionary<string, string> keyContext,
            string? associatedData = null,
            CancellationToken cancellationToken = default)
        {
            var keyPair = await this.CreateDataKeyAsync(
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

        /// <summary>Compatibility wrapper for <see cref="EncryptAsync"/>.</summary>
        public virtual Task<string> Encrypt(
            string data,
            Dictionary<string, string> keyContext,
            string? associatedData = null,
            CancellationToken cancellationToken = default)
        {
            return this.EncryptAsync(data, keyContext, associatedData, cancellationToken);
        }

        /// <summary>Decrypt data encrypted with <see cref="EncryptAsync"/>.</summary>
        /// <param name="encryptedData">The base64-encoded encrypted payload.</param>
        /// <param name="associatedData">Optional additional authenticated data (must match encryption).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The decrypted plaintext string.</returns>
        public virtual async Task<string> DecryptAsync(
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
            var decryptedKey = await this.DecryptDataKeyAsync(
                new DecryptDataKeyOptions { Keys = keyString },
                cancellationToken: cancellationToken);

            var plainKey = Convert.FromBase64String(decryptedKey.Key);
            var plaintext = new byte[ciphertext.Length];
            byte[] aad = associatedData != null ? Encoding.UTF8.GetBytes(associatedData) : Array.Empty<byte>();

            using var aes = new AesGcm(plainKey, TagSize);
            aes.Decrypt(iv, ciphertext, tag, plaintext, aad);

            return Encoding.UTF8.GetString(plaintext);
        }

        /// <summary>Compatibility wrapper for <see cref="DecryptAsync"/>.</summary>
        public virtual Task<string> Decrypt(
            string encryptedData,
            string? associatedData = null,
            CancellationToken cancellationToken = default)
        {
            return this.DecryptAsync(encryptedData, associatedData, cancellationToken);
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
