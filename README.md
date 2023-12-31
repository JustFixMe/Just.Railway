# Base for Railway Programming in .NET

This library uses features of C# to achieve railway-oriented programming.

The desire is to make somewhat user-friendly experience while using result-object pattern.

## Features

- Immutable ```Error``` class
- ```Result``` object
- A bunch of extensions to use result-object pattern with
- ```Try``` extensions to wrap function calls with result-object
- ```Ensure``` extensions to utilize result-object in validation scenarios

## Getting Started

### Install from NuGet.org

```sh
# install the package using NuGet
dotnet add package Just.Railway
```

## Examples

### Error

```csharp
using Just.Railway;
Error expectedError = Error.New(type: "Some Error", message: "Some error detail");
Error exceptionalError = Error.New(new Exception("Some Exception"));
Error manyErrors = Error.Many(expectedError, exceptionalError);
// the same result while using .Append(..) or +
manyErrors = expectedError.Append(exceptionalError);
manyErrors = expectedError + exceptionalError;
```

> **Note**
> You can easily serialize/deserialize Error to and from JSON

### Result

#### As return value:

```csharp
Result Foo()
{
    // ...
    if (SomeCondition())
        return Result.Failure(Error.New("Some Error"));
        // or just: return Error.New("Some Error");
    // ...
    return Result.Success();
}

Result<T> Bar()
{
    T value;
    // ...
    if (SomeCondition())
        return Error.New("Some Error");
    // ...
    return value;
}
```

#### Consume Result object

```csharp
Result<int> result = GetResult();

string value = result
    .Append("new") // -> Result<(int, string)>
    .Map((i, s) => $"{s} result {i}") // -> Result<string>
    .Match(
        onSuccess: x => x,
        onFailure: err => err.ToString()
    );
// value: "new result 1"

Result<int> GetResult() => Result.Success(1);
```

#### Recover from failure

```csharp
Result<string> failed = new NotImplementedException();

Result<string> result = failed.TryRecover(err => err.Type == "System.NotImplementedException"
    ? "recovered"
    : err);
// result with value: "recovered"
```

### Try

```csharp
Result result = Try.Run(SomeAction);
// you can pass up to 5 arguments like this
result = Try.Run(SomeActionWithArguments, 1, 2.0, "3");

// you also can call functions
Result<int> resultWithValue = Try.Run(SomeFunction);

void SomeAction() {}
void SomeActionWithArguments(int a1, double a2, string? a3) {}
int SomeFunction() => 1;
```

### Ensure

```csharp
int? value = GetValue();
Result<int> result = Ensure.That(value) // -> Ensure<int?>
    .NotNull() // -> Ensure<int>
    .Satisfies(i => i < 100)
    .Result();

int? GetValue() => 1;
```
