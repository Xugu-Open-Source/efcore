## About

_Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft_ adds JSON support for `System.Text.Json` (the Microsoft JSON stack) to [Microsoft.EntityFrameworkCore.XuGu](https://github.com/PomeloFoundation/Microsoft.EntityFrameworkCore.XuGu).

## How to Use

```csharp
optionsBuilder.UseXG(
    connectionString,
    serverVersion,
    options => options.UseMicrosoftJson())
```

## Related Packages

* [Microsoft.EntityFrameworkCore.XuGu](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.XuGu)
* [System.Text.Json](https://www.nuget.org/packages/System.Text.Json)

## License

_Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft_ is released as open source under the [MIT license](https://github.com/PomeloFoundation/Microsoft.EntityFrameworkCore.XuGu/blob/master/LICENSE).

## Feedback

Bug reports and contributions are welcome at our [GitHub repository](https://github.com/PomeloFoundation/Microsoft.EntityFrameworkCore.XuGu).