# UI rebuild spec — FluentUI is gone

FluentUI was removed from this solution. Every page must be rebuilt with **plain HTML5
elements + the CSS classes below**, which already exist and are visually validated in
`src/IdentityServer/IdentityServer/wwwroot/app.css`. Do not invent new class names, do not
add inline `style=` attributes except where this spec shows one, and never reintroduce any
`Fluent*` component, `Icons.Regular.*`, `Appearance.*`, `Orientation.*`, `Typography.*`,
`DesignThemeModes`, `Color.*` or `IDialogContentComponent`.

## Available components (namespace already in `_Imports.razor`)

```razor
<Icon Name="users" Size="18" Stroke="1.7" />
```
Valid `Name` values: dashboard, users, user, shield, document, apps, key, plug, plus,
search, bell, moon, sun, edit, trash, lock, unlock, disconnect, check, check-circle, x,
x-circle, alert, info, chevron-down, arrow-right, arrow-left, arrow-up, arrow-down,
logout, menu, chat, clock, globe, refresh, save.

```razor
<TagInput @bind-Values="redirectUris" Placeholder="https://app/callback" />
```
`Values` is `List<string>`. Use for redirect URI lists.

```razor
<StatCard Title="Users" Value="@count" IconName="users" Note="optional" Featured="true"
          Delta="3" DeltaText="3 today" />
```
`Value` is `int?`; renders a spinner while null.

## Services (constructor-free, inject by interface)

```razor
@inject IToastService ToastService
@inject IDialogService DialogService
```

- `ToastService.ShowSuccess(msg)` / `ShowError(msg)` / `ShowWarning(msg)` / `ShowInfo(msg)`
- Confirmations — **this replaces every `ShowConfirmationAsync` + `dialog.Result` pattern**:

```csharp
var ok = await DialogService.ConfirmAsync(
    L["ConfirmDeletionTitle"], L["DeleteUserConfirm", item.UserName ?? ""],
    L["Delete"], L["Cancel"], danger: true);
if (!ok) return;
```

## CSS vocabulary

**Page skeleton** — every page's root is a `<section class="card">`; the layout supplies the
shell, sidebar and the page `<h1>`, so pages must **not** render their own page title bar.

```razor
<section class="card">
    <div class="card-head">
        <h2 class="card-title">@L["UsersTitle"]</h2>
        <button type="button" class="btn btn-primary btn-sm" @onclick="Create">
            <Icon Name="plus" Size="15" Stroke="2.2" /> @L["New"]
        </button>
    </div>
    ...
</section>
```

**Loading / empty**
```razor
<div class="spinner spinner-lg spinner-center"></div>
<div class="empty"><p class="empty-title">@L["NoData"]</p><p>@L["NoDataHint"]</p></div>
```

**Table** (replaces FluentDataGrid; write the markup per page)
```razor
<div class="table-wrap">
  <table class="table">
    <thead><tr><th>@L["ColUsername"]</th><th class="num">@L["ColRoles"]</th><th></th></tr></thead>
    <tbody>
      @foreach (var item in items)
      {
        <tr>
          <td class="cell-strong">@item.UserName</td>
          <td class="num">@item.Roles.Count</td>
          <td>
            <div class="actions">
              <button type="button" class="btn-row" title="@L["Edit"]" @onclick="() => Edit(item)"><Icon Name="edit" Size="15" /></button>
              <button type="button" class="btn-row is-danger" title="@L["Delete"]" @onclick="() => DeleteAsync(item)"><Icon Name="trash" Size="15" /></button>
            </div>
          </td>
        </tr>
      }
    </tbody>
  </table>
</div>
```
Cell modifiers: `cell-strong` (emphasis), `cell-muted` (secondary), `num` (right-aligned
tabular numerals, use on both `th` and `td`).

**Badges** — `<span class="badge badge-green">@L["ActiveStatus"]</span>`;
variants `badge-green badge-red badge-amber badge-blue badge-gray`.

**Buttons** — `btn btn-primary`, `btn btn-outline`, `btn btn-ghost`, `btn btn-danger`;
add `btn-sm` for compact, `btn-block` for full width. Circular: `btn-icon`. In-row: `btn-row`
(+ `is-danger`). Always set `type="button"` unless it submits a form.

**Forms**
```razor
<div class="form">
  <div class="form-grid">
    <div class="field">
      <label class="field-label" for="name">@L["Name"] <span class="req">*</span></label>
      <input id="name" class="input" @bind="name" @bind:event="oninput" />
    </div>
  </div>
  <div class="field">
    <label class="field-label" for="type">@L["ClientType"]</label>
    <select id="type" class="select" @bind="clientType">
      <option value="confidential">confidential</option>
      <option value="public">public</option>
    </select>
  </div>
  <span class="field-hint">@L["SomeHint"]</span>
  <div class="form-actions">
    <button type="button" class="btn btn-primary" disabled="@(!CanSubmit || saving)" @onclick="SubmitAsync">@L["Create"]</button>
    <button type="button" class="btn btn-ghost" @onclick='() => Nav.NavigateTo("/roles")'>@L["Cancel"]</button>
  </div>
</div>
```
`form` caps width at 560px; add `form-wide` alongside it for full width.
`form-grid` auto-fits columns at 200px min. Password fields: `<input type="password" class="input">`.
Disabled binding must use `disabled="@boolExpr"`, never `Disabled=`.

**Lists** (avatar + two lines + right meta)
```razor
<div class="list">
  <div class="list-row">
    <span class="avatar avatar-c2">AB</span>
    <span class="list-body">
      <span class="list-title">Title</span>
      <span class="list-sub">Secondary</span>
    </span>
    <span class="list-meta">
      <span class="list-amount">12:04</span>
      <span class="badge badge-green">Active</span>
    </span>
  </div>
</div>
```
Avatar colours: `avatar-c1`…`avatar-c5`; sizes `avatar-sm`, `avatar-lg`.

**Alerts** — `<div class="alert alert-error">…</div>` (`alert-success`, `alert-info`).

## Blazor gotchas in this migration

- Two-way binding is `@bind="field"` on plain inputs — **never** `@bind-Value=`.
- `@onclick="() => Foo(x)"`; for a no-arg async handler just `@onclick="SubmitAsync"`.
- Razor cannot contain `<!-- -->` inside `@code` blocks; several files currently have
  HTML comments illegally placed inside C# — delete those lines and restore real code.
- Keep every existing `@inject`, service call, localization key, route and business rule
  exactly as-is. This is a **presentation-layer** migration only.
- Keep `@attribute [Authorize]` and `<PageTitle>` where present.
