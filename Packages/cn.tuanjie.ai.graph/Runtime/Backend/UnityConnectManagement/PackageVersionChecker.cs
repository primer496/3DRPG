#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public static class PackageVersionChecker
{
    private static Dictionary<string, string> packageVersions = new Dictionary<string, string>();

    public static string GetPackageVersion(string packageName, bool forceUpdate = false)
    {
        if (!forceUpdate && packageVersions.TryGetValue(packageName, out var version))
            return version;
        var request = Client.List(true); // 获取所有 Package 信息
        while (!request.IsCompleted)
        {
            // 等待请求完成
        }

        if (request.Status == StatusCode.Success)
        {
            foreach (var package in request.Result)
            {
                if (package.name == packageName)
                {
                    packageVersions.Add(package.name, package.version);
                    return package.version;
                }
            }
        }
        else if (request.Status >= StatusCode.Failure)
        {
            Debug.LogError($"Failed to fetch packages: {request.Error.message}");
        }

        return null; // 未找到该 Package
    }
}

public static class PackageVersionComparer
{
    /// <summary>
    /// 比较两个版本字符串
    /// </summary>
    /// <returns>
    /// -1: version1 < version2
    ///  0: version1 == version2
    ///  1: version1 > version2
    /// </returns>
    public static int CompareVersions(string version1, string version2)
    {
        if (string.IsNullOrEmpty(version1) && string.IsNullOrEmpty(version2))
            return 0;
        if (string.IsNullOrEmpty(version1))
            return -1;
        if (string.IsNullOrEmpty(version2))
            return 1;

        try
        {
            Version v1 = new Version(NormalizeVersion(version1));
            Version v2 = new Version(NormalizeVersion(version2));
            return v1.CompareTo(v2);
        }
        catch (Exception)
        {
            // 如果Version类解析失败，使用自定义比较
            return CompareVersionsManually(version1, version2);
        }
    }

    private static readonly string[] PreReleaseOrder =
    {
        "alpha", // α版本，内部测试版
        "beta", // β版本，公开测试版
        "rc", // Release Candidate，发布候选版
        "pre", // Pre-release，预发布版
        "snapshot", // 快照版
        "dev", // 开发版
        "nightly", // 每日构建版
        "final", // 最终版（通常不使用）
        "" // 空字符串表示正式版（最高优先级）
    };

    private static int GetPreReleasePriority(string preRelease)
    {
        if (string.IsNullOrEmpty(preRelease))
            return Array.IndexOf(PreReleaseOrder, "");

        // 处理带数字的预发布版本（如：alpha1, beta2, rc3）
        string baseType = preRelease.ToLower();
        int number = 0;

        // 提取数字部分
        for (int i = 0; i < baseType.Length; i++)
        {
            if (char.IsDigit(baseType[i]))
            {
                string numericPart = baseType.Substring(i);
                if (int.TryParse(numericPart, out number))
                {
                    baseType = baseType.Substring(0, i);
                }

                break;
            }
        }

        // 移除尾部的连字符或点号
        baseType = baseType.TrimEnd('-', '.');

        int index = Array.IndexOf(PreReleaseOrder, baseType);
        if (index >= 0)
        {
            // 返回基础优先级（乘以1000）加上数字部分
            return index * 1000 + number;
        }

        // 未知的预发布类型，按字母顺序排序
        return -1000000 + Math.Abs(baseType.GetHashCode());
    }

    /// <summary>
    /// 标准化版本字符串（处理语义化版本）
    /// </summary>
    private static string NormalizeVersion(string version)
    {
        // 移除前缀v（如：v1.2.3 -> 1.2.3）
        if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            version = version.Substring(1);
        }

        // 处理语义化版本后缀（如：1.2.3-alpha -> 1.2.3）
        // int dashIndex = version.IndexOf('-');
        // if (dashIndex >= 0)
        // {
        //     version = version.Substring(0, dashIndex);
        // }

        // 确保版本有足够的段
        string[] parts = version.Split('.');
        if (parts.Length < 4)
        {
            // 补全缺失的段为0
            version = string.Join(".", parts);
            for (int i = parts.Length; i < 4; i++)
            {
                version += ".0";
            }
        }

        return version;
    }

    /// <summary>
    /// 手动比较版本（处理非标准版本格式）
    /// </summary>
    private static int CompareVersionsManually(string version1, string version2)
    {
        // 分离版本号和预发布标签
        (string coreVersion1, string preRelease1) = SplitVersion(version1);
        (string coreVersion2, string preRelease2) = SplitVersion(version2);

        // 首先比较核心版本号
        int coreComparison = CompareCoreVersions(coreVersion1, coreVersion2);
        if (coreComparison != 0)
        {
            return coreComparison;
        }

        // 核心版本相同，比较预发布标签
        return ComparePreReleases(preRelease1, preRelease2);
    }

    private static (string coreVersion, string preRelease) SplitVersion(string version)
    {
        if (string.IsNullOrEmpty(version))
            return ("0", "");

        // 移除v前缀
        if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            version = version.Substring(1);
        }

        // 分离构建元数据（+后面的部分）
        int buildIndex = version.IndexOf('+');
        if (buildIndex >= 0)
        {
            version = version.Substring(0, buildIndex);
        }

        // 分离预发布标签（-后面的部分）
        int preReleaseIndex = version.IndexOf('-');
        if (preReleaseIndex >= 0)
        {
            return (version.Substring(0, preReleaseIndex), version.Substring(preReleaseIndex + 1));
        }

        return (version, "");
    }

    private static int CompareCoreVersions(string coreVersion1, string coreVersion2)
    {
        string[] v1Parts = coreVersion1.Split('.');
        string[] v2Parts = coreVersion2.Split('.');

        int maxLength = Math.Max(v1Parts.Length, v2Parts.Length);

        for (int i = 0; i < maxLength; i++)
        {
            int v1Part = i < v1Parts.Length ? ParseVersionPart(v1Parts[i]) : 0;
            int v2Part = i < v2Parts.Length ? ParseVersionPart(v2Parts[i]) : 0;

            if (v1Part < v2Part) return -1;
            if (v1Part > v2Part) return 1;
        }

        return 0;
    }

    private static int ComparePreReleases(string preRelease1, string preRelease2)
    {
        // 都有预发布标签或都没有
        if (string.IsNullOrEmpty(preRelease1) && string.IsNullOrEmpty(preRelease2))
            return 0;

        // 正式版比任何预发布版本都新
        if (string.IsNullOrEmpty(preRelease1) && !string.IsNullOrEmpty(preRelease2))
            return 1;
        if (!string.IsNullOrEmpty(preRelease1) && string.IsNullOrEmpty(preRelease2))
            return -1;

        // 比较预发布标签优先级
        int priority1 = GetPreReleasePriority(preRelease1);
        int priority2 = GetPreReleasePriority(preRelease2);

        if (priority1 < priority2) return -1;
        if (priority1 > priority2) return 1;

        // 优先级相同，按字母顺序比较（用于处理相同类型但不同数字的情况）
        return string.Compare(preRelease1, preRelease2, StringComparison.OrdinalIgnoreCase);
    }

    private static int ParseVersionPart(string part)
    {
        // 移除非数字字符（如：1.2.3-beta -> 只取数字部分）
        string numericPart = string.Empty;
        foreach (char c in part)
        {
            if (char.IsDigit(c))
            {
                numericPart += c;
            }
            else
            {
                break;
            }
        }

        return string.IsNullOrEmpty(numericPart) ? 0 : int.Parse(numericPart);
    }

    // 便捷方法
    public static bool IsNewer(string currentVersion, string comparedVersion)
    {
        return CompareVersions(currentVersion, comparedVersion) > 0;
    }

    public static bool IsOlder(string currentVersion, string comparedVersion)
    {
        return CompareVersions(currentVersion, comparedVersion) < 0;
    }

    public static bool IsSame(string currentVersion, string comparedVersion)
    {
        return CompareVersions(currentVersion, comparedVersion) == 0;
    }

    public static bool IsNewerOrSame(string currentVersion, string comparedVersion)
    {
        return CompareVersions(currentVersion, comparedVersion) >= 0;
    }
}
#endif