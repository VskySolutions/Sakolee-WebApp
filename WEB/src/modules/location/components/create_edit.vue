<template>
  <app-form-dialog
    v-model="isOpen"
    :title="isEditing ? 'Edit Location' : 'Create Location'"
    :saving="saving"
    :save-label="isEditing ? 'Save' : 'Create'"
    size="sm"
    @submit="submitForm"
    @cancel="resetForm"
  >
    <q-form ref="formRef" greedy>
      <!-- Location Name Field -->
      <app-text-field
        v-model="form.name"
        label="Location Name"
        required
        class="q-mb-md"
        :error="formErrors.name.hasError"
        :error-message="formErrors.name.message"
        @update:model-value="formErrors.name.hasError = false"
        :rules="[
          (v) => !!v?.trim() || 'Location name is required',
          (v) =>
            !v ||
            v.trim().length <= 100 ||
            'Location name cannot exceed 100 characters'
        ]"
      />

      <!-- Status Toggle -->
      <q-toggle
        v-model="form.active"
        label="Active"
      />
    </q-form>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, watch, computed } from "vue";
import { locationApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";

import AppFormDialog from "components/common/AppFormDialog.vue";
import AppTextField from "components/common/AppTextField.vue";

// Props and Emits
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  editingId: { type: [Object, String, Number], default: null },
  initialData: { type: Object, default: null }
});

const emit = defineEmits(["update:modelValue", "saved"]);

const notify = useNotify();
const saving = ref(false);
const formRef = ref(null);

const isOpen = ref(props.modelValue);

// Check if we are in editing mode
const isEditing = computed(() => !!props.editingId);

// Reactive form data
const form = reactive({
  name: "",
  active: true
});

// Reactive form error states
const formErrors = reactive({
  name: { hasError: false, message: "" }
});

// Watcher to handle prop changes and populate form when editing
watch(() => props.modelValue, (val) => {
  isOpen.value = val;
  if (val) {
    if (props.editingId && props.initialData) {
      const row = props.initialData;
      form.name = row.name || row.Name || row.locationName || row.LocationName || "";
      form.active = row.active ?? row.Active ?? row.is_active ?? true;
    } else {
      resetFormValues();
    }
  }
});

// Watcher to emit dialog visibility changes back to parent
watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

// Function to reset form fields and errors
const resetFormValues = () => {
  form.name = "";
  form.active = true;
  formErrors.name.hasError = false;
  formErrors.name.message = "";
};

// Function to cancel and close the dialog
const resetForm = () => {
  resetFormValues();
  isOpen.value = false;
};

// Function to handle form submission (Create / Update)
const submitForm = async ({ clearDraft } = {}) => {
  formErrors.name.hasError = false;
  formErrors.name.message = "";

  if (!(await formRef.value?.validate())) {
    return;
  }

  saving.value = true;

  try {
    const payload = {
      name: form.name.trim(),
      active: form.active
    };

    if (props.editingId) {
      await locationApi.update(props.editingId, payload);
      notify.success("Location updated successfully.");
    } else {
      await locationApi.create(payload);
      notify.success("Location created successfully.");
    }

    clearDraft?.();
    isOpen.value = false;
    resetFormValues();
    emit("saved");
  } catch (err) {
    const errCode = getApiErrorCode(err);
    const errMsg = getApiErrorMessage(err)?.toLowerCase() || '';

    if (errCode === ApiErrorCodes.DuplicateIdentifier || errMsg.includes('already exists')) {
      formErrors.name.hasError = true;
      formErrors.name.message = "A location with this name already exists.";
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};
</script>