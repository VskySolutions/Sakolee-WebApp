<template>
  <!-- Section rule above a band of cards: icon + title (+ count badge) on the left, a quiet note on the right. -->
  <div class="studio-section row items-center no-wrap q-mb-sm">
    <q-icon :name="icon" size="20px" :color="iconColor" class="q-mr-sm" />
    <div class="studio-section__title">{{ title }}</div>
    <q-badge v-if="badge" :color="badgeColor" class="studio-section__badge q-ml-sm">{{ badge }}</q-badge>
    <q-space />
    <div v-if="note" class="studio-section__note">{{ note }}</div>
  </div>
</template>

<script setup>
defineProps({
  title: { type: String, required: true },
  icon: { type: String, default: "o_dashboard" },
  iconColor: { type: String, default: "primary" },
  badge: { type: [String, Number, null], default: null },
  badgeColor: { type: String, default: "negative" },
  note: { type: [String, null], default: null }
});
</script>

<style scoped>
.studio-section__title {
  font-size: 15px;
  font-weight: 600;
  color: var(--on-surface);
}

.studio-section__badge {
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.04em;
  border-radius: 6px;
  padding: 2px 7px;
}

.studio-section__note {
  font-size: 12px;
  color: var(--outline);
  white-space: nowrap;
}
</style>
