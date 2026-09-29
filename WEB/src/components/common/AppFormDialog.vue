<template>
  <q-dialog
    v-model="dialogOpen"
    persistent
    transition-show="scale"
    transition-hide="scale"
  >
    <q-card
      class="app-form-dialog"
      :class="`app-form-dialog--${size}`"
    >
      <!-- Header -->
      <q-card-section class="app-form-dialog__header">
        <div class="text-h6 fw-600">
          {{ title }}
        </div>

        <q-btn
          flat
          round
          dense
          icon="o_close"
          :disable="saving"
          size="12px"
          class="text-grey-7"
          @click="cancel"
        >
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
            @click="cancel"
            class="close-btn br-12"
          />

          <q-btn
            v-if="!hideSave"
            unelevated
            no-caps
            color="primary"
            :label="saveLabel"
            :loading="saving"
            :disable="saving"
            @click="$emit('submit')"
            class="save-btn br-12"
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

  /*
   * sm = simple forms
   * md = normal CRUD forms
   * lg = larger/multi-section forms
   */
  size: {
    type: String,
    default: "md",
    validator: (value) => ["sm", "md", "lg"].includes(value)
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
.app-form-dialog {
  width: calc(100vw - 32px);
  max-height: calc(100vh - 48px);
  display: flex;
  flex-direction: column;
  border-radius: 16px;
  overflow: hidden;

  /*
   * Dialog width is controlled by the form complexity.
   * Content can increase height until max-height is reached;
   * after that only the body scrolls.
   */
  &--sm {
    max-width: 480px;
  }

  &--md {
    max-width: 680px;
  }

  &--lg {
    max-width: 900px;
  }

  &__header {
    min-height: 64px;
    padding: 16px 20px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
    flex-shrink: 0;
  }

  &__body {
    padding: 20px;
    overflow-y: auto;
    overflow-x: hidden;
    flex: 1 1 auto;
    background-color: #fbfbfe !important;
  }

  &__footer {
    min-height: 64px;
    padding: 12px 20px;
    flex-shrink: 0;
  }
}

/*
 * Mobile:
 * Keep the dialog inside the viewport with a small outer margin.
 * Width becomes effectively full-screen while preserving rounded edges.
 */
@media (max-width: 599px) {
  .app-form-dialog {
    width: calc(100vw - 24px);
    max-width: calc(100vw - 24px) !important;
    max-height: calc(100vh - 24px);

    &__header {
      padding: 14px 16px;
    }

    &__body {
      padding: 16px;
    }

    &__footer {
      padding: 12px 16px;
    }
  }
}
</style>
