# Angular 21 Signals

## 1. What Is a Signal?

A **signal** is a wrapper around a value that:
- Tracks where it is read
- Notifies consumers when it changes
- Enables fine-grained rendering updates

Signals are **functions**:
```ts
count(); // read value
```

---

## 2. Writable Signals

Writable signals hold mutable state.

```ts
const count = signal(0);
```

### Reading
```ts
count();
```

### Writing
```ts
count.set(3);
count.update(v => v + 1);
```

- Synchronous
- Typed as `WritableSignal<T>`
- Default equality: referential (`===`)

---

## 3. Computed Signals

Computed signals derive values from other signals.

```ts
const doubleCount = computed(() => count() * 2);
```

Properties:
- Read-only
- Lazy
- Memoized
- Dynamically tracked dependencies

❌ Invalid:
```ts
doubleCount.set(3); // compile-time error
```

---

## 4. Dynamic Dependencies

Only signals actually read during execution are tracked.

```ts
const conditional = computed(() => {
  return show() ? count() : 'hidden';
});
```

Dependencies may appear or disappear over time.

---

## 5. Effects (Side Effects)

Use `effect()` to run side effects when signals change.

```ts
effect(() => {
  console.log(count());
});
```

Use effects for:
- Logging
- Calling non-reactive APIs
- Synchronization

---

## 6. Avoiding Accidental Dependencies

Use `untracked()` to read signals without tracking.

```ts
effect(() => {
  const user = currentUser();
  untracked(() => {
    loggingService.log(user);
  });
});
```

---

## 7. Equality Functions

Custom equality prevents unnecessary updates.

```ts
const data = signal(['test'], { equal: _.isEqual });
```

Default: referential equality.

---

## 8. Type Checking Signals

```ts
isSignal(value);
isWritableSignal(value);
```

---

## 9. Signals in Components

- Reading a signal in a template registers it as a dependency
- Signal changes schedule change detection automatically
- Works with `OnPush`

---

## 10. Async & Advanced Topics

Signals are synchronous.

For async:
- Use resources
- Use RxJS interop

Advanced:
- `linkedSignal`
- Async resources

---

## 11. Rules for Coding Agents

### Do
- Treat signals as functions
- Use `computed` for derivations
- Use `effect` for side effects

### Don’t
- Set computed signals
- Mutate objects in-place
- Create accidental dependencies

---

## Mental Model

```
signal    → state
computed  → derived state
effect    → side effects
untracked → escape hatch
```

Signals are Angular’s core reactive primitive.
