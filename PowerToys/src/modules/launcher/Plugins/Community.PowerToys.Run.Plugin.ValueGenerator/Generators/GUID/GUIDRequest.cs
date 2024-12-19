// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Security.Cryptography;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.ValueGenerator.GUID
{
    public class GUIDRequest : IComputeRequest
    {
        public byte[] Result { get; set; }

        public bool IsSuccessful { get; set; }

        public string ErrorMessage { get; set; }

        private int Version { get; set; }

        public string Description => Version switch
        {
            1 => "UUID v1：基于当前时间和网卡 MAC 地址",
            3 => $"UUID v3：基于给定名称进行 {HashAlgorithmName.MD5} 散列计算",
            4 => "UUID v4：随机数值",
            5 => $"UUID v5：基于给定名称进行 {HashAlgorithmName.SHA1} 散列计算",
            7 => "UUID v7：当前时间+随机数值",
            _ => string.Empty,
        };

        private Guid? GuidNamespace { get; set; }

        private string GuidName { get; set; }

        private Guid GuidResult { get; set; }

        private static readonly string NullNamespaceError = $"第一个参数需要是一个 UUID 或者是以下选项之一：{string.Join("、", GUIDGenerator.PredefinedNamespaces.Keys)}";

        public GUIDRequest(int version, string guidNamespace = null, string name = null)
        {
            Version = version;

            if (Version is < 1 or > 7 or 2 or 6)
            {
                throw new ArgumentException("不支持此 GUID 版本，目前仅支持版本 1、3、4、5、7");
            }

            if (version is 3 or 5)
            {
                if (guidNamespace == null)
                {
                    throw new ArgumentNullException(null, NullNamespaceError);
                }

                if (GUIDGenerator.PredefinedNamespaces.TryGetValue(guidNamespace.ToLowerInvariant(), out Guid guid))
                {
                    GuidNamespace = guid;
                }
                else if (Guid.TryParse(guidNamespace, out guid))
                {
                    GuidNamespace = guid;
                }
                else
                {
                    throw new ArgumentNullException(null, NullNamespaceError);
                }

                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                else
                {
                    GuidName = name;
                }
            }
            else
            {
                GuidNamespace = null;
            }

            ErrorMessage = null;
        }

        public bool Compute()
        {
            IsSuccessful = true;
            try
            {
                Guid guid = Version switch
                {
                    1 => GUIDGenerator.V1(),
                    3 => GUIDGenerator.V3(GuidNamespace.Value, GuidName),
                    4 => GUIDGenerator.V4(),
                    5 => GUIDGenerator.V5(GuidNamespace.Value, GuidName),
                    7 => GUIDGenerator.V7(),
                    _ => default,
                };
                if (guid != default)
                {
                    GuidResult = guid;
                }

                Result = GuidResult.ToByteArray();
            }
            catch (InvalidOperationException e)
            {
                Log.Exception(e.Message, e, GetType());
                ErrorMessage = e.Message;
                IsSuccessful = false;
            }

            return IsSuccessful;
        }

        public string ResultToString()
        {
            if (!IsSuccessful)
            {
                return ErrorMessage;
            }

            return GuidResult.ToString();
        }
    }
}
