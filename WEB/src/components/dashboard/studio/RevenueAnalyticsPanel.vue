<template>
  <q-card flat bordered class="panel">
    <q-card-section class="row items-start no-wrap q-pb-sm">
      <div class="col">
        <div class="panel__title">Monthly Revenue Analytics</div>
        <div class="panel__caption">Comparative performance</div>
      </div>
      <div class="text-right">
        <div class="panel__stat-label">YTD Revenue</div>
        <div class="panel__stat-value">
          {{ ytd }}
          <span v-if="changePct !== null" class="panel__delta" :class="changePct >= 0 ? 'panel__delta--up' : 'panel__delta--down'">
            <q-icon :name="changePct >= 0 ? 'o_arrow_upward' : 'o_arrow_downward'" size="12px" />
            {{ Math.abs(changePct).toFixed(1) }}%
          </span>
        </div>
      </div>
    </q-card-section>

    <q-separator />

    <q-card-section class="q-pt-sm">
      <div v-if="!points.length" class="text-grey-6 text-center q-pa-md">No revenue recorded.</div>
      <area-chart
        v-else
        :points="points"
        color="var(--primary)"
        :aria-label="`Monthly revenue, ${points[0].label} to ${points[points.length - 1].label}`"
        :format-value="currency"
      />
    </q-card-section>
  </q-card>
</template>

<script setup>
import { computed } from "vue";
import AreaChart from "components/dashboard/charts/AreaChart.vue";

const props = defineProps({
  // { ytdTotal, changePct, points: [{ label, value }] }
  revenue: { type: [Object, null], default: null }
});

const points = computed(() => props.revenue?.points || []);
const changePct = computed(() => (props.revenue?.changePct ?? null));

// Whole dollars in the headline — cents on a year-to-date total are noise.
const ytd = computed(() =>
  props.revenue?.ytdTotal == null
    ? "—"
    : new Intl.NumberFormat("en-US", { style: "currency", currency: "USD", maximumFractionDigits: 0 }).format(props.revenue.ytdTotal));

const currency = (value) =>
  new Intl.NumberFormat("en-US", { style: "currency", currency: "USD", maximumFractionDigits: 0 }).format(value);
</script>

<style scoped>
.panel { border-radius: 16px; height: 100%; }

.panel__title {
  font-size: 14px;
  font-weight: 700;
  color: var(--on-surface);
}

.panel__caption {
  font-size: 11px;
  color: var(--outline);
}

.panel__stat-label {
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--outline);
}

.panel__stat-value {
  font-size: 16px;
  font-weight: 700;
  color: var(--primary);
  white-space: nowrap;
}

/* The arrow carries the direction alongside the colour, so the trend is never colour-alone. */
.panel__delta {
  font-size: 12px;
  font-weight: 600;
  margin-left: 4px;
}

.panel__delta--up { color: var(--success); }
.panel__delta--down { color: var(--error); }
</style>
