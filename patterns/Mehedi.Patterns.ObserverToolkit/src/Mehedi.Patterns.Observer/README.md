# Observer Pattern in C# — Synchronous & Asynchronous Implementations

This repository demonstrates how to implement the **Observer Design Pattern** in C# with both **synchronous** and **asynchronous** support, along with centralized management using `ObserverFactory` and `AsyncObserverFactory`.

## 📌 Features

* **Synchronous Observer Pattern**
* **Asynchronous Observer Pattern** using `Task`-based notifications
* **Thread-safe factories** for centralized observer management
* **Strongly-typed subjects** per key
* **Easy-to-use API** for registration and broadcasting


## 🚀 Getting Started

### 1️⃣ Clone the repo

```bash
git clone https://github.com/your-username/observer-pattern-csharp.git
cd observer-pattern-csharp
```

### 2️⃣ Build the project

```bash
dotnet build
```

### 3️⃣ Run examples

**Synchronous Example**

```bash
dotnet run --project examples/ProgramSync.csproj
```

**Asynchronous Example**

```bash
dotnet run --project examples/ProgramAsync.csproj
```

---

## 📖 Usage Examples

### **Synchronous with `ObserverFactory`**

```csharp
var factory = ObserverFactory.Instance;

factory.RegisterHandler<int>(
    key: "number-changed",
    sender: "MainProgram",
    action: value => Console.WriteLine($"[SYNC] Received value: {value}")
);

factory.Notify("number-changed", 42);
factory.Notify("number-changed", 99);
```

**Output:**

```
[SYNC] Received value: 42
[SYNC] Received value: 99
```

---

### **Asynchronous with `AsyncObserverFactory`**

```csharp
var asyncFactory = AsyncObserverFactory.Instance;

asyncFactory.RegisterHandler<int>(
    key: "number-changed-async",
    sender: "MainProgram",
    async value =>
    {
        await Task.Delay(500);
        Console.WriteLine($"[ASYNC] Received value: {value}");
    }
);

await asyncFactory.NotifyAsync("number-changed-async", 42);
await asyncFactory.NotifyAsync("number-changed-async", 99);
```

**Output:**

```
[ASYNC] Received value: 42
[ASYNC] Received value: 99
```

---

## 📦 Installation

You can copy the `ObserverFactory` and `AsyncObserverFactory` code into your project directly or package them into a shared library.

---

## 📚 References

1. [Microsoft Docs: Observer Design Pattern](https://learn.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern)
2. [Gang of Four — Design Patterns](https://en.wikipedia.org/wiki/Design_Patterns)
3. [Reactive Extensions (Rx.NET)](https://reactivex.io/)

---

## 📝 License

This project is licensed under the MIT License.
