# Angular 21 Signal Forms

## 1. Core Concept

Signal Forms manage form state using **Angular Signals** instead of `FormGroup` / `FormControl`.

**Mental model:**

```
signal(model) → form(model) → field tree → [field] directive
```

- One signal holds the entire form model
- `form()` creates a *field tree* mirroring the model shape
- Fields expose reactive state via signals

---

## 2. Creating a Form

### Define a model signal

```ts
interface LoginData {
  email: string;
  password: string;
}

const loginModel = signal<LoginData>({
  email: '',
  password: '',
});
```

### Create the form

```ts
const loginForm = form(loginModel);
```

- `loginForm.email`
- `loginForm.password`

Fields are accessed via dot notation.

---

## 3. Binding Inputs

Bind HTML elements using the **`[field]` directive**:

```html
<input type="email" [field]="loginForm.email" />
<input type="password" [field]="loginForm.password" />
```

What this does:
- Two-way binding
- Syncs value, touched, dirty, disabled, readonly
- No `[(ngModel)]`, no `formControlName`

---

## 4. Reading Field State

Each field is a **callable function** returning `FieldState`.

```ts
loginForm.email()
```

### Common signals

```ts
loginForm.email().value()      // current value
loginForm.email().valid()      // boolean
loginForm.email().touched()    // boolean
loginForm.email().dirty()      // boolean
loginForm.email().errors()     // ValidationError[]
```

---

## 5. Updating Values Programmatically

```ts
loginForm.email().value.set('alice@example.com');
```

- Updates field
- Updates model signal automatically

```ts
loginModel().email // 'alice@example.com'
```

---

## 6. Supported Input Types

### Text / Email

```html
<input type="text" [field]="form.name" />
<input type="email" [field]="form.email" />
```

### Number (auto-converts)

```html
<input type="number" [field]="form.age" />
```

### Date & Time (stored as strings)

```html
<input type="date" [field]="form.eventDate" />
<input type="time" [field]="form.eventTime" />
```

```ts
const date = new Date(form.eventDate().value());
```

### Textarea

```html
<textarea [field]="form.message"></textarea>
```

### Checkboxes

```html
<input type="checkbox" [field]="form.agreeToTerms" />
```

Multiple checkboxes → one boolean per option.

### Radio Buttons

```html
<input type="radio" value="free" [field]="form.plan" />
<input type="radio" value="premium" [field]="form.plan" />
```

Selected radio sets the field value.

### Select Dropdowns

```html
<select [field]="form.country">
  <option value="">Select</option>
  <option value="us">USA</option>
</select>
```

Dynamic options supported via `@for`.

⚠️ `multiple` select is **not supported**.

---

## 7. Validation

Validators are attached via a **schema function**:

```ts
const loginForm = form(loginModel, (schema) => {
  required(schema.email);
  email(schema.email);
});
```

### Custom error messages

```ts
required(schema.email, { message: 'Email is required' });
email(schema.email, { message: 'Invalid email' });
```

### Common validators

- `required`
- `email`
- `min`, `max`
- `minLength`, `maxLength`
- `pattern`

---

## 8. Field State Signals (Reference)

Each field provides:

| Signal        | Meaning |
|--------------|--------|
| `valid()`    | All validators pass |
| `touched()`  | Focused + blurred |
| `dirty()`    | Value changed |
| `disabled()` | Disabled state |
| `readonly()` | Readonly state |
| `pending()`  | Async validation running |
| `errors()`   | Array of validation errors |

---

## 9. Key Differences vs Classic Forms

| Classic Forms | Signal Forms |
|--------------|--------------|
| FormGroup | Signal model |
| FormControl | Field signal |
| Validators array | Schema function |
| Imperative APIs | Reactive signals |
| Change detection heavy | Signal-based |

---

## 10. Rules for Agents

✅ Do:
- Use signals for all reads/writes
- Bind only via `[field]`
- Treat fields as reactive state objects

❌ Don’t:
- Mix with `FormGroup`
- Use `ngModel`
- Mutate model objects directly

---

## Summary

Signal Forms are:
- Declarative
- Signal-native
- Model-driven
- Validation-aware

They represent Angular’s **future direction for forms**.
