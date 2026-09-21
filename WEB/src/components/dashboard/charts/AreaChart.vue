<template>
  <!-- The series colour rides a custom property rather than the SVG attributes: `var()` is not
       honoured in a presentation attribute like stroke/stop-color, only in a CSS property. -->
  <figure class="area-chart" :style="{ '--chart-color': color }">
    <div ref="plotRef" class="area-chart__plot" @pointermove="onMove" @pointerleave="onLeave">
      <svg
        class="area-chart__svg"
        :viewBox="`0 0 ${VB_W} ${VB_H}`"
        preserveAspectRatio="none"
        role="img"
        :aria-label="ariaLabel"
      >
        <defs>
          <linearGradient :id="gradientId" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" class="area-chart__stop-top" />
            <stop offset="100%" class="area-chart__stop-bottom" />
          </linearGradient>
        </defs>

        <path :d="areaPath" :fill="`url(#${gradientId})`" />
        <!-- non-scaling-stroke keeps the line 2px however the viewBox is stretched; without it
             preserveAspectRatio="none" distorts the stroke along with the geometry. -->
        <path
          :d="linePath"
          fill="none"
          class="area-chart__line"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
          vector-effect="non-scaling-stroke"
        />

        <template v-if="activeIndex !== null">
          <line
            :x1="coords[activeIndex].x"
            y1="0"
            :x2="coords[activeIndex].x"
            :y2="VB_H"
            class="area-chart__crosshair"
            vector-effect="non-scaling-stroke"
          />
        </template>
      </svg>

      <!-- The marker is HTML, not SVG: a circle inside a stretched viewBox renders as an ellipse. -->
      <span
        v-if="activeIndex !== null"
        class="area-chart__marker"
        :style="{ left: `${pctX(activeIndex)}%`, top: `${pctY(activeIndex)}%` }"
      />

      <div
        v-if="activeIndex !== null"
        class="area-chart__tooltip"
        :style="{ left: `${pctX(activeIndex)}%`, top: `${pctY(activeIndex)}%` }"
      >
        <span class="area-chart__tooltip-label">{{ points[activeIndex].label }}</span>
        <span class="area-chart__tooltip-value">{{ formatValue(points[activeIndex].value) }}</span>
      </div>
    </div>

    <div class="area-chart__axis">
      <span v-for="tick in axisTicks" :key="tick.label">{{ tick.label }}</span>
    </div>

    <!-- The table view: the same numbers, reachable by screen reader and by anyone who cannot use
         the hover layer. Visually hidden, not display:none, so it stays in the accessibility tree. -->
    <figcaption class="sr-only">
      <table>
        <caption>{{ ariaLabel }}</caption>
        <thead><tr><th scope="col">Period</th><th scope="col">Value</th></tr></thead>
        <tbody>
          <tr v-for="p in points" :key="p.label">
            <th scope="row">{{ p.label }}</th>
            <td>{{ formatValue(p.value) }}</td>
          </tr>
        </tbody>
      </table>
    </figcaption>
  </figure>
</template>

<script setup>
import { computed, ref } from "vue";

// Single-series area chart. One series carries no legend — the panel's heading names it.
const props = defineProps({
  // [{ label, value }]
  points: { type: Array, default: () => [] },
  color: { type: String, default: "#4648d4" },
  ariaLabel: { type: String, default: "Area chart" },
  // How many x-axis labels to print. Every month's label would collide at this width.
  axisTickCount: { type: Number, default: 6 },
  formatValue: { type: Function, default: (v) => String(v) }
});

const VB_W = 500;
const VB_H = 150;
// Headroom so the peak's stroke and marker are not clipped by the viewBox.
const PAD_TOP = 12;
const PAD_BOTTOM = 6;

// Unique per instance — two charts on one page would otherwise share a gradient id.
const gradientId = `areaGrad-${Math.random().toString(36).slice(2, 9)}`;

const values = computed(() => props.points.map((p) => Number(p.value) || 0));

// The band is padded by 5% of its own height at each end, so a flat series still draws mid-box
// rather than collapsing onto an edge.
const bounds = computed(() => {
  const vs = values.value;
  if (!vs.length) return { min: 0, max: 1 };
  const min = Math.min(...vs);
  const max = Math.max(...vs);
  if (min === max) return { min: min - 1, max: max + 1 };
  const pad = (max - min) * 0.05;
  return { min: min - pad, max: max + pad };
});

const coords = computed(() => {
  const n = props.points.length;
  const { min, max } = bounds.value;
  const plotH = VB_H - PAD_TOP - PAD_BOTTOM;
  return values.value.map((v, i) => ({
    x: n > 1 ? (i / (n - 1)) * VB_W : VB_W / 2,
    y: PAD_TOP + (1 - (v - min) / (max - min)) * plotH
  }));
});

// Catmull-Rom through the points, emitted as cubic beziers — a smooth curve that still passes
// through every value, unlike a hand-tuned Q/T path.
const linePath = computed(() => {
  const pts = coords.value;
  if (!pts.length) return "";
  if (pts.length === 1) return `M ${pts[0].x},${pts[0].y}`;

  let d = `M ${pts[0].x.toFixed(2)},${pts[0].y.toFixed(2)}`;
  for (let i = 0; i < pts.length - 1; i++) {
    const p0 = pts[i - 1] || pts[i];
    const p1 = pts[i];
    const p2 = pts[i + 1];
    const p3 = pts[i + 2] || p2;
    const c1x = p1.x + (p2.x - p0.x) / 6;
    const c1y = p1.y + (p2.y - p0.y) / 6;
    const c2x = p2.x - (p3.x - p1.x) / 6;
    const c2y = p2.y - (p3.y - p1.y) / 6;
    d += ` C ${c1x.toFixed(2)},${c1y.toFixed(2)} ${c2x.toFixed(2)},${c2y.toFixed(2)} ${p2.x.toFixed(2)},${p2.y.toFixed(2)}`;
  }
  return d;
});

const areaPath = computed(() => {
  const pts = coords.value;
  if (!pts.length) return "";
  return `${linePath.value} L ${pts[pts.length - 1].x.toFixed(2)},${VB_H} L ${pts[0].x.toFixed(2)},${VB_H} Z`;
});

const axisTicks = computed(() => {
  const n = props.points.length;
  if (!n) return [];
  const count = Math.min(props.axisTickCount, n);
  if (count < 2) return [{ label: props.points[0].label }];
  const step = (n - 1) / (count - 1);
  return Array.from({ length: count }, (_, i) => ({ label: props.points[Math.round(i * step)].label }));
});

// ---- Hover ----
const plotRef = ref(null);
const activeIndex = ref(null);

const pctX = (i) => (coords.value[i].x / VB_W) * 100;
const pctY = (i) => (coords.value[i].y / VB_H) * 100;

const onMove = (e) => {
  const el = plotRef.value;
  const n = props.points.length;
  if (!el || n === 0) return;
  const rect = el.getBoundingClientRect();
  if (!rect.width) return;
  const ratio = Math.min(Math.max((e.clientX - rect.left) / rect.width, 0), 1);
  activeIndex.value = Math.round(ratio * (n - 1));
};

const onLeave = () => { activeIndex.value = null; };
</script>

<style scoped>
.area-chart { margin: 0; }

.area-chart__plot {
  position: relative;
  width: 100%;
  height: 176px;
}

.area-chart__svg {
  width: 100%;
  height: 100%;
  display: block;
  overflow: visible;
}

.area-chart__line { stroke: var(--chart-color); }
.area-chart__stop-top { stop-color: var(--chart-color); stop-opacity: 0.25; }
.area-chart__stop-bottom { stop-color: var(--chart-color); stop-opacity: 0; }

.area-chart__crosshair {
  stroke: var(--outline-variant);
  stroke-width: 1;
  stroke-dasharray: 3 3;
}

.area-chart__marker {
  position: absolute;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: var(--white);
  border: 2px solid var(--chart-color);
  transform: translate(-50%, -50%);
  pointer-events: none;
}

.area-chart__tooltip {
  position: absolute;
  transform: translate(-50%, calc(-100% - 14px));
  pointer-events: none;
  white-space: nowrap;
  background: var(--on-surface);
  color: var(--white);
  border-radius: 8px;
  padding: 5px 9px;
  font-size: 11px;
  line-height: 1.35;
  display: flex;
  flex-direction: column;
  box-shadow: var(--shadow);
}

.area-chart__tooltip-label { opacity: 0.75; }
.area-chart__tooltip-value { font-weight: 700; }

.area-chart__axis {
  display: flex;
  justify-content: space-between;
  font-size: 10px;
  font-weight: 500;
  color: var(--outline);
  padding: 4px 2px 0;
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}
</style>
