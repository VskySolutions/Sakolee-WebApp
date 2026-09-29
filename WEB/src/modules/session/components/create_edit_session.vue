<template>
  <app-form-drawer
    v-model="isOpen"
    :title="editingId ? 'Edit Session' : 'Create Session'"
    :saving="saving"
    :save-label="editingId ? 'Save' : 'Create'"
    @submit="submitForm"
    @cancel="resetForm"
  >
    <q-form ref="formRef" greedy>
      <app-text-field
        v-model="form.sessionName"
        label="Session Name"
        required
        class="q-mb-md"
        :error="formErrors.sessionName.hasError"
        :error-message="formErrors.sessionName.message"
        @update:model-value="formErrors.sessionName.hasError = false"
        :rules="[
          (v) => !!v?.trim() || 'Session name is required',
          (v) =>
            !v ||
            v.trim().length <= 100 ||
            'Session name cannot exceed 100 characters'
        ]"
      />

      <q-toggle
        v-model="form.active"
        label="Active"
      />
    </q-form>
  </app-form-drawer>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { classSessionApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";

import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppTextField from "components/common/AppTextField.vue";

// Props and emits
const props = defineProps({
  //used to control the visibility of the form drawer
  modelValue: { type: Boolean, default: false },
  editingId: { type: [String, Number, null], default: null },
  initialData: { type: Object, default: null }
});

// Emits
const emit = defineEmits(["update:modelValue", "saved"]);

// State
const notify = useNotify();
const saving = ref(false);
const formRef = ref(null);

// Reactive form state
const form = reactive({
  sessionName: "",
  active: true
});

// Reactive form errors state
const formErrors = reactive({
  sessionName: { hasError: false, message: "" }
});

// Computed property to control the visibility of the form drawer
const isOpen = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val)
});

// Watch for changes in modelValue to populate form fields when editing
watch(
  () => props.modelValue,
  (val) => {
    if (val) {
      if (props.editingId && props.initialData) {
        form.sessionName = props.initialData.sessionName || props.initialData.SessionName || props.initialData.name || props.initialData.Name || "";
        form.active = props.initialData.active ?? props.initialData.Active ?? props.initialData.is_active ?? props.initialData.isActive ?? true;
      } else {
        resetFormFields();
      }
    }
  }
);

// Function to reset form fields to their default values
const resetFormFields = () => {
  form.sessionName = "";
  form.active = true;
  formErrors.sessionName.hasError = false;
  formErrors.sessionName.message = "";
};

// Function to reset the form and close the drawer
const resetForm = () => {
  emit("update:modelValue", false);
  resetFormFields();
};

// Function to handle form submission
const submitForm = async ({ clearDraft } = {}) => {
  formErrors.sessionName.hasError = false;
  formErrors.sessionName.message = "";

  if (!(await formRef.value?.validate())) {
    return;
  }

  saving.value = true;

  try {
    // Prepare payload for API
    const payload = {
      name: form.sessionName.trim(),
      sessionName: form.sessionName.trim(),
      isActive: Boolean(form.active),
      active: Boolean(form.active),
      Active: Boolean(form.active),
      is_active: Boolean(form.active)
    };

    // Call the appropriate API method based on whether we're editing or creating
    if (props.editingId) {
      await classSessionApi.update(props.editingId, payload);
      notify.success("Session updated successfully.");
    } else {
      await classSessionApi.create(payload);
      notify.success("Session created successfully.");
    }

    // Clear any draft data if applicable
    clearDraft?.();
    emit("saved");
    resetForm();
  } catch (err) {
    // Handle API errors, specifically checking for duplicate session names
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier || getApiErrorMessage(err)?.toLowerCase().includes('already exists')) {
      formErrors.sessionName.hasError = true;
      formErrors.sessionName.message = "A session with this name already exists.";
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};
</script>