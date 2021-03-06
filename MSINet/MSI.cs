﻿using MSINet.Interop;
using System;
using System.Collections.Generic;

namespace MSINet
{
    public static class MSI
    {
        /// <summary>
        /// Enumerate all installed product GUIDs
        /// </summary>
        /// <returns>An enumerator that returns all installed products GUIDs</returns>
        public static IEnumerable<Guid> EnumerateGUIDs()
        {
            MsiExitCodes ret = 0;
            uint i = 0, dummy2 = 0;
            do
            {
                string guid = new string(new char[39]);
                object dummy1;
                ret = MsiInterop.MsiEnumProductsEx(null, null, InstallContext.All, i, guid, out dummy1, null, ref dummy2);
                if (ret == MsiExitCodes.Success)
                {
                    if (Guid.TryParse(guid.TrimEnd('\0'), out var result))
                    {
                        yield return result;
                    }
                }
                i++;
            } while (ret != MsiExitCodes.NoMoreItems);

            yield break;
        }

        /// <summary>
        /// Get property of a product indicated by GUID. Throws exception if cannot read the property.
        /// </summary>
        /// <param name="productGUID">Product GUID</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>Property value, if available.</returns>
        /// <exception cref="MSIException">Throws MSIException if reading property was not successful</exception>
        public static String GetProperty(Guid productGUID, string propertyName)
        {
            String propertyValue;
            MsiExitCodes returnValue = TryGetProperty(productGUID, propertyName, out propertyValue);
            if (returnValue != MsiExitCodes.Success)
                throw new MSIException(returnValue);
            return propertyValue;
        }

        /// <summary>
        /// Tries to get a product indicated by GUID.
        /// </summary>
        /// <param name="productGUID">Product GUID</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="propertyValue">Property value or if not available - <c>null</c>.</param>
        /// <returns>MSI exit code</returns>
        public static MsiExitCodes TryGetProperty(Guid productGUID, string propertyName, out string propertyValue)
        {
            int len = 0;
            string productGuildStr = FormatGUID(productGUID);
            // Get the data len
            MsiExitCodes returnValue = MsiInterop.MsiGetProductInfo(productGuildStr, propertyName, null, ref len);
            if (returnValue != MsiExitCodes.Success)
            {
                propertyValue = null;
                return returnValue;
            }

            // increase for the terminating \0
            len++;
            propertyValue = new string(new char[len]);
            returnValue = MsiInterop.MsiGetProductInfo(productGuildStr, propertyName, propertyValue, ref len);
            if (returnValue != MsiExitCodes.Success)
            {
                propertyValue = null;
                return returnValue;
            }

            // trim trailing \0
            propertyValue = propertyValue.TrimEnd('\0');

            return MsiExitCodes.Success;
        }

        /// <summary>
        /// Checks if the product identified by the given <paramref name="productGUID"/> contains the property
        /// identified by <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="productGUID">Product GUID</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>True, if the Product contains this property, otherwise - false.</returns>
        public static bool ContainsProperty(Guid productGUID, string propertyName)
        {
            int len = 0;
            MsiExitCodes result = MsiInterop.MsiGetProductInfo(productGUID.ToString(), propertyName, null, ref len);
            return result == MsiExitCodes.Success;
        }

        private static string FormatGUID(Guid guid)
        {
            return string.Format("{{{0}}}\0", guid.ToString());
        }
    }
}
