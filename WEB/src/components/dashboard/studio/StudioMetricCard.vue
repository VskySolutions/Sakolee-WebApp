<template>
  <!-- A single KPI tile: label and value on the left, a tinted icon square on the right. -->
  <q-card
    flat
    bordered
    class="metric-card"
    :class="{ 'cursor-pointer': !!to }"
    v-bind="to ? { to } : {}"
  >
    <q-card-section class="row items-center no-wrap">
      <div class="col">
        <div class="metric-card__label ellipsis">{{ label }}</div>
        <div class="metric-card__value">{{ display }}</div>
      </div>
      <div class="metric-card__icon" :style="{ background: hue.tint }">
        <q-icon :name="icon" size="20px" :style="{ color: hue.accent }" />
      </div>
    </q-card-section>
  </q-card>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  label: { type: String, required: true },
  value: { type: [Number, String, null], default: null },
  icon: { type: String, default: "o_insights" },
  // One of the keys in HUES below.
  tone: { type: String, default: "indigo" },
  to: { type: [Object, String, null], default: null }
});

// The prototype gives every tile its own hue rather than reusing one accent; these are its nine,
// resolved through the tokens so the set stays in one place.
const HUES = {
  purple: { accent: "var(--hue-purple)", tint: "var(--hue-purple-tint)" },
  red: { accent: "var(--error)", tint: "var(--error-tint)" },
  indigo: { accent: "var(--primary)", tint: "var(--primary-tint)" },
  blue: { accent: "var(--hue-blue)", tint: "var(--hue-blue-tint)" },
  emerald: { accent: "var(--success)", tint: "var(--success-tint)" },
  cyan: { accent: "var(--hue-cyan)", tint: "var(--hue-cyan-tint)" },
  teal: { accent: "var(--hue-teal)", tint: "var(--hue-teal-tint)" },
  amber: { accent: "var(--warning)", tint: "var(--warning-tint)" },
  rose: { accent: "var(--danger)", tint: "var(--danger-tint)" }
};

const hue = computed(() => HUES[props.tone] ?? HUES.indigo);

// Thousands separators, matching the reference; non-numeric values pass straight through.
const display = computed(() => {
  const v = props.value;
  if (v == null) return "—";
  return typeof v === "number" ? v.toLocaleString() : v;
});
</script>

<style scoped>
.metric-card {
  border-radius: 16px;
  height: 100%;
  transition: box-shadow 0.2s ease, border-color 0.2s ease;
}

.metric-card.cursor-pointer:hover {
  border-color: var(--outline-variant);
  box-shadow: var(--shadow);
}

.metric-card__label {
  font-size: 12px;
  font-weight: 600;
  color: var(--outline);
  margin-bottom: 4px;
}

.metric-card__value {
  font-size: 26px;
  font-weight: 700;
  line-height: 1.1;
  color: var(--on-surface);
}

.metric-card__icon {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  margin-left: 12px;
}
</style>
