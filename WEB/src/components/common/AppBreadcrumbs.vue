<template>
  <q-breadcrumbs
    :class="noMargin ? 'text-grey-7' : 'q-mb-md text-grey-7'" class="text-uppercase q-mb-xs"
  >
    <template #separator>
      <q-icon
        size="1.5em"
        name="o_chevron_right"
        color="primary"
      />
    </template>

    <q-breadcrumbs-el
      v-for="(item, index) in items"
      :key="index"
      :label="item.label"
      :icon="item.icon"
      :to="item.to"
      :class="index === items.length - 1
        ? 'text-primary'
        : 'text-86 fw-600'"
    />
  </q-breadcrumbs>
</template>

<script setup>
defineProps({
  // [{ label, icon?, to? }]
  items: { type: Array, default: () => [] },
  noMargin: { type: Boolean, default: false }
});
</script>
<style scoped>
.q-breadcrumbs, .q-breadcrumbs__el {
    font-size: 11px !important;
    font-weight: 600 !important;
}
</style>
