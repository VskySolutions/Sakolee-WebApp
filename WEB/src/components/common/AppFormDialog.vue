<template>
  <q-dialog
    v-model="dialogOpen"
    persistent
    transition-show="scale"
    transition-hide="scale" class="dialogs-scss"
  >
    <q-card
      class="app-form-dialog"
      :class="`app-form-dialog--${size}`"
    >
      <!-- Header -->
      <q-card-section class="app-form-dialog__header">
        <div class="app-form-dialog__header-content">
          <div v-if="avatarText" class="app-form-dialog__avatar">
            {{ avatarText }}
          </div>

          <div class="app-form-dialog__title-content">
            <div class="app-form-dialog__title">
              {{ title }}
            </div>

            <div v-if="subtitle" class="app-form-dialog__subtitle">
              {{ subtitle }}
            </div>
          </div>
        </div>

        <q-btn flat round dense icon="o_close" :disable="saving" size="12px" class="text-grey-7" @click="cancel">
          <q-tooltip>Close</q-tooltip>
        </q-btn>
      </q-card-section>

      <q-separator />

      <!-- Scrollable content -->
      <q-card-section class="app-form-dialog__body">
        <slot />
      </q-card-section>

      <!-- Footer -->
      <template v-if="!hideFooter">
        <q-separator />

        <q-card-actions
          align="right"
          class="app-form-dialog__footer"
        >
          <q-btn
            flat
            no-caps
            label="Cancel"
            :disable="saving"
            class="close-btn br-12"
            @click="cancel"
          />

          <!-- Optional extra actions (e.g. "Save as Draft"): Cancel moves to the left, these sit beside Save. -->
          <template v-if="$slots['footer-actions']">
            <q-space />
            <slot name="footer-actions" />
          </template>

          <q-btn
            v-if="!hideSave"
            unelevated
            no-caps
            color="primary"
            :label="saveLabel"
            :loading="saving"
            :disable="saving"
            class="save-btn br-12"
            @click="$emit('submit')"
          />
        </q-card-actions>
      </template>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },

  title: {
    type: String,
    default: ""
  },

  subtitle: {
    type: String,
    default: ""
  },

  avatarText: {
    type: String,
    default: ""
  },

  saving: {
    type: Boolean,
    default: false
  },

  saveLabel: {
    type: String,
    default: "Save"
  },

  hideSave: {
    type: Boolean,
    default: false
  },

  hideFooter: {
    type: Boolean,
    default: false
  },

  size: {
    type: String,
    default: "md",
    validator: (value) => ["sm", "md", "lg", "xl"].includes(value)
  }
});

const emit = defineEmits([
  "update:modelValue",
  "submit",
  "cancel"
]);

const dialogOpen = computed({
  get: () => props.modelValue,
  set: (value) => emit("update:modelValue", value)
});

const cancel = () => {
  if (props.saving) return;

  emit("update:modelValue", false);
  emit("cancel");
};
</script>
<style scoped lang="scss">

</style>
