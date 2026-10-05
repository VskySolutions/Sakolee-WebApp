<template>
  <div :class="['detail-field', { 'detail-field--full': full }]">
    <div class="detail-label">
      {{ label }}
    </div>

    <div class="detail-value">
      {{ value || "—" }}
    </div>
  </div>
</template>

<script setup>
defineProps({
  label: {
    type: String,
    required: true
  },
  value: {
    type: [String, Number, Boolean],
    default: null
  },
  full: {
    type: Boolean,
    default: false
  }
});
</script>

<style scoped lang="scss">
.detail-field {
  min-width: 0;
}

.detail-field--full {
  margin-bottom: 16px;
}

.detail-label {
  font-size: 12px;
  font-weight: 500;
  color: #858b98;
  margin-bottom: 4px;
}

.detail-value {
  font-size: 14px;
  font-weight: 500;
  color: #252533;
  word-break: break-word;
}
</style>
