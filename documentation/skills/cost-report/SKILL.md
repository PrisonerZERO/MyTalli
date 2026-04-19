---
name: cost-report
description: Build branded financial and costing HTML documents for the MyTalli project. Use this skill whenever the user asks to create a cost report, pricing analysis, budget document, financial projection, cost comparison, infrastructure cost breakdown, or any HTML document that presents monetary data, tier pricing, or revenue analysis. Also use when the user says "cost document", "costing plan", "pricing plan", "budget report", or asks to document costs for any MyTalli service or feature.
---

# Cost Report Builder

This skill produces branded, single-file HTML documents for financial and costing analysis within the MyTalli project. The output should feel like a polished internal report — not a plain table dump.

The reference implementation is `documentation/MyTalli_CostingPlan.html` — read it before generating any new cost document to stay in sync with the latest patterns.

## Output Location

All cost/financial documents go in the `documentation/` folder at the repo root, named `MyTalli_{DocumentName}.html`.

## Document Structure

Every cost report follows this skeleton:

```
1. Purple swoosh header (title, subtitle, date)
2. Content wrapper (max-width: 960px, centered)
3. Sections, each with:
   - Section tag (purple pill label)
   - H2 heading
   - Descriptive paragraph
   - Data components (tables, cards, callouts, tip grids)
4. Footer
```

## Brand Foundation

These values are non-negotiable — they define the MyTalli look:

- **Font:** DM Sans from Google Fonts (weights 400, 500, 600, 700)
- **Page background:** `#f8f7fc`
- **Primary text:** `#1a1a2e`
- **Secondary text:** `#555`
- **Muted text:** `#999`
- **Primary purple:** `#6c5ce7`
- **Light purple:** `#8b5cf6`
- **Lavender:** `#a78bfa`
- **Soft purple bg:** `#f0edff`
- **Border:** `#e0dce8`
- **Success/profit:** `#27ae60`
- **Danger/loss:** `#e74c3c`
- **Warning:** `#f5c842`

The header uses the same purple gradient swoosh as the app's page branding — `linear-gradient(135deg, #6c5ce7 0%, #8b5cf6 50%, #6c5ce7 100%)` with two decorative `::before`/`::after` circles at `rgba(255,255,255,0.07)` and `rgba(255,255,255,0.05)`.

The logo text in the header is always `My<span>Talli</span>` where "Talli" is colored `#a78bfa`.

## CSS Convention

All CSS rules use single-line format with alphabetically-ordered declarations, matching the project's CSS formatting rule in CLAUDE.md.

## Component Library

### Section Tag
Purple pill label above each section heading.
```html
<span class="section-tag">Label Here</span>
```

### Callout
Left-bordered info box. Four variants based on border color:
```html
<div class="callout">           <!-- purple — general info -->
<div class="callout warning">   <!-- yellow — caution -->
<div class="callout success">   <!-- green — good news -->
<div class="callout danger">    <!-- red — risk/alert -->
```
Content is always `<p>` with a leading `<strong>` for the callout title.

### Data Table
Wrapped in `.table-wrap` for rounded borders and scroll overflow.
```html
<div class="table-wrap">
  <table>
    <thead><tr><th>...</th></tr></thead>
    <tbody><tr><td>...</td></tr></tbody>
  </table>
</div>
```
- Use `class="highlight-row"` on `<tr>` for the "current state" row
- Use `class="total-row"` on `<tr>` for summary/total rows (dark bg, white text, lavender last column)
- Use `class="cost"` on `<td>` and `<th>` for right-aligned monetary columns

### Cost Summary Cards
Grid of 3 big-number cards for headline metrics.
```html
<div class="cost-grid">
  <div class="cost-card highlight">   <!-- purple border for primary metric -->
    <div class="cost-label">Monthly Total</div>
    <div class="cost-amount"><span class="currency">$</span>75</div>
    <div class="cost-detail">Description</div>
  </div>
  <div class="cost-card">...</div>    <!-- default border -->
  <div class="cost-card savings">...</div>  <!-- green amount text -->
</div>
```

### Info Cards
Simple white cards for explanatory content.
```html
<div class="card">
  <h3>Card Title</h3>
  <p>Card content.</p>
</div>
```

### Tip Grid
2-column grid of optimization tips with icon badges.
```html
<div class="tip-grid">
  <div class="tip-card">
    <div class="tip-icon">$</div>
    <h4>Tip Title</h4>
    <p>Tip description.</p>
    <div class="tip-savings">Saves ~$X/mo</div>
  </div>
</div>
```

## Tier Badge Convention

When a table column shows service tiers that change across rows, use colored inline badges so the progression is visible at a glance. This is the most important visual convention in cost documents — plain text tier names are hard to scan.

### Badge Template
```html
<span style="background: {bg}; border-radius: 4px; color: {text}; font-size: 12px; font-weight: 600; padding: 2px 8px;">{Label}</span>
```

### Azure App Service Tier Colors
These are fixed — use the same colors everywhere for consistency across documents:

| Tier | Background | Text | Meaning |
|------|-----------|------|---------|
| S1 (Standard) | `#fee2e2` | `#b91c1c` | Red — legacy/current |
| P0v3 | `#d1fae5` | `#065f46` | Green — recommended |
| P1v3 | `#dbeafe` | `#1e40af` | Blue — growth |
| P2v3 | `#e9d5ff` | `#6b21a8` | Purple — scaling |
| P3v3 | `#fef3c7` | `#92400e` | Amber — enterprise |

### Azure SQL Database Tier Colors

| Tier | Background | Text |
|------|-----------|------|
| Basic | `#f3f4f6` | `#6b7280` | Gray — minimal |
| S0 | `#fef3c7` | `#92400e` | Amber |
| S1 | `#fed7aa` | `#9a3412` | Orange |
| S2 | `#fecaca` | `#991b1b` | Red |
| S3 | `#e9d5ff` | `#6b21a8` | Purple |

### Margin/Profit Badges
For net margin or profit/loss columns:
- **Profit:** green badge (`#d1fae5` / `#065f46`)
- **Loss:** red badge (`#fee2e2` / `#b91c1c`)

### Action Severity Badges
For monitoring or action columns:
- **Scale up (urgent):** red badge (`#fee2e2` / `#b91c1c`)
- **Scale decision:** amber badge (`#fef3c7` / `#92400e`)
- **Investigate:** blue badge (`#dbeafe` / `#1e40af`)

## When to Use Each Component

| Data Type | Component |
|-----------|-----------|
| 2-3 headline numbers | Cost summary cards |
| Tabular comparison | Data table with tier badges |
| Important context | Callout (pick variant by tone) |
| Actionable recommendations | Tip grid |
| Explanatory detail | Info card |
| Tier/service names in tables | Tier badges (always) |
| Monetary deltas (+/-) | Margin badges (green/red) |

## Bulleted Lists Inside Callouts

When a callout needs a list (e.g., explaining a naming convention), use inline-styled `<ul>` and `<li>` with purple bullets:

```html
<ul style="list-style: none; padding: 0; margin: 0;">
  <li style="color: #555; font-size: 14px; margin-bottom: 6px; padding-left: 16px; position: relative;">
    <span style="color: #6c5ce7; font-weight: 700; left: 0; position: absolute;">&bull;</span>
    <strong>Label</strong> — description
  </li>
</ul>
```

## Quality Checklist

Before declaring a cost document complete:

1. Header has title, subtitle, and "Last updated" date
2. Every section has a section tag + h2 + intro paragraph
3. All tier names in tables use colored badges (not plain text)
4. Monetary columns are right-aligned (`class="cost"`)
5. Current state rows use `class="highlight-row"`
6. At least one callout per section provides context or a key takeaway
7. Footer shows `&copy; {year} MyTalli — Internal Documentation`
8. CSS is single-line format with alphabetical declarations
9. All styles are in a single `<style>` block (no external CSS files)
10. Document is a single self-contained HTML file (no external dependencies except Google Fonts)
