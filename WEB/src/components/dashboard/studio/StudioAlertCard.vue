<template>
  <!-- One attention item. The severity tints the card border, the left rule, the pill and the action. -->
  <q-card
    flat
    bordered
    class="alert-card"
    :style="{ '--alert-accent': tone.accent, '--alert-rule': tone.rule, '--alert-tint': tone.tint, '--alert-edge': tone.edge }"
  >
    <q-card-section class="q-pb-sm">
      <div class="row items-start no-wrap">
        <div class="alert-card__pill">{{ severity }}</div>
        <q-space />
        <q-icon :name="tone.icon" size="18px" :style="{ color: tone.rule }" />
      </div>

      <div class="alert-card__title q-mt-sm">{{ alert.title }}</div>
      <div v-if="alert.detail" class="alert-card__detail">{{ alert.detail }}</div>
    </q-card-section>

    <q-separator />

    <q-card-section class="alert-card__foot row items-center no-wrap q-py-sm">
      <div class="alert-card__foot-note">{{ alert.footnote }}</div>
      <q-space />
      <q-btn
        v-if="alert.actionLabel && alert.actionRoute"
        flat
        dense
        no-caps
        size="sm"
        class="alert-card__action"
        icon-right="o_arrow_forward"
        :label="alert.actionLabel"
        :to="alert.actionRoute"
      />
    </q-card-section>
  </q-card>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  // { id, severity, title, detail, footnote, actionLabel, actionRoute }
  alert: { type: Object, required: true }
});

// Severity accents as the prototype sets them: `rule` draws the left border and the icon, `accent`
// the pill text and action link. Warning is the one case where the two differ (amber-500 vs -600).
const TONES = {
  critical: {
    accent: "var(--error)",
    rule: "var(--error)",
    tint: "var(--error-tint)",
    edge: "#fee2e2",
    icon: "o_priority_high"
  },
  warning: {
    accent: "var(--warning)",
    rule: "var(--warning-rule)",
    tint: "var(--warning-tint)",
    edge: "#fef3c7",
    icon: "o_warning"
  },
  info: {
    accent: "var(--primary)",
    rule: "var(--primary)",
    tint: "var(--primary-tint)",
    edge: "var(--primary-tint-strong)",
    icon: "o_info"
  }
};

const severity = computed(() => String(props.alert?.severity || "info").toLowerCase());
const tone = computed(() => TONES[severity.value] ?? TONES.info);
</script>

<style scoped>
.alert-card {
  border-radius: 16px;
  border-color: var(--alert-edge);
  border-left: 4px solid var(--alert-rule);
  height: 100%;
  display: flex;
  flex-direction: column;
}

.alert-card__pill {
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  border-radius: 4px;
  padding: 2px 8px;
  color: var(--alert-accent);
  background: var(--alert-tint);
}

.alert-card__title {
  font-size: 13px;
  font-weight: 700;
  color: var(--on-surface);
  line-height: 1.35;
}

.alert-card__detail {
  font-size: 11px;
  color: var(--on-surface-variant);
  margin-top: 2px;
}

/* The footer is pinned to the bottom so the three cards' rules line up at uneven title heights. */
.alert-card__foot { margin-top: auto; }

.alert-card__foot-note {
  font-size: 11px;
  color: var(--outline);
}

.alert-card__action {
  font-weight: 700;
  font-size: 12px;
  color: var(--alert-accent);
}
</style>
