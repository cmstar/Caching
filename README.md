# cmstar.Caching

[![NuGet](https://img.shields.io/nuget/v/cmstar.Caching.svg)](https://www.nuget.org/packages/cmstar.Caching/)

一个简单的缓存框架：
- 缓存的抽象定义 `ICacheProvider` 和快捷操作 `CacheOperation` 。
- 完全泛型接口。
- 提供数种不同的缓存实现，包括内存缓存和 redis 缓存。
- 支持异步（async）操作（内存缓存的异步只提供语义）。
- 支持缓存计数器的增减（Increase/Decrease）。
- 支持缓存复杂对象，支持单独修改对象指定字段。
- 可以缓存 null 。
- 可以指定缓存过期时间的随机区间，通过简单的方案应对缓存雪崩。
- 二级缓存。
- 简单的负载均衡。

支持的 .NET 版本：
- .NET Framework 4.6 或更高版本。[注\*]
- 其他支持 .NET Standard 2 的运行时如 .NET Core 2/3 、 .NET 5/6 。

> 注： .NET Framework 4.6 使用 StackExchange.Redis 1.2.6 版，它是最后一个支持4.6之前的版本。其他运行时则使用 2.x 版。如果有条件，应尽量使用高版本。

依赖库：
- [cmstar.RapidReflection](https://www.nuget.org/packages/cmstar.RapidReflection/) To emit IL for accessing type members.
- [cmstar.Serialization.Json](https://www.nuget.org/packages/cmstar.Serialization.Json/) For JSON serialization.
- [StackExchange.Redis](https://www.nuget.org/packages/StackExchange.Redis/) For redis operations.


## 快速使用

### 安装

通过 Package Manager:
```
Install-Package cmstar.Caching
```

或通过 dotnet-cli:
```
dotnet add package cmstar.Caching
```

redis 提供器则安装 `cmstar.Caching.Redis` 包。

### 缓存接口及基本用法

下面演示缓存接口 `ICacheProvider` 和几种不同的实现的声明，及基本用法。
在正式的使用中，更推荐基于封装后的快捷操作 `CacheOperation` 访问缓存，而不是直接访问 `ICacheProvider` 。

```csharp
using cmstar.Caching;
using cmstar.Caching.Redis;

// 定义缓存操作的入口。ICacheProvider 是缓存的抽象接口。
public static class Caches
{
    // 基于 System.Runtime.Caching.MemoryCache 实现的缓存。
    public readonly  static ICacheProvider Memory = new MemoryCacheProvider("demo");

    // 基于 StackExchange.Redis 库的 redis 缓存实现。连接字符串使用 StackExchange.Redis 的语法。
    public readonly  static ICacheProvider Redis = new cmstar.Caching.Redis.RedisCacheProvider("127.0.0.1:6379");

    // 基于 System.Web.HttpRuntime.Cache 实现的缓存。只支持 .net Framework 。
    // 这个缓存提供器是单例的，不能使用多个将缓存分区。从平台兼容性考虑很少用了。
    public readonly  static ICacheProvider Http = HttpRuntimeCacheProvider.Instance;
}
```

定义了缓存入口后，就可以使用它们了。

```csharp
public static async Task Demo()
{
    // 设置一个过期时间为10分钟的内存缓存 my-num ，值为 123 。
    // 缓存的 key 格式没有特别的要求，从 redis 兼容性考虑一般建议使用 redis 官方推荐的写法。
    Caches.Memory.Set("my:num", 123, TimeSpan.FromMinutes(10));

    // 读取缓存。
    Caches.Memory.Get<int>("my:num"); // -> 123

    // 如果类型支持转换（IConvertible），可以自动转换。若类型不兼容，会抛出异常。
    Caches.Memory.Get<string>("my:num"); // -> "123"

    // 读取不存在的 key 时，返回默认值。
    Caches.Memory.Get<int>("not.exist"); // -> 0

    // 使用复杂对象。这里为了方便演示使用了匿名对象。
    // 这里利用 TimeSpan.Zero 设置了一个不会过期的缓存。
    var obj = new { i = 123, s = "abc" };
    Caches.Memory.Set("my:obj", obj, TimeSpan.Zero);

    // 可以设置 null ，必须给定泛型类型。
    Caches.Memory.Set<string>("my:nullable:string", null, TimeSpan.FromMinutes(10));
    
    // 如果存储的值刚好是类型的默认值，使用 Get 方法就区分不出来是 key 不存在还是值就是默认值。
    // 可使用 TryGet 方法。
    if (Caches.Memory.TryGet<string>("my:nullable:string", out var myStr))
    {
        Console.WriteLine($"the key exists, the value is {myStr}");
    }
    Caches.Memory.TryGet<string>("not.exist", out _); // -> 返回 false 表示 key 不存在。

    // 操作 redis 缓存。
    // 支持异步操作。对于内存缓存异步操作仅提供语义，对于支持异步的提供器比如 redis 是有实际效果的。
    // 这里利用 TimeSpan.Zero 设置了一个不会过期的缓存。
    await Caches.Redis.SetAsync("my:str", "abc", TimeSpan.FromHours(1));

    // 移除缓存。
    Caches.Redis.Remove("my:str"); // -> true 表示目标 key 存在。
    await Caches.Redis.RemoveAsync("my:str"); // -> false 表示目标 key 不存在，因为前一步已经移除了。
}
```

其他缓存接口的更详细的说明可查看[扩展接口](#扩展接口)。

### 使用快捷操作

**这是建议的用法。**

泛型类 `CacheOperation` 封装 `ICacheProvider` 提供缓存的快捷操作，它描述了一种“模式”，用于从逻辑上统一定义和管理缓存，包括：
- 缓存的命名空间、 key 的前缀（不变的部分）和变量部分。
- 缓存过期时间及随机量。

```csharp
using cmstar.Caching;
using cmstar.Caching.Redis;

// 更贴近真实的应用场景。
// 定义缓存操作的入口。
public static class Caches
{
    // ICacheProvider 不再直接对外暴露，而是通过 CacheOperation 预定义缓存入口。
    private static readonly ICacheProvider Memory = new MemoryCacheProvider("demo");
    private static readonly ICacheProvider Redis = new RedisCacheProvider("127.0.0.1:6379");

    // 使用内存缓存。通过学生ID获取学生姓名。
    // 泛型参数说明这是一个输入值为 int 输出值为 string 的缓存。
    // 只要将这里的 Memory 换成 Redis ，就变成 redis 缓存了，业务代码不需要任何修改。
    public static readonly CacheOperation<int, string> StudentNameById
        = new CacheOperation<int, string>(
            "school", "student:name.by.id", Memory, CacheExpiration.FromMinutes(10));

    // 使用 redis 缓存。通过学生姓名、学年获取该年成绩。
    // 泛型参数说明这是一个输入值为 (string, int) 输出值为 int 的缓存。
    public static readonly CacheOperation<string, int, int> StudentScoreByNameAndYear
        = new CacheOperation<string, int, int>(
            "school", "student:score.of.year", Redis, CacheExpiration.FromMinutes(60, 15));
}
```

这里的“school”就是命名空间，其后是缓存的前缀。泛型参数表示了缓存 key 中的变量类型和顺序。最终 key 的拼接规则详见[缓存key的拼接规则](#缓存key的拼接规则)。

`CacheExpiration.FromMinutes(60, 15)` 定义了一个随机范围在 45-75 分钟的缓存过期时间，用于防止缓存雪崩。详见[缓存过期时间的随机范围](#缓存过期时间的随机范围)。

使用：
```csharp
// 为 StudentName 的读写提供线程安全。
// 如果是更复杂的场景，应该使用对应的加锁方案，比如基于数据库事务或其他分布式锁。
private static readonly object NameLock = new object();

// 常规缓存操作流程：
// 1. 尝试通过缓存获取数据；
// 2. 当缓存不存在时查询数据源；
// 3. 从数据源获取数据后创建（或更新）缓存。
public static string GetStudentName(int id)
{
    // CacheOperation.Key() 方法返回一个 KeyOperation 泛型对象，
    // 它绑定了当前缓存的 key ，并提供类似于 ICacheProvider 的全部操作。
    var cache = Caches.StudentNameById.Key(id);

    // 可直接通过 .Key 属性查看拼接后的 key 。 例如 id=123 则：
    Console.WriteLine(cache.Key); // -> school:student:name.by.id:123

    // 读取缓存值，若读取到值，即可直接返回。
    if (cache.TryGet(out var name))
        return name;

    lock (NameLock)
    {            
        // 从数据源——例如数据库的 SELECT 操作——查询 ID 对应的名字，此处省略过程。
        // SELECT name FROM student WHERE id=@id
        name = "John";

        cache.Set(name);
        return name;
    }
}

public static void SetStudentName(int id, string name)
{
    lock (NameLock)
    {
        // 执行更新，例如数据库的 UPDATE 操作。此处省略。
        // UPDATE student SET name=@name WHERE id=@id

        // 同步缓存，移除旧的或者直接用 Set 更新。
        Caches.StudentNameById.Key(id).Remove();
    }
}

// 异步操作演示。
public static async Task<int> GetScoreOfYearAsync(string name, int year)
{
    // 基于 CacheOperation 的泛型定义，这里 .Key() 方法必须输入一个 string 和一个 int 。
    var cache = Caches.StudentScoreByNameAndYear.Key(name, year);

    // 和非异步版有些不同，异步函数不支持 out 参数， 而是统一通过返回值表示处理结果。
    var tryGetResult = await cache.TryGetAsync();
    if (tryGetResult.HasValue)
        return tryGetResult.Value;

    // 从数据源获取，此处省略过程。
    var score = 123; 

    await cache.SetAsync(score);
    return score;
}
```

#### 缓存key的拼接规则

缓存 key 使用 redis 的 key 命名规则：
- 使用冒号“:”作为命名空间的区隔。
- 使用点“.”作为多个单词间的区隔。

其次，多个变量间使用下划线“_”分隔。总体规则为： `{namespace}:{prefix}:{var1}_{var2}_...` 。
如果变量中有下划线，其被转义为“\_”，反斜杠本身被转义为“\\”。

例如上例中的 `StudentScoreByNameAndYear.Key(name, year)` ， 若 `name=John_Doe year=2016` ，
则其缓存 key 为： `school:student:score.of.year:John\_Doe_2016`。

#### 缓存过期时间的随机范围

缓存雪崩的一个常见场景是缓存在短时间内大量过期，导致短时间大量回源查询。
如果缓存都使用相同的过期时间，程序首次启动时就容易建立很多过期时间相近的缓存。
一个最简单的应对方式是为缓存的过期时间添加一个浮动范围。

`CacheOperation` 接收的过期时间 `CacheExpiration` 就允许指定一个随机浮动范围。 
`CacheExpiration.FromMinutes(60, 15)` 指定了一个以60分钟为基数，15分钟上下浮动的范围，即45-75分钟。
每次缓存被设置时，都会在此区间内随机生成一个过期时间。


## 缓存提供器

缓存抽象接口：
- `ICacheProvider` 缓存提供器的基础接口，所有缓存提供器均需实现此接口，其他接口均继承此接口。
- `ICacheIncreasable` 扩展接口。定义整数型缓存值的增减（Increase/Decrease）操作。
- `ICacheFieldAccessable` 扩展接口。允许读写缓存的复杂对象的字段。
- `ICacheFieldIncreasable` 扩展接口。在 `ICacheFieldIncreasable` 的基础上，允许对整数型的字段进行增减（Increase/Decrease）操作。

预定义的缓存提供器：

redis 缓存由 `cmstar.Caching.Redis` 提供；其余均在 `cmstar.Caching` 里。

|名称|功能|实现扩展接口|
|-|-|-|
|MemoryCacheProvider|内存缓存，基于 System.Runtime.Caching|ICacheIncreasable<br>ICacheFieldAccessable<br>ICacheFieldIncreasable|
|MemoryCacheProviderSlim|MemoryCacheProvider 的轻量化版本，但性能更好|仅实现 ICacheProvider|
|RedisCacheProvider|redis 缓存，使用 redis string |ICacheIncreasable|
|RedisHashCacheProvider|redis 缓存，使用 redis hash|ICacheIncreasable<br>ICacheFieldAccessable<br>ICacheFieldIncreasable|
|HttpRuntimeCacheProvider|内存缓存，基于 ASP.NET System.Web.HttpRuntime.Cache|ICacheIncreasable<br>ICacheFieldAccessable<br>ICacheFieldIncreasable|
|HttpRuntimeCacheProviderSlim|HttpRuntimeCacheProvider 的轻量化版本，但性能更好|仅实现 ICacheProvider|
|CacheBalancer|[简单的负载均衡](#简单的负载均衡)|取决于加入的缓存提供器|
|L2CacheProvider|[简单的二级缓存](#简单的二级缓存)|取决于加入的缓存提供器|


## 支持的类型

### 内存缓存

可以存储任何 .net 对象。但在使用上应注意，若缓存引用类型（class），若非有意为止，应避免在获取缓存值后直接修改其属性，
因为缓存值不是克隆（clone）的，从缓存读取到的是缓存对象的引用，在代码上修改其属性会直接影响到缓存的值。
如果需要修改缓存对象的字段，可使用对应的接口，详见[修改对象的字段](#修改对象的字段)。

多数场景下，为了方便切换缓存提供器，从兼容性考虑，应该以与 redis 缓存兼容的方式使用缓存，即仅存储可序列化的对象。

> 倡导无状态的函数式编程。

### redis 缓存

对象应能够被 JSON 序列化。当前框架：
- 对于简单对象，以简单格式存储，比如字符串原始形式存储的。
- 对于复杂对象，使用 JSON 序列化后存储到 redis 。

> 序列化和反序列化可查看 `RedisConvert` 类里的 `ToRedisValue/FromRedisValue` 方法。


## 扩展接口

### 数值的增减 Increase/Decrease

如果缓存提供器实现了 `ICacheIncreasable` 接口，则其可以进行整数值缓存的增减操作。

下面是利用此特性实现的一个限流器的例子：
```csharp
using cmstar.Caching;

public static class Demo
{
    // 内存缓存实现了 ICacheIncreasable 。
    private static readonly ICacheIncreasable Memory = new MemoryCacheProvider("demo");

    // 定义一个用于限流器计数的缓存，重置周期是10秒。
    // 注意：缓存的过期时间不一定精确，取决于缓存提供器的实现。
    private static readonly CacheOperation<int> Counter
        = new CacheOperation<int>("demo", "rate.limit", Memory, CacheExpiration.FromSeconds(10));

    // 利用缓存限流，实现每10秒最多15次操作。
    public static void DoWithRateLimit()
    {
        while (true)
        {
            var value = Counter.Key().IncreaseOrCreate(1);
            if (value <= 15)
                break;

            // 等下一个周期。
            Thread.Sleep(1000);
        }

        // 执行后续操作。
    }
}
```

### 修改对象的字段

`ICacheFieldAccessable` 和 `ICacheFieldIncreasable` 用于操作缓存值的字段。
下面的示例使用一个简化的手机号短信验证码校验流程，演示这两个接口的使用。

```csharp
using cmstar.Caching;
using cmstar.Caching.Redis;

public class MobileAuth
{
    public string AuthCode; // 验证码。
    public int FailCounter; // 验证失败的次数。
}

public static class MobileValidation
{
    // RedisHashCacheProvider 实现了 ICacheFieldIncreasable 接口。
    private static readonly ICacheFieldIncreasable RedisHash = new RedisHashCacheProvider("127.0.0.1:6379");

    public static readonly CacheOperation<string, MobileAuth> Codes
        = new CacheOperation<string, MobileAuth>(
            "account", "login:mobile.validate", RedisHash, CacheExpiration.FromMinutes(5));

    // 为指定的手机号生成验证码。
    public static async Task GenerateAuthCodeAsync(string mobile)
    {
        var authCode = "123456"; // 生成一个验证码，此处省略过程。
        await Codes.Key(mobile).SetAsync(new MobileAuth { AuthCode = authCode });
    }

    // 校验用户输入的验证码。
    public static async Task<bool> ValidateAuthCodeAsync(string mobile, string authCode)
    {
        var cache = Codes.Key(mobile);

        // 读取已缓存的验证码。
        // 使用 FieldGetAsync 方法单独读取验证码字段，可以利用 lambda 表达式安全的获取字段名称。
        var cachedAuthCode = await cache.FieldGetAsync(x => x.AuthCode);

        // 如果缓存不存在，读取到字段的默认值。
        if (cachedAuthCode == null)
            return false; // Session not found.

        if (cachedAuthCode != authCode)
        {
            // 验证失败，利用 FieldIncreaseAsync 方法增加错误计数。
            // 这里也可以将 nameof 部分替换为 lambda 表达式： x => x.FailCounter 。
            // 利用 C# 的 nameof 操作符，也可以安全的获取到字段名称。省掉 lambda 表达式的解析过程，可以提高效率。
            var failCounter = await cache.FieldIncreaseAsync(nameof(MobileAuth.FailCounter), 1);

            // 验证失败超过次数上线，会话作废。
            if (failCounter > 3)
            {
                await cache.RemoveAsync();
            }
            return false; // Mismatch.
        }

        // 验证完成，原会话作废。
        await cache.RemoveAsync();
        return true;
    }
}
```


## 简单的二级缓存

`L2CacheProvider` 提供一个简单的二级缓存：
- 用一个具有较短过期时间的缓存 L1 作为原缓存 L2 的前置。
- 读取数据时先从 L1 获取；若获取不到再从 L2 获取，读取到后将 L2 的数据设置在 L1 上。
- `Set` 会同时建立或更新两级缓存的数据。
- `Remove` 会同时移除两级缓存的数据。

适用于短时间重复访问频率高，原缓存的访问开销较大的场景。

```csharp
using cmstar.Caching;
using cmstar.Caching.Redis;

public static class Caches
{
    private static readonly ICacheProvider Memory = new MemoryCacheProvider("demo");
    private static readonly ICacheProvider Redis = new RedisCacheProvider("127.0.0.1:6379");

    // 使用 Memory 作为 Redis 的前置缓存。单独指定 Memory 上的缓存过期时间为3分钟。
    private static readonly ICacheProvider L2Redis 
        = new L2CacheProvider(level2: Redis, level1: Memory, CacheExpiration.FromMinutes(3));

    // 这里给定的30分钟会作用于 Redis 上。
    public static readonly CacheOperation<int, string> StudentNameById
        = new CacheOperation<int, string>(
            "school", "student:name.by.id", L2Redis, CacheExpiration.FromMinutes(30));
}
```


## 简单的负载均衡

`CacheBalancer` 是一个简单缓存负载均衡器。
- 允许指定每个缓存提供器的分配比例。
- 同一个缓存 key 总是会分配到同一个缓存提供器上。
- 初始化后，不允许再修改增减缓存提供器或调整比例，所以也不用支持一致性哈希。

```csharp
using cmstar.Caching;
using cmstar.Caching.Redis;
using cmstar.Caching.LoadBalancing;

public static class Caches
{
    // CacheBalancer 实现了 ICacheProvider 。
    private static readonly ICacheProvider RedisBalancer = new CacheBalancer();

    static Caches()
    {
        ICacheProvider redis1 = new RedisCacheProvider("127.0.0.1:6379");
        ICacheProvider redis2 = new RedisHashCacheProvider("127.0.0.1:6380");

        // 按 7:3 的比例分配。
        RedisBalancer.AddNode(redis1, 70);
        RedisBalancer.AddNode(redis2, 30);
    }

    // 将 RedisBalancer 作为缓存提供器。
    public static readonly CacheOperation<int, string> StudentNameById
        = new CacheOperation<int, string>(
            "school", "student:name.by.id", RedisBalancer, CacheExpiration.FromMinutes(10));
}
```

## 其他语言的版本

- GO 版：[thisXYH/cache](https://github.com/thisXYH/cache)
