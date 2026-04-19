---
name: scaling-plan
description: Build branded infrastructure scaling and capacity planning HTML documents for the MyTalli project. Use this skill whenever the user asks to create a scaling plan, capacity plan, infrastructure roadmap, growth strategy document, or any HTML document that presents scaling tiers, growth milestones, monitoring thresholds, or architecture decisions tied to user count. Also use when the user says "scaling document", "capacity plan", "growth plan", "infrastructure plan", or asks to document how the app handles increasing traffic or users.
---

# Scaling Plan Builder

This skill produces branded, single-file HTML documents for infrastructure scaling and capacity planning within the MyTalli project. The output should feel like an actionable roadmap — not a dry spec sheet. Every section should answer "what changes, when, and why."

The reference implementation is `documentation/MyTalli_ScalingPlan.html` — read it before generating any new scaling document to stay in sync with the latest patterns.

## Output Location

All scaling/planning documents go in the `documentation/` folder at the repo root, named `MyTalli_{DocumentName}.html`.

## Document Structure

Every scaling document follows this skeleton:

```
1. Purple swoosh header (title, subtitle, date)
2. Content wrapper (max-width: 960px, centered)
3. Sections, each with:
   - Section tag (purple pill label)
   - H2 heading
   - Descriptive paragraph
   - Data components (tier cards, tables, timelines, callouts, metric badges)
4. Footer
```

## Brand Foundation

These values are shared across all MyTalli internal documents:

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
- **Success:** `#27ae60`
- **Danger:** `#e74c3c`
- **Warning:** `#f5c842`

The header uses the purple gradient swoosh — `linear-gradient(135deg, #6c5ce7 0%, #8b5cf6 50%, #6c5ce7 100%)` with two decorative `::before`/`::after` circles at `rgba(255,255,255,0.07)` and `rgba(255,255,255,0.05)`.

The logo text in the header is always `My<span>Talli</span>` where "Talli" is colored `#a78bfa`.

## CSS Convention

All CSS rules use single-line format with alphabetically-ordered declarations, matching the project's CSS formatting rule in CLAUDE.md.

## Component Library

### Section Tag
Purple pill label above each section heading.
```html
<span class="section-tag">Label Here</span>
```

### Metric Badges
Inline big-number stats for headline figures (e.g., memory per circuit, retention period). Use for 2-4 key numbers at the top of the document.
```html
<span class="metric">
  <span class="metric-value">~400 KB</span>
  <span class="metric-label">Memory per<br>user circuit</span>
</span>
```
- `.metric-value`: `#6c5ce7`, 28px, bold
- `.metric-label`: `#999`, 12px

### Tier Cards
2-column grid of growth stage cards. Each card represents a scaling milestone with specs. Use `.current` for where the app is today and `.recommended` for the suggested next move.

```html
<div class="tier-grid">
  <div class="tier-card current">          <!-- purple border + "CURRENT" label -->
    <div class="tier-name">Stage 1 — Launch</div>
    <div class="tier-users">1 - 50 concurrent users</div>
    <div class="tier-desc">Up to ~500 registered users</div>
    <ul class="tier-specs">
      <li>App Service: <span style="...badge...">Standard S1</span> (1 core, 1.75 GB)</li>
      <li>SQL Database: <span style="...badge...">Basic</span> (5 DTU)</li>
    </ul>
  </div>
  <div class="tier-card recommended">      <!-- green border + "RECOMMENDED" label -->
    ...
  </div>
  <div class="tier-card">                  <!-- default border, no label -->
    ...
  </div>
</div>
```

The `::after` pseudo-element creates the "CURRENT" / "RECOMMENDED" label automatically via CSS. Spec lists use purple checkmarks (`\2713`) as bullets.

Tier names inside spec lists should always use colored badges (see Tier Badge Convention below) so the progression is visible at a glance.

### Data Table
Same pattern as the cost-report skill. Wrapped in `.table-wrap` for rounded borders.
```html
<div class="table-wrap">
  <table>
    <thead><tr><th>...</th></tr></thead>
    <tbody><tr><td>...</td></tr></tbody>
  </table>
</div>
```
- `class="highlight-row"` on `<tr>` for current state

### Callout
Left-bordered info box with three variants:
```html
<div class="callout">           <!-- purple — general info -->
<div class="callout warning">   <!-- yellow — caution -->
<div class="callout success">   <!-- green — good news -->
```

### Info Cards
Simple white cards for explanatory content (e.g., "What Happens When a User Disconnects", "DbContext Thread Safety").
```html
<div class="card">
  <h3>Card Title</h3>
  <p>Card content.</p>
</div>
```

### Timeline
Vertical timeline for action items with milestone dots. Use for sequential scaling actions ordered by growth stage.

```html
<div class="timeline">
  <div class="timeline-item">              <!-- purple dot — immediate action -->
    <h4>Immediate — Action Title</h4>
    <p>Description of what to do and why.</p>
    <span class="timeline-trigger" style="background: #d1fae5; color: #065f46;">Trigger: Do this now</span>
  </div>
  <div class="timeline-item future">       <!-- gray dot — future action -->
    <h4>At X Users — Action Title</h4>
    <p>Description.</p>
    <span class="timeline-trigger" style="background: #f3f4f6; color: #6b7280;">Trigger: Condition</span>
  </div>
</div>
```

Timeline trigger pill colors:
- **Immediate actions:** green background (`#d1fae5` / `#065f46`)
- **Future actions:** gray background (`#f3f4f6` / `#6b7280`)

The timeline line and dots are CSS-driven — `.timeline::before` draws the vertical line, `.timeline-item::before` draws the dot (purple for current, gray via `.future` for deferred).

## Tier Badge Convention

When tier names appear anywhere — table cells, spec lists, callouts — use colored inline badges. This is the most important visual convention. Plain text tier names are hard to scan across rows and cards.

### Badge Template
```html
<span style="background: {bg}; border-radius: 4px; color: {text}; font-size: 12px; font-weight: 600; padding: 2px 8px;">{Label}</span>
```

### Azure App Service Tier Colors
Consistent across all MyTalli documents:

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

### Action Severity Badges
For monitoring tables or action columns:
- **Scale up (urgent):** red badge (`#fee2e2` / `#b91c1c`)
- **Scale decision:** amber badge (`#fef3c7` / `#92400e`)
- **Investigate:** blue badge (`#dbeafe` / `#1e40af`)

### Recommendation Badges
For settings/config tables:
- **Keep default (no action):** gray badge (`#f3f4f6` / `#6b7280`)
- **Change at scale (future action):** amber badge (`#fef3c7` / `#92400e`)

## When to Use Each Component

| Content Type | Component |
|-------------|-----------|
| 2-4 headline numbers | Metric badges |
| Growth stages with specs | Tier cards (2-col grid) |
| Settings or capacity data | Data table with badges |
| Sequential actions by milestone | Timeline |
| Important context or takeaway | Callout (pick variant by tone) |
| Explanatory detail | Info card |
| Tier/service names anywhere | Tier badges (always) |

## Quality Checklist

Before declaring a scaling document complete:

1. Header has title, subtitle, and "Last updated" date
2. Every section has a section tag + h2 + intro paragraph
3. Tier cards use `.current` and `.recommended` classes appropriately
4. All tier names — in tables, cards, callouts, everywhere — use colored badges
5. Timeline items use green triggers for immediate, gray for future
6. Monitoring/action columns use severity badges
7. Settings/config columns use recommendation badges (gray vs amber)
8. At least one callout per section provides context or a key takeaway
9. Footer shows `&copy; {year} MyTalli — Internal Documentation`
10. CSS is single-line format with alphabetical declarations
11. All styles are in a single `<style>` block (no external CSS files)
12. Document is a single self-contained HTML file (no external dependencies except Google Fonts)
