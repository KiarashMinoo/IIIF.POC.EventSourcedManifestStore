# Pages.Shared

## Contents

- [Overview](#overview)
- [Files](#files)
- [Diagrams](#diagrams)
- [See also](#see-also)

## Overview

This folder holds the one shared Razor layout every page renders into. It has no code-behind and defines no C# types — it is markup only, referenced by `Pages/_ViewStart.cshtml`.

## Files

| File | Primary type(s) | LOC (approx) | Responsibility |
|---|---|---|---|
| `_Layout.cshtml` | — | 31 | Page shell: `<head>`, header with navigation, a `TempData["StatusMessage"]` banner, and the footer. |

`_Layout.cshtml` sets the page `<title>` from `ViewData["Title"]`, links `wwwroot/css/site.css`, renders a header with links to `/Index` (Open Manifest) and `/Manifests/Create` (Import Manifest), renders `@RenderBody()` inside a `<main class="shell">`, and shows any `TempData["StatusMessage"]` set by the previous request — this is how `Create`, `Details`, and `Delete` surface their command results after a redirect.

## Diagrams

Not applicable. This folder has no types or control flow of its own to diagram; it is a static template composed around whatever page renders into it.

## See also

- [Pages](../README.md) — `_ViewStart.cshtml`, which selects this layout for every page.

[↑ Back to top](#contents)
