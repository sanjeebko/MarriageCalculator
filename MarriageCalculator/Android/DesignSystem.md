# Marriage Calculator Design Specification

## 1. Visual Identity & Theme
The design is inspired by "Tihar Night" (a Nepalese festival), blending traditional festive colors with a modern, high-tech "Metallic Noir" and "Glass Morphism" aesthetic.

### Core Color Palette (Tihar Night)
- **Deep Red Tika:** `#660000` (Primary Action Brand Color)
- **Tihar Night Blue:** `#1A1A2E` (Deep Background Color)
- **Gold Accent:** `#D4AF37` (Header, Highlights, Icons)
- **Marigold Orange:** `#FF8C00` (Secondary Actions, Offline Indicator)
- **Base Black:** `#0D0D1A` (Gradient Depth)

### Metallic Noir Palette (Login System)
- **Metal Gold:** `#D4AF37`
- **Silver Top:** `#F2F2F2`
- **Silver Bottom:** `#909090`
- **Silver Glow:** `#FFFFFF`

## 2. Typography
- **Festive Headline:** Serif (Nepalese traditional feel), Bold, 34sp. Used for "नमस्ते" greetings.
- **Body / Label:** Sans-Serif Medium, 14sp - 18sp.
- **Letter Spacing:** 1.25sp for high-end labels.

## 3. Component Specifications

### Glass Morphism Elements
- **Side Menu (Navigation Drawer):**
    - **Background:** Dark Navy Gradient (`#1A1A2E` @ 95% alpha to `#0D0D1A` @ 90% alpha).
    - **Layers:** Internal glossy sheen layer (vertical gradient) for depth.
    - **Border:** 1dp horizontal gradient (White @ 20% to Transparent) on the right edge.
    - **Items:** 44dp height, medium-compact spacing.

### Buttons
- **GlassButton:**
    - **Frosted Look:** Semi-transparent background with internal sheen.
    - **Border:** High-contrast 1dp border (White top, Black bottom).
    - **Compactness:** 44dp height (Standard/Medium).
- **MetallicButton:**
    - **Bezel:** Multi-color linear gradient (Rim vs. Face).
    - **Shadow:** 12dp elevation for active state, 4dp for disabled.
    - **Effect:** Embossed text look with offset shadows.

### Data Display
- **ActiveGameCardCompact:**
    - **Shape:** 12dp Rounded Corners.
    - **Container:** Frosted glass background (`#FFFFFF` @ 5% alpha).
    - **Border:** 1dp subtle white stroke.
    - **Layout:** Horizontal row with primary info (Title, Date) and a circular "Resume" glass action.

## 4. UI Principles
- **Compactness:** Vertical spacing is minimized to allow list density.
- **Layering:** Uses semi-transparent Boxes and Borders to simulate stacked glass panes.
- **Hierarchy:** Primary actions use Red Glass (`DeepRedTika`), secondary use Clear/White Glass.

## 5. Navigation & Layout
- **Scaffold Header:** `CenterAlignedTopAppBar` using `TiharNightBlue`.
- **Primary Nav:** Modal Navigation Drawer with swipe support.
- **Layout Spacing:** 16dp horizontal margins, 8dp-12dp vertical item spacing.
