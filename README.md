# cmstar.Caching

一个缓存框架。
- 提供缓存的抽象定义 `ICacheProvider` 。
- 封装后的快捷操作 `CacheOperation` 。
- 提供数种不同的缓存实现。
- 二级缓存。
- 缓存负载均衡。

已实现的缓存提供器：
- 内存缓存，基于 System.Runtime.Caching
- redis 缓存，基于 StackExchange.Redis
- 内存缓存，基于 ASP.NET System.Web.HttpRuntime.Cache （仅 .NET Framework）

支持的 .NET 版本：
- .NET Framework 4.6 或更高版本。
- 其他支持 .NET Standard 2 的运行时如 .NET Core 2/3 、 .NET 5/6 。

依赖库：
- [cmstar.RapidReflection](https://www.nuget.org/packages/cmstar.RapidReflection/) To emit IL for accessing type members.
- [cmstar.Serialization.Json](https://www.nuget.org/packages/cmstar.Serialization.Json/) For JSON serialization.

## 快速使用

待完善。

## 功能详情

### 缓存的抽象定义

### 快捷操作

### 二级缓存

### 负载均衡

## 其他语言的版本

- GO 版：[thisXYH/cache](https://github.com/thisXYH/cache)