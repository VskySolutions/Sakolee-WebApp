<template>
  <q-card flat bordered class="panel column">
    <q-card-section class="row items-start no-wrap panel__head q-pb-sm">
      <div class="col">
        <div class="panel__title">Unpaid Balances Aging</div>
        <div class="panel__caption">Receivables breakdown</div>
      </div>
      <div class="text-right">
        <div class="panel__stat-label">Total Overdue</div>
        <div class="panel__stat-value">{{ currency(receivables?.totalOverdue) }}</div>
      </div>
    </q-card-section>

    <q-separator />

    <q-card-section class="col q-pt-sm">
      <div v-if="!buckets.length" class="text-grey-6 text-center q-pa-md">No outstanding balances.</div>

      <div v-for="bucket in buckets" v-else :key="bucket.key" class="aging-row row items-center no-wrap">
        <!-- The severity ramp rides a dot, not the text: the light end of the ramp cannot meet text
             contrast on white, and the row's own label already names the bucket. -->
        <span v-if="severityColor(bucket.severity)" class="aging-row__dot" :style="{ background: severityColor(bucket.severity) }" />
        <span v-else class="aging-row__dot aging-row__dot--none" />
        <div class="col aging-row__label">{{ bucket.label }}</div>
        <div class="aging-row__amount">{{ currency(bucket.amount) }}</div>
      </div>
    </q-card-section>

    <q-card-section class="q-pt-none">
      <q-btn unelevated no-caps color="primary" class="full-width panel__cta" label="View Unpaid Families" @click="$emit('view-unpaid')" />
    </q-card-section>
  </q-card>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  // { totalOverdue, buckets: [{ key, label, amount, severity }] }
  receivables: { type: [Object, null], default: null }
});

defineEmits(["view-unpaid"]);

const buckets = computed(() => props.receivables?.buckets || []);

// Amber 500 / orange 600 / red 800 — the one three-step ramp that clears the palette validator on
// every check, including the normal-vision floor the prototype's amber-600/orange-600 pair fails.
const SEVERITY_COLORS = {
  low: "#f59e0b",
  medium: "#ea580c",
  high: "#991b1b"
};

const severityColor = (severity) => SEVERITY_COLORS[severity] || null;

const currency = (value) =>
  value == null
    ? "—"
    : new Intl.NumberFormat("en-US", { style: "currency", currency: "USD", minimumFractionDigits: 2 }).format(value);
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
  color: var(--error);
}

.aging-row {
  padding: 7px 0;
  border-bottom: 1px solid var(--line);
  font-size: 12px;
}

.aging-row:last-child { border-bottom: none; }

.aging-row__dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  margin-right: 10px;
  flex: 0 0 auto;
}

.aging-row__dot--none {
  background: transparent;
  border: 1px solid var(--outline-variant);
}

.aging-row__label {
  font-weight: 600;
  color: var(--on-surface);
}

.aging-row__amount {
  font-weight: 700;
  color: var(--on-surface);
  margin-left: 12px;
}

.panel__cta { border-radius: 12px; font-weight: 600; }
</style>
